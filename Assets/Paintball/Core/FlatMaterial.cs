using System.Collections.Generic;
using UnityEngine;

namespace ITKamianets.Paintball.Core
{
    /// <summary>Shared unlit or lit colors for placeholder arena and fighter meshes.</summary>
    public static class FlatMaterial
    {
        static readonly Dictionary<int, Material> Cache = new Dictionary<int, Material>();

        public static Material Get(Color color, bool unlit = false)
        {
            var key = (unlit ? 1 << 24 : 0)
                ^ Mathf.RoundToInt(color.r * 255f)
                ^ (Mathf.RoundToInt(color.g * 255f) << 8)
                ^ (Mathf.RoundToInt(color.b * 255f) << 16);

            if (Cache.TryGetValue(key, out var existing) && existing != null)
            {
                return existing;
            }

            var shader = Shader.Find(unlit ? "Universal Render Pipeline/Unlit" : "Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Sprites/Default");
            }

            var material = new Material(shader);
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }

            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", 0.16f);
            }

            material.enableInstancing = true;
            Cache[key] = material;
            return material;
        }
    }
}
