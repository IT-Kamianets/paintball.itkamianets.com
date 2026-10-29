using ITKamianets.Paintball.Core;
using ITKamianets.Paintball.Game;
using UnityEditor;
using UnityEngine;

namespace ITKamianets.Paintball.Editor
{
    public static class PlayerPrefabSetup
    {
        const string Path = "Assets/Prefabs/Player.prefab";

        [MenuItem("KP Paintball/Create Player Prefab")]
        public static void Create()
        {
            var fighter = FighterFactory.Create(new FighterSpawnRequest
            {
                Team = TeamId.Red,
                IsPlayer = true,
                Position = Vector3.zero,
                Yaw = 90f,
                Name = "Player",
            });

            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }

            PrefabUtility.SaveAsPrefabAsset(fighter.gameObject, Path);
            Object.DestroyImmediate(fighter.gameObject);
            AssetDatabase.SaveAssets();
            Debug.Log("Saved " + Path);
        }
    }
}
