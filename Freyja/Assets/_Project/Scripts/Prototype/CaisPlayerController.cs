using UnityEngine;

namespace Freyja.Prototype
{
    [RequireComponent(typeof(CharacterController))]
    [UnityEngine.Scripting.APIUpdating.MovedFrom("IlhaMisterio.Prototype")]
    public sealed class CaisPlayerController : MonoBehaviour
    {
        private const float Gravity = -18f;
        private const float GroundAdhesionVelocity = -2f;

        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float rotationSpeed = 540f;

        private CharacterController body;
        private Vector2 moveInput;
        private Vector3 planarVelocity;
        private float verticalVelocity;
        private bool missingCameraReported;

        public Transform CameraTransform
        {
            get => cameraTransform;
            set => cameraTransform = value;
        }

        public bool ControlEnabled { get; set; } = true;
        public float MoveSpeed
        {
            get => moveSpeed;
            set => moveSpeed = Mathf.Max(0f, value);
        }
        public Vector3 PlanarVelocity => planarVelocity;
        public float VerticalVelocity => verticalVelocity;
        public CharacterController Body => body;

        private void Awake()
        {
            body = GetComponent<CharacterController>();
            body.center = new Vector3(body.center.x, 0.9f, body.center.z);

            if (cameraTransform == null)
                ReportMissingCamera();
        }

        public void SetMoveInput(Vector2 input)
        {
            moveInput = Vector2.ClampMagnitude(input, 1f);
        }

        public void ResetTo(Vector3 feetPosition, Quaternion rotation)
        {
            if (body == null)
                body = GetComponent<CharacterController>();

            bool wasEnabled = body.enabled;
            body.enabled = false;
            transform.SetPositionAndRotation(feetPosition, rotation);
            moveInput = Vector2.zero;
            planarVelocity = Vector3.zero;
            verticalVelocity = 0f;
            body.enabled = wasEnabled;
        }

        public void Tick(float deltaTime)
        {
            if (body == null || !body.enabled || deltaTime <= 0f)
                return;

            Vector3 desiredPlanarVelocity = Vector3.zero;
            if (ControlEnabled && moveInput.sqrMagnitude > 0f)
            {
                if (cameraTransform == null)
                {
                    if (!missingCameraReported)
                        ReportMissingCamera();
                }
                else
                {
                    Vector3 forward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
                    Vector3 right = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
                    Vector2 clampedInput = Vector2.ClampMagnitude(moveInput, 1f);
                    Vector3 direction = forward * clampedInput.y + right * clampedInput.x;
                    desiredPlanarVelocity = Vector3.ClampMagnitude(direction, 1f) * moveSpeed;
                }
            }

            planarVelocity = desiredPlanarVelocity;
            if (planarVelocity.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(planarVelocity, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * deltaTime);
            }

            if (body.isGrounded && verticalVelocity < 0f)
                verticalVelocity = GroundAdhesionVelocity;
            else
                verticalVelocity += Gravity * deltaTime;

            Vector3 displacement = (planarVelocity + Vector3.up * verticalVelocity) * deltaTime;
            body.Move(displacement);

            if (body.isGrounded && verticalVelocity < 0f)
                verticalVelocity = GroundAdhesionVelocity;
        }

        private void Update()
        {
            Tick(Time.deltaTime);
        }

        private void OnDisable()
        {
            moveInput = Vector2.zero;
            planarVelocity = Vector3.zero;
        }

        private void ReportMissingCamera()
        {
            missingCameraReported = true;
            Debug.LogError("CaisPlayerController requires a camera transform for camera-relative movement.", this);
        }
    }
}
