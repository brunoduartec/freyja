using UnityEngine;

namespace Freyja.Environment
{
    public sealed class MooredBoat : MonoBehaviour
    {
        [SerializeField] private Material sea;
        [SerializeField, Range(0, 1)] private float tiltResponse = 1f;
        private Vector3 berth;
        private Quaternion heading;
        private float amplitude;

        private void Awake()
        {
            berth = transform.position;
            heading = transform.rotation;
            amplitude = sea.GetFloat("_WaveHeight");
        }

        private void LateUpdate()
        {
            // Same world-space phases and derivatives as ShelteredSea.shader.
            float time = Time.timeSinceLevelLoad;
            float a = .43f * berth.x + .20f * berth.z - 1.04f * time;
            float b = -.19f * berth.x + .36f * berth.z - .76f * time;
            float height = amplitude * (.72f * Mathf.Sin(a) + .28f * Mathf.Sin(b));
            float da = amplitude * .72f * Mathf.Cos(a) * tiltResponse;
            float db = amplitude * .28f * Mathf.Cos(b) * tiltResponse;
            var normal = new Vector3(-.43f * da + .19f * db, 1, -.20f * da - .36f * db);
            transform.SetPositionAndRotation(berth + Vector3.up * height,
                Quaternion.FromToRotation(Vector3.up, normal.normalized) * heading);
        }
    }
}
