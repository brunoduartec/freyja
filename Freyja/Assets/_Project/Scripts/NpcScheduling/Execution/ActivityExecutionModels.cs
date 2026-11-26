using System;
using System.Collections.Generic;

namespace Freyja.NpcScheduling.Execution
{
    public enum ActivityState { Pending, Preparing, Moving, Executing, CleaningUp, Completed, Cancelled, Failed }
    public enum OperationStatus { Running, Succeeded, Failed, Cancelled }
    public enum ExecutionEventKind { ActivityStarted, StepStarted, StepCompleted, ActivityCompleted, ActivityCancelled, ActivityFailed, ResourceReserved, ResourceReleased }

    public sealed class ActivityStep
    {
        public string Action { get; set; }
        public string Capability { get; set; }
        public string Target { get; set; }
        public string Point { get; set; }
        public string Animation { get; set; }
        public string Until { get; set; }
        public int DurationSeconds { get; set; }
    }

    public sealed class ActivityRecipeFile
    {
        public int SchemaVersion { get; set; }
        public List<ActivityRecipe> Recipes { get; set; }
    }

    public sealed class ActivityRecipe
    {
        public string ActivityId { get; set; }
        public List<ActivityStep> Steps { get; set; }
    }

    public sealed class ActivitySnapshot
    {
        public string NpcId { get; internal set; }
        public string ExecutionId { get; internal set; }
        public string IntendedActivityId { get; internal set; }
        public string ExecutedActivityId { get; internal set; }
        public string TargetId { get; internal set; }
        public string EffectiveTargetId { get; internal set; }
        public ActivityState State { get; internal set; }
        public string CurrentOperation { get; internal set; }
        public string ReasonCode { get; internal set; }
        public double StartedAtSeconds { get; internal set; }
        public double FinishedAtSeconds { get; internal set; }
        public string ResourceId { get; internal set; }
        public string ResidualOperationId { get; internal set; }
        public bool HasResidualOwnership { get { return ResourceId!=null || ResidualOperationId!=null; } }
        public bool IsFallback { get; internal set; }
    }

    public sealed class ActivityExecutionEvent
    {
        public long Sequence { get; internal set; }
        public ExecutionEventKind Kind { get; internal set; }
        public string NpcId { get; internal set; }
        public string ExecutionId { get; internal set; }
        public string IntendedActivityId { get; internal set; }
        public string ExecutedActivityId { get; internal set; }
        public string TargetId { get; internal set; }
        public string Step { get; internal set; }
        public string ReasonCode { get; internal set; }
        public int AbsoluteDay { get; internal set; }
        public int MinuteOfDay { get; internal set; }
        public double OperationalSeconds { get; internal set; }
        public bool Simulated { get { return true; } }
    }

    public sealed class ActivityOperation
    {
        public readonly string Id;
        public readonly string Action;
        public readonly string Target;
        public readonly string Point;
        public readonly string Animation;
        public readonly double StartedAt;
        public ActivityOperation(string id, string action, string target, string point, string animation, double startedAt)
        { Id=id; Action=action; Target=target; Point=point; Animation=animation; StartedAt=startedAt; }
    }

    public sealed class ActivityOperationResult
    {
        public readonly OperationStatus Status;
        public readonly string ReasonCode;
        public readonly string EffectiveTargetId;
        public ActivityOperationResult(OperationStatus status, string reasonCode, string effectiveTargetId = null)
        { Status=status; ReasonCode=reasonCode; EffectiveTargetId=effectiveTargetId; }
        public static ActivityOperationResult Running { get { return new ActivityOperationResult(OperationStatus.Running, null); } }
        public static ActivityOperationResult Succeeded(string effectiveTargetId = null) { return new ActivityOperationResult(OperationStatus.Succeeded, null, effectiveTargetId); }
        public static ActivityOperationResult Failed(string reason) { return new ActivityOperationResult(OperationStatus.Failed, reason); }
    }

    public sealed class ActivityCandidate
    {
        public readonly string Id, TargetId, ApproachPoint, PosePoint;
        public readonly double Distance;
        public ActivityCandidate(string id, string targetId, string approachPoint, string posePoint, double distance)
        { Id=id; TargetId=targetId; ApproachPoint=approachPoint; PosePoint=posePoint; Distance=distance; }
    }

    public interface IActivityPorts
    {
        IList<ActivityCandidate> FindTargets(string targetId, string capability, string npcId);
        IList<string> FindWaypoints(string regionId, string npcId);
        bool TryReserve(ActivityCandidate candidate, string npcId, string executionId, out string token);
        bool IsReservationValid(string token, string npcId, string executionId);
        bool Occupy(string token, string npcId, string executionId);
        bool Release(string token, string npcId, string executionId);
        ActivityOperation Begin(string action, string npcId, string executionId, string targetId, string point, string animation, double nowSeconds);
        ActivityOperationResult Poll(ActivityOperation operation, string npcId, string executionId, double nowSeconds);
        bool Cancel(ActivityOperation operation, string npcId, string executionId);
        void RestorePose(string npcId, string executionId, string targetId);
    }

    public interface IActivityEventSink { void Emit(ActivityExecutionEvent activityEvent); }
}
