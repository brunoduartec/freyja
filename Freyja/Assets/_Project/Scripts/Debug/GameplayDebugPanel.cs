using Freyja.Environment;
using Freyja.Prototype;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Freyja.Diagnostics
{
    [DisallowMultipleComponent]
    public sealed class GameplayDebugPanel : MonoBehaviour
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        [SerializeField] private CoastalDayNight clock;
        [SerializeField] private CaisSessionController session;
        [SerializeField] private Camera gameplayCamera;
        [SerializeField] private Shader colliderShader;
        [SerializeField] private GameObject view;
        [SerializeField] private Text timeLabel;
        [SerializeField] private Slider timeSlider;
        [SerializeField] private Button noonButton;
        [SerializeField] private Button midnightButton;
        [SerializeField] private Button autoButton;
        [SerializeField] private Text autoLabel;
        [SerializeField] private Button colliderButton;
        [SerializeField] private Text colliderLabel;
        [SerializeField] private Button closeButton;

        private PhysicsDebugOverlay physicsOverlay;
        private bool panelOpen;
        private bool restoreCapture;
        private bool showColliders;

        private void OnEnable()
        {
            if (clock == null || session == null || gameplayCamera == null || colliderShader == null
                || view == null || timeLabel == null || timeSlider == null || noonButton == null
                || midnightButton == null || autoButton == null || autoLabel == null
                || colliderButton == null || colliderLabel == null || closeButton == null)
            {
                Debug.LogError("GameplayDebugPanel is missing scene or UI references.", this);
                enabled = false;
                return;
            }

            physicsOverlay = new PhysicsDebugOverlay(colliderShader);
            timeSlider.onValueChanged.AddListener(HoldAt);
            noonButton.onClick.AddListener(SelectNoon);
            midnightButton.onClick.AddListener(SelectMidnight);
            autoButton.onClick.AddListener(ToggleClock);
            colliderButton.onClick.AddListener(ToggleColliders);
            closeButton.onClick.AddListener(Close);
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && (keyboard.f3Key.wasPressedThisFrame || panelOpen && keyboard.escapeKey.wasPressedThisFrame))
                SetPanelOpen(!panelOpen);

            if (panelOpen)
                RefreshView();
        }

        private void SetPanelOpen(bool open)
        {
            if (open == panelOpen)
                return;

            if (open)
            {
                if (!session.IsInitialized || session.IsModalOpen)
                    return;
                restoreCapture = session.IsCaptured;
            }

            panelOpen = open;
            view.SetActive(open);
            session.SetModalOpen(open, restoreCapture);
        }

        private void SetColliderVisibility(bool visible)
        {
            if (visible == showColliders)
                return;

            showColliders = visible;
            if (visible)
                Camera.onPostRender += DrawColliders;
            else
                Camera.onPostRender -= DrawColliders;
        }

        private void DrawColliders(Camera camera)
        {
            if (camera == gameplayCamera)
                physicsOverlay.Draw(camera);
        }

        private void OnDisable()
        {
            SetColliderVisibility(false);
            SetPanelOpen(false);
            if (physicsOverlay == null)
                return;

            timeSlider.onValueChanged.RemoveListener(HoldAt);
            noonButton.onClick.RemoveListener(SelectNoon);
            midnightButton.onClick.RemoveListener(SelectMidnight);
            autoButton.onClick.RemoveListener(ToggleClock);
            colliderButton.onClick.RemoveListener(ToggleColliders);
            closeButton.onClick.RemoveListener(Close);
            physicsOverlay.Dispose();
            physicsOverlay = null;
        }

        private void RefreshView()
        {
            int minutes = Mathf.FloorToInt(clock.Hour * 60f);
            timeLabel.text = $"Hora da ilha   {minutes / 60:00}:{minutes % 60:00}";
            timeSlider.SetValueWithoutNotify(clock.Hour);
            autoLabel.text = clock.AdvancesAutomatically ? "Ciclo automático ligado" : "Ciclo automático pausado";
            colliderLabel.text = showColliders ? "Ocultar colisores" : "Mostrar colisores";
        }

        private void Close() => SetPanelOpen(false);
        private void SelectNoon() => HoldAt(12f);
        private void SelectMidnight() => HoldAt(0f);
        private void ToggleClock() => clock.AdvancesAutomatically = !clock.AdvancesAutomatically;
        private void ToggleColliders() => SetColliderVisibility(!showColliders);

        private void HoldAt(float hour)
        {
            clock.AdvancesAutomatically = false;
            clock.SetHour(hour);
        }
#endif
    }
}
