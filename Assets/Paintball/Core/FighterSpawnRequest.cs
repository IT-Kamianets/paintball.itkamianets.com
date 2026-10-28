using System.Collections.Generic;
using UnityEngine;

namespace ITKamianets.Paintball.Core
{
    public struct FighterSpawnRequest
    {
        public TeamId Team;
        public bool IsPlayer;
        public Vector3 Position;
        public float Yaw;
        public string Name;
        public IReadOnlyList<Vector3> Posts;
    }
}
