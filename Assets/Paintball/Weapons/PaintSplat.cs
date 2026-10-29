using System.Collections.Generic;
using ITKamianets.Paintball.Core;
using UnityEngine;

namespace ITKamianets.Paintball.Weapons
{
    /// <summary>A flat paint mark. Old marks are removed so a match cannot fill the scene.</summary>
    public class PaintSplat : MonoBehaviour
    {
        const int MaxSplats = 120;

        static readonly List<PaintSplat> Alive = new List<PaintSplat>();

        float _age;
        Vector3 _fullScale;

        public static void Spawn(Vector3 point, Vector3 normal, Color color, float size)
        {
            if (normal.sqrMagnitude < 0.001f)
            {
                normal = Vector3.up;
            }

            Prune();
            while (Alive.Count >= MaxSplats)
            {
                var oldest = Alive[0];
                Alive.RemoveAt(0);
                if (oldest != null)
                {
                    Destroy(oldest.gameObject);
                }
            }

            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = "Paint";
            var collider = go.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }

            color = Color.Lerp(color, Color.black, Random.Range(0f, 0.18f));
            go.GetComponent<Renderer>().sharedMaterial = FlatMaterial.Get(color, true);
            go.transform.position = point + normal.normalized * 0.02f;
            go.transform.rotation = Quaternion.FromToRotation(Vector3.up, normal);
            var splat = go.AddComponent<PaintSplat>();
            splat._fullScale = new Vector3(size, 0.008f, size);
            go.transform.localScale = splat._fullScale;
            Alive.Add(splat);
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
            _age += Time.deltaTime;
            if (_age > 12f)
            {
                var shrink = Mathf.Clamp01((14f - _age) / 2f);
                transform.localScale = _fullScale * Mathf.Max(0.05f, shrink);
            }

            if (_age >= 14f)
            {
                Destroy(gameObject);
            }
        }

        static void Prune()
        {
            for (var i = Alive.Count - 1; i >= 0; i--)
            {
                if (Alive[i] == null)
                {
                    Alive.RemoveAt(i);
                }
            }
        }
    }
}
