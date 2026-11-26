using System.Collections.Generic;
using UnityEngine;

namespace Freyja.SmartObjects
{
    [DisallowMultipleComponent]
    public sealed class SmartObjectReservationOwner : MonoBehaviour
    {
        private readonly HashSet<ReservationHandle> handles = new HashSet<ReservationHandle>();

        public int ReservationCount => handles.Count;

        private void OnDisable() => ReleaseAll();
        private void OnDestroy() => ReleaseAll();

        internal void Track(ReservationHandle handle) => handles.Add(handle);
        internal void Forget(ReservationHandle handle) => handles.Remove(handle);

        internal void ReleaseSlot(SmartObject smartObject, InteractionSlot slot)
        {
            if (smartObject == null || slot == null) return;
            var matches = new List<ReservationHandle>();
            foreach (ReservationHandle handle in handles)
                if (handle.Object == smartObject && handle.SlotId == slot.SlotId) matches.Add(handle);
            foreach (ReservationHandle handle in matches) SmartObjectRegistry.Release(handle);
        }

        public bool Release(ReservationHandle handle)
        {
            if (!handles.Contains(handle)) return false;
            return SmartObjectRegistry.Release(handle);
        }

        public void ReleaseAll()
        {
            if (handles.Count == 0) return;
            var copy = new List<ReservationHandle>(handles);
            foreach (ReservationHandle handle in copy) SmartObjectRegistry.Release(handle);
            handles.Clear();
        }
    }
}
