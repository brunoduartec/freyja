using UnityEngine;

namespace Freyja.Prototype
{
    public sealed class InventoryPickup : MonoBehaviour
    {
        [SerializeField] private string pickupId;
        [SerializeField] private string itemId;
        [SerializeField] private string displayName;

        internal string PickupId => pickupId;
        internal string ItemId => itemId;
        internal string DisplayName => displayName;
        internal bool IsConfigured => !string.IsNullOrWhiteSpace(pickupId) &&
            !string.IsNullOrWhiteSpace(itemId) && !string.IsNullOrWhiteSpace(displayName);
    }
}
