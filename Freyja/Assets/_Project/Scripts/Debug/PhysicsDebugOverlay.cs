#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Freyja.Diagnostics
{
    internal sealed class PhysicsDebugOverlay : IDisposable
    {
        private const float Range = 28f;
        private const int TerrainChunk = 32;
        private const int CircleSegments = 24;
        private static readonly int Tint = Shader.PropertyToID("_Color");
        private static readonly int DepthTest = Shader.PropertyToID("_DepthTest");
        private static readonly Color WorldColor = new Color(0.22f, 0.9f, 1f, 0.62f);
        private static readonly Color PlayerColor = new Color(1f, 0.79f, 0.23f, 0.98f);
        private static readonly Color TerrainColor = new Color(0.22f, 0.9f, 1f, 0.23f);

        private readonly Material material;
        private readonly Mesh box;
        private readonly Mesh sphere;
        private readonly Mesh capsuleCap;
        private readonly Mesh capsuleSide;
        private readonly Dictionary<Mesh, Mesh> meshWires = new Dictionary<Mesh, Mesh>();
        private readonly Dictionary<(TerrainData, int, int), Mesh> terrainWires =
            new Dictionary<(TerrainData, int, int), Mesh>();
        private readonly Plane[] frustum = new Plane[6];
        private Collider[] colliders = Array.Empty<Collider>();
        private float nextRefresh;

        public PhysicsDebugOverlay(Shader shader)
        {
            material = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
            box = CreateBox();
            sphere = CreateSphere();
            capsuleCap = CreateCapsuleCap();
            capsuleSide = CreateCapsuleSide();
        }

        public void Draw(Camera camera)
        {
            if (Time.unscaledTime >= nextRefresh)
            {
                colliders = UnityEngine.Object.FindObjectsByType<Collider>(FindObjectsSortMode.None);
                nextRefresh = Time.unscaledTime + 0.5f;
            }

            GeometryUtility.CalculateFrustumPlanes(camera, frustum);
            Vector3 eye = camera.transform.position;
            foreach (Collider collider in colliders)
            {
                if (collider == null || !collider.enabled || !collider.gameObject.activeInHierarchy)
                    continue;

                Bounds bounds = collider.bounds;
                if (bounds.SqrDistance(eye) > Range * Range || !GeometryUtility.TestPlanesAABB(frustum, bounds))
                    continue;

                DrawCollider(collider, eye);
            }
        }

        private void DrawCollider(Collider collider, Vector3 eye)
        {
            Transform transform = collider.transform;
            Vector3 scale = transform.lossyScale;
            scale = new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
            switch (collider)
            {
                case CharacterController character:
                    DrawCapsule(transform, scale, character.center, character.height, character.radius, 1, PlayerColor);
                    break;
                case BoxCollider shape:
                    DrawMesh(box, transform.localToWorldMatrix * Matrix4x4.TRS(shape.center, Quaternion.identity, shape.size), WorldColor);
                    break;
                case SphereCollider shape:
                    float radius = shape.radius * Mathf.Max(scale.x, Mathf.Max(scale.y, scale.z));
                    DrawMesh(sphere, Matrix4x4.TRS(transform.TransformPoint(shape.center), transform.rotation, Vector3.one * radius), WorldColor);
                    break;
                case CapsuleCollider shape:
                    DrawCapsule(transform, scale, shape.center, shape.height, shape.radius, shape.direction, WorldColor);
                    break;
                case MeshCollider shape when shape.sharedMesh != null:
                    DrawMesh(GetMeshWire(shape.sharedMesh), transform.localToWorldMatrix, WorldColor);
                    break;
                case TerrainCollider terrain when terrain.terrainData != null:
                    DrawTerrain(terrain, eye);
                    break;
            }
        }

        private void DrawCapsule(Transform transform, Vector3 scale, Vector3 center, float height,
            float radius, int direction, Color color)
        {
            float radiusScale = Mathf.Max(scale[(direction + 1) % 3], scale[(direction + 2) % 3]);
            radius *= radiusScale;
            float halfSide = Mathf.Max(0f, height * scale[direction] * 0.5f - radius);

            var localAxis = Vector3.zero;
            localAxis[direction] = 1f;
            Quaternion rotation = transform.rotation * Quaternion.FromToRotation(Vector3.up, localAxis);
            Vector3 axis = rotation * Vector3.up;
            Vector3 worldCenter = transform.TransformPoint(center);
            var capScale = Vector3.one * radius;
            DrawMesh(capsuleCap, Matrix4x4.TRS(worldCenter + axis * halfSide, rotation, capScale), color);
            DrawMesh(capsuleCap, Matrix4x4.TRS(worldCenter - axis * halfSide,
                rotation * Quaternion.Euler(180f, 0f, 0f), capScale), color);
            DrawMesh(capsuleSide, Matrix4x4.TRS(worldCenter, rotation, new Vector3(radius, halfSide, radius)), color);
        }

        private Mesh GetMeshWire(Mesh source)
        {
            if (meshWires.TryGetValue(source, out Mesh wire))
                return wire;

            if (!source.isReadable)
            {
                Debug.LogWarning($"Physics overlay cannot read the vertices of {source.name}. Enable Read/Write for this mesh.");
                meshWires.Add(source, null);
                return null;
            }

            int[] triangles = source.triangles;
            var edges = new HashSet<(int, int)>();
            var indices = new List<int>();
            void AddEdge(int a, int b)
            {
                if (!edges.Add((Mathf.Min(a, b), Mathf.Max(a, b))))
                    return;
                indices.Add(a);
                indices.Add(b);
            }

            for (int i = 0; i < triangles.Length; i += 3)
            {
                AddEdge(triangles[i], triangles[i + 1]);
                AddEdge(triangles[i + 1], triangles[i + 2]);
                AddEdge(triangles[i + 2], triangles[i]);
            }

            wire = CreateLines(source.vertices, indices.ToArray());
            meshWires.Add(source, wire);
            return wire;
        }

        private void DrawTerrain(TerrainCollider terrain, Vector3 eye)
        {
            TerrainData data = terrain.terrainData;
            Vector3 local = terrain.transform.InverseTransformPoint(eye);
            Vector3 step = data.heightmapScale;
            int cells = data.heightmapResolution - 1;
            int lastChunk = (cells - 1) / TerrainChunk;
            int minX = Mathf.Clamp(Mathf.FloorToInt((local.x - Range) / step.x / TerrainChunk), 0, lastChunk);
            int maxX = Mathf.Clamp(Mathf.FloorToInt((local.x + Range) / step.x / TerrainChunk), 0, lastChunk);
            int minZ = Mathf.Clamp(Mathf.FloorToInt((local.z - Range) / step.z / TerrainChunk), 0, lastChunk);
            int maxZ = Mathf.Clamp(Mathf.FloorToInt((local.z + Range) / step.z / TerrainChunk), 0, lastChunk);

            for (int z = minZ; z <= maxZ; z++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    var key = (data, x, z);
                    if (!terrainWires.TryGetValue(key, out Mesh wire))
                    {
                        wire = CreateTerrainChunk(data, x * TerrainChunk, z * TerrainChunk);
                        terrainWires.Add(key, wire);
                    }
                    Matrix4x4 matrix = terrain.transform.localToWorldMatrix;
                    matrix.m13 += 0.012f;
                    DrawMesh(wire, matrix, TerrainColor, CompareFunction.LessEqual);
                }
            }
        }

        private static Mesh CreateTerrainChunk(TerrainData data, int startX, int startZ)
        {
            int width = Mathf.Min(TerrainChunk + 1, data.heightmapResolution - startX);
            int depth = Mathf.Min(TerrainChunk + 1, data.heightmapResolution - startZ);
            float[,] heights = data.GetHeights(startX, startZ, width, depth);
            bool[,] ground = data.GetHoles(startX, startZ, width - 1, depth - 1);
            Vector3 step = data.heightmapScale;
            var vertices = new Vector3[width * depth];
            var edges = new List<int>(vertices.Length * 4);

            void AddEdge(int a, int b)
            {
                edges.Add(a);
                edges.Add(b);
            }

            for (int z = 0; z < depth; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = z * width + x;
                    vertices[index] = new Vector3((startX + x) * step.x, heights[z, x] * step.y, (startZ + z) * step.z);

                    if (x + 1 < width && (z + 1 < depth && ground[z, x] || z > 0 && ground[z - 1, x]))
                        AddEdge(index, index + 1);
                    if (z + 1 < depth && (x + 1 < width && ground[z, x] || x > 0 && ground[z, x - 1]))
                        AddEdge(index, index + width);
                }
            }
            return CreateLines(vertices, edges.ToArray());
        }

        private void DrawMesh(Mesh mesh, Matrix4x4 matrix, Color color, CompareFunction depth = CompareFunction.Always)
        {
            if (mesh == null)
                return;

            material.SetColor(Tint, color);
            material.SetInt(DepthTest, (int)depth);
            if (material.SetPass(0))
                Graphics.DrawMeshNow(mesh, matrix);
        }

        private static Mesh CreateBox()
        {
            const float h = 0.5f;
            var corners = new[]
            {
                new Vector3(-h,-h,-h), new Vector3(h,-h,-h), new Vector3(h,-h,h), new Vector3(-h,-h,h),
                new Vector3(-h,h,-h), new Vector3(h,h,-h), new Vector3(h,h,h), new Vector3(-h,h,h)
            };
            return CreateLines(corners, new[] { 0,1, 1,2, 2,3, 3,0, 4,5, 5,6, 6,7, 7,4, 0,4, 1,5, 2,6, 3,7 });
        }

        private static Mesh CreateSphere()
        {
            var points = new List<Vector3>();
            AddArc(points, Vector3.right, Vector3.forward, 2f * Mathf.PI);
            AddArc(points, Vector3.right, Vector3.up, 2f * Mathf.PI);
            AddArc(points, Vector3.forward, Vector3.up, 2f * Mathf.PI);
            return CreateLines(points);
        }

        private static Mesh CreateCapsuleCap()
        {
            var points = new List<Vector3>();
            AddArc(points, Vector3.right, Vector3.forward, 2f * Mathf.PI);
            AddArc(points, Vector3.right, Vector3.up, Mathf.PI);
            AddArc(points, Vector3.forward, Vector3.up, Mathf.PI);
            return CreateLines(points);
        }

        private static Mesh CreateCapsuleSide()
        {
            var points = new List<Vector3>();
            foreach (Vector3 side in new[] { Vector3.right, Vector3.left, Vector3.forward, Vector3.back })
            {
                points.Add(side + Vector3.up);
                points.Add(side - Vector3.up);
            }
            return CreateLines(points);
        }

        private static void AddArc(List<Vector3> points, Vector3 x, Vector3 y, float sweep)
        {
            Vector3 At(float angle) => x * Mathf.Cos(angle) + y * Mathf.Sin(angle);
            for (int i = 0; i < CircleSegments; i++)
            {
                points.Add(At(sweep * i / CircleSegments));
                points.Add(At(sweep * (i + 1) / CircleSegments));
            }
        }

        private static Mesh CreateLines(List<Vector3> vertices)
        {
            var indices = new int[vertices.Count];
            for (int i = 0; i < indices.Length; i++)
                indices[i] = i;
            return CreateLines(vertices.ToArray(), indices);
        }

        private static Mesh CreateLines(Vector3[] vertices, int[] indices)
        {
            var mesh = new Mesh { hideFlags = HideFlags.HideAndDontSave };
            if (vertices.Length > ushort.MaxValue)
                mesh.indexFormat = IndexFormat.UInt32;
            mesh.vertices = vertices;
            mesh.SetIndices(indices, MeshTopology.Lines, 0);
            return mesh;
        }

        public void Dispose()
        {
            UnityEngine.Object.Destroy(material);
            UnityEngine.Object.Destroy(box);
            UnityEngine.Object.Destroy(sphere);
            UnityEngine.Object.Destroy(capsuleCap);
            UnityEngine.Object.Destroy(capsuleSide);
            foreach (Mesh wire in meshWires.Values)
                UnityEngine.Object.Destroy(wire);
            foreach (Mesh wire in terrainWires.Values)
                UnityEngine.Object.Destroy(wire);
        }
    }
}
#endif
