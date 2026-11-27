using System;
using System.Collections.Generic;
using Freyja.NpcAnimation;
using Freyja.Prototype;
using Freyja.NpcScheduling.Execution;
using UnityEngine;

namespace Freyja.NpcScheduling.Unity
{
    [Serializable]
    public sealed class NpcAnimatorBinding
    {
        [SerializeField] private string recipeAnimationId;
        [SerializeField] private string stateFullPath;
        [SerializeField] private int layer;
        [SerializeField,Min(0f)] private float transitionSeconds = 0.1f;
        [SerializeField,Min(0.1f)] private float timeoutSeconds = 8f;
        public string RecipeAnimationId => recipeAnimationId;
        public string StateFullPath => stateFullPath;
        public int Layer => layer;
        public float TransitionSeconds => transitionSeconds;
        public float TimeoutSeconds => timeoutSeconds;
    }

    public sealed class NpcAnimationTicket
    {
        internal readonly NpcAnimationOperationIdentity Identity;
        internal readonly int StateHash, Layer;
        internal readonly int CancelStateHash;
        internal readonly double StartedAt;
        internal readonly float TimeoutSeconds;
        internal readonly string AnimationId;
        internal readonly float TransitionSeconds;
        internal NpcAnimationTicket(ActivityOperation operation,string npcId,string executionId,int stateHash,
            int cancelStateHash,int layer,double startedAt,float timeout,float transition,string animationId)
        {
            Identity=new NpcAnimationOperationIdentity(operation,npcId,executionId);
            StateHash=stateHash;CancelStateHash=cancelStateHash;Layer=layer;StartedAt=startedAt;TimeoutSeconds=timeout;
            TransitionSeconds=transition;AnimationId=animationId;
        }
    }

    [DisallowMultipleComponent]
    public sealed class NpcAnimatorBindings : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private NpcAnimationProfile profile;
        // Kept as a migration fallback for serialized actors not yet assigned a profile.
        [SerializeField] private NpcAnimatorBinding[] bindings = Array.Empty<NpcAnimatorBinding>();
        private readonly Dictionary<string,NpcAnimatorBinding> byId = new Dictionary<string,NpcAnimatorBinding>(StringComparer.Ordinal);
        private readonly HashSet<string> duplicateIds = new HashSet<string>(StringComparer.Ordinal);
        private NpcAnimationTicket activeTicket;
        private ActivityOperation poseHoldOwner;
        private string poseHoldNpcId, poseHoldExecutionId, poseHoldAnimationId;
        private NpcAnimationDriver animationDriver;

        private void OnEnable()
        {
            animationDriver = GetComponentInParent<NpcAnimationDriver>();
            if(profile==null)profile=NpcAnimationProfile.LoadSharedDefault();
            Rebuild();
        }
        private void OnDisable()
        {
            byId.Clear();duplicateIds.Clear();
            if(activeTicket!=null)
            {
                activeTicket.Identity.TryCancel(activeTicket.Identity.Operation,activeTicket.Identity.NpcId,
                    activeTicket.Identity.ExecutionId);
                activeTicket=null;
            }
            ClearPoseHold();
            if(animationDriver!=null)animationDriver.SetAnimationOperationActive(false);
        }

        public bool TryBegin(string animationId,ActivityOperation operation,string npcId,string executionId,double now,
            out NpcAnimationTicket ticket,out string reason)
        {
            ticket=null; reason=null;
            if(operation==null||String.IsNullOrWhiteSpace(npcId)||String.IsNullOrWhiteSpace(executionId))
            {reason="animation_operation_missing";return false;}
            if(Double.IsNaN(now)||Double.IsInfinity(now)){reason="animation_time_invalid";return false;}
            if(activeTicket!=null&&activeTicket.Identity.State==NpcAnimationOperationState.Running)
            {reason="animation_operation_in_progress";return false;}
            if(animator==null||!animator.isActiveAndEnabled){reason="animator_unavailable";return false;}
            int layer;string statePath;float transition;float timeout;
            if(profile!=null)
            {
                if(!profile.TryGetAction(animationId,out NpcAnimationActionBinding action,out reason))return false;
                if(action.Mode==NpcAnimationActionMode.Continuous){reason="animation_binding_continuous";return false;}
                layer=action.Layer;statePath=action.StateFullPath;transition=action.TransitionSeconds;timeout=action.TimeoutSeconds;
            }
            else
            {
                if(duplicateIds.Contains(animationId??String.Empty)){reason="animation_binding_duplicate";return false;}
                if(!byId.TryGetValue(animationId??String.Empty,out NpcAnimatorBinding binding))
                {reason="animation_binding_missing";return false;}
                layer=binding.Layer;statePath=binding.StateFullPath;transition=binding.TransitionSeconds;timeout=binding.TimeoutSeconds;
            }
            if(layer<0||layer>=animator.layerCount){reason="animator_layer_missing";return false;}
            if(String.IsNullOrWhiteSpace(statePath)){reason="animator_state_missing";return false;}
            if(!IsFinite(timeout)||timeout<=0f||!IsFinite(transition)||transition<0f)
            {reason="animation_binding_invalid";return false;}
            int stateHash=Animator.StringToHash(statePath);
            if(!animator.HasState(layer,stateHash)){reason="animator_state_missing";return false;}
            int cancelStateHash=ResolveCancelStateHash(layer);
            if(poseHoldOwner!=null)
            {
                bool sameExecution=poseHoldNpcId==npcId&&poseHoldExecutionId==executionId;
                if(!String.Equals(animationId,"stand_up",StringComparison.Ordinal)||!sameExecution)
                {reason="animation_pose_hold_active";return false;}
                ClearPoseHold();
            }
            ticket=new NpcAnimationTicket(operation,npcId,executionId,stateHash,cancelStateHash,layer,now,timeout,
                transition,animationId);
            activeTicket=ticket;
            SetAnimationOperationActive(true);
            animator.CrossFadeInFixedTime(stateHash,transition,layer,0f);
            return true;
        }

        public bool TryStartPoseHold(string animationId,ActivityOperation owningOperation,string npcId,string executionId,
            out string reason)
        {
            reason=null;
            if(owningOperation==null||String.IsNullOrWhiteSpace(npcId)||String.IsNullOrWhiteSpace(executionId))
            {reason="animation_operation_missing";return false;}
            if(animator==null||!animator.isActiveAndEnabled){reason="animator_unavailable";return false;}
            if(profile==null||!profile.TryGetAction(animationId,out NpcAnimationActionBinding binding,out reason))
            {if(String.IsNullOrEmpty(reason))reason="animation_binding_missing";return false;}
            if(binding.Mode!=NpcAnimationActionMode.Continuous){reason="animation_binding_not_continuous";return false;}
            if(activeTicket!=null&&activeTicket.Identity.State==NpcAnimationOperationState.Running)
            {reason="animation_operation_in_progress";return false;}
            if(poseHoldOwner!=null)
            {
                if(ReferenceEquals(poseHoldOwner,owningOperation)&&poseHoldNpcId==npcId&&
                    poseHoldExecutionId==executionId&&poseHoldAnimationId==animationId)return true;
                reason="animation_pose_hold_active";
                return false;
            }
            if(binding.Layer<0||binding.Layer>=animator.layerCount){reason="animator_layer_missing";return false;}
            if(String.IsNullOrWhiteSpace(binding.StateFullPath)){reason="animator_state_missing";return false;}
            if(!IsFinite(binding.TransitionSeconds)||binding.TransitionSeconds<0f)
            {reason="animation_binding_invalid";return false;}
            int stateHash=Animator.StringToHash(binding.StateFullPath);
            if(!animator.HasState(binding.Layer,stateHash)){reason="animator_state_missing";return false;}

            animator.CrossFadeInFixedTime(stateHash,binding.TransitionSeconds,binding.Layer,0f);
            poseHoldOwner=owningOperation;
            poseHoldNpcId=npcId;
            poseHoldExecutionId=executionId;
            poseHoldAnimationId=animationId;
            SetAnimationOperationActive(true);
            return true;
        }

        public bool TryPoll(NpcAnimationTicket ticket,ActivityOperation operation,string npcId,string executionId,double now,
            out bool completed,out string reason)
        {
            completed=false; reason=null;
            if(ticket==null){reason="animation_operation_missing";return false;}
            if(!Owns(ticket,operation,npcId,executionId)){reason="animation_operation_stale";return false;}
            if(Double.IsNaN(now)||Double.IsInfinity(now)||now<ticket.StartedAt)
            {reason="animation_time_invalid";FailTicket(ticket);return false;}
            if(animator==null||!animator.isActiveAndEnabled)
            {reason="animator_unavailable";FailTicket(ticket);return false;}
            if(now-ticket.StartedAt>ticket.TimeoutSeconds){reason="animation_timeout";FailTicket(ticket);return false;}
            if(ticket.Layer<0||ticket.Layer>=animator.layerCount)
            {reason="animator_layer_missing";FailTicket(ticket);return false;}
            if(animator.IsInTransition(ticket.Layer))return true;
            AnimatorStateInfo state=animator.GetCurrentAnimatorStateInfo(ticket.Layer);
            if(state.fullPathHash!=ticket.StateHash)return true;
            if(state.loop){reason="animation_state_loops";FailTicket(ticket);return false;}
            completed=state.normalizedTime>=1f;
            if(completed)
            {
                // The seated loop owns the layer until stand_up completes. Return to the shared
                // locomotion blend tree before the Activity Core releases the Smart Object claim.
                if(String.Equals(ticket.AnimationId,"stand_up",StringComparison.Ordinal)&&
                    ticket.CancelStateHash!=0&&animator.HasState(ticket.Layer,ticket.CancelStateHash))
                    animator.CrossFadeInFixedTime(ticket.CancelStateHash,ticket.TransitionSeconds,ticket.Layer,0f);
                if(!ticket.Identity.TrySucceed(operation,npcId,executionId))
                {completed=false;reason="animation_operation_stale";return false;}
                if(ReferenceEquals(activeTicket,ticket))activeTicket=null;
                SetAnimationOperationActive(poseHoldOwner!=null);
            }
            return true;
        }

        public bool TryCancel(NpcAnimationTicket ticket,ActivityOperation operation,string npcId,string executionId,
            double now,bool preservePose)
        {
            if(ticket==null)return operation==null;
            if(!ticket.Identity.MatchesOwner(operation,npcId,executionId))return false;
            if(ticket.Identity.State!=NpcAnimationOperationState.Running)return true;
            if(!ReferenceEquals(activeTicket,ticket))return false;
            if(animator==null||!animator.isActiveAndEnabled||ticket.Layer<0||ticket.Layer>=animator.layerCount)
                return MarkCancelled(ticket);
            if(preservePose)
            {
                // Keep the operation's layer locked through cleanup. The Activity Core will
                // start stand_up for this execution before it releases the occupied resource.
                poseHoldOwner=operation;
                poseHoldNpcId=npcId;
                poseHoldExecutionId=executionId;
                poseHoldAnimationId=ticket.AnimationId;
                return MarkCancelled(ticket);
            }

            int cancelHash=ticket.CancelStateHash;
            if(cancelHash==0||!animator.HasState(ticket.Layer,cancelHash))return false;
            // The operation owns this layer until cancellation; also replace a transition that has not entered yet.
            animator.CrossFadeInFixedTime(cancelHash,ticket.TransitionSeconds,ticket.Layer,0f);
            return MarkCancelled(ticket);
        }

        private bool MarkCancelled(NpcAnimationTicket ticket)
        {
            bool cancelled=ticket.Identity.TryCancel(ticket.Identity.Operation,ticket.Identity.NpcId,
                ticket.Identity.ExecutionId);
            if(cancelled&&ReferenceEquals(activeTicket,ticket))activeTicket=null;
            if(cancelled)SetAnimationOperationActive(poseHoldOwner!=null);
            return cancelled;
        }

        private void ClearPoseHold()
        {
            poseHoldOwner=null;
            poseHoldNpcId=null;
            poseHoldExecutionId=null;
            poseHoldAnimationId=null;
            if(activeTicket==null)SetAnimationOperationActive(false);
        }

        private void FailTicket(NpcAnimationTicket ticket)
        {
            if(animator!=null&&animator.isActiveAndEnabled&&ticket.Layer>=0&&ticket.Layer<animator.layerCount&&
                ticket.CancelStateHash!=0&&animator.HasState(ticket.Layer,ticket.CancelStateHash))
            {
                animator.CrossFadeInFixedTime(ticket.CancelStateHash,ticket.TransitionSeconds,ticket.Layer,0f);
            }
            ticket.Identity.TryFail(ticket.Identity.Operation,ticket.Identity.NpcId,ticket.Identity.ExecutionId);
            if(ReferenceEquals(activeTicket,ticket))activeTicket=null;
            SetAnimationOperationActive(poseHoldOwner!=null);
        }

        private void SetAnimationOperationActive(bool active)
        {
            if(animationDriver==null&&animator!=null)animationDriver=animator.GetComponentInParent<NpcAnimationDriver>();
            if(animationDriver!=null)animationDriver.SetAnimationOperationActive(active);
        }

        private bool Owns(NpcAnimationTicket ticket,ActivityOperation operation,string npcId,string executionId)
        {return ReferenceEquals(ticket,activeTicket)&&ticket.Identity.IsCurrent(operation,npcId,executionId);}

        private int ResolveCancelStateHash(int layer)
        {
            if(profile!=null&&!String.IsNullOrWhiteSpace(profile.CancelStateFullPath))
            {
                int configured=Animator.StringToHash(profile.CancelStateFullPath);
                if(animator.HasState(layer,configured))return configured;
            }
            if(animator.IsInTransition(layer))
            {
                AnimatorStateInfo next=animator.GetNextAnimatorStateInfo(layer);
                if(next.fullPathHash!=0&&animator.HasState(layer,next.fullPathHash))return next.fullPathHash;
            }
            AnimatorStateInfo current=animator.GetCurrentAnimatorStateInfo(layer);
            return current.fullPathHash!=0&&animator.HasState(layer,current.fullPathHash)?current.fullPathHash:0;
        }

        private static bool IsFinite(float value)=>!Single.IsNaN(value)&&!Single.IsInfinity(value);

        private void Rebuild()
        {
            byId.Clear();duplicateIds.Clear();
            if(bindings==null)return;
            foreach(NpcAnimatorBinding binding in bindings)
            {
                if(binding==null||String.IsNullOrWhiteSpace(binding.RecipeAnimationId)||String.IsNullOrWhiteSpace(binding.StateFullPath))
                { Debug.LogError("Animator binding has a missing animation ID or state path.",this); continue; }
                if(byId.ContainsKey(binding.RecipeAnimationId)||duplicateIds.Contains(binding.RecipeAnimationId))
                { byId.Remove(binding.RecipeAnimationId); duplicateIds.Add(binding.RecipeAnimationId); Debug.LogError("Duplicate Animator binding '"+binding.RecipeAnimationId+"'.",this); continue; }
                byId.Add(binding.RecipeAnimationId,binding);
            }
        }
    }
}
