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

        public const int MaxBatchSize = 255;
        public const float MaxWindBend = 0.12f;

        [SerializeField, HideInInspector] private Cell[] cells = Array.Empty<Cell>();

        public Cell[] Cells => cells;

#if UNITY_EDITOR
        public void SetCells(Cell[] value) => cells = value;
#endif
    }
}
