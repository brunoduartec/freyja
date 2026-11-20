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

        [SerializeField] private Transform bendView;
        [SerializeField, Min(0.1f)] private float bendRadius = 10f;
        [System.Serializable]
        private struct RouteView
        {
            public Vector2 center;
            [Min(0.1f)] public float radius;
            public float heading;
        }

        [SerializeField] private RouteView[] routeViews = System.Array.Empty<RouteView>();
        private bool manuallyOrbited;

        private float yaw = DefaultYaw;
        [SerializeField] private float initialPitch = DefaultPitch;
        [SerializeField, Min(0f)] private float shoulderOffset;
        private float pitch = DefaultPitch;
        private float requestedYaw;
        private float requestedPitch;
        private float currentDistance = DefaultDistance;
        private Vector2 pendingLookInput;
        private bool hasPendingLookInput;
        private Vector3 lastFreeCameraPosition;
        private bool hasLastFreeCameraPosition;
        private bool reportedNoFreeInitialPosition;
        private bool conversation;
        private Vector3 conversationPosition;
        private Quaternion conversationRotation;
        private float conversationFov;
        private float explorationFov;

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

            if (!manuallyOrbited && delta.sqrMagnitude > 0f)
            {
                requestedYaw = yaw;
                manuallyOrbited = true;
            }
            pendingLookInput += delta;
            hasPendingLookInput = true;
        }

        public void ResetView(float yawDegrees)
        {
            manuallyOrbited = false;
            yaw = yawDegrees;
            requestedYaw = yawDegrees;
            pitch = Mathf.Clamp(initialPitch, minimumPitch, maximumPitch);
            requestedPitch = pitch;
            pendingLookInput = Vector2.zero;
            hasPendingLookInput = false;
            currentDistance = Mathf.Max(0f, distance);
            SnapToView();
        }

        internal void ShowConversation(Vector3 position, Quaternion rotation, float fov)
        {
            if (!conversation)
                explorationFov = ViewCamera.fieldOfView;
            conversationPosition = position;
            conversationRotation = rotation;
            conversationFov = fov;
            conversation = true;
        }

        internal void EndConversation()
        {
            if (!conversation) return;
            conversation = false;
            ViewCamera.fieldOfView = explorationFov;
            SnapToView();
        }

        public void Tick(float dt)
        {
            if (target == null)
                return;

            if (conversation)
            {
                float blend = 1f - Mathf.Exp(-14f * Mathf.Max(0f, dt));
                transform.SetPositionAndRotation(Vector3.Lerp(transform.position, conversationPosition, blend),
                    Quaternion.Slerp(transform.rotation, conversationRotation, blend));
                ViewCamera.fieldOfView = Mathf.Lerp(ViewCamera.fieldOfView, conversationFov, blend);
                return;
            }

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
            yaw = Mathf.LerpAngle(yaw, FramedYaw(), rotationBlend);
            pitch = Mathf.Lerp(pitch, requestedPitch, rotationBlend);

            Quaternion orbitRotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 pivot = GetPivot(orbitRotation);
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

        private Vector3 GetPivot(Quaternion rotation)
        {
            Vector3 pivot = target.position + Vector3.up * pivotHeight;
            if (shoulderOffset <= 0f)
                return pivot;

            Vector3 right = rotation * Vector3.right;
            float offset = shoulderOffset;
            if (Physics.SphereCast(pivot, GetProbeRadius(), right, out RaycastHit hit,
                    offset, obstacleMask, QueryTriggerInteraction.Ignore))
                offset = Mathf.Max(0f, hit.distance - collisionPadding);
            return pivot + right * offset;
        }

        private float FindSafeDistance(Vector3 pivot, Vector3 direction, float desiredDistance, float radius)
        {
            if (obstacleMask.value == 0 || desiredDistance <= 0f)
                return desiredDistance;

            if (Physics.SphereCast(
                    pivot, radius, direction, out RaycastHit hit, desiredDistance,
                    obstacleMask, QueryTriggerInteraction.Ignore))
            {
                desiredDistance = Mathf.Clamp(hit.distance - collisionPadding, 0f, desiredDistance);
            }

            // On steep slopes a clear pivot can still leave the torso behind a ridge.
            Vector3 torso = pivot - Vector3.up * Mathf.Min(0.4f, pivotHeight * 0.3f);
            if (Physics.SphereCast(torso, radius, direction, out hit, desiredDistance,
                    obstacleMask, QueryTriggerInteraction.Ignore))
                desiredDistance = Mathf.Max(0f, hit.distance - collisionPadding);

            if (shoulderOffset > 0f)
            {
                Vector3 chest = target.position + Vector3.up * Mathf.Max(0f, pivotHeight - 0.35f);
                Vector3 sightline = pivot + direction * desiredDistance - chest;
                if (Physics.SphereCast(chest, radius, sightline.normalized, out hit,
                        sightline.magnitude, obstacleMask, QueryTriggerInteraction.Ignore))
                    desiredDistance = Mathf.Clamp(Vector3.Dot(hit.point - pivot, direction)
                        - radius - collisionPadding, 0f, desiredDistance);
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

        private float FramedYaw()
        {
            if (manuallyOrbited)
                return requestedYaw;

            Vector2 position = new Vector2(target.position.x, target.position.z);
            float framed = requestedYaw;
            if (bendView != null)
            {
                Vector2 bend = new Vector2(bendView.position.x, bendView.position.z);
                float approach = 1f - Mathf.SmoothStep(0f, 1f,
                    Mathf.InverseLerp(65f, 110f, Mathf.Abs(Mathf.DeltaAngle(requestedYaw, 0f))));
                framed = Mathf.LerpAngle(requestedYaw, bendView.eulerAngles.y,
                    ViewWeight(position, bend, bendRadius) * approach);
            }

            float totalWeight = 0f;
            float correction = 0f;
            foreach (var view in routeViews)
            {
                float weight = ViewWeight(position, view.center, view.radius);
                if (weight <= 0f)
                    continue;
                float delta = Mathf.DeltaAngle(requestedYaw, view.heading);
                if (Mathf.Abs(delta) > 90f)
                    delta = Mathf.DeltaAngle(requestedYaw, view.heading + 180f);
                // At a crossing, keep the player's current view when the route runs across it.
                weight *= 1f - Mathf.InverseLerp(55f, 85f, Mathf.Abs(delta));
                correction += delta * weight;
                totalWeight += weight;
            }
            return framed + correction / Mathf.Max(1f, totalWeight);
        }

        private static float ViewWeight(Vector2 position, Vector2 center, float radius)
        {
            float distance = Vector2.Distance(position, center);
            return Mathf.SmoothStep(0f, 1f, 1f - distance / Mathf.Max(0.1f, radius));
        }

        private void SnapToView()
        {
            if (target == null)
                return;

            yaw = FramedYaw();
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 pivot = GetPivot(rotation);
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
