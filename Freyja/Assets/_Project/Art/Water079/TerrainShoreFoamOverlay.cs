using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Freyja.Environment
{
    [DisallowMultipleComponent]
    public sealed class TerrainShoreFoamOverlay : MonoBehaviour
    {
        [SerializeField] private Terrain terrain;
        [SerializeField] private Material overlayMaterial;
        [SerializeField] private string sandLayerName = "BeachSand";
        [SerializeField, Range(0f, 1f)] private float minimumSandWeight = 0.18f;
        [SerializeField, Min(0f)] private float waterLevel;
        [SerializeField, Min(0.1f)] private float shorelineRunup = 2.8f;
        [SerializeField, Min(0f)] private float surfaceOffset = 0.035f;

        private GameObject overlayObject;
        private Mesh overlayMesh;

        public int GeneratedQuadCount { get; private set; }

        private void OnEnable() => Rebuild();

        private void OnDisable() => ClearGeneratedMesh();
        public void Initialize(Terrain source, Material material)
        {
            terrain = source;
            overlayMaterial = material;
            Rebuild();
        }

        private void OnDestroy() => ClearGeneratedMesh();

        private void Rebuild()
        {
            ClearGeneratedMesh();
            if (!Application.isPlaying)
                return;
            if (!terrain)
                terrain = GetComponent<Terrain>();
            if (!terrain || !overlayMaterial)
                return;

            TerrainData data = terrain.terrainData;
            TerrainLayer[] layers = data.terrainLayers;
            int sandLayer = Array.FindIndex(layers, layer => layer && layer.name == sandLayerName);
            if (sandLayer < 0)
                return;

            int heightResolution = data.heightmapResolution;
            float[,] heights = data.GetHeights(0, 0, heightResolution, heightResolution);
            int alphaWidth = data.alphamapWidth;
            int alphaHeight = data.alphamapHeight;
            float[,,] alpha = data.GetAlphamaps(0, 0, alphaWidth, alphaHeight);
            Vector3 size = data.size;
            Vector3 origin = terrain.transform.position;
            int cellsX = heightResolution - 1;
            int cellsZ = heightResolution - 1;
            float stepX = size.x / cellsX;
            float stepZ = size.z / cellsZ;

            var vertices = new List<Vector3>();
            var colors = new List<Color>();
            var indices = new List<int>();

            for (int z = 0; z < cellsZ; z++)
            {
                for (int x = 0; x < cellsX; x++)
                {
                    float w00 = SandWeight(alpha, x, z, cellsX, cellsZ, alphaWidth, alphaHeight, sandLayer);
                    float w10 = SandWeight(alpha, x + 1, z, cellsX, cellsZ, alphaWidth, alphaHeight, sandLayer);
                    float w01 = SandWeight(alpha, x, z + 1, cellsX, cellsZ, alphaWidth, alphaHeight, sandLayer);
                    float w11 = SandWeight(alpha, x + 1, z + 1, cellsX, cellsZ, alphaWidth, alphaHeight, sandLayer);
                    if ((w00 + w10 + w01 + w11) * 0.25f < minimumSandWeight)
                        continue;

                    float h00 = origin.y + heights[z, x] * size.y;
                    float h10 = origin.y + heights[z, x + 1] * size.y;
                    float h01 = origin.y + heights[z + 1, x] * size.y;
                    float h11 = origin.y + heights[z + 1, x + 1] * size.y;
                    float minimumHeight = Mathf.Min(Mathf.Min(h00, h10), Mathf.Min(h01, h11));
                    float maximumHeight = Mathf.Max(Mathf.Max(h00, h10), Mathf.Max(h01, h11));
                    if (maximumHeight < waterLevel - 0.25f || minimumHeight > waterLevel + shorelineRunup + 0.6f)
                        continue;

                    int first = vertices.Count;
                    vertices.Add(new Vector3(x * stepX, heights[z, x] * size.y + surfaceOffset, z * stepZ));
                    vertices.Add(new Vector3((x + 1) * stepX, heights[z, x + 1] * size.y + surfaceOffset, z * stepZ));
                    vertices.Add(new Vector3((x + 1) * stepX, heights[z + 1, x + 1] * size.y + surfaceOffset, (z + 1) * stepZ));
                    vertices.Add(new Vector3(x * stepX, heights[z + 1, x] * size.y + surfaceOffset, (z + 1) * stepZ));
                    colors.Add(SandColor(w00));
                    colors.Add(SandColor(w10));
                    colors.Add(SandColor(w11));
                    colors.Add(SandColor(w01));
                    indices.Add(first);
                    indices.Add(first + 1);
                    indices.Add(first + 2);
                    indices.Add(first);
                    indices.Add(first + 2);
                    indices.Add(first + 3);
                    GeneratedQuadCount++;
                }
            }

            if (vertices.Count == 0)
                return;

            overlayMesh = new Mesh { name = "Generated BeachSand Foam Overlay" };
            overlayMesh.indexFormat = vertices.Count > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16;
            overlayMesh.SetVertices(vertices);
            overlayMesh.SetColors(colors);
            overlayMesh.SetTriangles(indices, 0, true);
            overlayMesh.RecalculateBounds();

            overlayObject = new GameObject("Coastal Foam Overlay — BeachSand");
            overlayObject.transform.SetParent(terrain.transform, false);
            overlayObject.layer = terrain.gameObject.layer;
            MeshFilter filter = overlayObject.AddComponent<MeshFilter>();
            filter.sharedMesh = overlayMesh;
            MeshRenderer renderer = overlayObject.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = overlayMaterial;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        }

        private static float SandWeight(float[,,] alpha, int x, int z, int cellsX, int cellsZ,
            int width, int height, int layer)
        {
            int alphaX = Mathf.Clamp(Mathf.RoundToInt((float)x / cellsX * (width - 1)), 0, width - 1);
            int alphaZ = Mathf.Clamp(Mathf.RoundToInt((float)z / cellsZ * (height - 1)), 0, height - 1);
            return alpha[alphaZ, alphaX, layer];
        }

        private static Color SandColor(float weight)
        {
            float mask = Mathf.SmoothStep(0.08f, 0.58f, weight);
            return new Color(1f, 1f, 1f, mask);
        }

        private void ClearGeneratedMesh()
        {
            GeneratedQuadCount = 0;
            if (overlayObject)
            {
                if (Application.isPlaying)
                    Destroy(overlayObject);
                else
                    DestroyImmediate(overlayObject);
            }
            if (overlayMesh)
            {
                if (Application.isPlaying)
                    Destroy(overlayMesh);
                else
                    DestroyImmediate(overlayMesh);
            }
            overlayObject = null;
            overlayMesh = null;
        }
    }
}
