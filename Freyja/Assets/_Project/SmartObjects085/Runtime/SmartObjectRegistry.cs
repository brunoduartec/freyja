using System;
using System.Collections.Generic;
using UnityEngine;

namespace Freyja.SmartObjects
{
    public enum ReservationFailure
    {
        None,
        InvalidInput,
        ObjectUnavailable,
        DuplicateObjectId,
        SlotNotFound,
        SlotUnavailable,
        IncompatibleActivity,
        InvalidConfiguration
    }

    public readonly struct SmartObjectCandidate
    {
        public SmartObject Object { get; }
        public InteractionSlot Slot { get; }
        public string ActivityId { get; }
        public float DistanceSquared { get; }

        internal SmartObjectCandidate(SmartObject smartObject, InteractionSlot slot, string activityId, float distanceSquared)
        {
            Object = smartObject;
            Slot = slot;
            ActivityId = activityId;
            DistanceSquared = distanceSquared;
        }
    }

    public static class SmartObjectRegistry
    {
        private static readonly Dictionary<string, HashSet<SmartObject>> ObjectsById = new Dictionary<string, HashSet<SmartObject>>(StringComparer.Ordinal);
        private static int mainThreadId;

        public static event Action<string, IReadOnlyCollection<SmartObject>> DuplicateObjectIdDetected;

        static SmartObjectRegistry() => mainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSubsystem()
        {
            foreach (HashSet<SmartObject> group in ObjectsById.Values)
                foreach (SmartObject item in group)
                    if (item != null) item.InvalidateClaims();
            ObjectsById.Clear();
            mainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
        }

        public static void Register(SmartObject smartObject)
        {
            EnsureMainThread();
            if (smartObject == null || !smartObject.IsAvailable || string.IsNullOrWhiteSpace(smartObject.ObjectId)) return;
            if (!ObjectsById.TryGetValue(smartObject.ObjectId, out HashSet<SmartObject> group))
                ObjectsById.Add(smartObject.ObjectId, group = new HashSet<SmartObject>());
            group.Add(smartObject);
            if (group.Count > 1)
            {
                Debug.LogWarning($"Duplicate SmartObject ID '{smartObject.ObjectId}' detected; registry lookup excludes this ID until repaired.", smartObject);
                DuplicateObjectIdDetected?.Invoke(smartObject.ObjectId, group);
            }
        }

        public static void Unregister(SmartObject smartObject)
        {
            EnsureMainThread();
            if (smartObject == null) return;
            smartObject.InvalidateClaims();
            if (!ObjectsById.TryGetValue(smartObject.ObjectId, out HashSet<SmartObject> group)) return;
            group.Remove(smartObject);
            if (group.Count == 0) ObjectsById.Remove(smartObject.ObjectId);
        }

        public static bool TryGetById(string objectId, out SmartObject smartObject)
        {
            EnsureMainThread();
            smartObject = null;
            if (string.IsNullOrWhiteSpace(objectId) || !ObjectsById.TryGetValue(objectId, out HashSet<SmartObject> group) || group.Count != 1) return false;
            foreach (SmartObject candidate in group)
                if (candidate != null && candidate.IsAvailable) { smartObject = candidate; return true; }
            return false;
        }

        public static IReadOnlyList<SmartObjectCandidate> FindCandidates(Vector3 origin, string activityId, string regionId = null)
        {
            if (string.IsNullOrWhiteSpace(activityId)) return Array.Empty<SmartObjectCandidate>();
            return FindCandidates(origin, new[] { activityId }, regionId);
        }

        public static IReadOnlyList<SmartObjectCandidate> FindCandidates(Vector3 origin, IReadOnlyCollection<string> activityIds, string regionId = null)
        {
            EnsureMainThread();
            var results = new List<SmartObjectCandidate>();
            if (activityIds == null || activityIds.Count == 0) return results;
            foreach (KeyValuePair<string, HashSet<SmartObject>> entry in ObjectsById)
            {
                HashSet<SmartObject> group = entry.Value;
                if (group.Count != 1) continue;
                foreach (SmartObject smartObject in group)
                {
                    if (smartObject == null || !smartObject.IsAvailable) continue;
                    if (!string.IsNullOrEmpty(regionId) && !string.Equals(regionId, smartObject.RegionId, StringComparison.Ordinal)) continue;
                    // A slot ID is the public local address used by handles; never expose
                    // ambiguous addresses from an invalid object configuration.
                    if (smartObject.HasDuplicateSlotIds(out _)) continue;
                    foreach (InteractionSlot slot in smartObject.Slots)
                    {
                        if (slot == null || slot.State != InteractionSlotState.Available || slot.Approach == null || slot.Pose == null || !slot.TryGetFirstCompatibleActivity(activityIds, out string chosen)) continue;
                        Vector3 position = slot.Approach != null ? slot.Approach.position : smartObject.transform.position;
                        results.Add(new SmartObjectCandidate(smartObject, slot, chosen, (position - origin).sqrMagnitude));
                    }
                }
            }
            results.Sort(CompareCandidates);
            return results;
        }

        private static int CompareCandidates(SmartObjectCandidate a, SmartObjectCandidate b)
        {
            int distance = a.DistanceSquared.CompareTo(b.DistanceSquared);
            if (distance != 0) return distance;
            int objectId = string.Compare(a.Object.ObjectId, b.Object.ObjectId, StringComparison.Ordinal);
            return objectId != 0 ? objectId : string.Compare(a.Slot.SlotId, b.Slot.SlotId, StringComparison.Ordinal);
        }

        public static bool TryReserve(SmartObject smartObject, string slotId, string activityId, SmartObjectReservationOwner owner, out ReservationHandle handle, out ReservationFailure failure)
        {
            EnsureMainThread();
            handle = default;
            failure = ReservationFailure.InvalidInput;
            if (smartObject == null || owner == null || string.IsNullOrWhiteSpace(slotId) || string.IsNullOrWhiteSpace(activityId)) return false;
            if (!smartObject.IsAvailable || !owner.isActiveAndEnabled) { failure = ReservationFailure.ObjectUnavailable; return false; }
            if (!TryGetById(smartObject.ObjectId, out SmartObject indexed) || indexed != smartObject)
            {
                failure = ObjectsById.TryGetValue(smartObject.ObjectId, out HashSet<SmartObject> group) && group.Count > 1
                    ? ReservationFailure.DuplicateObjectId : ReservationFailure.ObjectUnavailable;
                return false;
            }
            if (smartObject.HasDuplicateSlotIds(out _)) { failure = ReservationFailure.InvalidConfiguration; return false; }
            if (!smartObject.TryGetSlot(slotId, out InteractionSlot slot)) { failure = ReservationFailure.SlotNotFound; return false; }
            if (slot.State != InteractionSlotState.Available) { failure = ReservationFailure.SlotUnavailable; return false; }
            if (slot.Approach == null || slot.Pose == null) { failure = ReservationFailure.InvalidConfiguration; return false; }
            if (!slot.SupportsActivity(activityId)) { failure = ReservationFailure.IncompatibleActivity; return false; }
            slot.Claim(owner, activityId.Trim(), out string token);
            handle = new ReservationHandle(smartObject, slotId, owner, token);
            owner.Track(handle);
            failure = ReservationFailure.None;
            return true;
        }

        internal static bool IsValid(ReservationHandle handle)
        {
            if (!IsMainThread() || handle.Object == null || handle.Owner == null || !handle.Object.IsAvailable || !handle.Owner.isActiveAndEnabled) return false;
            return handle.Object.TryGetSlot(handle.SlotId, out InteractionSlot slot) && handle.Object.Contains(slot) && slot.Matches(handle.Owner, handle.Token);
        }

        internal static bool TryOccupy(ReservationHandle handle)
        {
            EnsureMainThread();
            if (!IsValid(handle) || !handle.Object.TryGetSlot(handle.SlotId, out InteractionSlot slot) || slot.State != InteractionSlotState.Reserved) return false;
            slot.SetOccupied();
            return true;
        }

        internal static bool Release(ReservationHandle handle)
        {
            EnsureMainThread();
            if (handle.Object == null || !handle.Object.TryGetSlot(handle.SlotId, out InteractionSlot slot)) return false;
            bool released = slot.Release(handle.Owner, handle.Token);
            if (released) handle.Owner?.Forget(handle);
            return released;
        }

        internal static void ResetForTests() => ResetSubsystem();
        private static bool IsMainThread() => System.Threading.Thread.CurrentThread.ManagedThreadId == mainThreadId;
        private static void EnsureMainThread()
        {
            if (!IsMainThread()) throw new InvalidOperationException("SmartObjectRegistry is a Unity main-thread API.");
        }
    }
}
