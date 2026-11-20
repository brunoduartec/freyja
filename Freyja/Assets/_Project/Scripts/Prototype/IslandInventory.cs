using System.Collections.Generic;
using UnityEngine;

namespace Freyja.Prototype
{
    public sealed class IslandInventory : MonoBehaviour
    {
        [SerializeField] private InventoryPickup[] pickups;
        [SerializeField, Min(0.1f)] private float pickupDistance = 1.8f;

        private readonly HashSet<string> collected = new();
        private readonly SortedDictionary<string, (string Name, int Quantity)> items = new();

        internal IEnumerable<(string Name, int Quantity)> Items => items.Values;
        internal bool IsEmpty => items.Count == 0;

        public int GetQuantity(string itemId) =>
            itemId != null && items.TryGetValue(itemId, out var entry) ? entry.Quantity : 0;

        internal InventoryPickup FindNearby(Vector3 playerPosition, float closerThan)
        {
            InventoryPickup nearest = null;
            float rangeSquared = pickupDistance * pickupDistance;
            if (pickups == null) return null;

            foreach (var pickup in pickups)
            {
                if (pickup == null || !pickup.isActiveAndEnabled || !pickup.IsConfigured ||
                    collected.Contains(pickup.PickupId)) continue;

                float distance = (pickup.transform.position - playerPosition).sqrMagnitude;
                if (distance > rangeSquared || distance >= closerThan) continue;
                nearest = pickup;
                closerThan = distance;
            }
            return nearest;
        }

        internal bool TryCollect(InventoryPickup pickup)
        {
            if (pickup == null || !pickup.isActiveAndEnabled || !pickup.IsConfigured ||
                !collected.Add(pickup.PickupId)) return false;

            items.TryGetValue(pickup.ItemId, out var entry);
            items[pickup.ItemId] = (entry.Name ?? pickup.DisplayName, entry.Quantity + 1);
            pickup.gameObject.SetActive(false);
            return true;
        }
    }
}
