using System;

namespace Freyja.SmartObjects
{
    public readonly struct ReservationHandle : IEquatable<ReservationHandle>
    {
        internal readonly SmartObject Object;
        internal readonly SmartObjectReservationOwner Owner;
        internal readonly string Token;

        public string ObjectId { get; }
        public string SlotId { get; }
        public bool IsValid => SmartObjectRegistry.IsValid(this);

        internal ReservationHandle(SmartObject smartObject, string slotId, SmartObjectReservationOwner owner, string token)
        {
            Object = smartObject;
            ObjectId = smartObject != null ? smartObject.ObjectId : null;
            SlotId = slotId;
            Owner = owner;
            Token = token;
        }

        public bool Equals(ReservationHandle other) => Object == other.Object && Owner == other.Owner && ObjectId == other.ObjectId && SlotId == other.SlotId && Token == other.Token;
        public override bool Equals(object obj) => obj is ReservationHandle other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(ObjectId, SlotId, Token, Owner);
        public static bool operator ==(ReservationHandle left, ReservationHandle right) => left.Equals(right);
        public static bool operator !=(ReservationHandle left, ReservationHandle right) => !left.Equals(right);

        public bool TryOccupy() => SmartObjectRegistry.TryOccupy(this);
        public bool Release() => SmartObjectRegistry.Release(this);
    }
}
