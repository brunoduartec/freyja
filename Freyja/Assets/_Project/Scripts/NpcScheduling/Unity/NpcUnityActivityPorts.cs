using System;
using System.Collections.Generic;
using Freyja.NpcScheduling.Execution;
using Freyja.Prototype;
using Freyja.SmartObjects;
using UnityEngine;

namespace Freyja.NpcScheduling.Unity
{
    [DisallowMultipleComponent]
    public sealed class NpcUnityActivityPorts : MonoBehaviour, IActivityPorts
    {
        [SerializeField,Min(0.01f)] private float moveTimeoutSeconds = 90f;
        [SerializeField,Min(0.01f)] private float arrivalTolerance = 0.3f;
        [SerializeField,Min(0.01f)] private float alignmentTolerance = 0.6f;

        private sealed class Claim
        {
            public ReservationHandle Handle;
            public SmartObjectReservationOwner Owner;
            public string NpcId, ExecutionId;
        }
        private sealed class Operation
        {
            public ActivityOperation Value;
            public string NpcId, ExecutionId;
            public Transform Target;
            public long MoveTicket;
            public NpcAnimationTicket AnimationTicket;
            public OperationStatus Status;
            public string Reason;
        }

        private readonly Dictionary<string,SmartObjectCandidate> candidates=new Dictionary<string,SmartObjectCandidate>(StringComparer.Ordinal);
        private readonly Dictionary<string,Transform> smartObjectPoints=new Dictionary<string,Transform>(StringComparer.Ordinal);
        private readonly Dictionary<string,Claim> claims=new Dictionary<string,Claim>(StringComparer.Ordinal);
        private readonly Dictionary<string,Operation> operations=new Dictionary<string,Operation>(StringComparer.Ordinal);
        private string npcId;
        private SmartObjectReservationOwner owner;
        private NpcLocationRegistry locations;
        private NpcAnimatorBindings animations;
        private IslandWaypointWalker walker;
        private Transform actor;
        private long candidateSequence,operationSequence,claimSequence;

        public void Initialize(string actorId,SmartObjectReservationOwner reservationOwner,
            NpcLocationRegistry locationRegistry,NpcAnimatorBindings animatorBindings,Transform actorTransform)
        {
            npcId=actorId; owner=reservationOwner; locations=locationRegistry; animations=animatorBindings; actor=actorTransform;
            walker=actorTransform!=null?actorTransform.GetComponent<IslandWaypointWalker>():null;
            if(String.IsNullOrWhiteSpace(npcId)||owner==null||locations==null||animations==null||actor==null||walker==null)
                throw new InvalidOperationException("activity_ports_dependency_missing: requires owner, location registry, animator bindings, actor and existing IslandWaypointWalker");
            if(!locations.UsesNetwork(walker.Network))
                throw new InvalidOperationException("location_registry_waypoint_network_mismatch");
            if(!owner.isActiveAndEnabled||!walker.isActiveAndEnabled)throw new InvalidOperationException("activity_ports_component_disabled");
        }

        public IList<ActivityCandidate> FindTargets(string targetId,string capability,string requestedNpcId)
        {
            var results=new List<ActivityCandidate>();
            if(!IsNpc(requestedNpcId)||actor==null||String.IsNullOrWhiteSpace(targetId)||String.IsNullOrWhiteSpace(capability))return results;
            candidates.Clear(); smartObjectPoints.Clear();
            IReadOnlyList<SmartObjectCandidate> found=SmartObjectRegistry.FindCandidates(actor.position,capability,targetId);
            foreach(SmartObjectCandidate item in found)
            {
                if(item.Object==null||item.Slot==null||item.Slot.Approach==null||item.Slot.Pose==null)continue;
                string key="candidate_"+(++candidateSequence).ToString("D8");
                string approach=key+"_approach",pose=key+"_pose";
                candidates.Add(key,item);
                smartObjectPoints.Add(approach,item.Slot.Approach);
                smartObjectPoints.Add(pose,item.Slot.Pose);
                results.Add(new ActivityCandidate(key,item.Object.ObjectId,approach,pose,Math.Sqrt(item.DistanceSquared)));
            }
            return results;
        }

        public IList<string> FindWaypoints(string regionId,string requestedNpcId)
        { return IsNpc(requestedNpcId)&&locations!=null?locations.FindWaypointIds(regionId):new List<string>(); }

        public bool TryReserve(ActivityCandidate candidate,string requestedNpcId,string executionId,out string token)
        {
            token=null;
            if(!IsNpc(requestedNpcId)||owner==null||String.IsNullOrWhiteSpace(executionId)||candidate==null||
                !candidates.TryGetValue(candidate.Id,out SmartObjectCandidate source)||source.Object==null||source.Slot==null)return false;
            if(!String.Equals(candidate.TargetId,source.Object.ObjectId,StringComparison.Ordinal))return false;
            if(!SmartObjectRegistry.TryReserve(source.Object,source.Slot.SlotId,source.ActivityId,owner,out ReservationHandle handle,out _))return false;
            if(!handle.IsValid)
            {
                owner.Release(handle);
                return false;
            }
            token=npcId+"/"+executionId+"/claim_"+(++claimSequence).ToString("D8");
            claims.Add(token,new Claim{Handle=handle,Owner=owner,NpcId=npcId,ExecutionId=executionId});
            return true;
        }

        public bool IsReservationValid(string token,string requestedNpcId,string executionId)
        { return TryGetClaim(token,requestedNpcId,executionId,out Claim claim)&&claim.Handle.IsValid; }

        public bool Occupy(string token,string requestedNpcId,string executionId)
        {
            if(!TryGetClaim(token,requestedNpcId,executionId,out Claim claim)||!claim.Handle.IsValid)return false;
            return claim.Handle.TryOccupy();
        }

        public bool Release(string token,string requestedNpcId,string executionId)
        {
            if(!TryGetClaim(token,requestedNpcId,executionId,out Claim claim))return false;
            if(!claim.Owner.Release(claim.Handle))return false;
            claims.Remove(token);
            return true;
        }

        public ActivityOperation Begin(string action,string requestedNpcId,string executionId,
            string targetId,string point,string animation,double nowSeconds)
        {
            if(!IsNpc(requestedNpcId)||String.IsNullOrWhiteSpace(executionId))return null;
            foreach(Operation active in operations.Values)
                if(active.Status==OperationStatus.Running)return null;
            var value=new ActivityOperation("npcop_"+(++operationSequence).ToString("D8"),action,targetId,point,animation,nowSeconds);
            var operation=new Operation{Value=value,NpcId=npcId,ExecutionId=executionId,Status=OperationStatus.Running};
            operations.Add(value.Id,operation);
            switch(action)
            {
                case "move_to": BeginMove(operation,targetId,point); break;
                case "align_to": Align(operation,point); break;
                case "play_animation": BeginAnimation(operation,animation,nowSeconds); break;
                default: SetFailed(operation,"unsupported_operation"); break;
            }
            return value;
        }

        public ActivityOperationResult Poll(ActivityOperation operation,string requestedNpcId,string executionId,double nowSeconds)
        {
            if(!TryGetOperation(operation,requestedNpcId,executionId,out Operation active))return ActivityOperationResult.Failed("operation_owner_mismatch");
            if(active.Status==OperationStatus.Failed)return FailAndForget(active,active.Reason);
            if(active.Status==OperationStatus.Cancelled){operations.Remove(active.Value.Id);return new ActivityOperationResult(OperationStatus.Cancelled,"operation_cancelled");}
            if(active.Value.Action=="move_to"&&(active.Target==null||!active.Target.gameObject.activeInHierarchy))
                return FailMovementAndForget(active,"destination_unavailable");
            if(active.Status==OperationStatus.Succeeded)return SucceedAndForget(active);
            if(HasOwnedClaim(executionId)&&!IsOwnedClaimValid(executionId))
                return active.Value.Action=="move_to"?FailMovementAndForget(active,"reservation_invalid"):FailAndForgetAfterSet(active,"reservation_invalid");
            if(nowSeconds-active.Value.StartedAt>moveTimeoutSeconds)
                return active.Value.Action=="move_to"?FailMovementAndForget(active,"operation_timeout"):FailAndForgetAfterSet(active,"operation_timeout");

            if(active.Value.Action=="move_to")
            {
                WaypointMoveStatus status=walker.GetMoveStatus(active.MoveTicket);
                if(status==WaypointMoveStatus.Running)return ActivityOperationResult.Running;
                if(status!=WaypointMoveStatus.Succeeded){SetFailed(active,status==WaypointMoveStatus.Unknown?"movement_operation_stale":"path_unreachable");return FailAndForget(active,active.Reason);}
                if(Vector3.Distance(actor.position,active.Target.position)>arrivalTolerance){SetFailed(active,"arrival_tolerance_not_met");return FailAndForget(active,active.Reason);}
                active.Status=OperationStatus.Succeeded;
                return SucceedAndForget(active);
            }

            if(active.Value.Action=="play_animation")
            {
                if(!animations.TryPoll(active.AnimationTicket,nowSeconds,out bool completed,out string reason))
                {SetFailed(active,reason??"animation_failed");return FailAndForget(active,active.Reason);}
                if(!completed)return ActivityOperationResult.Running;
                active.Status=OperationStatus.Succeeded;
                return SucceedAndForget(active);
            }

            return FailAndForget(active,"operation_state_invalid");
        }

        public bool Cancel(ActivityOperation operation,string requestedNpcId,string executionId,double nowSeconds)
        {
            if(!TryGetOperation(operation,requestedNpcId,executionId,out Operation active))return operation==null;
            if(active.Status!=OperationStatus.Running){operations.Remove(active.Value.Id);return true;}
            if(active.Value.Action=="move_to")
            {
                if(!walker.CancelMove(active.MoveTicket))return false;
                active.Status=OperationStatus.Cancelled; operations.Remove(active.Value.Id); return true;
            }
            if(active.Value.Action=="play_animation")
            {
                if(animations.TryPoll(active.AnimationTicket,nowSeconds,out bool completed,out _ )&& !completed)return false;
                // A completed or expired ticket no longer owns the animator operation. Core
                // cleanup immediately starts its own bounded stand_up operation.
                active.Status=OperationStatus.Cancelled; operations.Remove(active.Value.Id); return true;
            }
            active.Status=OperationStatus.Cancelled; operations.Remove(active.Value.Id);
            return true;
        }

        private void BeginMove(Operation operation,string targetId,string pointId)
        {
            string resolved=pointId=="target"?targetId:pointId;
            if(!TryResolvePoint(resolved,out Transform target)){SetFailed(operation,"destination_unavailable");return;}
            if(!walker.TryBeginMove(target,out long ticket)){SetFailed(operation,"path_unavailable");return;}
            operation.Target=target;operation.MoveTicket=ticket;
        }

        private void Align(Operation operation,string pointId)
        {
            if(!TryResolvePoint(pointId,out Transform pose)){SetFailed(operation,"pose_point_unavailable");return;}
            if(Vector3.Distance(actor.position,pose.position)>alignmentTolerance){SetFailed(operation,"pose_alignment_out_of_range");return;}
            if(!walker.CancelMove(GetCurrentMovementTicket()))
            {
                // A completed movement has already stopped the walker; a stale ticket cannot move it.
                if(walker.IsWalking){SetFailed(operation,"movement_owner_mismatch");return;}
            }
            actor.SetPositionAndRotation(pose.position,pose.rotation);
            operation.Target=pose;operation.Status=OperationStatus.Succeeded;
        }

        private void BeginAnimation(Operation operation,string animationId,double nowSeconds)
        {
            if(!animations.TryBegin(animationId,operationSequence,nowSeconds,out NpcAnimationTicket ticket,out string reason))
            {SetFailed(operation,reason??"animation_not_started");return;}
            operation.AnimationTicket=ticket;
        }

        private long GetCurrentMovementTicket()
        {
            foreach(Operation operation in operations.Values)
                if(operation.Value.Action=="move_to"&&operation.MoveTicket!=0)return operation.MoveTicket;
            return 0;
        }

        private bool TryResolvePoint(string pointId,out Transform point)
        {
            if(smartObjectPoints.TryGetValue(pointId??String.Empty,out point)&&point!=null&&point.gameObject.activeInHierarchy)return true;
            return locations!=null&&locations.TryGetPoint(pointId,out point);
        }

        private bool TryGetClaim(string token,string requestedNpcId,string executionId,out Claim claim)
        {
            claim=null;
            return IsNpc(requestedNpcId)&&!String.IsNullOrWhiteSpace(token)&&!String.IsNullOrWhiteSpace(executionId)&&
                claims.TryGetValue(token,out claim)&&claim.NpcId==npcId&&claim.ExecutionId==executionId&&claim.Owner==owner;
        }

        private bool HasOwnedClaim(string executionId)
        {
            foreach(Claim claim in claims.Values)if(claim.NpcId==npcId&&claim.ExecutionId==executionId)return true;
            return false;
        }

        private bool IsOwnedClaimValid(string executionId)
        {
            foreach(Claim claim in claims.Values)if(claim.NpcId==npcId&&claim.ExecutionId==executionId)return claim.Handle.IsValid;
            return false;
        }

        private bool TryGetOperation(ActivityOperation value,string requestedNpcId,string executionId,out Operation operation)
        {
            operation=null;
            return value!=null&&IsNpc(requestedNpcId)&&!String.IsNullOrWhiteSpace(executionId)&&operations.TryGetValue(value.Id,out operation)&&
                ReferenceEquals(operation.Value,value)&&operation.NpcId==npcId&&operation.ExecutionId==executionId;
        }

        private bool IsNpc(string value)=>String.Equals(value,npcId,StringComparison.Ordinal);
        private static void SetFailed(Operation operation,string reason){operation.Status=OperationStatus.Failed;operation.Reason=reason;}
        private ActivityOperationResult FailMovementAndForget(Operation operation,string reason)
        {
            if(walker!=null&&operation.MoveTicket!=0)walker.CancelMove(operation.MoveTicket);
            return FailAndForgetAfterSet(operation,reason);
        }
        private ActivityOperationResult FailAndForgetAfterSet(Operation operation,string reason)
        {SetFailed(operation,reason);return FailAndForget(operation,reason);}
        private ActivityOperationResult FailAndForget(Operation operation,string reason)
        { operations.Remove(operation.Value.Id); return ActivityOperationResult.Failed(reason??"operation_failed"); }
        private ActivityOperationResult SucceedAndForget(Operation operation)
        { operations.Remove(operation.Value.Id); return ActivityOperationResult.Succeeded(operation.Value.Target); }
    }
}
