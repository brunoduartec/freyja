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
        private const float MinimumFramingDistance = 1.35f;
        private const float StairFollowSpeed = 9f;

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
        private bool recoveringOrbit;
        private float previousShoulderOffset;
        private CaisPlayerController followedPlayer;
        private float followedHeight;
        private bool smoothingStairHeight;
        private bool reportedNoFreeInitialPosition;
        private bool conversation;
        private Vector3 conversationPosition;
        private Quaternion conversationRotation;
        private float conversationFov;
        private float explorationFov;

        public Transform Target
        {
            get => target;
            set
            {
                target = value;
                followedPlayer = target != null ? target.GetComponent<CaisPlayerController>() : null;
            }
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

            Target = target;
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

            float stairBlend = 1f - Mathf.Exp(-StairFollowSpeed * safeDt);
            ResolveOrbit(rotationBlend, stairBlend, snap: false);
        }

        private void ResolveOrbit(float blend, float heightBlend, bool snap)
        {
            Quaternion orbit = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 center = target.position + Vector3.up * pivotHeight;
            bool onStairs = followedPlayer != null && followedPlayer.HasStairPose;
            if (snap || (!onStairs && !smoothingStairHeight))
                followedHeight = center.y;
            else
                followedHeight = Mathf.Lerp(followedHeight, center.y, heightBlend);
            smoothingStairHeight = onStairs || Mathf.Abs(center.y - followedHeight) > 0.005f;

            Vector3 orbitPivot = center;
            orbitPivot.y = followedHeight;
            Vector3 shoulder = onStairs ? orbitPivot : GetShoulderPivot(orbitPivot, orbit);
            float appliedShoulderOffset = Vector3.Distance(shoulder, orbitPivot);
            bool shoulderChanged = Mathf.Abs(appliedShoulderOffset - previousShoulderOffset) > 0.05f;
            previousShoulderOffset = appliedShoulderOffset;
            float desiredDistance = Mathf.Max(0f, distance);
            float minimumDistance = Mathf.Min(MinimumFramingDistance, desiredDistance);
            float radius = GetProbeRadius();

            float safeDistance = FindSafeDistance(shoulder, orbit * Vector3.back, desiredDistance, radius,
                appliedShoulderOffset > 0f);
            Vector3 position = shoulder + orbit * Vector3.back * safeDistance;
            Quaternion rotation = orbit;
            bool adjusted = safeDistance < minimumDistance || !HasClearView(position, radius)
                || !IsTargetInFrame(position, rotation);
            if (adjusted)
            {
                if (TryFindClearOrbit(center, orbit, desiredDistance, minimumDistance, radius, out Vector3 centered))
                {
                    position = centered;
                }
                else
                {
                    // Keep the logical yaw used by movement while lifting the view above the obstruction.
                    for (int elevation = 18; elevation <= 36; elevation += 18)
                    {
                        Quaternion raised = Quaternion.Euler(Mathf.Min(pitch + elevation, 80f), yaw, 0f);
                        if (TryFindClearOrbit(center, raised, desiredDistance, minimumDistance, radius, out Vector3 elevated))
                        {
                            position = elevated;
                            rotation = raised;
                            break;
                        }
                    }
                }

                if (!HasClearView(position, radius) && hasLastFreeCameraPosition &&
                    Vector3.Distance(lastFreeCameraPosition, center) <= desiredDistance &&
                    HasClearView(lastFreeCameraPosition, radius) && IsTargetInFrame(lastFreeCameraPosition, orbit))
                {
                    position = lastFreeCameraPosition;
                }
            }

            if (!IsPositionFree(position, radius))
            {
                if (!reportedNoFreeInitialPosition)
                {
                    Debug.LogWarning("CaisCameraRig could not find a free camera position.", this);
                    reportedNoFreeInitialPosition = true;
                }
                return;
            }

            if (!snap && (adjusted || recoveringOrbit || shoulderChanged))
            {
                Vector3 blendedPosition = Vector3.Lerp(transform.position, position, blend);
                Quaternion blendedRotation = Quaternion.Euler(
                    Mathf.LerpAngle(transform.eulerAngles.x, rotation.eulerAngles.x, blend), yaw, 0f);
                if (Vector3.Distance(blendedPosition, center) >= minimumDistance &&
                    HasClearView(blendedPosition, radius) && IsTargetInFrame(blendedPosition, blendedRotation))
                {
                    recoveringOrbit = adjusted || Vector3.SqrMagnitude(blendedPosition - position) > 0.0025f;
                    position = blendedPosition;
                    rotation = blendedRotation;
                }
                else
                {
                    recoveringOrbit = adjusted;
                }
            }
            else if (!snap && safeDistance > currentDistance)
            {
                // An obstruction shortens the orbit immediately. Restore distance gradually.
                float distanceToApply = Mathf.Max(minimumDistance, Mathf.Lerp(currentDistance, safeDistance, blend));
                Vector3 closerPosition = shoulder + orbit * Vector3.back * distanceToApply;
                if (HasClearView(closerPosition, radius) && IsTargetInFrame(closerPosition, orbit))
                    position = closerPosition;
            }

            transform.SetPositionAndRotation(position, rotation);
            currentDistance = Vector3.Distance(adjusted ? center : shoulder, position);
            if (HasClearView(position, radius))
            {
                lastFreeCameraPosition = position;
                hasLastFreeCameraPosition = true;
            }
        }

        private bool TryFindClearOrbit(Vector3 pivot, Quaternion rotation, float desiredDistance,
            float minimumDistance, float radius, out Vector3 position)
        {
            Vector3 back = rotation * Vector3.back;
            float safeDistance = FindSafeDistance(pivot, back, desiredDistance, radius, offsetShoulder: false);
            position = pivot + back * safeDistance;
            return safeDistance >= minimumDistance && HasClearView(position, radius) && IsTargetInFrame(position, rotation);
        }

        private Vector3 ChestPosition => target.position + Vector3.up * Mathf.Max(0f, pivotHeight - 0.35f);

        private bool HasClearView(Vector3 position, float radius)
        {
            return IsPositionFree(position, radius) && !Physics.Linecast(
                ChestPosition, position, obstacleMask, QueryTriggerInteraction.Ignore);
        }

        private bool IsTargetInFrame(Vector3 position, Quaternion rotation)
        {
            Camera cameraToUse = ViewCamera;
            if (cameraToUse == null || cameraToUse.orthographic)
                return true;

            Vector3 chest = Quaternion.Inverse(rotation) * (ChestPosition - position);
            float verticalLimit = chest.z * Mathf.Tan(cameraToUse.fieldOfView * 0.5f * Mathf.Deg2Rad);
            return chest.z > cameraToUse.nearClipPlane && Mathf.Abs(chest.y) < verticalLimit
                && Mathf.Abs(chest.x) < verticalLimit * cameraToUse.aspect;
        }

        private Vector3 GetShoulderPivot(Vector3 pivot, Quaternion rotation)
        {
            if (shoulderOffset <= 0f)
                return pivot;

            Vector3 right = rotation * Vector3.right;
            float offset = shoulderOffset;
            if (Physics.SphereCast(pivot, GetProbeRadius(), right, out RaycastHit hit,
                    offset, obstacleMask, QueryTriggerInteraction.Ignore))
                offset = Mathf.Max(0f, hit.distance - collisionPadding);
            return pivot + right * offset;
        }

        private float FindSafeDistance(Vector3 pivot, Vector3 direction, float desiredDistance, float radius, bool offsetShoulder)
        {
            if (obstacleMask.value == 0 || desiredDistance <= 0f)
                return desiredDistance;

            if (!IsPositionFree(pivot, radius))
                return 0f;

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

            if (offsetShoulder)
            {
                Vector3 chest = ChestPosition;
                Vector3 sightline = pivot + direction * desiredDistance - chest;
                if (Physics.SphereCast(chest, radius, sightline.normalized, out hit,
                        sightline.magnitude, obstacleMask, QueryTriggerInteraction.Ignore))
                    desiredDistance = Mathf.Clamp(Vector3.Dot(hit.point - pivot, direction)
                        - radius - collisionPadding, 0f, desiredDistance);
            }
            return desiredDistance;
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
            hasLastFreeCameraPosition = false;
            recoveringOrbit = false;
            reportedNoFreeInitialPosition = false;
            ResolveOrbit(1f, 1f, snap: true);
        }
    }
}
