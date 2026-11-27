using System;

namespace Freyja.NpcAnimation
{
    public enum NpcAnimationOperationState
    {
        Running,
        Succeeded,
        Failed,
        Cancelled
    }

    public sealed class NpcAnimationOperationIdentity
    {
        public object Operation { get; }
        public string NpcId { get; }
        public string ExecutionId { get; }
        public NpcAnimationOperationState State { get; private set; }

        public NpcAnimationOperationIdentity(object operation, string npcId, string executionId)
        {
            Operation = operation;
            NpcId = npcId;
            ExecutionId = executionId;
            State = NpcAnimationOperationState.Running;
        }

        public bool IsCurrent(object operation, string npcId, string executionId)
        {
            return State == NpcAnimationOperationState.Running && MatchesOwner(operation, npcId, executionId);
        }

        public bool TrySucceed(object operation, string npcId, string executionId)
        {
            if (!IsCurrent(operation, npcId, executionId)) return false;
            State = NpcAnimationOperationState.Succeeded;
            return true;
        }

        public bool TryFail(object operation, string npcId, string executionId)
        {
            if (!IsCurrent(operation, npcId, executionId)) return false;
            State = NpcAnimationOperationState.Failed;
            return true;
        }

        public bool TryCancel(object operation, string npcId, string executionId)
        {
            if (!MatchesOwner(operation, npcId, executionId)) return false;
            if (State != NpcAnimationOperationState.Running) return true;
            State = NpcAnimationOperationState.Cancelled;
            return true;
        }

        public bool MatchesOwner(object operation, string npcId, string executionId)
        {
            return ReferenceEquals(Operation, operation) &&
                String.Equals(NpcId, npcId, StringComparison.Ordinal) &&
                String.Equals(ExecutionId, executionId, StringComparison.Ordinal);
        }
    }
}
