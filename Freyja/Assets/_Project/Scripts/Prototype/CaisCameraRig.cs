using UnityEngine;

namespace Freyja.Prototype
{
    [DisallowMultipleComponent]
    [UnityEngine.Scripting.APIUpdating.MovedFrom("IlhaMisterio.Prototype")]
    public sealed class CaisCameraRig : MonoBehaviour
    {
        private const float DefaultDistance = 4f;
        private const float DefaultPivotHeight = 1.35f;
        private const float DefaultPitch = 18f;
        private const float DefaultYaw = 180f;

        [SerializeField] private Transform target;
        [SerializeField] private Camera viewCamera;
        [SerializeField] private LayerMask obstacleMask;
        [SerializeField, Min(0f)] private float distance = DefaultDistance;
        [SerializeField, Min(0f)] private float pivotHeight = DefaultPivotHeight;
        [SerializeField, Min(0f)] private float lookSensitivity = 0.13f;
        [SerializeField, Min(0.01f)] private float returnSpeed = 12f;
        [SerializeField, Min(0f)] private float collisionPadding = 0.03f;
        [SerializeField, Min(0.01f)] private float fallbackProbeRadius = 0.18f;
        [SerializeField] private float minimumPitch = -20f;
        [SerializeField] private float maximumPitch = 65f;
        [SerializeField] private bool controlEnabled;

        private float yaw = DefaultYaw;
        private float pitch = DefaultPitch;
        private float requestedYaw;
        private float requestedPitch;
        private float currentDistance = DefaultDistance;
        private Vector2 pendingLookInput;
        private bool hasPendingLookInput;
        private Vector3 lastFreeCameraPosition;
        private bool hasLastFreeCameraPosition;
        private bool reportedNoFreeInitialPosition;

        public Transform Target
        {
            get => target;
            set => target = value;
        }

        public LayerMask ObstacleMask
        {
            get => obstacleMask;
            set => obstacleMask = value;
        }

        public Camera ViewCamera
        {
            get => viewCamera != null ? viewCamera : GetComponent<Camera>();
            set => viewCamera = value;
        }

        public bool ControlEnabled
        {
            get => controlEnabled;
            set => controlEnabled = value;
        }

        public float Yaw => yaw;
        public float Pitch => pitch;

        private void Awake()
        {
            if (viewCamera == null)
                viewCamera = GetComponent<Camera>();

            ResetView(yaw);
        }

        private void LateUpdate()
        {
            Tick(Time.deltaTime);
        }

        public void SetLookInput(Vector2 delta)
        {
            if (!controlEnabled)
                return;

            pendingLookInput += delta;
            hasPendingLookInput = true;
        }

        public void ResetView(float yawDegrees)
        {
            yaw = yawDegrees;
            requestedYaw = yawDegrees;
            pitch = Mathf.Clamp(DefaultPitch, minimumPitch, maximumPitch);
            requestedPitch = pitch;
            pendingLookInput = Vector2.zero;
            hasPendingLookInput = false;
            currentDistance = Mathf.Max(0f, distance);
            SnapToView();
        }

        public void Tick(float dt)
        {
            if (target == null)
                return;

            if (controlEnabled && hasPendingLookInput)
            {
                requestedYaw += pendingLookInput.x * lookSensitivity;
                requestedPitch = Mathf.Clamp(
                    requestedPitch - pendingLookInput.y * lookSensitivity,
                    minimumPitch,
                    maximumPitch);
            }

            // Input is a mouse displacement and is consumed once, independent of dt.
            pendingLookInput = Vector2.zero;
            hasPendingLookInput = false;

            float safeDt = Mathf.Max(0f, dt);
            float rotationBlend = returnSpeed <= 0f ? 1f : 1f - Mathf.Exp(-returnSpeed * safeDt);
            yaw = Mathf.LerpAngle(yaw, requestedYaw, rotationBlend);
            pitch = Mathf.Lerp(pitch, requestedPitch, rotationBlend);

            Quaternion orbitRotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 pivot = target.position + Vector3.up * pivotHeight;
            Vector3 back = orbitRotation * Vector3.back;
            float desiredDistance = Mathf.Max(0f, distance);
            float probeRadius = GetProbeRadius();

            if (obstacleMask.value != 0 && Physics.CheckSphere(
                    pivot, probeRadius, obstacleMask, QueryTriggerInteraction.Ignore))
            {
                if (!hasLastFreeCameraPosition)
                {
                    if (TryFindInitialFreePosition(pivot, orbitRotation, back, desiredDistance, probeRadius, out Vector3 freePosition))
                    {
                        lastFreeCameraPosition = freePosition;
                        hasLastFreeCameraPosition = true;
                        currentDistance = Vector3.Distance(pivot, freePosition);
                    }
                    else if (!reportedNoFreeInitialPosition)
                    {
                        Debug.LogWarning("CaisCameraRig could not find a free initial camera position; keeping its current position.", this);
                        reportedNoFreeInitialPosition = true;
                    }
                }

                if (hasLastFreeCameraPosition)
                    transform.SetPositionAndRotation(lastFreeCameraPosition, orbitRotation);
                return;
            }

            float safeDistance = FindSafeDistance(pivot, back, desiredDistance, probeRadius);

            // Collision response is immediate; only the return to the requested distance is eased.
            if (safeDistance < currentDistance)
                currentDistance = safeDistance;
            else
                currentDistance = Mathf.Lerp(currentDistance, safeDistance, rotationBlend);

            transform.SetPositionAndRotation(pivot + back * currentDistance, orbitRotation);
            if (IsPositionFree(transform.position, probeRadius))
            {
                lastFreeCameraPosition = transform.position;
                hasLastFreeCameraPosition = true;
            }
        }

        private float FindSafeDistance(Vector3 pivot, Vector3 direction, float desiredDistance, float radius)
        {
            if (obstacleMask.value == 0 || desiredDistance <= 0f)
                return desiredDistance;

            if (Physics.SphereCast(
                    pivot, radius, direction, out RaycastHit hit, desiredDistance,
                    obstacleMask, QueryTriggerInteraction.Ignore))
            {
                return Mathf.Clamp(hit.distance - collisionPadding, 0f, desiredDistance);
            }

            return desiredDistance;
        }

        private bool TryFindInitialFreePosition(
            Vector3 pivot, Quaternion orbitRotation, Vector3 back, float desiredDistance, float radius,
            out Vector3 freePosition)
        {
            // This only runs when the initial pivot is obstructed and no previous free point exists.
            float[] elevations = { 0f, 15f, 30f, 45f, 60f, 75f };
            float[] distanceFractions = { 1f, 0.75f, 0.5f, 0.25f };
            Vector3 right = orbitRotation * Vector3.right;

            for (int elevationIndex = 0; elevationIndex < elevations.Length; elevationIndex++)
            {
                Vector3 direction = Quaternion.AngleAxis(elevations[elevationIndex], right) * back;
                for (int distanceIndex = 0; distanceIndex < distanceFractions.Length; distanceIndex++)
                {
                    float candidateDistance = desiredDistance * distanceFractions[distanceIndex];
                    Vector3 candidate = pivot + direction * candidateDistance;
                    if (IsPositionFree(candidate, radius))
                    {
                        freePosition = candidate;
                        return true;
                    }
                }
            }

            freePosition = default;
            return false;
        }

        private bool IsPositionFree(Vector3 position, float radius)
        {
            return obstacleMask.value == 0 || !Physics.CheckSphere(
                position, radius, obstacleMask, QueryTriggerInteraction.Ignore);
        }

        private float GetProbeRadius()
        {
            Camera cameraToUse = ViewCamera;
            if (cameraToUse == null)
                return fallbackProbeRadius;

            float near = cameraToUse.nearClipPlane;
            float halfHeight = near * Mathf.Tan(cameraToUse.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float halfWidth = halfHeight * cameraToUse.aspect;
            float nearClipRadius = Mathf.Sqrt(near * near + halfHeight * halfHeight + halfWidth * halfWidth);
            return Mathf.Max(fallbackProbeRadius, nearClipRadius);
        }

        private void SnapToView()
        {
            if (target == null)
                return;

            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 pivot = target.position + Vector3.up * pivotHeight;
            Vector3 back = rotation * Vector3.back;
            float desiredDistance = Mathf.Max(0f, distance);
            float radius = GetProbeRadius();

            if (obstacleMask.value != 0 && Physics.CheckSphere(
                    pivot, radius, obstacleMask, QueryTriggerInteraction.Ignore))
            {
                if (TryFindInitialFreePosition(pivot, rotation, back, desiredDistance, radius, out Vector3 freePosition))
                {
                    lastFreeCameraPosition = freePosition;
                    hasLastFreeCameraPosition = true;
                    currentDistance = Vector3.Distance(pivot, freePosition);
                    transform.SetPositionAndRotation(freePosition, rotation);
                }
                else if (!reportedNoFreeInitialPosition)
                {
                    Debug.LogWarning("CaisCameraRig could not find a free initial camera position; keeping its current position.", this);
                    reportedNoFreeInitialPosition = true;
                }
                return;
            }

            currentDistance = FindSafeDistance(pivot, back, desiredDistance, radius);
            transform.SetPositionAndRotation(pivot + back * currentDistance, rotation);
            if (IsPositionFree(transform.position, radius))
            {
                lastFreeCameraPosition = transform.position;
                hasLastFreeCameraPosition = true;
            }
        }
    }
}
