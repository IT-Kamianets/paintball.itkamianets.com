using System.Collections.Generic;
using ITKamianets.Engine.Character;
using ITKamianets.Engine.Core;
using UnityEngine;
using UnityEngine.AI;

namespace ITKamianets.Paintball.Core
{
    /// <summary>
    /// Drives a bot through the same <see cref="CharacterMotor"/> as the player.
    /// Paths come from the arena NavMesh; shooting goes through <see cref="IMarker"/>.
    /// </summary>
    [DefaultExecutionOrder(-35)]
    public class PaintballBot : MonoBehaviour
    {
        CharacterMotor _motor;
        Fighter _fighter;
        IMarker _marker;
        MatchDirector _director;
        IReadOnlyList<Vector3> _posts;
        NavMeshPath _path;
        Vector3 _destination;
        float _readyAt;
        float _repathAt;
        float _stuck;
        float _seed;

        public void SetPosts(IReadOnlyList<Vector3> posts)
        {
            _posts = posts;
        }

        void Awake()
        {
            _motor = GetComponent<CharacterMotor>();
            _fighter = GetComponent<Fighter>();
            _marker = GetComponent<IMarker>();
            _seed = Random.Range(0f, 10f);
            _readyAt = Time.time + 1f;
            PickPost();
        }

        void Update()
        {
            if (_director == null)
            {
                EngineServices.TryGet(out _director);
            }

            if (_fighter == null || _motor == null || _director == null || !_fighter.IsAlive || !_director.IsRunning)
            {
                if (_motor != null)
                {
                    _motor.SetCommand(new CharacterCommand { Yaw = _motor.Yaw });
                }

                return;
            }

            var enemy = FindTarget();
            var move = Vector3.zero;
            var yaw = _motor.Yaw;
            var sprint = false;
            var jump = false;

            if (enemy != null)
            {
                var toEyes = enemy.Motor.EyePosition - _motor.EyePosition;
                var flat = toEyes;
                flat.y = 0f;
                var distance = flat.magnitude;
                var side = Vector3.Cross(Vector3.up, flat.sqrMagnitude > 0.01f ? flat.normalized : transform.forward);
                var strafe = Mathf.Sign(Mathf.Sin(Time.time * 1.4f + _seed));
                Vector3 goal;
                if (distance > 16f)
                {
                    goal = enemy.transform.position;
                    sprint = true;
                }
                else if (distance < 5.5f)
                {
                    goal = transform.position - flat.normalized * 4f;
                }
                else
                {
                    goal = transform.position + side * (3f * strafe);
                }

                move = DesiredMove(goal);
                yaw = Mathf.MoveTowardsAngle(_motor.Yaw, Mathf.Atan2(flat.x, flat.z) * Mathf.Rad2Deg, 280f * Time.deltaTime);

                if (Time.time >= _readyAt && _marker != null && distance < 30f)
                {
                    var facing = Quaternion.Euler(0f, _motor.Yaw, 0f) * Vector3.forward;
                    if (Vector3.Angle(facing, flat.sqrMagnitude > 0.01f ? flat.normalized : facing) < 18f)
                    {
                        _marker.TryFire(new Ray(_motor.EyePosition, toEyes.sqrMagnitude > 0.01f ? toEyes.normalized : facing), _fighter);
                    }
                }
            }
            else
            {
                if (Time.time >= _repathAt || FlatDistance(transform.position, _destination) < 1.6f)
                {
                    PickPost();
                }

                move = DesiredMove(_destination);
                sprint = FlatDistance(transform.position, _destination) > 8f;
                if (move.sqrMagnitude > 0.01f)
                {
                    yaw = Mathf.Atan2(move.x, move.z) * Mathf.Rad2Deg;
                }
            }

            move += Separation();
            if (move.sqrMagnitude > 1f)
            {
                move.Normalize();
            }

            var planar = _motor.Velocity;
            planar.y = 0f;
            if (move.sqrMagnitude > 0.2f && planar.sqrMagnitude < 0.04f)
            {
                _stuck += Time.deltaTime;
                if (_stuck > 1.2f)
                {
                    _stuck = 0f;
                    jump = true;
                    PickPost();
                }
            }
            else
            {
                _stuck = 0f;
            }

            _motor.SetCommand(new CharacterCommand
            {
                Move = move,
                Yaw = yaw,
                Sprint = sprint && move.sqrMagnitude > 0.1f,
                Jump = jump,
            });
        }

        Fighter FindTarget()
        {
            Fighter best = null;
            var bestDistance = float.MaxValue;
            var fighters = _director.Fighters;
            for (var i = 0; i < fighters.Count; i++)
            {
                var other = fighters[i];
                if (other == null || other == _fighter || !other.IsAlive || other.IsInvulnerable || other.Team == _fighter.Team)
                {
                    continue;
                }

                var to = other.Motor.transform.position + Vector3.up - _motor.EyePosition;
                var distance = to.magnitude;
                if (distance > 32f || distance < 0.1f || !CanSee(other, to, distance))
                {
                    continue;
                }

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = other;
                }
            }

            return best;
        }

        bool CanSee(Fighter target, Vector3 to, float distance)
        {
            var direction = to / distance;
            var origin = _motor.EyePosition + direction * 0.45f;
            if (!Physics.Raycast(origin, direction, out var hit, distance, ~0, QueryTriggerInteraction.Ignore))
            {
                return true;
            }

            return hit.collider.GetComponentInParent<Fighter>() == target;
        }

        Vector3 DesiredMove(Vector3 destination)
        {
            var target = destination;
            if (NavMesh.SamplePosition(transform.position, out var from, 2f, NavMesh.AllAreas))
            {
                if (_path == null)
                {
                    _path = new NavMeshPath();
                }

                if (NavMesh.CalculatePath(from.position, destination, NavMesh.AllAreas, _path)
                    && _path.status != NavMeshPathStatus.PathInvalid
                    && _path.corners.Length > 1)
                {
                    var corner = 1;
                    if (_path.corners.Length > 2 && FlatDistance(transform.position, _path.corners[1]) < 0.55f)
                    {
                        corner = 2;
                    }

                    target = _path.corners[corner];
                }
            }

            var delta = target - transform.position;
            delta.y = 0f;
            return delta.sqrMagnitude < 0.0001f ? Vector3.zero : delta.normalized;
        }

        Vector3 Separation()
        {
            var push = Vector3.zero;
            var fighters = _director.Fighters;
            for (var i = 0; i < fighters.Count; i++)
            {
                var other = fighters[i];
                if (other == null || other == _fighter || !other.IsAlive)
                {
                    continue;
                }

                var away = transform.position - other.transform.position;
                away.y = 0f;
                if (away.sqrMagnitude < 1.6f * 1.6f && away.sqrMagnitude > 0.0001f)
                {
                    push += away.normalized * 0.75f;
                }
            }

            return push;
        }

        void PickPost()
        {
            _repathAt = Time.time + Random.Range(3.5f, 6f);
            if (_posts == null || _posts.Count == 0)
            {
                var scatter = Random.insideUnitSphere * 8f;
                scatter.y = 0f;
                _destination = transform.position + scatter;
                return;
            }

            for (var attempt = 0; attempt < 6; attempt++)
            {
                var post = _posts[Random.Range(0, _posts.Count)];
                var forward = _fighter.Team == TeamId.Red ? post.x > -4f : post.x < 4f;
                if (forward || attempt == 5)
                {
                    _destination = post;
                    return;
                }
            }
        }

        static float FlatDistance(Vector3 a, Vector3 b)
        {
            a.y = 0f;
            b.y = 0f;
            return Vector3.Distance(a, b);
        }
    }
}
