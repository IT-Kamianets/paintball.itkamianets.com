using System.Collections.Generic;
using ITKamianets.Engine.Core;
using ITKamianets.Paintball.Core;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace ITKamianets.Paintball.Maps
{
    /// <summary>
    /// One paintball field: stone walls like the Kamianets fortress, inflatable bunkers,
    /// a ramp, steps and a crouch tunnel. Built in meters when the match boots.
    /// </summary>
    public class FortressArena : MonoBehaviour
    {
        static readonly Color Turf = new Color(0.15f, 0.38f, 0.18f);
        static readonly Color Line = new Color(0.86f, 0.87f, 0.78f);
        static readonly Color Stone = new Color(0.52f, 0.49f, 0.44f);
        static readonly Color StoneDark = new Color(0.32f, 0.30f, 0.27f);
        static readonly Color Wood = new Color(0.45f, 0.29f, 0.15f);
        static readonly Color Inflatable = new Color(0.93f, 0.75f, 0.12f);
        static readonly Color Barrel = new Color(0.14f, 0.14f, 0.15f);
        static readonly Color Net = new Color(0.20f, 0.24f, 0.30f);

        readonly List<TeamSpawn> _spawns = new List<TeamSpawn>();
        readonly List<Vector3> _posts = new List<Vector3>();

        public IReadOnlyList<TeamSpawn> Spawns => _spawns;
        public IReadOnlyList<Vector3> Posts => _posts;

        public static FortressArena ExistingOrBuild()
        {
            var existing = Object.FindAnyObjectByType<FortressArena>();
            if (existing != null)
            {
                existing.EnsureNavMesh();
                return existing;
            }

            return Build();
        }

        public static FortressArena Build()
        {
            var root = new GameObject("Fortress Arena");
            var arena = root.AddComponent<FortressArena>();
            arena.CreateGeometry();
            arena.Bake();
            arena.SnapPosts();
            return arena;
        }

        public void EnsureNavMesh()
        {
            var mesh = NavMesh.CalculateTriangulation();
            if (mesh.vertices == null || mesh.vertices.Length == 0)
            {
                Bake();
                SnapPosts();
            }
        }

        void CreateGeometry()
        {
            const float halfX = 22f;
            const float halfZ = 14f;
            const float wallH = 3.4f;
            const float wallT = 0.6f;

            Box("Turf", new Vector3(0f, -0.1f, 0f), new Vector3(halfX * 2f + 6f, 0.2f, halfZ * 2f + 6f), Turf);
            Decal("Mid line north", new Vector3(0f, 0.03f, 9f), new Vector3(0.14f, 0.02f, 8f), Line);
            Decal("Mid line south", new Vector3(0f, 0.03f, -9f), new Vector3(0.14f, 0.02f, 8f), Line);

            Box("Wall West", new Vector3(-(halfX + wallT * 0.5f), wallH * 0.5f, 0f), new Vector3(wallT, wallH, halfZ * 2f + wallT), Net);
            Box("Wall East", new Vector3(halfX + wallT * 0.5f, wallH * 0.5f, 0f), new Vector3(wallT, wallH, halfZ * 2f + wallT), Net);
            Box("Wall North", new Vector3(0f, wallH * 0.5f, halfZ + wallT * 0.5f), new Vector3(halfX * 2f + wallT, wallH, wallT), Net);
            Box("Wall South", new Vector3(0f, wallH * 0.5f, -(halfZ + wallT * 0.5f)), new Vector3(halfX * 2f + wallT, wallH, wallT), Net);
            Merlons(halfX, halfZ, wallH);

            Box("Tower", new Vector3(0f, 5f, 20.5f), new Vector3(5.2f, 10f, 5.2f), Stone);
            Box("Tower cap", new Vector3(0f, 11f, 20.5f), new Vector3(3.4f, 2.2f, 3.4f), StoneDark);
            Box("Tower south", new Vector3(-16f, 3.5f, -20f), new Vector3(4.2f, 7f, 4.2f), StoneDark);

            BuildKeep();
            BuildSpawnCover(TeamId.Red, -1f);
            BuildSpawnCover(TeamId.Blue, 1f);
            BuildLanes();
            BuildNorth();
            BuildProps();

            AddPost(-14f, 0f);
            AddPost(-14f, 6.5f);
            AddPost(-14f, -6.5f);
            AddPost(-8f, 9f);
            AddPost(-8f, -9f);
            AddPost(-2.2f, 0f);
            AddPost(2.2f, 0f);
            AddPost(0f, 2.2f);
            AddPost(0f, -2.2f);
            AddPost(0f, 7.2f);
            AddPost(0f, -7f);
            AddPost(8f, 9f);
            AddPost(8f, -9f);
            AddPost(14f, 0f);
            AddPost(14f, 6.5f);
            AddPost(14f, -6.5f);
            AddPost(-7f, 11.2f, 1.2f);
            AddPost(8f, 10.4f, 1.2f);
        }

        void BuildKeep()
        {
            const float h = 2.35f;
            Box("Keep north", new Vector3(0f, h * 0.5f, 4.2f), new Vector3(8.8f, h, 0.4f), Stone);
            Box("Keep south", new Vector3(0f, h * 0.5f, -4.2f), new Vector3(8.8f, h, 0.4f), Stone);
            DoorWall("Keep west", -4.2f, true);
            DoorWall("Keep east", 4.2f, true);
            Cylinder("Keep pillar", new Vector3(0f, 1.6f, 0f), 1.5f, 3.2f, StoneDark);
        }

        void DoorWall(string name, float x, bool alongZ)
        {
            const float h = 2.35f;
            if (!alongZ)
            {
                return;
            }

            Box(name + " a", new Vector3(x, h * 0.5f, -2.5f), new Vector3(0.4f, h, 3f), Stone);
            Box(name + " b", new Vector3(x, h * 0.5f, 2.5f), new Vector3(0.4f, h, 3f), Stone);
        }

        void BuildSpawnCover(TeamId team, float sign)
        {
            var x = 19.2f * sign;
            var coverX = 17.2f * sign;
            var yaw = team == TeamId.Red ? 90f : -90f;
            var color = TeamColors.Of(team);
            float[] lanes = { -5f, 0f, 5f };
            for (var i = 0; i < lanes.Length; i++)
            {
                var z = lanes[i];
                Box(team + " bunker", new Vector3(coverX, 1.05f, z), new Vector3(0.4f, 2.1f, 1.8f), Stone);
                Box(team + " lip", new Vector3(coverX, 2.2f, z), new Vector3(0.55f, 0.22f, 1.9f), color);
                var spawnGo = new GameObject(team + " spawn " + (i + 1));
                spawnGo.transform.SetParent(transform, false);
                spawnGo.transform.position = new Vector3(x, 0f, z);
                spawnGo.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
                var spawn = spawnGo.AddComponent<TeamSpawn>();
                spawn.Team = team;
                _spawns.Add(spawn);
            }

            Decal(team + " mat", new Vector3(x, 0.03f, 0f), new Vector3(3.2f, 0.02f, 12f), color);
        }

        void BuildLanes()
        {
            Box("Low wall west", new Vector3(-12f, 0.55f, 1f), new Vector3(0.35f, 1.1f, 4.6f), Wood);
            Box("Low wall east", new Vector3(12f, 0.55f, -1f), new Vector3(0.35f, 1.1f, 4.6f), Wood);
            Box("Low wall north", new Vector3(-2f, 0.5f, 8.2f), new Vector3(4.2f, 1f, 0.35f), Wood);
            Cylinder("Bunker NW", new Vector3(-10f, 0.72f, 7f), 1.7f, 1.45f, Inflatable);
            Cylinder("Bunker SW", new Vector3(-10f, 0.72f, -7f), 1.7f, 1.45f, Inflatable);
            Cylinder("Bunker NE", new Vector3(10f, 0.72f, 7f), 1.7f, 1.45f, Inflatable);
            Cylinder("Bunker SE", new Vector3(10f, 0.72f, -7f), 1.7f, 1.45f, Inflatable);
            Box("Crate", new Vector3(6.2f, 0.6f, 2.2f), new Vector3(1.2f, 1.2f, 1.2f), Wood);
            Box("Crate", new Vector3(-6.2f, 0.6f, -2.4f), new Vector3(1.15f, 1.15f, 1.15f), Wood);
            Box("Crate", new Vector3(13.5f, 0.55f, 9f), new Vector3(1.3f, 1.1f, 1.1f), Wood);

            Box("Tunnel left", new Vector3(-1.15f, 0.75f, -11f), new Vector3(0.3f, 1.5f, 3.2f), Wood);
            Box("Tunnel right", new Vector3(1.15f, 0.75f, -11f), new Vector3(0.3f, 1.5f, 3.2f), Wood);
            Box("Tunnel roof", new Vector3(0f, 1.48f, -11f), new Vector3(2.6f, 0.2f, 3.2f), Wood);
        }

        void BuildNorth()
        {
            Box("Platform base", new Vector3(-7f, 0.45f, 11.2f), new Vector3(4f, 0.9f, 2.6f), Stone);
            Box("Platform", new Vector3(-7f, 1.02f, 11.2f), new Vector3(4.6f, 0.16f, 3.2f), Wood);
            var ramp = Box("Ramp", new Vector3(-7f, 0.55f, 7.6f), new Vector3(2.4f, 0.18f, 4.3f), new Color(0.55f, 0.42f, 0.24f));
            ramp.transform.localRotation = Quaternion.Euler(-15f, 0f, 0f);

            for (var i = 0; i < 4; i++)
            {
                var top = 0.28f * (i + 1);
                Box("Step " + (i + 1), new Vector3(8f, top * 0.5f, 6.3f + i * 0.9f), new Vector3(2.4f, top, 0.85f), Stone);
            }

            Box("Step deck", new Vector3(8f, 0.56f, 10.3f), new Vector3(2.6f, 1.12f, 2.2f), Stone);

            var steep = Box("Steep ramp", new Vector3(16.2f, 0.7f, -8f), new Vector3(1.8f, 0.18f, 3.1f), new Color(0.65f, 0.28f, 0.16f));
            steep.transform.localRotation = Quaternion.Euler(-52f, 25f, 0f);
        }

        void BuildProps()
        {
            Cylinder("Barrel", new Vector3(5.5f, 0.5f, -10.5f), 0.7f, 1f, Barrel);
            Cylinder("Barrel", new Vector3(6.2f, 0.5f, -10.2f), 0.62f, 1f, Barrel);
            Cylinder("Barrel", new Vector3(-15.5f, 0.48f, 8.5f), 0.66f, 0.95f, Barrel);
            Cylinder("Barrel", new Vector3(-15f, 0.48f, 9.1f), 0.58f, 0.95f, Barrel);
        }

        void Merlons(float halfX, float halfZ, float wallH)
        {
            var y = wallH + 0.35f;
            for (var x = -halfX + 2f; x <= halfX - 2f; x += 4f)
            {
                Box("Merlon", new Vector3(x, y, halfZ + 0.3f), new Vector3(0.7f, 0.7f, 0.7f), StoneDark);
                Box("Merlon", new Vector3(x, y, -(halfZ + 0.3f)), new Vector3(0.7f, 0.7f, 0.7f), StoneDark);
            }

            for (var z = -halfZ + 2f; z <= halfZ - 2f; z += 4f)
            {
                Box("Merlon", new Vector3(-(halfX + 0.3f), y, z), new Vector3(0.7f, 0.7f, 0.7f), StoneDark);
                Box("Merlon", new Vector3(halfX + 0.3f, y, z), new Vector3(0.7f, 0.7f, 0.7f), StoneDark);
            }
        }

        void AddPost(float x, float z, float y = 0.1f)
        {
            _posts.Add(new Vector3(x, y, z));
        }

        GameObject Box(string name, Vector3 center, Vector3 size, Color color)
        {
            return Primitive(PrimitiveType.Cube, name, center, size, color, true);
        }

        void Decal(string name, Vector3 center, Vector3 size, Color color)
        {
            Primitive(PrimitiveType.Cube, name, center, size, color, false);
        }

        GameObject Primitive(PrimitiveType type, string name, Vector3 center, Vector3 size, Color color, bool solid)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.isStatic = true;
            go.transform.SetParent(transform, false);
            go.transform.localPosition = center;
            go.transform.localScale = size;
            go.GetComponent<Renderer>().sharedMaterial = FlatMaterial.Get(color);
            if (!solid)
            {
                var collider = go.GetComponent<Collider>();
                if (collider != null)
                {
                    Object.DestroyImmediate(collider);
                }
            }

            return go;
        }

        void Cylinder(string name, Vector3 center, float diameter, float height, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.isStatic = true;
            go.transform.SetParent(transform, false);
            go.transform.localPosition = center;
            go.transform.localScale = new Vector3(diameter, height * 0.5f, diameter);
            go.GetComponent<Renderer>().sharedMaterial = FlatMaterial.Get(color);
        }

        void Bake()
        {
            var surface = gameObject.GetComponent<NavMeshSurface>();
            if (surface == null)
            {
                surface = gameObject.AddComponent<NavMeshSurface>();
            }

            surface.collectObjects = CollectObjects.Children;
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            surface.layerMask = ~0;
            try
            {
                surface.BuildNavMesh();
                EngineLog.Info("Paintball", "Arena navigation baked.");
            }
            catch (System.Exception exception)
            {
                EngineLog.Warning("Paintball", "Arena navigation failed: " + exception.Message);
            }
        }

        void SnapPosts()
        {
            for (var i = 0; i < _posts.Count; i++)
            {
                if (NavMesh.SamplePosition(_posts[i], out var hit, 3f, NavMesh.AllAreas))
                {
                    _posts[i] = hit.position;
                }
            }
        }
    }
}
