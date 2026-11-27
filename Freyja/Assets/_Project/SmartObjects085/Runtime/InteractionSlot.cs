using System;
using System.Collections.Generic;
using UnityEngine;

namespace Freyja.SmartObjects
{
    public enum InteractionSlotState
    {
        Available,
        Reserved,
        Occupied,
        Disabled
    }

    [Serializable]
    public sealed class InteractionSlot
    {
        [SerializeField] private string slotId;
        [SerializeField] private List<string> activityIds = new List<string>();
        [SerializeField] private Transform approach = null;
        [SerializeField] private Transform pose = null;
        [SerializeField] private bool enabled = true;

        [NonSerialized] private InteractionSlotState state;
        [NonSerialized] private SmartObjectReservationOwner owner;
        [NonSerialized] private string token;
        [NonSerialized] private string reservedActivityId;

        public string SlotId => slotId;
        public IReadOnlyList<string> ActivityIds => activityIds;
        public Transform Approach => approach;
        public Transform Pose => pose;
        public bool IsEnabled => enabled;
        public InteractionSlotState State => !enabled ? InteractionSlotState.Disabled : state;
        public SmartObjectReservationOwner ReservationOwner => owner;
        internal SmartObjectReservationOwner Owner => owner;
        internal string Token => token;
        internal string ReservedActivityId => reservedActivityId;

        internal void SetSlotIdIfEmpty(string value)
        {
            if (string.IsNullOrWhiteSpace(slotId)) slotId = value;
        }

        public bool SupportsActivity(string activityId)
        {
            if (!enabled || string.IsNullOrWhiteSpace(activityId)) return false;
            for (int i = 0; i < activityIds.Count; i++)
                if (string.Equals(activityIds[i]?.Trim(), activityId.Trim(), StringComparison.Ordinal)) return true;
            return false;
        }

        internal bool TryGetFirstCompatibleActivity(IReadOnlyCollection<string> requested, out string activity)
        {
            activity = null;
            if (!enabled || requested == null) return false;
            for (int i = 0; i < activityIds.Count; i++)
            {
                string candidate = activityIds[i]?.Trim();
                if (string.IsNullOrEmpty(candidate)) continue;
                foreach (string wanted in requested)
                    if (string.Equals(candidate, wanted?.Trim(), StringComparison.Ordinal))
                    {
                        activity = candidate;
                        return true;
                    }
            }
            return false;
        }

        internal void ResetRuntime()
        {
            owner = null;
            token = null;
            reservedActivityId = null;
            state = enabled ? InteractionSlotState.Available : InteractionSlotState.Disabled;
        }

        internal void SetEnabled(bool value)
        {
            enabled = value;
            if (!enabled) ResetRuntime();
            else if (state == InteractionSlotState.Disabled) ResetRuntime();
        }

        internal void Claim(SmartObjectReservationOwner newOwner, string activityId, out string newToken)
        {
            owner = newOwner;
            reservedActivityId = activityId;
            token = Guid.NewGuid().ToString("N");
            state = InteractionSlotState.Reserved;
            newToken = token;
        }

        internal bool Matches(SmartObjectReservationOwner expectedOwner, string expectedToken)
        {
            return enabled && owner == expectedOwner && !string.IsNullOrEmpty(token) && token == expectedToken;
        }

        internal void SetOccupied() => state = InteractionSlotState.Occupied;

        internal bool Release(SmartObjectReservationOwner expectedOwner, string expectedToken)
        {
            if (!Matches(expectedOwner, expectedToken)) return false;
            ResetRuntime();
            return true;
        }

        internal bool HasDuplicateOrEmptyActivity(out string invalid)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < activityIds.Count; i++)
            {
                string value = activityIds[i]?.Trim();
                if (string.IsNullOrEmpty(value) || !seen.Add(value))
                {
                    invalid = value ?? string.Empty;
                    return true;
                }
            }
            invalid = null;
            return false;
        }
    }
}
