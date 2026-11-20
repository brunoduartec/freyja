using UnityEngine;
namespace Freyja.WorldBuilder
{
    public sealed class AssetDescriptor:ScriptableObject
    {
        public GameObject sourceModel,normalizedPrefab;
        public Vector2 targetFootprint;
        public Vector3 orientation;
        public Bounds sourceBounds,finalBounds;
        public float uniformScale;
        public string[] warnings;
    }
}
