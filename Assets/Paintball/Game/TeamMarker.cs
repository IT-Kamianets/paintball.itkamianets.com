using UnityEngine;

namespace ITKamianets.Paintball.Game
{
    /// <summary>A team-colored quad above a fighter, turned toward the camera.</summary>
    public class TeamMarker : MonoBehaviour
    {
        Transform _camera;

        void LateUpdate()
        {
            if (_camera == null && UnityEngine.Camera.main != null)
            {
                _camera = UnityEngine.Camera.main.transform;
            }

            if (_camera == null)
            {
                return;
            }

            var toCamera = _camera.position - transform.position;
            if (toCamera.sqrMagnitude < 0.001f)
            {
                return;
            }

            transform.rotation = Quaternion.LookRotation(toCamera);
        }
    }
}
