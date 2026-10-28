using ITKamianets.Paintball.Core;
using UnityEngine;

namespace ITKamianets.Paintball.Game
{
    /// <summary>The marker mesh parented to the camera so it stays visible in first person.</summary>
    public class GunView : MonoBehaviour
    {
        Renderer _tank;

        public static GunView Create(Transform camera)
        {
            var root = new GameObject("Marker view");
            root.transform.SetParent(camera, false);
            var view = root.AddComponent<GunView>();
            view.Build();
            root.SetActive(false);
            return view;
        }

        public void SetTeam(TeamId team)
        {
            _tank.sharedMaterial = FlatMaterial.Get(TeamColors.Of(team), true);
        }

        void Build()
        {
            Piece(PrimitiveType.Cube, "Body", new Vector3(0.16f, -0.16f, 0.42f), new Vector3(0.07f, 0.08f, 0.28f), Quaternion.identity, new Color(0.12f, 0.12f, 0.13f));
            Piece(PrimitiveType.Cylinder, "Barrel", new Vector3(0.16f, -0.14f, 0.62f), new Vector3(0.035f, 0.1f, 0.035f), Quaternion.Euler(90f, 0f, 0f), new Color(0.08f, 0.08f, 0.09f));
            _tank = Piece(PrimitiveType.Sphere, "Tank", new Vector3(0.16f, -0.2f, 0.32f), Vector3.one * 0.09f, Quaternion.identity, TeamColors.Red).GetComponent<Renderer>();
        }

        GameObject Piece(PrimitiveType type, string name, Vector3 position, Vector3 scale, Quaternion rotation, Color color)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(transform, false);
            go.transform.localPosition = position;
            go.transform.localRotation = rotation;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = FlatMaterial.Get(color, true);
            var collider = go.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }

            return go;
        }
    }
}
