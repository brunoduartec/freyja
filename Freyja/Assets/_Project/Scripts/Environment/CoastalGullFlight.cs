using UnityEngine;

namespace Freyja.Environment
{
    [DisallowMultipleComponent]
    public sealed class CoastalGullFlight : MonoBehaviour
    {
        [SerializeField] private CoastalDayNight clock;
        [SerializeField] private Transform bird;
        [SerializeField] private Transform leftWing;
        [SerializeField] private Transform rightWing;
        [SerializeField] private Vector2 radii = new Vector2(15f, 6f);
        [SerializeField, Min(1f)] private float period = 20f;
        [SerializeField] private float phaseDegrees;

        private Quaternion leftRest;
        private Quaternion rightRest;

        private void Awake()
        {
            if (clock == null || bird == null || leftWing == null || rightWing == null)
            {
                Debug.LogError("CoastalGullFlight requires a clock, bird and both wing pivots.", this);
                enabled = false;
                return;
            }

            leftRest = leftWing.localRotation;
            rightRest = rightWing.localRotation;
        }

        private void LateUpdate()
        {
            float hour = clock.Hour;
            bool daylight = hour >= 6f && hour < 18f;
            if (bird.gameObject.activeSelf != daylight)
                bird.gameObject.SetActive(daylight);
            if (!daylight)
                return;

            float time = Time.timeSinceLevelLoad;
            float angle = (time / period) * 2f * Mathf.PI + phaseDegrees * Mathf.Deg2Rad;
            float sin = Mathf.Sin(angle);
            float cos = Mathf.Cos(angle);
            bird.localPosition = new Vector3(radii.x * cos, 0.45f * Mathf.Sin(2f * angle), radii.y * sin);
            Vector3 tangent = new Vector3(-radii.x * sin, 0.9f * Mathf.Cos(2f * angle), radii.y * cos);
            bird.localRotation = Quaternion.LookRotation(tangent) * Quaternion.Euler(0f, 0f, -12f);

            float stroke = 22f * Mathf.Pow(Mathf.Max(0f, Mathf.Cos(time * 1.5f + angle)), 3f)
                * Mathf.Sin(time * 12f + phaseDegrees);
            leftWing.localRotation = Quaternion.AngleAxis(-stroke, Vector3.forward) * leftRest;
            rightWing.localRotation = Quaternion.AngleAxis(stroke, Vector3.forward) * rightRest;
        }
    }
}
