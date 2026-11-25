using System;
using UnityEngine;

namespace Freyja.Environment
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-200)]
    public sealed class GlobalWindController : MonoBehaviour
    {
        public enum Preset { Calm, Breeze, Strong }

        [Serializable]
        public struct WindProfile
        {
            [Range(0, 1)] public float strength;
            [Min(0)] public float speed;
            [Min(0)] public float frequency;
            [Range(0, 1)] public float gust;
            [Range(0, 1)] public float turbulence;

            public WindProfile(float strength, float speed, float frequency, float gust, float turbulence)
            {
                this.strength = strength;
                this.speed = speed;
                this.frequency = frequency;
                this.gust = gust;
                this.turbulence = turbulence;
            }

            internal static WindProfile Blend(WindProfile current, WindProfile target, float weight) => new(
                Mathf.Lerp(current.strength, target.strength, weight),
                Mathf.Lerp(current.speed, target.speed, weight),
                Mathf.Lerp(current.frequency, target.frequency, weight),
                Mathf.Lerp(current.gust, target.gust, weight),
                Mathf.Lerp(current.turbulence, target.turbulence, weight));
        }

        [SerializeField] private Preset preset = Preset.Breeze;
        [SerializeField] private Vector2 direction = new(0.8f, 0.6f);
        [SerializeField, Min(0f)] private float transitionSeconds = 0.8f;
        [SerializeField] private WindProfile calm = new(0f, 0f, 0.32f, 0f, 0f);
        [SerializeField] private WindProfile breeze = new(0.55f, 1.7f, 0.32f, 0.5f, 0.25f);
        [SerializeField] private WindProfile strong = new(1f, 2.9f, 0.32f, 0.85f, 0.5f);

        private static readonly int WindId = Shader.PropertyToID("_FreyjaWind");
        private static readonly int GustId = Shader.PropertyToID("_FreyjaWindGust");
        private static GlobalWindController owner;

        private WindProfile current;
        private float heading;
        private float phase;

        public static GlobalWindController Active => owner;
        public Preset CurrentPreset { get => preset; set => preset = value; }
        public Vector2 Direction { get => direction; set => direction = value; }

        private WindProfile Target => preset switch
        {
            Preset.Calm => new WindProfile(0, calm.speed, calm.frequency, calm.gust, calm.turbulence),
            Preset.Strong => strong,
            _ => breeze
        };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetGlobals()
        {
            owner = null;
            Shader.SetGlobalVector(WindId, Vector4.zero);
            Shader.SetGlobalVector(GustId, Vector4.zero);
        }

        private void OnEnable()
        {
            if (owner && owner != this)
            {
                Debug.LogWarning("A global wind controller is already active.", this);
                enabled = false;
                return;
            }

            owner = this;
            heading = ResolveHeading(direction, 0);
            current = Target;
            phase = 0;
            Publish();
        }

        private void OnDisable()
        {
            if (owner == this)
                ResetGlobals();
        }

        private void Update()
        {
            float delta = Time.deltaTime;
            if (!(delta > 0))
                return;

            float weight = transitionSeconds > 0 ? 1 - Mathf.Exp(-4 * delta / transitionSeconds) : 1;
            current = WindProfile.Blend(current, Target, weight);
            if (preset == Preset.Calm && current.strength < 0.0001f)
                current.strength = 0;
            heading += Mathf.DeltaAngle(heading, ResolveHeading(direction, heading)) * weight;
            phase += current.speed * delta;
            Publish();
        }

        private void Publish()
        {
            float angle = heading * Mathf.Deg2Rad;
            Shader.SetGlobalVector(WindId, new Vector4(Mathf.Cos(angle), Mathf.Sin(angle), current.strength, phase));
            Shader.SetGlobalVector(GustId, new Vector4(current.frequency, current.gust, current.turbulence, 0));
        }

        public static float ResolveHeading(Vector2 value, float fallback)
        {
            if (!float.IsFinite(value.x) || !float.IsFinite(value.y) || value.sqrMagnitude < 1e-8f)
                return fallback;
            return Mathf.Atan2(value.y, value.x) * Mathf.Rad2Deg;
        }
    }
}
