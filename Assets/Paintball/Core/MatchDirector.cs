using System;
using System.Collections.Generic;
using ITKamianets.Engine.Core;
using UnityEngine;

namespace ITKamianets.Paintball.Core
{
    /// <summary>One 3v3 match: spawns, eliminations, team score and respawn.</summary>
    public class MatchDirector : MonoBehaviour
    {
        readonly List<Fighter> _fighters = new List<Fighter>();
        readonly List<TeamSpawn> _spawns = new List<TeamSpawn>();
        IReadOnlyList<Vector3> _posts = Array.Empty<Vector3>();

        public MatchScore Score { get; private set; } = new MatchScore(MatchRules.ScoreLimit);
        public bool IsRunning { get; private set; }
        public Fighter Player { get; private set; }
        public float StartedAt { get; private set; }
        public IReadOnlyList<Fighter> Fighters => _fighters;
        public IReadOnlyList<Vector3> Posts => _posts;

        public event Action<Fighter> Respawned;

        void Awake()
        {
            EngineServices.Set(this);
        }

        void OnDestroy()
        {
            if (EngineServices.TryGet<MatchDirector>(out var current) && current == this)
            {
                EngineServices.Remove<MatchDirector>();
            }
        }

        public void Begin(TeamId playerTeam, IReadOnlyList<TeamSpawn> spawns, IReadOnlyList<Vector3> posts, Func<FighterSpawnRequest, Fighter> create)
        {
            Clear();
            _spawns.Clear();
            if (spawns != null)
            {
                _spawns.AddRange(spawns);
            }

            _posts = posts ?? Array.Empty<Vector3>();
            var enemy = playerTeam == TeamId.Red ? TeamId.Blue : TeamId.Red;
            Player = Spawn(create, playerTeam, true, 0, "Гравець");
            Spawn(create, playerTeam, false, 1, "Напарник 1");
            Spawn(create, playerTeam, false, 2, "Напарник 2");
            Spawn(create, enemy, false, 0, "Суперник 1");
            Spawn(create, enemy, false, 1, "Суперник 2");
            Spawn(create, enemy, false, 2, "Суперник 3");
            IsRunning = true;
            StartedAt = Time.time;
            EngineLog.Info("Paintball", "Match started. Player team: " + playerTeam);
        }

        public void Clear()
        {
            IsRunning = false;
            for (var i = _fighters.Count - 1; i >= 0; i--)
            {
                if (_fighters[i] != null)
                {
                    _fighters[i].gameObject.SetActive(false);
                    Destroy(_fighters[i].gameObject);
                }
            }

            _fighters.Clear();
            Player = null;
            Score = new MatchScore(MatchRules.ScoreLimit);
        }

        public void NotifyHit(Fighter shooter, Fighter victim)
        {
            if (!IsRunning || shooter == null || victim == null || !shooter.IsAlive || !victim.IsAlive)
            {
                return;
            }

            if (shooter.Team == victim.Team || victim.IsInvulnerable)
            {
                return;
            }

            victim.KnockOut(Time.time + MatchRules.RespawnSeconds);
            if (!Score.TryScore(shooter.Team, victim.Team))
            {
                return;
            }

            EngineEvents.Publish(new PaintballElimination
            {
                KillerTeam = shooter.Team,
                VictimTeam = victim.Team,
                KillerIsPlayer = shooter.IsPlayer,
                VictimIsPlayer = victim.IsPlayer,
            });
            EngineLog.Info("Paintball", shooter.DisplayName + " eliminated " + victim.DisplayName + " (" + Score.Red + ":" + Score.Blue + ")");

            if (!Score.Winner.HasValue)
            {
                return;
            }

            IsRunning = false;
            EngineEvents.Publish(new PaintballMatchEnded { Winner = Score.Winner.Value });
            EngineLog.Info("Paintball", "Match won by " + Score.Winner.Value);
        }

        void Update()
        {
            if (!IsRunning)
            {
                return;
            }

            for (var i = 0; i < _fighters.Count; i++)
            {
                var fighter = _fighters[i];
                if (fighter != null && !fighter.IsAlive && Time.time >= fighter.RespawnAt)
                {
                    Respawn(fighter);
                }
            }
        }

        Fighter Spawn(Func<FighterSpawnRequest, Fighter> create, TeamId team, bool isPlayer, int index, string name)
        {
            var placed = Place(team, index);
            var fighter = create(new FighterSpawnRequest
            {
                Team = team,
                IsPlayer = isPlayer,
                Position = placed.Position,
                Yaw = placed.Yaw,
                Name = name,
                Posts = _posts,
            });
            fighter.InvulnerableUntil = Time.time + MatchRules.SpawnProtectionSeconds;
            _fighters.Add(fighter);
            return fighter;
        }

        void Respawn(Fighter fighter)
        {
            var placed = FreePoint(fighter.Team);
            fighter.Revive(placed.Position, placed.Yaw, Time.time + MatchRules.SpawnProtectionSeconds);
            Respawned?.Invoke(fighter);
        }

        struct Placed
        {
            public Vector3 Position;
            public float Yaw;
        }

        Placed Place(TeamId team, int index)
        {
            var options = new List<TeamSpawn>();
            for (var i = 0; i < _spawns.Count; i++)
            {
                if (_spawns[i] != null && _spawns[i].Team == team)
                {
                    options.Add(_spawns[i]);
                }
            }

            if (options.Count == 0)
            {
                return new Placed
                {
                    Position = new Vector3(team == TeamId.Red ? -8f : 8f, 0.05f, 0f),
                    Yaw = team == TeamId.Red ? 90f : -90f,
                };
            }

            var spawn = options[Mathf.Abs(index) % options.Count];
            var position = spawn.transform.position;
            position.y = 0.05f;
            return new Placed { Position = position, Yaw = spawn.transform.eulerAngles.y };
        }

        Placed FreePoint(TeamId team)
        {
            for (var attempt = 0; attempt < 4; attempt++)
            {
                var placed = Place(team, UnityEngine.Random.Range(0, 3));
                if (IsClear(placed.Position))
                {
                    return placed;
                }
            }

            return Place(team, 0);
        }

        bool IsClear(Vector3 position)
        {
            for (var i = 0; i < _fighters.Count; i++)
            {
                var other = _fighters[i];
                if (other == null || !other.IsAlive)
                {
                    continue;
                }

                var delta = other.transform.position - position;
                delta.y = 0f;
                if (delta.sqrMagnitude < 2.2f * 2.2f)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
