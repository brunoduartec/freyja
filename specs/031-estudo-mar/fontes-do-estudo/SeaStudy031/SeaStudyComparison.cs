using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Freyja.Environment
{
    [DisallowMultipleComponent]
    public sealed class SeaStudyComparison : MonoBehaviour
    {
        public enum Mode
        {
            Reference,
            Surface,
            Geometry
        }

        [SerializeField] private Renderer reference;
        [SerializeField] private Renderer surface;
        [SerializeField] private Renderer geometry;
        [SerializeField] private Mode initialMode = Mode.Surface;

        public Mode CurrentMode { get; private set; }

        private void Awake()
        {
            if (!reference || !surface || !geometry)
            {
                Debug.LogError("Sea study requires three renderers.", this);
                enabled = false;
                return;
            }

            Select(initialMode);
        }

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard == null)
                return;

            if (keyboard.digit1Key.wasPressedThisFrame)
                Select(Mode.Reference);
            else if (keyboard.digit2Key.wasPressedThisFrame)
                Select(Mode.Surface);
            else if (keyboard.digit3Key.wasPressedThisFrame)
                Select(Mode.Geometry);
#elif ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.Alpha1))
                Select(Mode.Reference);
            else if (Input.GetKeyDown(KeyCode.Alpha2))
                Select(Mode.Surface);
            else if (Input.GetKeyDown(KeyCode.Alpha3))
                Select(Mode.Geometry);
#endif
        }

        public void Select(Mode mode)
        {
            if (mode < Mode.Reference || mode > Mode.Geometry)
                throw new ArgumentOutOfRangeException(nameof(mode));

            reference.enabled = mode == Mode.Reference;
            surface.enabled = mode == Mode.Surface;
            geometry.enabled = mode == Mode.Geometry;
            CurrentMode = mode;
        }
    }
}
