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
        private const float TouchdownLead = 0.22f;
        private const float FirstSwingDistance = 0.30f;
        private const float FootSpacing = 0.13f;
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
        private Bounds[] flightBounds;
        private Vector3[] ascentDirections;
        private float leftWeight;
        private float rightWeight;
        private float ascentBlend;
        private float[][] touchdownDistances;
        private int plantedFlight = -1;
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
        private int ascentFlight = -1;

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
            flightBounds = new Bounds[stairFlights.Length];
            ascentDirections = new Vector3[stairFlights.Length];
            touchdownDistances = new float[stairFlights.Length][];
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
                    bounds.Encapsulate(tread.bounds);
                flightBounds[i] = bounds;
                Vector3 direction = Vector3.ProjectOnPlane(treads[treads.Length - 1].bounds.center - treads[0].bounds.center,
                    Vector3.up).normalized;
                ascentDirections[i] = direction;
                touchdownDistances[i] = new float[treads.Length];
                for (int step = 0; step < treads.Length; step++)
                {
                    BoxCollider tread = treads[step];
                    Vector3 halfSize = tread.size * 0.5f;
                    float depth = Mathf.Abs(Vector3.Dot(tread.transform.TransformVector(Vector3.right * halfSize.x), direction))
                        + Mathf.Abs(Vector3.Dot(tread.transform.TransformVector(Vector3.forward * halfSize.z), direction));
                    touchdownDistances[i][step] = Vector3.Dot(tread.bounds.center, direction) - depth - TouchdownLead;
                }
            }
            leftAnchors = new Pose[maximumTreads + 2];
            rightAnchors = new Pose[maximumTreads + 2];
        }

        public Vector3 LimitAscentSpeed(Vector3 velocity)
        {
            ascentFlight = -1;
            if (velocity.sqrMagnitude < 0.01f)
                return velocity;

            int flight = FindNearbyFlight();
            Vector3 direction = velocity.normalized;
            Vector3 position = player.transform.position;
            if (flight >= 0 && Vector3.Dot(direction, ascentDirections[flight]) > 0.55f
                && (TryGetTreadHeight(flight, position + direction * (FirstSwingDistance + TouchdownLead), out _)
                    || TryGetTreadHeight(flight, position, out _)))
            {
                ascentFlight = flight;
                return Vector3.ClampMagnitude(velocity, AscentSpeed);
            }

            return velocity;
        }

        public bool TryGetAscentAnimationTime(out float normalizedTime)
        {
            normalizedTime = 0f;
            int flight = ascentFlight >= 0 ? ascentFlight : plantedFlight;
            if (flight < 0 || flight != FindNearbyFlight() ||
                (ascentFlight < 0 && player.GroundSpeed > 0.1f))
                return false;

            float position = Vector3.Dot(player.transform.position, ascentDirections[flight]);
            float[] contacts = touchdownDistances[flight];
            if (position < contacts[0] - FirstSwingDistance || position > contacts[contacts.Length - 1] + 0.24f)
                return false;

            GetStepPhase(flight, out int nextStep, out float phase);
            normalizedTime = ((nextStep & 1) + (nextStep == contacts.Length ? 0f : phase)) * 0.5f;
            return true;
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (layerIndex != 0)
                return;

            int nearbyFlight = FindNearbyFlight();
            Vector3 position = player.transform.position;
            bool climbing = ascentFlight >= 0 && ascentFlight == nearbyFlight;
            bool holding = plantedFlight >= 0 && plantedFlight == nearbyFlight && player.GroundSpeed < 0.1f;
            if (climbing && plantedFlight != nearbyFlight)
            {
                plantedFlight = nearbyFlight;
                float lane = Vector3.Dot(position, Vector3.Cross(Vector3.up, ascentDirections[nearbyFlight]));
                for (int step = -2; step < flightTreads[nearbyFlight].Length; step++)
                {
                    leftAnchors[step + 2] = CalculateFootAnchor(nearbyFlight, step, lane - FootSpacing);
                    rightAnchors[step + 2] = CalculateFootAnchor(nearbyFlight, step, lane + FootSpacing);
                }
            }

            if (climbing || holding)
            {
                float advance = Vector3.Dot(position, ascentDirections[plantedFlight]);
                float[] contacts = touchdownDistances[plantedFlight];
                float firstContact = contacts[0];
                float lastContact = contacts[contacts.Length - 1];
                // Finish the walk-to-stair blend by the first touchdown at any walking speed.
                ascentBlend = Mathf.SmoothStep(0f, 1f,
                    Mathf.InverseLerp(firstContact - FirstSwingDistance, firstContact, advance));
                ascentBlend *= 1f - Mathf.SmoothStep(0f, 1f,
                    Mathf.InverseLerp(lastContact + 0.12f, lastContact + 0.42f, advance));
            }
            else
            {
                ascentBlend = Mathf.MoveTowards(ascentBlend, 0f, 9f * Time.deltaTime);
            }
            if (ascentBlend == 0f && !climbing && !holding)
                plantedFlight = -1;
            bool usingAscent = nearbyFlight >= 0 && nearbyFlight == plantedFlight && ascentBlend > 0f;
            Pose left = default;
            Pose right = default;
            if (usingAscent)
            {
                GetStepPhase(nearbyFlight, out int nextStep, out float progress);
                left = GetAscentFoot(nearbyFlight, nextStep, progress, true);
                right = GetAscentFoot(nearbyFlight, nextStep, progress, false);
                float leftDrop = GetRequiredPelvisDrop(leftHip, leftLegLength, left.position);
                float rightDrop = GetRequiredPelvisDrop(rightHip, rightLegLength, right.position);
                float drop = Mathf.Clamp(Mathf.Max(leftDrop, rightDrop), PelvisDrop, 0.38f);
                Vector3 uphill = ascentDirections[nearbyFlight];
                Vector3 across = Vector3.Cross(Vector3.up, uphill);
                bool leftSupports = (nextStep & 1) != 0;
                float supportSide = leftSupports ? -1f : 1f;
                float transfer = supportSide * (2f * StepTravel.Evaluate(progress) - 1f);
                animator.bodyPosition += (-Vector3.up * drop + across * (transfer * 0.045f)) * ascentBlend;
                animator.bodyRotation = Quaternion.AngleAxis(5f * ascentBlend, across) * animator.bodyRotation;
            }

            PlaceFoot(AvatarIKGoal.LeftFoot, nearbyFlight, -FootSpacing, left, usingAscent, ref leftWeight);
            PlaceFoot(AvatarIKGoal.RightFoot, nearbyFlight, FootSpacing, right, usingAscent, ref rightWeight);
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

        private int FindNearbyFlight()
        {
            if (player.IsJumping || player.Body == null || (!player.Body.isGrounded && plantedFlight < 0))
                return -1;

            Vector3 position = player.transform.position;
            const float maximumDistance = 0.7f;
            for (int i = 0; i < flightBounds.Length; i++)
            {
                if ((player.Body.isGrounded || i == plantedFlight)
                    && flightBounds[i].SqrDistance(position) <= maximumDistance * maximumDistance)
                    return i;
            }

            return -1;
        }

        private bool TryGetTreadHeight(int flight, Vector3 position, out float height)
        {
            var probe = new Ray(position + Vector3.up * ProbeHeight, Vector3.down);
            height = float.NegativeInfinity;
            foreach (BoxCollider tread in flightTreads[flight])
            {
                if (tread.Raycast(probe, out RaycastHit hit, ProbeHeight + ProbeDepth))
                    height = Mathf.Max(height, hit.point.y);
            }
            return height > float.NegativeInfinity;
        }

        private void GetStepPhase(int flight, out int nextStep, out float progress)
        {
            float position = Vector3.Dot(player.transform.position, ascentDirections[flight]);
            float[] contacts = touchdownDistances[flight];
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

        private Pose GetAscentFoot(int flight, int nextStep, float progress, bool isLeft)
        {
            int parity = isLeft ? 0 : 1;
            int plantedStep = nextStep - 1;
            if ((plantedStep & 1) != parity)
                plantedStep--;

            Pose[] anchors = isLeft ? leftAnchors : rightAnchors;
            Pose sole = anchors[plantedStep + 2];
            if (nextStep < flightTreads[flight].Length && (nextStep & 1) == parity)
            {
                Pose landing = anchors[nextStep + 2];
                float travel = StepTravel.Evaluate(progress);
                sole.position = Vector3.Lerp(sole.position, landing.position, travel);
                sole.position.y = Mathf.Lerp(anchors[plantedStep + 2].position.y, landing.position.y,
                    Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(progress * 2.25f))) + StepLift.Evaluate(progress);
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
            Vector3 direction = ascentDirections[flight];
            Vector3 across = Vector3.Cross(Vector3.up, direction);
            BoxCollider tread = flightTreads[flight][Mathf.Max(0, step)];
            Vector3 center = tread.bounds.center;
            float shoeCenter = (ToeReach - HeelReach) * 0.5f;
            Vector3 target = center + across * (lane - Vector3.Dot(center, across)) - direction * shoeCenter;
            Vector3 local = tread.transform.InverseTransformPoint(target) - tread.center;
            Vector3 half = tread.size * 0.5f;
            local.x = Mathf.Clamp(local.x, -half.x * 0.85f, half.x * 0.85f);
            local.z = Mathf.Clamp(local.z, -half.z * 0.85f, half.z * 0.85f);
            local.y = half.y;
            target = tread.transform.TransformPoint(tread.center + local);
            if (step < 0)
            {
                float setback = step == -1 ? 0.10f : 0.26f;
                float distance = touchdownDistances[flight][0] + TouchdownLead - setback;
                target += direction * (distance - Vector3.Dot(target, direction));
            }

            Vector3 normal = Vector3.up;
            bool hasSurface = TryGetShoeSurface(flight, target, out RaycastHit hit)
                && (step < 0 || hit.point.y >= target.y - 0.002f);
            if (hasSurface)
            {
                target.y = hit.point.y;
                normal = hit.normal;
            }
            else if (step < 0)
            {
                target.y = player.transform.position.y;
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
            leftAnkle.rotation = Quaternion.Slerp(leftAnkle.rotation, leftShoeTarget, ascentBlend);
            rightAnkle.rotation = Quaternion.Slerp(rightAnkle.rotation, rightShoeTarget, ascentBlend);
        }

        private void PlaceFoot(AvatarIKGoal foot, int nearbyFlight, float lateralOffset, Pose target, bool usingAscent, ref float weight)
        {
            Vector3 pose = animator.GetIKPosition(foot);
            float treadHeight = 0f;
            bool hasTread = nearbyFlight >= 0 && TryGetTreadHeight(nearbyFlight, pose, out treadHeight);
            if (usingAscent)
            {
                pose = Vector3.Lerp(pose, target.position, ascentBlend);
                if (foot == AvatarIKGoal.LeftFoot)
                    leftShoeTarget = target.rotation;
                else
                    rightShoeTarget = target.rotation;
                shoePoseFrame = Time.frameCount;
            }
            animator.SetIKRotationWeight(foot, 0f);

            AvatarIKHint knee = foot == AvatarIKGoal.LeftFoot ? AvatarIKHint.LeftKnee : AvatarIKHint.RightKnee;
            if (usingAscent)
            {
                Vector3 direction = ascentDirections[nearbyFlight];
                Vector3 kneeTarget = player.transform.position + direction * 0.48f
                    + Vector3.up * 0.55f + Vector3.Cross(Vector3.up, direction) * lateralOffset;
                animator.SetIKHintPosition(knee, kneeTarget);
            }
            animator.SetIKHintPositionWeight(knee, usingAscent ? ascentBlend * 0.25f : 0f);

            weight = Mathf.MoveTowards(weight, hasTread || usingAscent ? 1f : 0f,
                12f * Time.deltaTime);
            float ascentWeight = usingAscent ? ascentBlend : 0f;
            if (hasTread && ascentWeight < 1f)
            {
                float swing = Mathf.Max(0f, pose.y - player.transform.position.y - AnimatedStanceHeight);
                float targetHeight = treadHeight + AnkleClearance + swing;
                pose.y += Mathf.Clamp(targetHeight - pose.y, -MaximumFootCorrection, MaximumFootCorrection) * (1f - ascentWeight);
            }

            animator.SetIKPositionWeight(foot, Mathf.Max(weight, ascentWeight));
            animator.SetIKPosition(foot, pose);
        }
    }
}
