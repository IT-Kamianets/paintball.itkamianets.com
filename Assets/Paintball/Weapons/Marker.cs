using ITKamianets.Engine.CharacterView;
using ITKamianets.Engine.Input;
using ITKamianets.Paintball.Core;
using UnityEngine;

namespace ITKamianets.Paintball.Weapons
{
    /// <summary>
    /// Semi-automatic paintball marker. The local player fires with Primary from
    /// <see cref="EngineInput"/> along the camera aim; bots call <see cref="TryFire"/>.
    /// </summary>
    [DefaultExecutionOrder(-45)]
    public class Marker : MonoBehaviour, IMarker
    {
        public const int HopperSize = 150;
        const float PlayerInterval = 0.1f;
        const float BotInterval = 0.52f;
        const float MuzzleSpeed = 48f;
        const float ReloadSeconds = 1.5f;

        Fighter _fighter;
        float _nextShot;
        float _reloadDone;
        float _armedAt;

        public int Balls { get; private set; } = HopperSize;
        public int Capacity => HopperSize;
        public bool IsReloading { get; private set; }

        void Awake()
        {
            _fighter = GetComponent<Fighter>();
        }

        void Update()
        {
            if (IsReloading && Time.time >= _reloadDone)
            {
                Balls = HopperSize;
                IsReloading = false;
            }

            if (_fighter == null || !_fighter.IsPlayer || !_fighter.IsAlive)
            {
                return;
            }

            var input = EngineInput.Default;
            var controls = GetComponent<CharacterViewControls>();
            if (!input.PlayerEnabled)
            {
                _armedAt = Time.time + 0.25f;
                if (controls != null)
                {
                    controls.FaceCamera = false;
                }

                return;
            }

            if (controls != null)
            {
                controls.FaceCamera = input.Player.Primary.IsPressed() || controls.IsAiming;
            }

            if (Time.time < _armedAt || !input.Player.Primary.IsPressed())
            {
                return;
            }

            var camera = UnityEngine.Camera.main;
            if (camera == null)
            {
                return;
            }

            var view = camera.GetComponent<CharacterViewCamera>();
            var aim = view != null ? view.AimRay : new Ray(camera.transform.position, camera.transform.forward);
            TryFire(aim, _fighter);
        }

        public bool TryFire(Ray aim, Fighter shooter)
        {
            if (shooter == null || !shooter.IsAlive || IsReloading || Time.time < _nextShot)
            {
                return false;
            }

            if (Balls <= 0)
            {
                IsReloading = true;
                _reloadDone = Time.time + ReloadSeconds;
                return false;
            }

            var forward = aim.direction.sqrMagnitude > 0.0001f ? aim.direction.normalized : Vector3.forward;
            var direction = Spread(forward, shooter.IsPlayer);
            Balls--;
            _nextShot = Time.time + (shooter.IsPlayer ? PlayerInterval : BotInterval);
            PaintProjectile.Spawn(aim.origin + forward * 0.7f, direction * MuzzleSpeed, TeamColors.Of(shooter.Team), shooter);
            ShotAudio.PlayShot(aim.origin);
            if (Balls <= 0)
            {
                IsReloading = true;
                _reloadDone = Time.time + ReloadSeconds;
            }

            return true;
        }

        static Vector3 Spread(Vector3 forward, bool player)
        {
            var degrees = player ? 0.35f : Random.Range(3.2f, 5.6f);
            var side = Vector3.Cross(forward, Vector3.up);
            if (side.sqrMagnitude < 0.0001f)
            {
                side = Vector3.right;
            }

            var shot = Quaternion.AngleAxis(Random.Range(-degrees, degrees), side.normalized) * forward;
            return Quaternion.AngleAxis(Random.Range(-degrees, degrees), Vector3.up) * shot;
        }
    }
}
