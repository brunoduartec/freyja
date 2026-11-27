using System;
using UnityEngine;

namespace Freyja.Environment
{
    [PreferBinarySerialization]
    public sealed class GrassDensityLayout : ScriptableObject
    {
        [Serializable]
        public struct Tuft
        {
            public Vector3 Root;
            public float Yaw;
            public float Scale;
            public int MeshIndex;

            public Matrix4x4 Matrix => Matrix4x4.TRS(Root, Quaternion.Euler(0, Yaw, 0), Vector3.one * Scale);
        }

        [Serializable]
        public sealed class Region
        {
            public Tuft[] Tufts = Array.Empty<Tuft>();
        }

        [SerializeField, Range(3f, 24f)] private float patchSize = 9.5f;
        [SerializeField, Range(0f, 1f)] private float contrast = 0.8f;
        [SerializeField] private Vector2 offset = new(34.6f, 71.3f);

        public float SampleCoverage(Vector3 position)
        {
            Vector2 point = new Vector2(position.x, position.z) / patchSize + offset;
            var warp = new Vector2(
                Mathf.PerlinNoise(point.x * 0.63f + 13f, point.y * 0.63f),
                Mathf.PerlinNoise(point.x * 0.63f, point.y * 0.63f + 29f));
            point += (warp - Vector2.one * 0.5f) * 0.67f;
            float large = Mathf.PerlinNoise(point.x, point.y);
            float fine = Mathf.PerlinNoise(point.x * 2.83f + 17f, point.y * 2.83f - 36f);
            float patch = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.22f, 0.74f, 0.73f * large + 0.27f * fine));
            return Mathf.Lerp(1f, Mathf.Lerp(0.27f, 1f, patch), contrast);
        }

        [SerializeField] private CoastalGrassLayout source;
        [SerializeField] private Region[] regions = Array.Empty<Region>();

#if UNITY_EDITOR
        public bool Matches(CoastalGrassLayout layout) => source == layout && regions.Length == layout.Cells.Length;
        public Tuft[] GetTufts(int region) => regions[region].Tufts;
#else
        internal bool Matches(CoastalGrassLayout layout) => source == layout && regions.Length == layout.Cells.Length;
        internal Tuft[] GetTufts(int region) => regions[region].Tufts;
#endif

#if UNITY_EDITOR
        public void Configure(CoastalGrassLayout layout, Region[] newRegions)
        {
            if (newRegions.Length != layout.Cells.Length)
                throw new ArgumentException("The density layout must use the source regions.", nameof(newRegions));
            source = layout;
            regions = newRegions;
        }
#endif
    }
}
