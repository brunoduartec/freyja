using UnityEngine;
using UnityEngine.Rendering;

namespace Freyja.Environment
{
    [DisallowMultipleComponent]
    public sealed class CoastalDayNight : MonoBehaviour
    {
        [SerializeField] private Light sun;
        [SerializeField] private Material sky;
        [SerializeField, Range(0f, 24f)] private float hour = 9f;
        [SerializeField, Min(1f)] private float cycleSeconds = 96f;
        [SerializeField] private bool advanceClock = true;
        [SerializeField, Range(0f, 360f)] private float sunriseAzimuth = 155f;

        public float Hour => Mathf.Repeat(hour, 24f);
        public bool AdvancesAutomatically
        {
            get => advanceClock;
            set => advanceClock = value;
        }

        public void SetHour(float value)
        {
            hour = Mathf.Repeat(value, 24f);
            if (runtimeSky != null)
                ApplyLighting();
        }

        private static readonly int Zenith = Shader.PropertyToID("_Zenith");
        private static readonly int Horizon = Shader.PropertyToID("_Horizon");
        private static readonly int CloudLight = Shader.PropertyToID("_CloudLight");
        private static readonly int CloudShade = Shader.PropertyToID("_CloudShade");
        private static readonly int SolarTint = Shader.PropertyToID("_SolarTint");
        private static readonly int SolarDirection = Shader.PropertyToID("_SolarDirection");
        private static readonly int CloudDrift = Shader.PropertyToID("_CloudDrift");

        private static readonly Palette[] Palettes =
        {
            new Palette(new Color(0.052f, 0.09f, 0.21f), new Color(0.23f, 0.34f, 0.53f),
                new Color(0.28f, 0.37f, 0.55f), new Color(0.13f, 0.20f, 0.34f),
                new Color(0.34f, 0.44f, 0.64f), new Color(1f, 0.50f, 0.22f)),
            new Palette(new Color(0.29f, 0.48f, 0.72f), new Color(0.96f, 0.67f, 0.44f),
                new Color(1f, 0.85f, 0.68f), new Color(0.50f, 0.50f, 0.67f),
                new Color(0.56f, 0.57f, 0.63f), new Color(1f, 0.72f, 0.45f)),
            new Palette(new Color(0.20f, 0.51f, 0.78f), new Color(0.69f, 0.85f, 0.92f),
                new Color(1f, 0.98f, 0.89f), new Color(0.55f, 0.68f, 0.79f),
                new Color(0.63f, 0.71f, 0.79f), new Color(1f, 0.96f, 0.79f)),
            new Palette(new Color(0.30f, 0.28f, 0.55f), new Color(0.96f, 0.48f, 0.31f),
                new Color(1f, 0.70f, 0.48f), new Color(0.49f, 0.33f, 0.50f),
                new Color(0.50f, 0.44f, 0.56f), new Color(1f, 0.48f, 0.24f))
        };

        private Material runtimeSky;
        private Material previousSky;
        private Light previousSun;
        private AmbientMode previousAmbientMode;
        private Color previousAmbientSky;
        private Color previousAmbientEquator;
        private Color previousAmbientGround;
        private Quaternion previousSunRotation;
        private Color previousSunColor;
        private float previousSunIntensity;
        private float previousShadowStrength;
        private float cloudTravel;

        private void OnEnable()
        {
            if (sun == null || sun.type != LightType.Directional || sky == null)
            {
                Debug.LogError("CoastalDayNight requires a directional light and a sky material.", this);
                enabled = false;
                return;
            }

            previousSky = RenderSettings.skybox;
            previousSun = RenderSettings.sun;
            previousAmbientMode = RenderSettings.ambientMode;
            previousAmbientSky = RenderSettings.ambientSkyColor;
            previousAmbientEquator = RenderSettings.ambientEquatorColor;
            previousAmbientGround = RenderSettings.ambientGroundColor;
            previousSunRotation = sun.transform.rotation;
            previousSunColor = sun.color;
            previousSunIntensity = sun.intensity;
            previousShadowStrength = sun.shadowStrength;

            runtimeSky = new Material(sky);
            RenderSettings.skybox = runtimeSky;
            RenderSettings.sun = sun;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            ApplyLighting();
        }

        private void Update()
        {
            float delta = Time.deltaTime;
            if (advanceClock)
                hour = Mathf.Repeat(hour + delta * 24f / Mathf.Max(1f, cycleSeconds), 24f);

            cloudTravel = Mathf.Repeat(cloudTravel + delta * 0.004f, 10f);
            ApplyLighting();
        }

        private void OnDisable()
        {
            if (runtimeSky == null)
                return;

            if (RenderSettings.skybox == runtimeSky)
            {
                RenderSettings.skybox = previousSky;
                RenderSettings.sun = previousSun;
                RenderSettings.ambientMode = previousAmbientMode;
                RenderSettings.ambientSkyColor = previousAmbientSky;
                RenderSettings.ambientEquatorColor = previousAmbientEquator;
                RenderSettings.ambientGroundColor = previousAmbientGround;
            }

            if (sun != null)
            {
                sun.transform.rotation = previousSunRotation;
                sun.color = previousSunColor;
                sun.intensity = previousSunIntensity;
                sun.shadowStrength = previousShadowStrength;
            }

            Destroy(runtimeSky);
            runtimeSky = null;
        }

        private void ApplyLighting()
        {
            float phase = Mathf.Repeat(hour, 24f) / 6f;
            int index = Mathf.FloorToInt(phase);
            float blend = Mathf.SmoothStep(0f, 1f, phase - index);
            Palette from = Palettes[index];
            Palette to = Palettes[(index + 1) % Palettes.Length];

            float orbit = (hour - 6f) * Mathf.PI / 12f;
            Vector3 east = Quaternion.Euler(0f, sunriseAzimuth, 0f) * Vector3.forward;
            Vector3 high = 0.82f * Vector3.up + 0.57236f * Vector3.Cross(east, Vector3.up);
            Vector3 direction = (Mathf.Cos(orbit) * east + Mathf.Sin(orbit) * high).normalized;
            float daylight = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(-0.045f, 0.23f, direction.y));

            Color sunlight = Color.Lerp(from.Sun, to.Sun, blend);
            sun.transform.rotation = Quaternion.LookRotation(-direction);
            sun.color = sunlight;
            sun.intensity = 1.16f * daylight;
            sun.shadowStrength = previousShadowStrength * daylight;

            Color ambient = Color.Lerp(from.Ambient, to.Ambient, blend);
            RenderSettings.ambientSkyColor = ambient;
            RenderSettings.ambientEquatorColor = ambient * 0.76f;
            RenderSettings.ambientGroundColor = ambient * 0.56f;

            runtimeSky.SetColor(Zenith, Color.Lerp(from.Zenith, to.Zenith, blend));
            runtimeSky.SetColor(Horizon, Color.Lerp(from.Horizon, to.Horizon, blend));
            runtimeSky.SetColor(CloudLight, Color.Lerp(from.CloudLight, to.CloudLight, blend));
            runtimeSky.SetColor(CloudShade, Color.Lerp(from.CloudShade, to.CloudShade, blend));
            runtimeSky.SetColor(SolarTint, sunlight);
            runtimeSky.SetVector(SolarDirection, new Vector4(direction.x, direction.y, direction.z, 0f));
            runtimeSky.SetFloat(CloudDrift, cloudTravel);
        }

        private readonly struct Palette
        {
            public readonly Color Zenith;
            public readonly Color Horizon;
            public readonly Color CloudLight;
            public readonly Color CloudShade;
            public readonly Color Ambient;
            public readonly Color Sun;

            public Palette(Color zenith, Color horizon, Color cloudLight, Color cloudShade, Color ambient, Color sun)
            {
                Zenith = zenith;
                Horizon = horizon;
                CloudLight = cloudLight;
                CloudShade = cloudShade;
                Ambient = ambient;
                Sun = sun;
            }
        }
    }
}
