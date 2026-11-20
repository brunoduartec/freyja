using UnityEngine;

namespace Freyja.Prototype
{
    internal sealed class ConversationFraming
    {
        private static readonly float[] Angles = { 0f, 18f, -18f, 34f, -34f, 50f, -50f, 65f, -65f };
        private static readonly float[] Distances = { 1f, 0.78f, 1.2f };
        private readonly CaisCameraRig rig;
        private readonly Transform player;
        private readonly Transform resident;
        private readonly Quaternion playerRotation;
        private readonly Quaternion residentRotation;

        private ConversationFraming(CaisCameraRig rig, Transform player, Transform resident)
        {
            this.rig = rig;
            this.player = player;
            this.resident = resident;
            playerRotation = player.rotation;
            residentRotation = resident.rotation;
        }

        public static ConversationFraming Begin(CaisCameraRig rig, Transform player, Transform resident)
        {
            if (rig == null || rig.ViewCamera == null) return null;

            Vector3 firstFace = FacePosition(player);
            Vector3 secondFace = FacePosition(resident);
            Vector3 across = Vector3.ProjectOnPlane(secondFace - firstFace, Vector3.up);
            if (across.sqrMagnitude < 0.04f)
                across = Vector3.ProjectOnPlane(resident.position - player.position, Vector3.up);
            if (across.sqrMagnitude < 0.01f)
                across = player.right;

            Vector3 midpoint = (firstFace + secondFace) * 0.5f;
            Vector3 front = Vector3.Cross(across.normalized, Vector3.up);
            float baseDistance = Mathf.Max(1.55f, across.magnitude * 1.13f);
            float aspect = Mathf.Max(1f, rig.ViewCamera.aspect);
            var bestPosition = midpoint - front * baseDistance;
            var bestFocus = midpoint - Vector3.up * 0.38f;
            float bestFov = 43f;
            float bestScore = float.NegativeInfinity;

            foreach (float angle in Angles)
            {
                Vector3 heading = Quaternion.AngleAxis(angle, Vector3.up) * front;
                foreach (float multiplier in Distances)
                {
                    float distance = baseDistance * multiplier;
                    var position = midpoint - heading * distance + Vector3.up * 0.10f;
                    var right = Vector3.Cross(Vector3.up, heading);
                    float width = Mathf.Abs(Vector3.Dot(secondFace - firstFace, right));
                    float fov = Mathf.Clamp(2f * Mathf.Atan(width / (1.05f * distance * aspect))
                        * Mathf.Rad2Deg, 32f, 58f);
                    float halfHeight = distance * Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
                    Vector3 focus = midpoint - Vector3.up * (halfHeight * 0.47f);
                    bool firstClear = IsClear(position, firstFace, player, resident);
                    bool secondClear = IsClear(position, secondFace, player, resident);
                    float score = (firstClear ? 12f : 0f) + (secondClear ? 12f : 0f)
                        - Mathf.Abs(angle) / 18f - Mathf.Abs(multiplier - 1f) * 3f;
                    if (score <= bestScore) continue;
                    bestScore = score;
                    bestPosition = position;
                    bestFocus = focus;
                    bestFov = fov;
                }
            }

            var framing = new ConversationFraming(rig, player, resident);
            FaceConversation(player, across, bestPosition);
            FaceConversation(resident, -across, bestPosition);
            rig.ShowConversation(bestPosition, Quaternion.LookRotation(bestFocus - bestPosition), bestFov);
            return framing;
        }

        public void End()
        {
            if (player != null) player.rotation = playerRotation;
            if (resident != null) resident.rotation = residentRotation;
            if (rig != null) rig.EndConversation();
        }

        private static Vector3 FacePosition(Transform actor)
        {
            var animator = actor.GetComponentInChildren<Animator>();
            var head = animator != null && animator.isHuman
                ? animator.GetBoneTransform(HumanBodyBones.Head)
                : null;
            return head != null ? head.position + Vector3.up * 0.08f : actor.position + Vector3.up * 1.58f;
        }

        private static bool IsClear(Vector3 camera, Vector3 face, Transform player, Transform resident)
        {
            Vector3 delta = face - camera;
            foreach (var hit in Physics.SphereCastAll(camera, 0.07f, delta.normalized, delta.magnitude,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                if (!hit.transform.IsChildOf(player) && !hit.transform.IsChildOf(resident))
                    return false;
            }
            return true;
        }

        private static void FaceConversation(Transform actor, Vector3 partnerDirection, Vector3 camera)
        {
            Vector3 viewerDirection = Vector3.ProjectOnPlane(camera - actor.position, Vector3.up);
            Vector3 direction = partnerDirection.normalized * 0.51f + viewerDirection.normalized * 0.74f;
            if (direction.sqrMagnitude > 0.001f)
                actor.rotation = Quaternion.LookRotation(direction);
        }
    }
}
