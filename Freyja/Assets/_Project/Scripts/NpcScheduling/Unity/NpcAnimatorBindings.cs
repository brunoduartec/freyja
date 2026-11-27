using System;
using System.Collections.Generic;
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
        internal readonly long OperationId;
        internal readonly int StateHash, Layer;
        internal readonly double StartedAt;
        internal readonly float TimeoutSeconds;
        internal readonly string AnimationId;
        internal NpcAnimationTicket(long operationId,int stateHash,int layer,double startedAt,float timeout,string animationId)
        {OperationId=operationId;StateHash=stateHash;Layer=layer;StartedAt=startedAt;TimeoutSeconds=timeout;AnimationId=animationId;}
    }

    [DisallowMultipleComponent]
    public sealed class NpcAnimatorBindings : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private NpcAnimatorBinding[] bindings = Array.Empty<NpcAnimatorBinding>();
        private readonly Dictionary<string,NpcAnimatorBinding> byId = new Dictionary<string,NpcAnimatorBinding>(StringComparer.Ordinal);
        private readonly HashSet<string> duplicateIds = new HashSet<string>(StringComparer.Ordinal);

        private void OnEnable() => Rebuild();
        private void OnDisable(){byId.Clear();duplicateIds.Clear();}

        public bool TryBegin(string animationId,long operationId,double now,out NpcAnimationTicket ticket,out string reason)
        {
            ticket=null; reason=null;
            if(animator==null||!animator.isActiveAndEnabled){reason="animator_unavailable";return false;}
            if(duplicateIds.Contains(animationId??String.Empty)){reason="animation_binding_duplicate";return false;}
            if(!byId.TryGetValue(animationId??String.Empty,out NpcAnimatorBinding binding)){reason="animation_binding_missing";return false;}
            if(binding.Layer<0||binding.Layer>=animator.layerCount){reason="animator_layer_missing";return false;}
            int stateHash=Animator.StringToHash(binding.StateFullPath);
            if(!animator.HasState(binding.Layer,stateHash)){reason="animator_state_missing";return false;}
            ticket=new NpcAnimationTicket(operationId,stateHash,binding.Layer,now,binding.TimeoutSeconds,animationId);
            animator.CrossFadeInFixedTime(stateHash,binding.TransitionSeconds,binding.Layer,0f);
            return true;
        }

        public bool TryPoll(NpcAnimationTicket ticket,double now,out bool completed,out string reason)
        {
            completed=false; reason=null;
            if(ticket==null){reason="animation_operation_missing";return false;}
            if(animator==null||!animator.isActiveAndEnabled){reason="animator_unavailable";return false;}
            if(now-ticket.StartedAt>ticket.TimeoutSeconds){reason="animation_timeout";return false;}
            if(ticket.Layer<0||ticket.Layer>=animator.layerCount){reason="animator_layer_missing";return false;}
            if(animator.IsInTransition(ticket.Layer))return true;
            AnimatorStateInfo state=animator.GetCurrentAnimatorStateInfo(ticket.Layer);
            if(state.fullPathHash!=ticket.StateHash)return true;
            if(state.loop){reason="animation_state_loops";return false;}
            completed=state.normalizedTime>=1f;
            return true;
        }

        public bool IsAnimationComplete(NpcAnimationTicket ticket)
        {
            if(ticket==null||animator==null||!animator.isActiveAndEnabled||ticket.Layer<0||ticket.Layer>=animator.layerCount)return false;
            if(animator.IsInTransition(ticket.Layer))return false;
            AnimatorStateInfo state=animator.GetCurrentAnimatorStateInfo(ticket.Layer);
            return state.fullPathHash==ticket.StateHash&&!state.loop&&state.normalizedTime>=1f;
        }

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
