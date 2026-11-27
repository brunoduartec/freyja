using System;
using System.Collections.Generic;
using UnityEngine;

namespace Freyja.SmartObjects
{
    [DisallowMultipleComponent]
    public sealed class SmartObject : MonoBehaviour
    {
        [SerializeField] private string objectId;
        [SerializeField] private string regionId;
        [SerializeField] private List<InteractionSlot> slots = new List<InteractionSlot>();

        public string ObjectId => objectId;
        public string RegionId => regionId;
        public IReadOnlyList<InteractionSlot> Slots => slots;
        public bool IsAvailable => isActiveAndEnabled && gameObject.activeInHierarchy;

        private void OnEnable()
        {
            EnsureStableIds();
            ResetSlotStates();
            SmartObjectRegistry.Register(this);
        }

        private void OnDisable() => SmartObjectRegistry.Unregister(this);
        private void OnDestroy() => SmartObjectRegistry.Unregister(this);

        private void OnValidate()
        {
            EnsureStableIds();
            if (isActiveAndEnabled)
                SmartObjectRegistry.Refresh(this);
        }

        internal void EnsureStableIds()
        {
            if (string.IsNullOrWhiteSpace(objectId)) objectId = Guid.NewGuid().ToString("N");
            objectId = objectId?.Trim();
            regionId = regionId?.Trim();
            if (slots == null) slots = new List<InteractionSlot>();
            foreach (InteractionSlot slot in slots)
            {
                if (slot == null) continue;
                var id = slot.SlotId?.Trim();
                if (string.IsNullOrEmpty(id)) slot.SetSlotIdIfEmpty(Guid.NewGuid().ToString("N"));
            }
        }

        public bool TryGetSlot(string slotId, out InteractionSlot slot)
        {
            slot = null;
            if (string.IsNullOrEmpty(slotId) || slots == null) return false;
            foreach (InteractionSlot item in slots)
                if (item != null && string.Equals(item.SlotId, slotId, StringComparison.Ordinal))
                {
                    slot = item;
                    return true;
                }
            return false;
        }

        public bool HasDuplicateSlotIds(out string duplicateId)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);
            if (slots != null)
                foreach (InteractionSlot slot in slots)
                {
                    string id = slot?.SlotId?.Trim();
                    if (string.IsNullOrEmpty(id) || !seen.Add(id))
                    {
                        duplicateId = id ?? string.Empty;
                        return true;
                    }
                }
            duplicateId = null;
            return false;
        }

        public void SetSlotEnabled(string slotId, bool value)
        {
            if (TryGetSlot(slotId, out InteractionSlot slot))
            {
                if (!value && slot.Owner != null) slot.Owner.ReleaseSlot(this, slot);
                slot.SetEnabled(value);
            }
        }

        internal void ResetSlotStates()
        {
            if (slots == null) return;
            foreach (InteractionSlot slot in slots) slot?.ResetRuntime();
        }

        internal void InvalidateClaims()
        {
            if (slots == null) return;
            foreach (InteractionSlot slot in slots)
            {
                if (slot?.Owner != null) slot.Owner.ReleaseSlot(this, slot);
                slot?.ResetRuntime();
            }
        }

        internal bool Contains(InteractionSlot slot) => slot != null && slots != null && slots.Contains(slot);
    }
}
