using System;
using UnityEngine;

namespace Freyja.Environment
{
    [DisallowMultipleComponent]
    public sealed class VillageNightLights : MonoBehaviour
    {
        private const float DuskStart = 17.5f;
        private const float DawnStart = 5.5f;
        private const float TransitionHours = 0.5f;

        [SerializeField] private CoastalDayNight clock;
        [SerializeField] private Fixture[] fixtures = Array.Empty<Fixture>();

        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
        private MaterialPropertyBlock glassProperties;

        private void OnEnable()
        {
            if (clock == null)
            {
                Debug.LogError("VillageNightLights requires the scene's day and night clock.", this);
                enabled = false;
                return;
            }

            glassProperties ??= new MaterialPropertyBlock();
            ApplyHour(clock.Hour);
        }

        private void LateUpdate()
        {
            ApplyHour(clock.Hour);
        }

        private void OnDisable()
        {
            if (glassProperties == null)
                return;

            foreach (Fixture fixture in fixtures)
                fixture.SetAmount(0f, glassProperties);
        }

        private void ApplyHour(float hour)
        {
            foreach (Fixture fixture in fixtures)
            {
                float dusk = Mathf.InverseLerp(DuskStart + fixture.DelayHours,
                    DuskStart + fixture.DelayHours + TransitionHours, hour);
                float dawn = Mathf.InverseLerp(DawnStart + fixture.DelayHours + TransitionHours,
                    DawnStart + fixture.DelayHours, hour);
                fixture.SetAmount(Mathf.SmoothStep(0f, 1f, Mathf.Max(dusk, dawn)), glassProperties);
            }
        }

        [Serializable]
        private sealed class Fixture
        {
            [SerializeField] private Light source;
            [SerializeField] private Renderer[] panes = Array.Empty<Renderer>();
            [SerializeField, Min(0f)] private float nightIntensity = 2f;
            [SerializeField, ColorUsage(false, true)] private Color nightEmission = new Color(1f, 0.48f, 0.14f);
            [SerializeField, Range(0f, 0.5f)] private float delayHours;

            public float DelayHours => delayHours;
            private float lastAmount = -1f;

            public void SetAmount(float amount, MaterialPropertyBlock properties)
            {
                if (amount == lastAmount)
                    return;

                lastAmount = amount;
                if (source != null)
                {
                    source.intensity = nightIntensity * amount;
                    source.enabled = source.intensity > 0f;
                }

                foreach (Renderer pane in panes)
                {
                    if (pane == null)
                        continue;

                    pane.GetPropertyBlock(properties);
                    properties.SetColor(EmissionColor, nightEmission * amount);
                    pane.SetPropertyBlock(properties);
                }
            }
        }
    }
}
