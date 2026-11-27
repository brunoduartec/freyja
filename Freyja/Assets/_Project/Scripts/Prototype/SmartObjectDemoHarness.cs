using System;
using System.Collections.Generic;
using Freyja.Prototype;
using Freyja.SmartObjects;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Freyja.SmartObjects.Demo
{
    public sealed class SmartObjectDemoHarness : MonoBehaviour
    {
        public enum RequestState
        {
            Waiting,
            Reserved,
            Moving,
            OccupiedAtPoseReference,
            Cancelled,
            UnreachableReleased,
            TimedOutReleased,
            FailedReleased
        }

        [Serializable]
        public sealed class AgentRequest
        {
            public GameObject actor;
            [Min(1f)] public float timeoutSeconds = 45f;
        }

        [SerializeField] private List<AgentRequest> requests = new List<AgentRequest>();
        [SerializeField] private Transform unreachableProbe;
        [SerializeField] private bool showDebugOverlay = true;

        private sealed class RuntimeRequest
        {
            public GameObject Actor;
            public IslandWaypointWalker Walker;
            public SmartObjectReservationOwner Owner;
            public SmartObjectSeatedPose SeatedPose;
            public InteractionSlot Slot;
            public ReservationHandle Handle;
            public float StartedAt;
            public float Timeout;
            public RequestState State;
        }

        private readonly List<RuntimeRequest> activeRequests = new List<RuntimeRequest>();
        public int RequestCount => activeRequests.Count;

        private void Start()
        {
            activeRequests.Clear();
            if (requests == null) return;

            for (int i = 0; i < requests.Count; i++)
            {
                AgentRequest request = requests[i];
                if (request == null || request.actor == null) continue;

                var owner = request.actor.GetComponent<SmartObjectReservationOwner>();
                var walker = request.actor.GetComponent<IslandWaypointWalker>();
                var seatedPose = request.actor.GetComponent<SmartObjectSeatedPose>();
                if (seatedPose == null) seatedPose = request.actor.AddComponent<SmartObjectSeatedPose>();
                var runtime = new RuntimeRequest
                {
                    Actor = request.actor,
                    Walker = walker,
                    Owner = owner,
                    SeatedPose = seatedPose,
                    Timeout = Mathf.Max(1f, request.timeoutSeconds),
                    State = RequestState.Waiting
                };
                activeRequests.Add(runtime);
                BeginRequest(runtime);
            }
        }

        private void Update()
        {
            for (int i = 0; i < activeRequests.Count; i++)
                Tick(activeRequests[i]);

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return;
            if (keyboard.f1Key.wasPressedThisFrame) CancelFirstMovingRequest();
            if (keyboard.f2Key.wasPressedThisFrame && activeRequests.Count > 0) TestUnreachablePath(0);
            if (keyboard.f3Key.wasPressedThisFrame) ReleaseAll();
        }

        private static void BeginRequest(RuntimeRequest request)
        {
            if (request.Actor == null || request.Owner == null || request.Walker == null)
            {
                request.State = RequestState.FailedReleased;
                Debug.LogWarning("[SmartObjectsDemo] Agente sem actor, SmartObjectReservationOwner ou IslandWaypointWalker.", request.Actor);
                return;
            }

            IReadOnlyList<SmartObjectCandidate> candidates = SmartObjectRegistry.FindCandidates(request.Actor.transform.position, "sit");
            for (int i = 0; i < candidates.Count; i++)
            {
                SmartObjectCandidate candidate = candidates[i];
                if (!SmartObjectRegistry.TryReserve(candidate.Object, candidate.Slot.SlotId, candidate.ActivityId,
                        request.Owner, out ReservationHandle handle, out _))
                    continue;

                request.Handle = handle;
                request.Slot = candidate.Slot;
                request.StartedAt = Time.time;
                request.State = RequestState.Reserved;

                if (!request.Walker.TryMoveTo(candidate.Slot.Approach))
                {
                    Release(request, RequestState.UnreachableReleased);
                    Debug.LogWarning($"[SmartObjectsDemo] Caminho indisponível para {request.Actor.name}; reserva liberada.", request.Actor);
                    return;
                }

                request.State = RequestState.Moving;
                Debug.Log($"[SmartObjectsDemo] {request.Actor.name} reservou {handle.ObjectId}/{handle.SlotId}; iniciando aproximação.", request.Actor);
                return;
            }

            request.State = RequestState.FailedReleased;
            Debug.LogWarning($"[SmartObjectsDemo] Nenhum slot sit livre para {request.Actor.name}.", request.Actor);
        }

        private static void Tick(RuntimeRequest request)
        {
            if (request.State != RequestState.Moving) return;
            if (!request.Handle.IsValid)
            {
                request.Walker?.Stop();
                request.State = RequestState.FailedReleased;
                return;
            }

            if (Time.time - request.StartedAt >= request.Timeout)
            {
                Release(request, RequestState.TimedOutReleased);
                Debug.LogWarning($"[SmartObjectsDemo] Timeout para {request.Actor.name}; reserva liberada.", request.Actor);
                return;
            }

            if (request.Walker != null && request.Walker.IsWalking) return;
            if (request.Actor == null || request.Slot == null || request.Slot.Approach == null || request.Slot.Pose == null)
            {
                Release(request, RequestState.FailedReleased);
                return;
            }

            float arrivalTolerance = 0.4f;
            if (Vector3.Distance(request.Actor.transform.position, request.Slot.Approach.position) > arrivalTolerance)
            {
                Release(request, RequestState.UnreachableReleased);
                Debug.LogWarning($"[SmartObjectsDemo] {request.Actor.name} não chegou à aproximação; reserva liberada.", request.Actor);
                return;
            }

            if (!request.Handle.TryOccupy())
            {
                Release(request, RequestState.FailedReleased);
                return;
            }

            // Alignment demonstrates only the final transform contract; no sit animation or IK is implied.
            request.Actor.transform.SetPositionAndRotation(request.Slot.Pose.position, request.Slot.Pose.rotation);
            if (request.SeatedPose == null || !request.SeatedPose.Sit())
            {
                Release(request, RequestState.FailedReleased);
                Debug.LogWarning($"[SmartObjectsDemo] {request.Actor.name} não possui Avatar Humanoid para aplicar a pose sentada; reserva liberada.", request.Actor);
                return;
            }
            request.State = RequestState.OccupiedAtPoseReference;
            Debug.Log($"[SmartObjectsDemo] {request.Actor.name} chegou e sentou em {request.Handle.ObjectId}/{request.Handle.SlotId}.", request.Actor);
        }

        public bool CancelRequest(int index)
        {
            if (!TryGetRequest(index, out RuntimeRequest request) ||
                (request.State != RequestState.Moving && request.State != RequestState.Reserved)) return false;
            Release(request, RequestState.Cancelled);
            return true;
        }

        public bool TestUnreachablePath(int index)
        {
            if (!TryGetRequest(index, out RuntimeRequest request) || request.Actor == null || request.Owner == null ||
                request.Walker == null || unreachableProbe == null) return false;

            if (request.Handle.IsValid)
                Release(request, RequestState.Cancelled);

            BeginRequest(request);
            if (!request.Handle.IsValid) return false;

            bool accepted = request.Walker.TryMoveTo(unreachableProbe);
            if (accepted) request.Walker.Stop();
            Release(request, RequestState.UnreachableReleased);
            Debug.Log($"[SmartObjectsDemo] Caminho de diagnóstico {(accepted ? "interrompido" : "rejeitado")}; handle liberado para {request.Actor.name}.", request.Actor);
            return !accepted;
        }

        public bool ReleaseRequest(int index)
        {
            if (!TryGetRequest(index, out RuntimeRequest request) || !request.Handle.IsValid) return false;
            Release(request, RequestState.Cancelled);
            return true;
        }

        public void ReleaseAll()
        {
            foreach (RuntimeRequest request in activeRequests)
                if (request.Handle.IsValid) Release(request, RequestState.Cancelled);
        }

        public RequestState GetRequestState(int index) =>
            TryGetRequest(index, out RuntimeRequest request) ? request.State : RequestState.FailedReleased;

        private void CancelFirstMovingRequest()
        {
            for (int i = 0; i < activeRequests.Count; i++)
                if (CancelRequest(i)) return;
        }

        private bool TryGetRequest(int index, out RuntimeRequest request)
        {
            request = null;
            if (index < 0 || index >= activeRequests.Count) return false;
            request = activeRequests[index];
            return true;
        }

        private static void Release(RuntimeRequest request, RequestState finalState)
        {
            request.Walker?.Stop();
            request.SeatedPose?.StandUp();
            request.Owner?.Release(request.Handle);
            request.State = finalState;
        }

        private void OnGUI()
        {
            if (!showDebugOverlay || !Application.isPlaying) return;
            GUILayout.BeginArea(new Rect(12f, 12f, 540f, 190f), GUI.skin.box);
            GUILayout.Label("SMART OBJECTS · demonstração de aproximação/reserva (sem animação de atividade)", GUI.skin.label);
            for (int i = 0; i < activeRequests.Count; i++)
            {
                RuntimeRequest request = activeRequests[i];
                string actor = request.Actor != null ? request.Actor.name : "agente removido";
                string slot = request.Handle.ObjectId != null ? $"{request.Handle.ObjectId}/{request.Handle.SlotId}" : "sem slot";
                GUILayout.Label($"{actor}: {request.State} · {slot}");
            }
            GUILayout.Label("F1 cancelar rota · F2 testar destino inválido · F3 liberar reservas");
            GUILayout.EndArea();
        }
    }
}
