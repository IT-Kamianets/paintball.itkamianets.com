using ITKamianets.Engine.Character;
using ITKamianets.Engine.CharacterView;
using ITKamianets.Paintball.Core;
using ITKamianets.Paintball.Weapons;
using UnityEngine;

namespace ITKamianets.Paintball.Game
{
    public static class FighterFactory
    {
        public static Fighter Create(FighterSpawnRequest request)
        {
            var go = new GameObject(string.IsNullOrEmpty(request.Name) ? "Fighter" : request.Name);
            go.transform.SetPositionAndRotation(request.Position, Quaternion.Euler(0f, request.Yaw, 0f));

            var motor = go.AddComponent<CharacterMotor>();
            if (!request.IsPlayer)
            {
                motor.Settings.WalkSpeed = 3.5f;
                motor.Settings.SprintSpeed = 5.3f;
            }

            BuildBody(go.transform, request.Team, request.IsPlayer);
            var fighter = go.AddComponent<Fighter>();
            fighter.Initialize(request.Team, request.IsPlayer, go.name, motor);
            go.AddComponent<Marker>();

            if (request.IsPlayer)
            {
                go.AddComponent<CharacterViewControls>();
            }
            else
            {
                go.AddComponent<PaintballBot>().SetPosts(request.Posts);
            }

            motor.Teleport(request.Position, request.Yaw);
            return fighter;
        }

        static void BuildBody(Transform root, TeamId team, bool player)
        {
            var color = TeamColors.Of(team);
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(root, false);
            body.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            body.transform.localScale = new Vector3(0.7f, 0.9f, 0.7f);
            body.GetComponent<Renderer>().sharedMaterial = FlatMaterial.Get(color);
            RemoveCollider(body);

            var nose = GameObject.CreatePrimitive(PrimitiveType.Cube);
            nose.name = "Nose";
            nose.transform.SetParent(root, false);
            nose.transform.localPosition = new Vector3(0f, 1.48f, 0.42f);
            nose.transform.localScale = new Vector3(0.16f, 0.12f, 0.28f);
            nose.GetComponent<Renderer>().sharedMaterial = FlatMaterial.Get(Color.Lerp(color, Color.white, 0.55f));
            RemoveCollider(nose);

            if (player)
            {
                return;
            }

            var marker = GameObject.CreatePrimitive(PrimitiveType.Quad);
            marker.name = "Team marker";
            marker.transform.SetParent(root, false);
            marker.transform.localPosition = new Vector3(0f, 2.05f, 0f);
            marker.transform.localScale = Vector3.one * 0.28f;
            marker.GetComponent<Renderer>().sharedMaterial = FlatMaterial.Get(color, true);
            RemoveCollider(marker);
            marker.AddComponent<TeamMarker>();
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
