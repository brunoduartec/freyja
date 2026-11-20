using UnityEngine;

namespace Freyja.Prototype
{
    [RequireComponent(typeof(Animator))]
    public sealed class StairFootPlacement : MonoBehaviour
    {
        private const float AnkleClearance = 0.16f;
        private const float AnimatedStanceHeight = 0.28f;
        private const float MaximumFootCorrection = 0.34f;
        private const float ProbeHeight = 0.55f;
        private const float ProbeDepth = 0.9f;
        private const float AscentSpeed = 1.25f;
        private const float DescentStepsPerSecond = 3f;
        private const float DescentContactLead = 0.08f;
        private const float DescentExitDistance = 0.27f;
        private const float MinimumLongitudinalFraction = 0.15f;
        private const float TouchdownLead = 0.22f;
        private const float FirstSwingDistance = 0.30f;
        private const float FootSpacing = 0.13f;
        private const float AnchorLaneTolerance = 0.02f;
        private const float ToeReach = 0.155f;
        private const float HeelReach = 0.085f;
        private const float EdgeClearance = 0.025f;

        // The same authored step is sampled between consecutive tread contacts.
        private static readonly AnimationCurve StepTravel = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        private static readonly AnimationCurve StepLift = new AnimationCurve(
            new Keyframe(0f, 0f, 0f, 0f), new Keyframe(0.36f, 0.165f, 0f, 0f),
            new Keyframe(0.63f, 0.115f, -0.42f, -0.42f), new Keyframe(1f, 0f, 0f, 0f));
        private static readonly AnimationCurve ShoePitch = new AnimationCurve(
            new Keyframe(0f, 0f), new Keyframe(0.24f, 12f), new Keyframe(0.61f, -9f), new Keyframe(1f, 0f));
        private const float PelvisDrop = 0.13f;

        [SerializeField] private Transform[] stairFlights = System.Array.Empty<Transform>();
        [SerializeField] private CaisPlayerController player;

        private Animator animator;
        private Transform leftHip;
        private Transform rightHip;
        private float leftLegLength;
        private float rightLegLength;
        private BoxCollider[][] flightTreads;
        private BoxCollider[] flightLandings;
        private Bounds[] flightBounds;
        private Vector3[] ascentDirections;
        private float leftWeight;
        private float rightWeight;
        private float stairBlend;
        private float[][] ascentContacts;
        private float[][] descentContacts;
        private int plantedFlight = -1;
        private bool anchorsValid;
        private float anchorLane;
        private Pose[] leftAnchors;
        private Pose[] rightAnchors;
        private Quaternion leftShoeTarget;
        private Quaternion rightShoeTarget;
        private int shoePoseFrame = -1;
        private Transform leftAnkle;
        private Transform rightAnkle;
        private Quaternion leftRestRotation;
        private Quaternion rightRestRotation;
        private float leftSoleHeight;
        private float rightSoleHeight;
        private int activeFlight = -1;
        private bool descending;
        private bool plantedDescending;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            leftAnkle = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            rightAnkle = animator.GetBoneTransform(HumanBodyBones.RightFoot);
            Quaternion inverseFacing = Quaternion.Inverse(player.transform.rotation);
            leftRestRotation = inverseFacing * leftAnkle.rotation;
            rightRestRotation = inverseFacing * rightAnkle.rotation;
            leftSoleHeight = Vector3.Dot(leftAnkle.position - animator.transform.position, Vector3.up);
            rightSoleHeight = Vector3.Dot(rightAnkle.position - animator.transform.position, Vector3.up);
            leftHip = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
            rightHip = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
            leftLegLength = GetLegLength(leftHip, HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftFoot);
            rightLegLength = GetLegLength(rightHip, HumanBodyBones.RightLowerLeg, HumanBodyBones.RightFoot);
            flightTreads = new BoxCollider[stairFlights.Length][];
            flightLandings = new BoxCollider[stairFlights.Length];
            flightBounds = new Bounds[stairFlights.Length];
            ascentDirections = new Vector3[stairFlights.Length];
            ascentContacts = new float[stairFlights.Length][];
            descentContacts = new float[stairFlights.Length][];
            int maximumTreads = 0;
            for (int i = 0; i < stairFlights.Length; i++)
            {
                BoxCollider[] treads = stairFlights[i].GetComponentsInChildren<BoxCollider>();
                flightTreads[i] = treads;
                if (treads.Length == 0)
                    continue;

                maximumTreads = Mathf.Max(maximumTreads, treads.Length);
                System.Array.Sort(treads, (a, b) => a.bounds.max.y.CompareTo(b.bounds.max.y));
                Bounds bounds = treads[0].bounds;
                foreach (BoxCollider tread in treads)
                {
                    bounds.Encapsulate(tread.bounds);
                    if (tread.name == "Patamar")
                        flightLandings[i] = tread;
                }
                flightBounds[i] = bounds;
                Vector3 direction = Vector3.ProjectOnPlane(treads[treads.Length - 1].bounds.center - treads[0].bounds.center,
                    Vector3.up).normalized;
                ascentDirections[i] = direction;
                ascentContacts[i] = new float[treads.Length];
                descentContacts[i] = new float[treads.Length + 1];
                for (int step = 0; step < treads.Length; step++)
                {
                    BoxCollider tread = treads[step];
                    Vector3 halfSize = tread.size * 0.5f;
                    float depth = Mathf.Abs(Vector3.Dot(tread.transform.TransformVector(Vector3.right * halfSize.x), direction))
                        + Mathf.Abs(Vector3.Dot(tread.transform.TransformVector(Vector3.forward * halfSize.z), direction));
                    ascentContacts[i][step] = Vector3.Dot(tread.bounds.center, direction) - depth - TouchdownLead;
                    descentContacts[i][treads.Length - 1 - step] =
                        Vector3.Dot(tread.bounds.center, -direction) - DescentContactLead;
                }
                descentContacts[i][treads.Length] = descentContacts[i][treads.Length - 1] + DescentExitDistance;
            }
            leftAnchors = new Pose[maximumTreads + 3];
            rightAnchors = new Pose[maximumTreads + 3];
        }

        public Vector3 FilterGroundVelocity(Vector3 velocity, bool allowStairMovement)
        {
            activeFlight = -1;
            if (!allowStairMovement || velocity.sqrMagnitude < 0.0001f)
                return velocity;

            int flight = FindNearbyFlight(requireGrounded: false);
            if (flight < 0 || flightTreads[flight].Length < 2)
                return velocity;

            Vector3 position = player.transform.position;
            Vector3 uphill = ascentDirections[flight];
            bool overTread = TryGetTreadHeight(flight, position, out float treadHeight, out BoxCollider surface);
            // Step offset can raise the capsule before its center crosses the riser.
            if (overTread && surface != flightLandings[flight]
                && Mathf.Abs(position.y - treadHeight) <= player.Body.stepOffset)
            {
                float longitudinalSpeed = Vector3.Dot(velocity, uphill);
                if (Mathf.Abs(longitudinalSpeed) < velocity.magnitude * MinimumLongitudinalFraction)
                    return Vector3.zero;

                velocity = uphill * longitudinalSpeed;
            }

            Vector3 direction = velocity.normalized;
            float alignment = Vector3.Dot(direction, uphill);
            if (alignment < -0.55f && (player.Body.isGrounded || flight == plantedFlight))
            {
                float[] contacts = descentContacts[flight];
                float advance = Vector3.Dot(position, -uphill);
                if (advance >= contacts[0] - FirstSwingDistance && advance <= contacts[contacts.Length - 1]
                    && (overTread || flight == plantedFlight
                        || TryGetTreadHeight(flight, position - uphill * FirstSwingDistance, out _, out _)))
                {
                    activeFlight = flight;
                    descending = true;
                    float stride = (contacts[contacts.Length - 2] - contacts[0]) / (contacts.Length - 2);
                    return -uphill * Mathf.Min(Vector3.Dot(velocity, -uphill), stride * DescentStepsPerSecond);
                }
            }

            if ((player.Body.isGrounded || flight == plantedFlight)
                && velocity.sqrMagnitude >= 0.01f && alignment > 0.55f
                && (TryGetTreadHeight(flight, position + direction * (FirstSwingDistance + TouchdownLead), out _, out _)
                    || overTread))
            {
                activeFlight = flight;
                descending = false;
                return Vector3.ClampMagnitude(velocity, AscentSpeed);
            }

            return velocity;
        }

        public bool TryGetStairAnimationTime(out float normalizedTime, out bool goingDown)
        {
            normalizedTime = 0f;
            int flight = activeFlight >= 0 ? activeFlight : plantedFlight;
            goingDown = activeFlight >= 0 ? descending : plantedDescending;
            if (flight < 0 || flight != FindNearbyFlight() ||
                (activeFlight < 0 && !IsHoldingStairPose(flight)))
                return false;

            float position = Vector3.Dot(player.transform.position, GetStairDirection(flight, goingDown));
            float[] contacts = goingDown ? descentContacts[flight] : ascentContacts[flight];
            if (position < contacts[0] - FirstSwingDistance || position > contacts[contacts.Length - 1] + 0.24f)
                return false;

            GetStepPhase(position, contacts, out int nextStep, out float phase);
            normalizedTime = ((nextStep & 1) + (nextStep == contacts.Length ? 0f : phase)) * 0.5f;
            return true;
        }

        private Vector3 GetStairDirection(int flight, bool goingDown)
        {
            return goingDown ? -ascentDirections[flight] : ascentDirections[flight];
        }

        private bool IsHoldingStairPose(int nearbyFlight)
        {
            return anchorsValid && nearbyFlight == plantedFlight &&
                !player.HasMoveIntent && !player.IsJumping;
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (layerIndex != 0)
                return;

            int nearbyFlight = FindNearbyFlight();
            Vector3 position = player.transform.position;
            bool climbing = activeFlight >= 0 && activeFlight == nearbyFlight;
            bool holding = IsHoldingStairPose(nearbyFlight);
            if (climbing)
            {
                float lane = Vector3.Dot(position, Vector3.Cross(Vector3.up, GetStairDirection(nearbyFlight, descending)));
                if (!anchorsValid || plantedFlight != nearbyFlight
                    || plantedDescending != descending || Mathf.Abs(lane - anchorLane) > AnchorLaneTolerance)
                {
                    plantedFlight = nearbyFlight;
                    plantedDescending = descending;
                    anchorsValid = true;
                    anchorLane = lane;
                    int contactCount = descending ? descentContacts[nearbyFlight].Length : ascentContacts[nearbyFlight].Length;
                    for (int step = -2; step < contactCount; step++)
                    {
                        leftAnchors[step + 2] = CalculateFootAnchor(nearbyFlight, step, lane - FootSpacing);
                        rightAnchors[step + 2] = CalculateFootAnchor(nearbyFlight, step, lane + FootSpacing);
                    }
                }
            }

            if (climbing || holding)
            {
                float advance = Vector3.Dot(position, GetStairDirection(plantedFlight, plantedDescending));
                float[] contacts = plantedDescending ? descentContacts[plantedFlight] : ascentContacts[plantedFlight];
                float firstContact = contacts[0];
                float lastContact = contacts[contacts.Length - 1];
                // Finish the walk-to-stair blend by the first touchdown at any walking speed.
                stairBlend = Mathf.SmoothStep(0f, 1f,
                    Mathf.InverseLerp(firstContact - FirstSwingDistance, firstContact, advance));
                stairBlend *= 1f - Mathf.SmoothStep(0f, 1f,
                    plantedDescending
                        ? Mathf.InverseLerp(lastContact - 0.12f, lastContact + 0.12f, advance)
                        : Mathf.InverseLerp(lastContact + 0.12f, lastContact + 0.42f, advance));
            }
            else
            {
                anchorsValid = false;
                stairBlend = Mathf.MoveTowards(stairBlend, 0f, 9f * Time.deltaTime);
            }
            if (stairBlend == 0f && !climbing && !holding)
                plantedFlight = -1;
            bool usingStairPose = nearbyFlight >= 0 && nearbyFlight == plantedFlight && stairBlend > 0f;
            Pose left = default;
            Pose right = default;
            if (usingStairPose)
            {
                Vector3 direction = GetStairDirection(nearbyFlight, plantedDescending);
                float[] contacts = plantedDescending ? descentContacts[nearbyFlight] : ascentContacts[nearbyFlight];
                GetStepPhase(Vector3.Dot(position, direction), contacts, out int nextStep, out float progress);
                left = GetStairFoot(nearbyFlight, nextStep, progress, true);
                right = GetStairFoot(nearbyFlight, nextStep, progress, false);
                float leftDrop = GetRequiredPelvisDrop(leftHip, leftLegLength, left.position);
                float rightDrop = GetRequiredPelvisDrop(rightHip, rightLegLength, right.position);
                float drop = Mathf.Clamp(Mathf.Max(leftDrop, rightDrop), PelvisDrop, 0.38f);
                Vector3 across = Vector3.Cross(Vector3.up, direction);
                bool leftSupports = (nextStep & 1) != 0;
                float supportSide = leftSupports ? -1f : 1f;
                float transfer = supportSide * (2f * StepTravel.Evaluate(progress) - 1f);
                animator.bodyPosition += (-Vector3.up * drop + across * (transfer * 0.045f)) * stairBlend;
                animator.bodyRotation = Quaternion.AngleAxis((plantedDescending ? 2f : 5f) * stairBlend, across) * animator.bodyRotation;
            }

            PlaceFoot(AvatarIKGoal.LeftFoot, nearbyFlight, -FootSpacing, left, usingStairPose, ref leftWeight);
            PlaceFoot(AvatarIKGoal.RightFoot, nearbyFlight, FootSpacing, right, usingStairPose, ref rightWeight);
        }

        private float GetLegLength(Transform hip, HumanBodyBones kneeBone, HumanBodyBones ankleBone)
        {
            Transform knee = animator.GetBoneTransform(kneeBone);
            Transform ankle = animator.GetBoneTransform(ankleBone);
            return Vector3.Distance(hip.position, knee.position) + Vector3.Distance(knee.position, ankle.position);
        }

        private static float GetRequiredPelvisDrop(Transform hip, float legLength, Vector3 target)
        {
            Vector3 difference = hip.position - target;
            float horizontalDistanceSquared = difference.x * difference.x + difference.z * difference.z;
            float verticalReach = Mathf.Sqrt(Mathf.Max(0f, legLength * legLength - horizontalDistanceSquared));
            return difference.y - verticalReach + 0.05f;
        }

        private int FindNearbyFlight(bool requireGrounded = true)
        {
            if (player.IsJumping || player.Body == null)
                return -1;

            bool canUseAnyFlight = !requireGrounded || player.Body.isGrounded;
            Vector3 position = player.transform.position;
            const float maximumDistance = 0.7f;
            for (int i = 0; i < flightBounds.Length; i++)
            {
                if ((canUseAnyFlight || i == plantedFlight)
                    && flightBounds[i].SqrDistance(position) <= maximumDistance * maximumDistance)
                    return i;
            }

            return -1;
        }

        private bool TryGetTreadHeight(int flight, Vector3 position, out float height, out BoxCollider surface)
        {
            var probe = new Ray(position + Vector3.up * ProbeHeight, Vector3.down);
            height = float.NegativeInfinity;
            surface = null;
            foreach (BoxCollider tread in flightTreads[flight])
            {
                if (tread.Raycast(probe, out RaycastHit hit, ProbeHeight + ProbeDepth) && hit.point.y > height)
                {
                    height = hit.point.y;
                    surface = tread;
                }
            }
            return surface != null;
        }

        private static void GetStepPhase(float position, float[] contacts, out int nextStep, out float progress)
        {
            nextStep = 0;
            while (nextStep < contacts.Length && position >= contacts[nextStep])
                nextStep++;

            if (nextStep == contacts.Length)
            {
                progress = 1f;
                return;
            }

            float previousContact = nextStep == 0 ? contacts[0] - FirstSwingDistance : contacts[nextStep - 1];
            progress = Mathf.InverseLerp(previousContact, contacts[nextStep], position);
        }

        private Pose GetStairFoot(int flight, int nextStep, float progress, bool isLeft)
        {
            int parity = isLeft ? 0 : 1;
            int plantedStep = nextStep - 1;
            if ((plantedStep & 1) != parity)
                plantedStep--;

            Pose[] anchors = isLeft ? leftAnchors : rightAnchors;
            Pose sole = anchors[plantedStep + 2];
            int contactCount = plantedDescending ? descentContacts[flight].Length : ascentContacts[flight].Length;
            if (nextStep < contactCount && (nextStep & 1) == parity)
            {
                Pose landing = anchors[nextStep + 2];
                float travel = StepTravel.Evaluate(progress);
                sole.position = Vector3.Lerp(sole.position, landing.position, travel);
                float heightProgress = plantedDescending ? Mathf.InverseLerp(0.25f, 1f, progress) : progress * 2.25f;
                float lift = StepLift.Evaluate(progress) * (plantedDescending ? 0.45f : 1f);
                sole.position.y = Mathf.Lerp(anchors[plantedStep + 2].position.y, landing.position.y,
                    Mathf.SmoothStep(0f, 1f, heightProgress)) + lift;
                sole.rotation = Quaternion.Slerp(sole.rotation, landing.rotation, travel)
                    * Quaternion.AngleAxis(-ShoePitch.Evaluate(progress), Vector3.right);
                RaiseShoeOverEdge(flight, progress, ref sole);
            }

            float soleHeight = isLeft ? leftSoleHeight : rightSoleHeight;
            sole.position += sole.rotation * Vector3.up * soleHeight;
            sole.rotation *= isLeft ? leftRestRotation : rightRestRotation;
            return sole;
        }

        private void RaiseShoeOverEdge(int flight, float progress, ref Pose sole)
        {
            // Check the swept shoe footprint, with clearance tapering to zero at touchdown.
            float clearance = EdgeClearance * Mathf.Sin(progress * Mathf.PI);
            Vector3 toe = sole.position + sole.rotation * Vector3.forward * ToeReach;
            Vector3 heel = sole.position - sole.rotation * Vector3.forward * HeelReach;
            float rise = 0f;
            if (TryGetShoeSurface(flight, toe, out RaycastHit toeHit))
                rise = Mathf.Max(rise, toeHit.point.y + clearance - toe.y);
            if (TryGetShoeSurface(flight, heel, out RaycastHit heelHit))
                rise = Mathf.Max(rise, heelHit.point.y + clearance - heel.y);
            sole.position.y += rise * Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((1f - progress) * 10f));
        }

        private bool TryGetShoeSurface(int flight, Vector3 point, out RaycastHit hit)
        {
            return Physics.Raycast(point + Vector3.up * ProbeHeight, Vector3.down, out hit,
                ProbeHeight + ProbeDepth, 1 << flightTreads[flight][0].gameObject.layer, QueryTriggerInteraction.Ignore);
        }

        private Pose CalculateFootAnchor(int flight, int step, float lane)
        {
            Vector3 direction = GetStairDirection(flight, plantedDescending);
            Vector3 across = Vector3.Cross(Vector3.up, direction);
            BoxCollider[] treads = flightTreads[flight];
            int treadIndex = plantedDescending ? treads.Length - 1 - step : step;
            BoxCollider tread = treads[Mathf.Clamp(treadIndex, 0, treads.Length - 1)];
            Vector3 center = tread.bounds.center;
            float shoeCenter = (ToeReach - HeelReach) * 0.5f;
            Vector3 target = center + across * (lane - Vector3.Dot(center, across)) - direction * shoeCenter;
            Vector3 local = tread.transform.InverseTransformPoint(target) - tread.center;
            Vector3 half = tread.size * 0.5f;
            local.x = Mathf.Clamp(local.x, -half.x * 0.85f, half.x * 0.85f);
            local.z = Mathf.Clamp(local.z, -half.z * 0.85f, half.z * 0.85f);
            local.y = half.y;
            target = tread.transform.TransformPoint(tread.center + local);
            bool pastBottom = plantedDescending && step >= treads.Length;
            if (plantedDescending && (step < 0 || pastBottom))
            {
                float[] contacts = descentContacts[flight];
                float advance = pastBottom
                    ? contacts[contacts.Length - 1] + DescentContactLead
                    : contacts[0] + DescentContactLead - (step == -1 ? 0.26f : 0.43f);
                target += direction * (advance - Vector3.Dot(target, direction));
            }
            else if (step < 0)
            {
                float setback = step == -1 ? 0.10f : 0.26f;
                float distance = ascentContacts[flight][0] + TouchdownLead - setback;
                target += direction * (distance - Vector3.Dot(target, direction));
            }

            Vector3 normal = Vector3.up;
            bool hasSurface = TryGetShoeSurface(flight, target, out RaycastHit hit)
                && (step < 0 || pastBottom || hit.point.y >= target.y - 0.002f);
            if (hasSurface)
            {
                target.y = hit.point.y;
                normal = hit.normal;
            }
            else if (pastBottom)
            {
                target.y -= treads[1].bounds.max.y - treads[0].bounds.max.y;
            }
            else if (step < 0)
            {
                target.y = plantedDescending ? tread.bounds.max.y : player.transform.position.y;
            }

            Quaternion rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(direction, normal), normal);
            if (hasSurface && !(hit.collider is BoxCollider))
            {
                Vector3 toe = target + rotation * Vector3.forward * ToeReach;
                Vector3 heel = target - rotation * Vector3.forward * HeelReach;
                float rise = 0f;
                if (TryGetShoeSurface(flight, toe, out RaycastHit toeHit) && toeHit.collider == hit.collider)
                    rise = Mathf.Max(rise, toeHit.point.y - toe.y);
                if (TryGetShoeSurface(flight, heel, out RaycastHit heelHit) && heelHit.collider == hit.collider)
                    rise = Mathf.Max(rise, heelHit.point.y - heel.y);
                target.y += rise;
            }
            return new Pose(target, rotation);
        }

        private void LateUpdate()
        {
            if (shoePoseFrame != Time.frameCount)
                return;

            // Position IK has already solved the knees. Orient the shoe in its calibrated bone frame.
            leftAnkle.rotation = Quaternion.Slerp(leftAnkle.rotation, leftShoeTarget, stairBlend);
            rightAnkle.rotation = Quaternion.Slerp(rightAnkle.rotation, rightShoeTarget, stairBlend);
        }

        private void PlaceFoot(AvatarIKGoal foot, int nearbyFlight, float lateralOffset, Pose target, bool usingStairPose, ref float weight)
        {
            Vector3 pose = animator.GetIKPosition(foot);
            float treadHeight = 0f;
            bool hasTread = nearbyFlight >= 0 && TryGetTreadHeight(nearbyFlight, pose, out treadHeight, out _);
            if (usingStairPose)
            {
                pose = Vector3.Lerp(pose, target.position, stairBlend);
                if (foot == AvatarIKGoal.LeftFoot)
                    leftShoeTarget = target.rotation;
                else
                    rightShoeTarget = target.rotation;
                shoePoseFrame = Time.frameCount;
            }
            animator.SetIKRotationWeight(foot, 0f);

            AvatarIKHint knee = foot == AvatarIKGoal.LeftFoot ? AvatarIKHint.LeftKnee : AvatarIKHint.RightKnee;
            if (usingStairPose)
            {
                Vector3 direction = GetStairDirection(nearbyFlight, plantedDescending);
                Vector3 kneeTarget = player.transform.position + direction * 0.48f
                    + Vector3.up * 0.55f + Vector3.Cross(Vector3.up, direction) * lateralOffset;
                animator.SetIKHintPosition(knee, kneeTarget);
            }
            animator.SetIKHintPositionWeight(knee, usingStairPose ? stairBlend * 0.25f : 0f);

            weight = Mathf.MoveTowards(weight, hasTread || usingStairPose ? 1f : 0f,
                12f * Time.deltaTime);
            float stairWeight = usingStairPose ? stairBlend : 0f;
            if (hasTread && stairWeight < 1f)
            {
                float swing = Mathf.Max(0f, pose.y - player.transform.position.y - AnimatedStanceHeight);
                float targetHeight = treadHeight + AnkleClearance + swing;
                pose.y += Mathf.Clamp(targetHeight - pose.y, -MaximumFootCorrection, MaximumFootCorrection) * (1f - stairWeight);
            }

            animator.SetIKPositionWeight(foot, Mathf.Max(weight, stairWeight));
            animator.SetIKPosition(foot, pose);
        }
    }
}
