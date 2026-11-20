using UnityEngine;
using UnityEngine.AI;

namespace Freyja.Prototype
{
    public sealed class IslandWaypointNetwork : MonoBehaviour
    {
        [SerializeField] private NavMeshData navigation;

        private NavMeshDataInstance instance;

        public bool IsReady => instance.valid;

        private void OnEnable()
        {
            if (navigation != null)
                instance = NavMesh.AddNavMeshData(navigation);
        }

        private void OnDisable()
        {
            if (instance.valid)
                instance.Remove();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.15f, 0.85f, 1f, 0.8f);
            foreach (Transform group in transform)
            foreach (Transform point in group)
                Gizmos.DrawWireSphere(point.position + Vector3.up * 0.15f, 0.23f);
        }
    }
}
