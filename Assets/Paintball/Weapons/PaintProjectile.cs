using System.Collections.Generic;
using ITKamianets.Engine.Core;
using ITKamianets.Paintball.Core;
using UnityEngine;

namespace ITKamianets.Paintball.Weapons
{
    /// <summary>A paint ball with drop. One hit on the other team eliminates that fighter.</summary>
    public class PaintProjectile : MonoBehaviour
    {
        const float Gravity = 9.5f;
        const float Life = 2.2f;
        const float Radius = 0.07f;

        static readonly List<PaintProjectile> Alive = new List<PaintProjectile>();
        static readonly RaycastHit[] Hits = new RaycastHit[24];

        Fighter _shooter;
        Vector3 _velocity;
        float _age;
        Color _color;

        public static void Spawn(Vector3 position, Vector3 velocity, Color color, Fighter shooter)
        {
            var ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ball.name = "Paint ball";
            RemoveCollider(ball);
            ball.transform.position = position;
            ball.transform.localScale = Vector3.one * 0.12f;
            ball.GetComponent<Renderer>().sharedMaterial = FlatMaterial.Get(color, true);
            var projectile = ball.AddComponent<PaintProjectile>();
            projectile._shooter = shooter;
            projectile._velocity = velocity;
            projectile._color = color;
            Alive.Add(projectile);
        }

        public static void Clear()
        {
            for (var i = Alive.Count - 1; i >= 0; i--)
            {
                if (Alive[i] != null)
                {
                    Destroy(Alive[i].gameObject);
                }
            }

            Alive.Clear();
        }

        void OnDestroy()
        {
            Alive.Remove(this);
        }

        void Update()
        {
            var delta = _velocity * Time.deltaTime;
            var distance = delta.magnitude;
            if (distance > 0.0001f && Impact(delta / distance, distance))
            {
                return;
            }

            transform.position += delta;
            _velocity += Vector3.down * Gravity * Time.deltaTime;
            _age += Time.deltaTime;
            if (_age >= Life)
            {
                PaintSplat.Spawn(transform.position, Vector3.up, _color, 0.18f);
                Destroy(gameObject);
            }
        }

        bool Impact(Vector3 direction, float distance)
        {
            var count = Physics.SphereCastNonAlloc(transform.position, Radius, direction, Hits, distance, ~0, QueryTriggerInteraction.Ignore);
            var best = float.MaxValue;
            var found = -1;
            for (var i = 0; i < count; i++)
            {
                if (Hits[i].distance <= 0.001f)
                {
                    continue;
                }

                var fighter = Hits[i].collider.GetComponentInParent<Fighter>();
                if (fighter != null && fighter == _shooter)
                {
                    continue;
                }

                if (Hits[i].distance < best)
                {
                    best = Hits[i].distance;
                    found = i;
                }
            }

            if (found < 0)
            {
                return false;
            }

            var hit = Hits[found];
            PaintSplat.Spawn(hit.point, hit.normal, _color, 0.4f);
            ShotAudio.PlaySplat(hit.point);
            var victim = hit.collider.GetComponentInParent<Fighter>();
            if (victim != null)
            {
                for (var i = 0; i < 5; i++)
                {
                    var normal = Vector3.Slerp(hit.normal, Random.onUnitSphere, 0.65f).normalized;
                    PaintSplat.Spawn(hit.point, normal, _color, Random.Range(0.12f, 0.22f));
                }

                if (EngineServices.TryGet<MatchDirector>(out var match))
                {
                    match.NotifyHit(_shooter, victim);
                }
            }

            Destroy(gameObject);
            return true;
        }

        static void RemoveCollider(GameObject go)
        {
            var collider = go.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }
        }
    }
}
