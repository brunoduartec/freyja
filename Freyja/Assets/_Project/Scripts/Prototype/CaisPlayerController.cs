using UnityEngine;

namespace Freyja.Prototype
{
    [RequireComponent(typeof(CharacterController))]
    [UnityEngine.Scripting.APIUpdating.MovedFrom("IlhaMisterio.Prototype")]
    public sealed class CaisPlayerController : MonoBehaviour
    {
        private const float Gravity = -18f;
        private const float GroundAdhesionVelocity = -2f;
        private const float StairContactBias = 0.5f;

        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField, Min(1f)] private float runSpeedMultiplier = 1.8f;
        [SerializeField, Min(0.01f)] private float jumpHeight = 0.55f;
        [SerializeField] private float rotationSpeed = 540f;
        [SerializeField] private StairFootPlacement stairPlacement;
        [SerializeField] private MeshCollider[] stairRamps = System.Array.Empty<MeshCollider>();

        private CharacterController body;
        private Vector2 moveInput;
        private Vector3 planarVelocity;
        private Vector3 stairGroundNormal;
        private float verticalVelocity;
        private bool missingCameraReported;
        private bool runHeld;
        private bool jumpPressed;

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
        public float GroundSpeed { get; private set; }
        public bool HasMoveIntent { get; private set; }
        internal bool IsOnStairRamp => body != null && body.isGrounded && stairGroundNormal.y > 0f;
        internal bool IsOnStairs => IsOnStairRamp
            || (stairPlacement != null && stairPlacement.TryGetStairAnimationTime(out _, out _));
        public bool IsRunning => GroundSpeed > moveSpeed * 1.05f && runHeld;
        public bool IsJumping { get; private set; }
        public float VerticalVelocity => verticalVelocity;
        public CharacterController Body => body;

        internal bool IsNearStairRamp()
        {
            if (body == null)
                return false;

            float reachSquared = body.radius * body.radius;
            foreach (MeshCollider ramp in stairRamps)
            {
                if (ramp != null && ramp.enabled && ramp.bounds.SqrDistance(transform.position) <= reachSquared)
                    return true;
            }
            return false;
        }

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

        public void SetTraversalInput(bool run, bool jump)
        {
            runHeld = run;
            jumpPressed = jump;
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
            stairGroundNormal = Vector3.zero;
            verticalVelocity = 0f;
            GroundSpeed = 0f;
            HasMoveIntent = false;
            IsJumping = false;
            SetTraversalInput(false, false);
            body.enabled = wasEnabled;
        }

        public void Tick(float deltaTime)
        {
            GroundSpeed = 0f;
            HasMoveIntent = false;
            if (body == null || !body.enabled || deltaTime <= 0f)
                return;

            bool startJump = ControlEnabled && jumpPressed && body.isGrounded;
            jumpPressed = false;
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
                    float speed = moveSpeed * (runHeld ? runSpeedMultiplier : 1f);
                    desiredPlanarVelocity = Vector3.ClampMagnitude(direction, 1f) * speed;
                }
            }

            planarVelocity = stairPlacement != null
                ? stairPlacement.FilterGroundVelocity(desiredPlanarVelocity, !IsJumping && !startJump)
                : desiredPlanarVelocity;
            HasMoveIntent = planarVelocity.sqrMagnitude > 0.0001f;
            if (HasMoveIntent)
            {
                Quaternion targetRotation = Quaternion.LookRotation(planarVelocity, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * deltaTime);
            }

            if (startJump)
            {
                verticalVelocity = Mathf.Sqrt(-2f * Gravity * jumpHeight);
                IsJumping = true;
            }
            else if (body.isGrounded && verticalVelocity < 0f)
                verticalVelocity = GroundAdhesionVelocity;
            else
                verticalVelocity += Gravity * deltaTime;

            float moveVertical = verticalVelocity;
            if (body.isGrounded && !IsJumping && planarVelocity.sqrMagnitude > 0f)
            {
                // Use the ramp face ahead of the capsule. At a landing edge, Move
                // can report the rounded contact instead of the slope normal.
                Vector3 normal = FindStairRampAhead(planarVelocity * deltaTime);
                if (normal.y <= 0f)
                    normal = stairGroundNormal;
                if (normal.y > 0f)
                {
                    float rampVertical = -Vector3.Dot(normal, planarVelocity) / normal.y;
                    moveVertical = Mathf.Min(moveVertical, rampVertical - StairContactBias);
                }
            }

            Vector3 displacement = (planarVelocity + Vector3.up * moveVertical) * deltaTime;
            Vector3 previousPosition = transform.position;
            stairGroundNormal = Vector3.zero;
            CollisionFlags collisions = body.Move(displacement);
            if ((collisions & CollisionFlags.Above) != 0 && verticalVelocity > 0f)
                verticalVelocity = 0f;

            if (body.isGrounded && planarVelocity.sqrMagnitude > 0f)
            {
                Vector3 travel = transform.position - previousPosition;
                travel.y = 0f;
                GroundSpeed = travel.magnitude / deltaTime;
            }

            if (body.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = GroundAdhesionVelocity;
                IsJumping = false;
            }
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (hit.collider is MeshCollider ramp && System.Array.IndexOf(stairRamps, ramp) >= 0
                && hit.normal.y >= Mathf.Cos(body.slopeLimit * Mathf.Deg2Rad)
                && hit.point.y <= transform.position.y + body.radius)
            {
                stairGroundNormal = hit.normal;
            }
        }

        private Vector3 FindStairRampAhead(Vector3 displacement)
        {
            // The surface under the center can be lower than the rounded capsule's contact.
            float reach = displacement.magnitude * Mathf.Tan(body.slopeLimit * Mathf.Deg2Rad)
                + body.radius + body.skinWidth;
            Vector3 foot = transform.position + displacement;
            var ray = new Ray(foot + Vector3.up * body.skinWidth, Vector3.down);
            foreach (MeshCollider ramp in stairRamps)
            {
                if (ramp == null || !ramp.enabled || !ramp.gameObject.activeInHierarchy)
                    continue;

                Bounds bounds = ramp.bounds;
                bounds.Expand(Vector3.up * (reach + body.skinWidth) * 2f);
                if (bounds.Contains(ray.origin) && ramp.Raycast(ray, out RaycastHit hit, reach + body.skinWidth)
                    && hit.normal.y >= Mathf.Cos(body.slopeLimit * Mathf.Deg2Rad))
                    return hit.normal;
            }
            return Vector3.zero;
        }

        private void Update()
        {
            Tick(Time.deltaTime);
        }

        private void OnDisable()
        {
            GroundSpeed = 0f;
            moveInput = Vector2.zero;
            planarVelocity = Vector3.zero;
            stairGroundNormal = Vector3.zero;
            verticalVelocity = 0f;
            IsJumping = false;
            SetTraversalInput(false, false);
        }

        private void ReportMissingCamera()
        {
            missingCameraReported = true;
            Debug.LogError("CaisPlayerController requires a camera transform for camera-relative movement.", this);
        }
    }
}
