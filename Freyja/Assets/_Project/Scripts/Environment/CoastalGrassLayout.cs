using System;
using UnityEngine;

namespace Freyja.Environment
{
    public sealed class CoastalGrassLayout : ScriptableObject
    {
        [Serializable]
        public sealed class Batch
        {
            public int MeshIndex;
            public Matrix4x4[] Instances;
        }

        [Serializable]
        public sealed class Cell
        {
            public Bounds Bounds;
            public Batch[] Batches;
        }

        public enum ManualKind { Add, Replace, Remove }
        public enum ManualSource { Layout, Authored, Density }

        [Serializable]
        public struct RootKey : IEquatable<RootKey>
        {
            public ManualSource Source;
            public int MeshIndex;
            public int Xmm;
            public int Zmm;

            public RootKey(ManualSource source, int meshIndex, Vector3 root)
            {
                Source = source;
                MeshIndex = meshIndex;
                Xmm = Mathf.RoundToInt(root.x * 1000f);
                Zmm = Mathf.RoundToInt(root.z * 1000f);
            }

            public bool Equals(RootKey other) => Source == other.Source && MeshIndex == other.MeshIndex && Xmm == other.Xmm && Zmm == other.Zmm;
            public override bool Equals(object obj) => obj is RootKey other && Equals(other);
            public override int GetHashCode() => HashCode.Combine((int)Source, MeshIndex, Xmm, Zmm);
        }

        [Serializable]
        public struct ManualEdit
        {
            public string StableId;
            public ManualKind Kind;
            public RootKey Source;
            public Vector3 Root;
            public float Yaw;
            public float Scale;
            public int MeshIndex;
            public Matrix4x4 Matrix => Matrix4x4.TRS(Root, Quaternion.Euler(0, Yaw, 0), Vector3.one * Scale);
        }

        public const int MaxBatchSize = 255;
        public const float MaxWindBend = 0.12f;

        [SerializeField, HideInInspector] private Cell[] cells = Array.Empty<Cell>();
        [SerializeField, HideInInspector] private ManualEdit[] manualEdits = Array.Empty<ManualEdit>();

        public Cell[] Cells => cells;
        public ManualEdit[] ManualEdits => manualEdits;

#if UNITY_EDITOR
        public void SetCells(Cell[] value) => cells = value;
        public void SetManualEdits(ManualEdit[] value) => manualEdits = value ?? Array.Empty<ManualEdit>();
#endif
    }
}
