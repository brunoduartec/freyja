using UnityEngine;

namespace Freyja.Environment
{
    [DisallowMultipleComponent]
    public sealed class GrassExclusionVolume : MonoBehaviour
    {
        [SerializeField] private CoastalGrass grass;
        [SerializeField] private Vector3 center;
        [SerializeField] private Vector3 size = new(4f, 3f, 4f);

        private Matrix4x4 lastTransform;
        private Matrix4x4 worldToLocal;
        private Vector3 lastCenter;
        private Vector3 lastSize;

        internal Bounds Coverage { get; private set; }

        private void OnEnable()
        {
            CacheShape();
            if (grass)
                grass.AddExclusion(this);
        }

        private void OnDisable()
        {
            if (grass)
                grass.RemoveExclusion(this);
        }

        private void LateUpdate()
        {
            if (transform.localToWorldMatrix == lastTransform && center == lastCenter && size == lastSize)
                return;

            Bounds previous = Coverage;
            CacheShape();
            if (grass)
                grass.MoveExclusion(previous, Coverage);
        }

        public void Configure(CoastalGrass renderer, Vector3 localCenter, Vector3 localSize)
        {
            if (isActiveAndEnabled && grass)
                grass.RemoveExclusion(this);
            grass = renderer;
            center = localCenter;
            size = localSize;
            CacheShape();
            if (isActiveAndEnabled && grass)
                grass.AddExclusion(this);
        }

        internal bool Contains(Vector3 point)
        {
            if (!Coverage.Contains(point))
                return false;

            Vector3 local = worldToLocal.MultiplyPoint3x4(point) - lastCenter;
            Vector3 half = lastSize * 0.5f;
            return Mathf.Abs(local.x) <= half.x && Mathf.Abs(local.y) <= half.y && Mathf.Abs(local.z) <= half.z;
        }

        private void CacheShape()
        {
            lastTransform = transform.localToWorldMatrix;
            worldToLocal = transform.worldToLocalMatrix;
            lastCenter = center;
            lastSize = new Vector3(Mathf.Abs(size.x), Mathf.Abs(size.y), Mathf.Abs(size.z));
            Vector3 half = lastSize * 0.5f;
            Vector3 x = lastTransform.MultiplyVector(new Vector3(half.x, 0, 0));
            Vector3 y = lastTransform.MultiplyVector(new Vector3(0, half.y, 0));
            Vector3 z = lastTransform.MultiplyVector(new Vector3(0, 0, half.z));
            Vector3 extents = new(
                Mathf.Abs(x.x) + Mathf.Abs(y.x) + Mathf.Abs(z.x),
                Mathf.Abs(x.y) + Mathf.Abs(y.y) + Mathf.Abs(z.y),
                Mathf.Abs(x.z) + Mathf.Abs(y.z) + Mathf.Abs(z.z));
            Coverage = new Bounds(lastTransform.MultiplyPoint3x4(lastCenter), extents * 2);
        }

        private void OnDrawGizmosSelected()
        {
            Matrix4x4 previous = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = new Color(1, 0.5f, 0.1f, 0.8f);
            Gizmos.DrawWireCube(center, size);
            Gizmos.matrix = previous;
        }
    }
}
