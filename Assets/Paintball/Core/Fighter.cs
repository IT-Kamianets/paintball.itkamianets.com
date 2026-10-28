using ITKamianets.Engine.Character;
using UnityEngine;

namespace ITKamianets.Paintball.Core
{
    /// <summary>One paintball fighter: the local player or a bot. The motor moves them; this tracks team and life.</summary>
    public class Fighter : MonoBehaviour
    {
        public TeamId Team { get; private set; }
        public bool IsPlayer { get; private set; }
        public string DisplayName { get; private set; }
        public CharacterMotor Motor { get; private set; }
        public bool IsAlive { get; private set; }
        public float RespawnAt { get; private set; }
        public float InvulnerableUntil { get; internal set; }

        public bool IsInvulnerable => IsAlive && Time.time < InvulnerableUntil;

        public void Initialize(TeamId team, bool isPlayer, string displayName, CharacterMotor motor)
        {
            Team = team;
            IsPlayer = isPlayer;
            DisplayName = displayName;
            Motor = motor;
            IsAlive = true;
            gameObject.name = displayName;
        }

        public void KnockOut(float respawnAt)
        {
            IsAlive = false;
            RespawnAt = respawnAt;
            Motor.SimulateInUpdate = false;
            Motor.SetCommand(new CharacterCommand { Yaw = Motor.Yaw });
            Motor.GetComponent<CharacterController>().enabled = false;
            SetRenderers(false);
        }

        public void Revive(Vector3 position, float yaw, float invulnerableUntil)
        {
            IsAlive = true;
            InvulnerableUntil = invulnerableUntil;
            Motor.SimulateInUpdate = true;
            Motor.Teleport(position, yaw);
            SetRenderers(true);
        }

        void SetRenderers(bool enabled)
        {
            var renderers = GetComponentsInChildren<Renderer>(true);
            for (var i = 0; i < renderers.Length; i++)
            {
                renderers[i].enabled = enabled;
            }
        }
    }
}
