using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Freyja.Prototype
{
    [DefaultExecutionOrder(-100)]
    [UnityEngine.Scripting.APIUpdating.MovedFrom("IlhaMisterio.Prototype")]
    public sealed class CaisSessionController : MonoBehaviour
    {
        [SerializeField] private CaisPlayerController player;
        [SerializeField] private CaisCameraRig cameraRig;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference lookAction;
        [SerializeField] private InputActionReference releaseAction;
        [SerializeField] private InputActionReference resumeAction;
        [SerializeField] private InputActionReference sprintAction;
        [SerializeField] private InputActionReference jumpAction;
        [SerializeField] private LayerMask solidMask;
        [SerializeField] private Text controlsText;
        [SerializeField] private string controlsTitle = "CAMINHAR PELO CAIS";
        [SerializeField] private float fallHeight = -0.5f;
        [SerializeField] private Bounds recoveryBounds = new Bounds(new Vector3(0, 4, -3), new Vector3(28, 20, 30));

        private InputAction move, look, release, resume, sprint, jump;
        private bool focused, captured, initialized, suppressFirstSample;
        private bool modalOpen;
        private Vector3 safePosition;
        private Quaternion safeRotation;
        public CaisCameraRig CameraRig => cameraRig;
        public bool IsCaptured => captured;
        public bool HasFocus => focused;
        public bool IsInitialized => initialized;
        public int RecoveryCount { get; private set; }

        private void OnEnable()
        {
            focused = Application.isFocused;
            captured = initialized = modalOpen = false;
            RecoveryCount = 0;
            if (player != null) player.ControlEnabled = false;
            if (cameraRig != null) cameraRig.ControlEnabled = false;
            if (moveAction == null || lookAction == null || releaseAction == null || resumeAction == null)
                return;
            move = moveAction.action.Clone();
            look = lookAction.action.Clone();
            release = releaseAction.action.Clone();
            resume = resumeAction.action.Clone();
            sprint = sprintAction != null ? sprintAction.action?.Clone() : null;
            jump = jumpAction != null ? jumpAction.action?.Clone() : null;
            move.Enable(); look.Enable(); release.Enable(); resume.Enable();
            sprint?.Enable();
            jump?.Enable();
        }

        private void Start()
        {
            if (player == null || cameraRig == null || spawnPoint == null || move == null)
            {
                Fail("Referencias obrigatorias do prototipo ausentes.");
                return;
            }
            player.CameraTransform = cameraRig.transform;
            cameraRig.Target = player.transform;
            if (!Physics.Raycast(spawnPoint.position + Vector3.up * 0.5f, Vector3.down,
                out RaycastHit ground, 2f, solidMask, QueryTriggerInteraction.Ignore))
            {
                Fail("SpawnPoint sem piso solido.");
                return;
            }
            safePosition = ground.point + Vector3.up * 0.035f;
            safeRotation = spawnPoint.rotation;
            var body = player.Body;
            if (body == null || !recoveryBounds.Contains(safePosition))
            {
                Fail("SpawnPoint ou capsula invalidos.");
                return;
            }
            float radius = Mathf.Max(0.01f, body.radius - body.skinWidth);
            Vector3 lower = safePosition + Vector3.up * (body.radius + 0.05f);
            Vector3 upper = safePosition + Vector3.up * (body.height - body.radius);
            if (Physics.CheckCapsule(lower, upper, radius, solidMask, QueryTriggerInteraction.Ignore))
            {
                Fail("SpawnPoint obstruido.");
                return;
            }
            initialized = true;
            player.ResetTo(safePosition, safeRotation);
            cameraRig.ResetView(safeRotation.eulerAngles.y);
            ReleaseControl();
        }

        private void Update()
        {
            if (!initialized || modalOpen) return;
            if (!focused)
            {
                ReleaseControl();
                return;
            }
            if (captured && (release.WasPressedThisFrame() || Cursor.lockState != CursorLockMode.Locked))
            {
                ReleaseControl();
                return;
            }
            if (!captured)
            {
                if (resume.WasPressedThisFrame()) CaptureControl();
                return;
            }
            if (suppressFirstSample)
            {
                suppressFirstSample = false;
                player.SetMoveInput(Vector2.zero);
                cameraRig.SetLookInput(Vector2.zero);
                return;
            }
            player.SetMoveInput(move.ReadValue<Vector2>());
            player.SetTraversalInput(sprint?.IsPressed() == true, jump?.WasPressedThisFrame() == true);
            cameraRig.SetLookInput(look.ReadValue<Vector2>());
        }

        private void LateUpdate()
        {
            if (initialized && (player.transform.position.y < fallHeight || !recoveryBounds.Contains(player.transform.position)))
                Recover();
        }

        public void CaptureControl()
        {
            if (!initialized || !focused || modalOpen) return;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            captured = true;
            suppressFirstSample = true;
            player.ControlEnabled = cameraRig.ControlEnabled = true;
            ShowInstructions();
        }

        public void SetModalOpen(bool open)
        {
            if (modalOpen == open) return;
            modalOpen = open;
            if (open) ReleaseControl();
            else CaptureControl();
        }

        public void ReleaseControl()
        {
            captured = false;
            suppressFirstSample = true;
            if (player != null)
            {
                player.ControlEnabled = false;
                player.SetMoveInput(Vector2.zero);
                player.SetTraversalInput(false, false);
            }
            if (cameraRig != null) { cameraRig.ControlEnabled = false; cameraRig.SetLookInput(Vector2.zero); }
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            ShowInstructions();
        }

        public void Recover()
        {
            if (!initialized) return;
            bool resumeControl = captured && focused && Cursor.lockState == CursorLockMode.Locked;
            player.ResetTo(safePosition, safeRotation);
            cameraRig.ResetView(safeRotation.eulerAngles.y);
            RecoveryCount++;
            suppressFirstSample = true;
            if (!resumeControl) ReleaseControl();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            focused = hasFocus;
            if (!hasFocus) ReleaseControl();
        }

        private void Fail(string message)
        {
            initialized = false;
            ReleaseControl();
            if (controlsText != null) controlsText.text = "Cena indisponivel: " + message;
            Debug.LogError(message, this);
        }

        private void ShowInstructions()
        {
            if (controlsText == null) return;
            string traversal = (sprint != null ? "   |   Shift esquerdo  correr" : "")
                + (jump != null ? "   |   Espaço  pular" : "");
            string instructions = "\nWASD  caminhar   |   Mouse  olhar   |   Esc  liberar cursor" + traversal;
            controlsText.text = modalOpen ? string.Empty
                : controlsTitle + (captured ? "" : "\nClique para controlar") + instructions;
        }

        private void OnDisable()
        {
            initialized = false;
            ReleaseControl();
            move?.Dispose(); look?.Dispose(); release?.Dispose(); resume?.Dispose();
            sprint?.Dispose();
            jump?.Dispose();
            move = look = release = resume = null;
            sprint = jump = null;
        }
    }
}
