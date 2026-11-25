using System;
using System.Collections.Generic;
using System.Linq;
using Freyja.Environment;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Freyja.Editor
{
    public static partial class CoastalGrassAuthoring
    {
        private const string ScenePath = "Assets/_Project/Scenes/Ilha_Principal.unity";
        private const string ArtPath = "Assets/_Project/Art/Grass029/";
        private const float CellSize = 8;
        private const float Spacing = 0.23f;
        private const float PathPadding = 0.28f;
        private static readonly string[] MeshNames = { "CoastalTuftFan", "CoastalTuftTall", "CoastalTuftSimple" };

        [MenuItem("Freyja/Environment/Bake Coastal Grass Pilot")]
        public static void BakePilot() => Bake(false);

        [MenuItem("Freyja/Environment/Bake Island Grass")]
        public static void BakeIsland() => Bake(true);

        private static void Bake(bool wholeIsland)
        {
            Scene scene = SceneManager.GetActiveScene();
            if (scene.path != ScenePath || EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Open Ilha_Principal in Edit Mode.");

            GameObject[] roots = scene.GetRootGameObjects();
            var island = roots.Single(root => root.name == "Island023" && root.activeInHierarchy).transform;
            var terrain = island.GetComponentInChildren<Terrain>();
            var tracks = island.Cast<Transform>().Single(child => child.name.StartsWith("Ruas e caminhos", StringComparison.Ordinal));
            var accesses = island.Cast<Transform>().Single(child => child.name.StartsWith("Acessos", StringComparison.Ordinal));
            var spawn = island.Cast<Transform>().Single(child => child.name == "Spawn023");
            var camera = roots.Select(root => root.GetComponent<Camera>()).Single(value => value && value.CompareTag("MainCamera"));

            var exportedMeshes = AssetDatabase.LoadAllAssetsAtPath(ArtPath + "CoastalGrass.fbx").OfType<Mesh>().ToArray();
            Mesh[] meshes = MeshNames.Select(name => exportedMeshes.Single(mesh => mesh.name == name)).ToArray();
            if (meshes.Any(mesh => mesh.bounds.max.y < 0.15f || mesh.bounds.max.y > 0.45f))
                throw new InvalidOperationException("Import the Coastal Grass meshes in meters with Y up.");

            Shader shader = Shader.Find("Freyja/Coastal Grass");
            if (!shader || !shader.isSupported)
                throw new InvalidOperationException("Coastal Grass shader was not imported or supported.");

            float pathPadding = wholeIsland ? 0.30f : PathPadding;
            var pathMeshes = tracks.GetComponentsInChildren<MeshFilter>().AsEnumerable();
            var accessMarkers = accesses.Cast<Transform>();
            if (wholeIsland)
            {
                pathMeshes = pathMeshes.Concat(accesses.GetComponentsInChildren<MeshFilter>());
                Transform buildings = island.Cast<Transform>()
                    .Single(child => child.name.StartsWith("Construções", StringComparison.Ordinal));
                accessMarkers = buildings.GetComponentsInChildren<Transform>()
                    .Where(child => child.name == "Door023");
                if (!accessMarkers.Any())
                    throw new InvalidOperationException("The island grass bake requires the door markers.");
            }

            var path = pathMeshes.SelectMany(filter => ProjectTriangles(filter, pathPadding)).ToArray();
            var noGrass = accessMarkers.Select(access => Xz(access.position)).ToList();
            Physics.SyncTransforms();
            CoastalGrassLayout.Cell[] cells = wholeIsland
                ? BuildIslandCells(terrain, meshes, path, noGrass)
                : BuildCells(terrain, meshes, path, noGrass, spawn.position);
            if (cells.Length == 0)
                throw new InvalidOperationException("No grass positions passed the terrain and walking masks.");

            string layoutPath = wholeIsland ? IslandArtPath + "IslandGrassLayout.asset" : ArtPath + "CoastalGrassLayout.asset";
            var layout = AssetDatabase.LoadAssetAtPath<CoastalGrassLayout>(layoutPath);
            if (!layout)
            {
                layout = ScriptableObject.CreateInstance<CoastalGrassLayout>();
                AssetDatabase.CreateAsset(layout, layoutPath);
            }

            layout.SetCells(cells);
            EditorUtility.SetDirty(layout);
            AssetDatabase.SaveAssetIfDirty(layout);

            var material = AssetDatabase.LoadAssetAtPath<Material>(ArtPath + "CoastalGrass.mat");
            if (!material)
            {
                material = new Material(shader) { enableInstancing = true };
                AssetDatabase.CreateAsset(material, ArtPath + "CoastalGrass.mat");
            }

            string fieldName = wholeIsland ? "Coastal Grass — Ilha" : "Coastal Grass — Cais";
            var grass = island.GetComponentInChildren<CoastalGrass>(true);
            if (!grass)
            {
                GameObject field = new(fieldName);
                Undo.RegisterCreatedObjectUndo(field, "Add coastal grass");
                field.transform.SetParent(island, false);
                grass = Undo.AddComponent<CoastalGrass>(field);
            }

            if (grass.gameObject.name != fieldName)
            {
                Undo.RecordObject(grass.gameObject, "Rename coastal grass field");
                grass.gameObject.name = fieldName;
            }

            Undo.RecordObject(grass, "Bake coastal grass");
            grass.Configure(camera, layout, material, meshes);
            EditorUtility.SetDirty(grass);
            EditorSceneManager.MarkSceneDirty(scene);
            int count = cells.Sum(cell => cell.Batches.Sum(batch => batch.Instances.Length));
            Debug.Log($"Coastal grass baked: {count} tufts in {cells.Length} cells.", grass);
        }

        private static CoastalGrassLayout.Cell[] BuildCells(Terrain terrain, Mesh[] meshes, PathTriangle[] path,
            List<Vector2> accessPoints, Vector3 spawn)
        {
            TerrainData data = terrain.terrainData;
            Vector3 origin = terrain.GetPosition();
            int grassLayer = Array.FindIndex(data.terrainLayers, layer => layer && layer.name == "Grass");
            if (grassLayer < 0)
                throw new InvalidOperationException("The pilot requires the Grass terrain layer.");

            int width = data.alphamapWidth;
            int height = data.alphamapHeight;
            float[,,] coverage = data.GetAlphamaps(0, 0, width, height);
            float left = spawn.x - 20;
            float bottom = spawn.z - 3;
            var cells = new List<CoastalGrassLayout.Cell>();
            var random = new System.Random(29);

            for (int cellZ = 0; cellZ < 7; cellZ++)
            for (int cellX = 0; cellX < 5; cellX++)
            {
                Vector2 minimum = new(left + cellX * CellSize, bottom + cellZ * CellSize);
                Vector2 maximum = minimum + Vector2.one * CellSize;
                PathTriangle[] localPaths = path.Where(triangle => triangle.Overlaps(minimum, maximum)).ToArray();
                var variants = meshes.Select(_ => new List<Matrix4x4>()).ToArray();
                var bounds = new Bounds();
                bool hasBounds = false;
                int steps = Mathf.CeilToInt(CellSize / Spacing);
                float step = CellSize / steps;

                for (int row = 0; row < steps; row++)
                for (int column = 0; column < steps; column++)
                {
                    float x = minimum.x + (column + Next(random)) * step;
                    float z = minimum.y + (row + Next(random)) * step;
                    var point = new Vector2(x, z);
                    float u = (x - origin.x) / data.size.x;
                    float v = (z - origin.z) / data.size.z;
                    if (u < 0 || v < 0 || u >= 1 || v >= 1)
                        continue;

                    int tx = Mathf.RoundToInt(u * (width - 1));
                    int tz = Mathf.RoundToInt(v * (height - 1));
                    float cover = coverage[tz, tx, grassLayer];
                    float patch = Mathf.SmoothStep(0.67f, 0.98f, Mathf.PerlinNoise(x * 0.17f, z * 0.17f));
                    if (cover < 0.63f || Next(random) > cover * patch || data.GetSteepness(u, v) > 35 ||
                        OnPath(point, localPaths) || NearAccess(point, accessPoints))
                        continue;

                    float ground = origin.y + data.GetInterpolatedHeight(u, v);
                    if (ground < 1.4f ||
                        !Physics.Raycast(new Vector3(x, origin.y + data.size.y + 10, z), Vector3.down,
                            out RaycastHit hit, data.size.y + 20, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore) ||
                        hit.collider.gameObject != terrain.gameObject)
                        continue;

                    float choice = Next(random);
                    int variant = choice < 0.29f ? 0 : choice < 0.54f ? 1 : 2;
                    float scale = Mathf.Lerp(0.86f, 1.23f, Next(random));
                    Quaternion rotation = Quaternion.Euler(0, Next(random) * 360, 0);
                    Vector3 position = new(x, ground + 0.003f, z);
                    Matrix4x4 matrix = Matrix4x4.TRS(position, rotation, Vector3.one * scale);
                    variants[variant].Add(matrix);

                    IncludeTuftBounds(ref bounds, ref hasBounds, meshes[variant], position, scale);
                }

                if (!hasBounds)
                    continue;

                cells.Add(PackCell(bounds, variants));
            }

            return cells.ToArray();
        }

        private static void IncludeTuftBounds(ref Bounds bounds, ref bool hasBounds, Mesh mesh, Vector3 position, float scale)
        {
            Bounds meshBounds = mesh.bounds;
            float radius = scale * (meshBounds.center.magnitude + meshBounds.extents.magnitude) +
                CoastalGrassLayout.MaxWindBend;
            var instanceBounds = new Bounds(position, Vector3.one * (2 * radius));
            if (hasBounds)
                bounds.Encapsulate(instanceBounds);
            else
                bounds = instanceBounds;
            hasBounds = true;
        }

        private static CoastalGrassLayout.Cell PackCell(Bounds bounds, List<Matrix4x4>[] variants)
        {
            var batches = new List<CoastalGrassLayout.Batch>();
            for (int variant = 0; variant < variants.Length; variant++)
            {
                List<Matrix4x4> matrices = variants[variant];
                for (int start = 0; start < matrices.Count; start += CoastalGrassLayout.MaxBatchSize)
                {
                    int count = Mathf.Min(CoastalGrassLayout.MaxBatchSize, matrices.Count - start);
                    batches.Add(new CoastalGrassLayout.Batch
                    {
                        MeshIndex = variant,
                        Instances = matrices.GetRange(start, count).ToArray()
                    });
                }
            }
            return new CoastalGrassLayout.Cell { Bounds = bounds, Batches = batches.ToArray() };
        }

        private static float Next(System.Random random) => (float)random.NextDouble();
        private static Vector2 Xz(Vector3 value) => new(value.x, value.z);

        private static bool OnPath(Vector2 point, PathTriangle[] paths)
        {
            foreach (PathTriangle triangle in paths)
                if (triangle.Contains(point))
                    return true;
            return false;
        }

        private static bool NearAccess(Vector2 point, List<Vector2> accesses)
        {
            foreach (Vector2 access in accesses)
                if ((point - access).sqrMagnitude < 1.25f * 1.25f)
                    return true;
            return false;
        }

        private static IEnumerable<PathTriangle> ProjectTriangles(MeshFilter filter, float padding)
        {
            Mesh mesh = filter.sharedMesh;
            if (!mesh)
                yield break;

            Vector2[] vertices = mesh.vertices.Select(vertex => Xz(filter.transform.TransformPoint(vertex))).ToArray();
            int[] indices = mesh.triangles;
            for (int index = 0; index < indices.Length; index += 3)
            {
                Vector2 a = vertices[indices[index]];
                Vector2 b = vertices[indices[index + 1]];
                Vector2 c = vertices[indices[index + 2]];
                if (Mathf.Abs(PathTriangle.Cross(b - a, c - a)) > 0.00001f)
                    yield return new PathTriangle(a, b, c, padding);
            }
        }

        private readonly struct PathTriangle
        {
            private readonly Vector2 a;
            private readonly Vector2 b;
            private readonly Vector2 c;
            private readonly Vector2 min;
            private readonly Vector2 max;

            private readonly float padding;

            public PathTriangle(Vector2 a, Vector2 b, Vector2 c, float padding)
            {
                this.a = a;
                this.b = b;
                this.c = c;
                this.padding = padding;
                Vector2 margin = Vector2.one * padding;
                min = Vector2.Min(a, Vector2.Min(b, c)) - margin;
                max = Vector2.Max(a, Vector2.Max(b, c)) + margin;
            }

            public bool Overlaps(Vector2 left, Vector2 right) =>
                max.x >= left.x && min.x <= right.x && max.y >= left.y && min.y <= right.y;

            public bool Contains(Vector2 point)
            {
                if (!Overlaps(point, point))
                    return false;

                float ab = Cross(b - a, point - a);
                float bc = Cross(c - b, point - b);
                float ca = Cross(a - c, point - c);
                if ((ab >= 0 && bc >= 0 && ca >= 0) || (ab <= 0 && bc <= 0 && ca <= 0))
                    return true;

                return NearSegment(point, a, b) || NearSegment(point, b, c) || NearSegment(point, c, a);
            }

            private bool NearSegment(Vector2 point, Vector2 from, Vector2 to)
            {
                Vector2 edge = to - from;
                float t = Mathf.Clamp01(Vector2.Dot(point - from, edge) / edge.sqrMagnitude);
                return (point - (from + t * edge)).sqrMagnitude < padding * padding;
            }

            public static float Cross(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;
        }
    }
}
