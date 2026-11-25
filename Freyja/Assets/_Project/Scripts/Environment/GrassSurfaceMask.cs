using System.Collections.Generic;
using UnityEngine;

namespace Freyja.Environment
{
    public sealed class GrassSurfaceMask : ScriptableObject
    {
        [SerializeField] private Vector2Int[] blockedRoots = System.Array.Empty<Vector2Int>();

        internal HashSet<Vector2Int> BuildLookup() => new(blockedRoots);

        internal static Vector2Int ToKey(Vector3 position) =>
            new(Mathf.RoundToInt(position.x * 1024), Mathf.RoundToInt(position.z * 1024));

#if UNITY_EDITOR
        public void Configure(IEnumerable<Vector3> blocked)
        {
            var keys = new HashSet<Vector2Int>();
            foreach (Vector3 point in blocked)
                keys.Add(ToKey(point));
            blockedRoots = new Vector2Int[keys.Count];
            keys.CopyTo(blockedRoots);
            System.Array.Sort(blockedRoots, (a, b) => a.x == b.x ? a.y.CompareTo(b.y) : a.x.CompareTo(b.x));
        }
#endif
    }
}
