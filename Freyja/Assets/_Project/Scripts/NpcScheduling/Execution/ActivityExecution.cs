using System;
using System.Collections.Generic;
using System.Linq;
using Freyja.NpcScheduling;

namespace Freyja.NpcScheduling.Execution
{
    public sealed class ActivityExecution
    {
        private readonly ActivityRecipe recipe;
        private readonly IActivityPorts ports;
        private readonly IActivityEventSink sink;
        private readonly List<string> resources = new List<string>();
        private ActivityIntent intent;
        private ActivityOperation operation;
        private ActivityCandidate candidate;
        private IList<ActivityCandidate> candidates;
        private IList<string> waypoints;
        private int candidateIndex, waypointIndex, stepIndex;
        private double lastOperationalSeconds, stepStartedSeconds, startedSeconds;
        private int lastGameMinute;
        private bool initialized, occupied, waitStarted, scheduleEndedNormally, operationCancelPending;
        private string reason, effectiveTarget;
        private readonly int maxTransitionsPerTick;

        public readonly string ExecutionId;
        public readonly string IntendedActivityId;
        public readonly string ExecutedActivityId;
        public readonly bool IsFallback;
        public ActivityState State { get; private set; }
        public ActivityIntent Intent { get { return intent; } }
        public ActivitySnapshot Snapshot { get { return MakeSnapshot(); } }

        public ActivityExecution(string executionId, ActivityIntent intent, ActivityRecipe recipe, bool fallback,
            IActivityPorts ports, IActivityEventSink sink, int maxTransitionsPerTick = 12)
        {
            if (intent == null || recipe == null || ports == null) throw new ArgumentNullException();
            if (maxTransitionsPerTick < 1) throw new ArgumentOutOfRangeException("maxTransitionsPerTick");
            ExecutionId=executionId; this.intent=intent; this.recipe=recipe; IsFallback=fallback;
            IntendedActivityId=intent.ActivityId; ExecutedActivityId=recipe.ActivityId;
            this.ports=ports; this.sink=sink; this.maxTransitionsPerTick=maxTransitionsPerTick; State=ActivityState.Pending;
        }

        public bool IsTerminal { get { return State==ActivityState.Completed || State==ActivityState.Cancelled || State==ActivityState.Failed; } }
        public string CurrentAction { get { return operation==null ? (stepIndex<recipe.Steps.Count?recipe.Steps[stepIndex].Action:null) : operation.Action; } }

        public void UpdateIntent(ActivityIntent updated) { if(updated==null)throw new ArgumentNullException("updated"); intent=updated; }

        public ActivitySnapshot Tick(double operationalSeconds, GameTimeSnapshot gameTime)
        {
            if(gameTime==null)throw new ArgumentNullException("gameTime");
            int gameMinute=checked((gameTime.AbsoluteDay-1)*1440+gameTime.MinuteOfDay);
            if (!initialized)
            {
                initialized=true; lastOperationalSeconds=operationalSeconds; lastGameMinute=gameMinute; startedSeconds=operationalSeconds;
                State=ActivityState.Preparing; Emit(ExecutionEventKind.ActivityStarted,null,null);
            }
            else if(operationalSeconds<lastOperationalSeconds || gameMinute<lastGameMinute)
            { Fail("clock_regressed",operationalSeconds,gameTime); return Snapshot; }
            lastOperationalSeconds=operationalSeconds; lastGameMinute=gameMinute;
            if(IsTerminal)return Snapshot;

            bool expired=HasExpired(intent,gameTime);
            bool scheduleWait=stepIndex<recipe.Steps.Count && recipe.Steps[stepIndex].Action=="wait" && recipe.Steps[stepIndex].Until=="schedule_end";
            if(expired && !scheduleWait && !scheduleEndedNormally){ Cancel("schedule_expired",operationalSeconds,gameTime); return Snapshot; }

            for(int transitions=0;transitions<maxTransitionsPerTick&&!IsTerminal;transitions++)
            {
                if(stepIndex>=recipe.Steps.Count){ Complete(operationalSeconds,gameTime); break; }
                ActivityStep step=recipe.Steps[stepIndex];
                if(State==ActivityState.Preparing && stepIndex==0) Emit(ExecutionEventKind.StepStarted,step.Action,null);
                OperationStatus outcome;
                try{outcome=Process(step,operationalSeconds,gameTime);}
                catch{reason="adapter_exception";outcome=OperationStatus.Failed;}
                if(outcome==OperationStatus.Running)break;
                if(outcome==OperationStatus.Failed){Fail(reason??"operation_failed",operationalSeconds,gameTime);break;}
                if(outcome==OperationStatus.Cancelled){Cancel(reason??"operation_cancelled",operationalSeconds,gameTime);break;}
                Emit(ExecutionEventKind.StepCompleted,step.Action,null);
                bool repeatWander=recipe.ActivityId=="wander"&&step.Action=="wait"&&step.Until=="duration"&&!HasExpired(intent,gameTime);
                stepIndex=repeatWander?1:stepIndex+1; operation=null; stepStartedSeconds=0;waitStarted=false;
                if(stepIndex<recipe.Steps.Count)Emit(ExecutionEventKind.StepStarted,recipe.Steps[stepIndex].Action,null);
            }
            return Snapshot;
        }

        public void Cancel(string code, double now, GameTimeSnapshot gameTime)
        {
            if(IsTerminal)return; reason=code??"external_cancel";
            lastOperationalSeconds=now;lastGameMinute=checked((gameTime.AbsoluteDay-1)*1440+gameTime.MinuteOfDay);
            TryCancelOperation(now);
            if(occupied&&!operationCancelPending)PollPoseRestore(now);
            if(!occupied)Cleanup(now,gameTime);
            State=ActivityState.Cancelled; Emit(ExecutionEventKind.ActivityCancelled,null,operationCancelPending?"operation_cancel_failed":reason);
        }

        private OperationStatus Process(ActivityStep step,double now,GameTimeSnapshot gameTime)
        {
            if(step.Action=="find_target")
            {
                string target=step.Target=="intent_target"?intent.TargetId:step.Target;
                candidates=ports.FindTargets(target,step.Capability,intent.NpcId)??new List<ActivityCandidate>();
                candidates=candidates.OrderBy(x=>x.Distance).ThenBy(x=>x.Id,StringComparer.Ordinal).ToList();candidateIndex=0;candidate=null;
                if(candidates.Count==0){reason="target_unavailable";return OperationStatus.Failed;}
                candidate=candidates[0];effectiveTarget=candidate.TargetId;return OperationStatus.Succeeded;
            }
            if(step.Action=="reserve_slot")
            {
                while(candidates!=null&&candidateIndex<candidates.Count)
                {
                    var value=candidates[candidateIndex++];string token;
                    if(ports.TryReserve(value,intent.NpcId,ExecutionId,out token)&&!String.IsNullOrEmpty(token))
                    {candidate=value;resources.Add(token);effectiveTarget=value.TargetId;Emit(ExecutionEventKind.ResourceReserved,"reserve_slot",null);return OperationStatus.Succeeded;}
                }
                reason="resource_unavailable";return OperationStatus.Failed;
            }
            if(step.Action=="occupy_slot")
            {
                string token=CurrentToken();if(token==null||!ports.IsReservationValid(token,intent.NpcId,ExecutionId)){reason="reservation_invalid";return OperationStatus.Failed;}
                if(!ports.Occupy(token,intent.NpcId,ExecutionId)){reason="occupy_rejected";return OperationStatus.Failed;}occupied=true;return OperationStatus.Succeeded;
            }
            if(step.Action=="release_slot") return ReleaseCurrent(now,gameTime)?OperationStatus.Succeeded:OperationStatus.Failed;
            if(step.Action=="find_waypoints")
            {
                waypoints=ports.FindWaypoints(intent.TargetId,intent.NpcId)??new List<string>();waypointIndex=0;
                if(waypoints.Count==0){reason="target_unavailable";return OperationStatus.Failed;}return OperationStatus.Succeeded;
            }
            if(step.Action=="wait")
            {
                if(step.Until=="schedule_end")
                {
                    if(!HasExpired(intent,gameTime))return OperationStatus.Running;
                    scheduleEndedNormally=true;return OperationStatus.Succeeded;
                }
                if(!waitStarted){stepStartedSeconds=now;waitStarted=true;}
                return now-stepStartedSeconds>=step.DurationSeconds?OperationStatus.Succeeded:OperationStatus.Running;
            }
            if(step.Action=="move_to"||step.Action=="align_to"||step.Action=="play_animation")
            {
                if(operation==null)
                {
                    string target=effectiveTarget??intent.TargetId;string point=step.Point;
                    if(step.Action=="move_to")
                    {
                        if(step.Point=="approach")point=candidate==null?null:candidate.ApproachPoint;
                        else if(step.Point=="next")
                        {
                            if(waypoints==null||waypoints.Count==0){reason="target_unavailable";return OperationStatus.Failed;}
                            point=waypoints[waypointIndex%waypoints.Count];waypointIndex++;
                        }
                        else if(step.Point=="target" && String.IsNullOrEmpty(intent.TargetId))return OperationStatus.Succeeded;
                    }
                    if(step.Action=="align_to")point=candidate==null?step.Point:candidate.PosePoint;
                    operation=ports.Begin(step.Action,intent.NpcId,ExecutionId,target,point,step.Animation,now);
                    if(operation==null){reason="operation_not_started";return OperationStatus.Failed;}
                    State=step.Action=="move_to"?ActivityState.Moving:ActivityState.Executing;
                }
                var result=ports.Poll(operation,intent.NpcId,ExecutionId,now);
                if(result==null){reason="operation_invalid_result";return OperationStatus.Failed;}
                if(result.Status==OperationStatus.Succeeded){if(result.EffectiveTargetId!=null)effectiveTarget=result.EffectiveTargetId;operation=null;return OperationStatus.Succeeded;}
                if(result.Status==OperationStatus.Failed){reason=result.ReasonCode??(step.Action=="move_to"?"path_failed":step.Action=="play_animation"?"animation_rejected":"alignment_failed");operation=null;return OperationStatus.Failed;}
                if(result.Status==OperationStatus.Cancelled){reason="operation_cancelled";operation=null;return OperationStatus.Cancelled;}
                return OperationStatus.Running;
            }
            reason="unknown_operation";return OperationStatus.Failed;
        }

        private void Complete(double now,GameTimeSnapshot time)
        {
            Cleanup(now,time);if(resources.Count>0||operationCancelPending){State=ActivityState.Failed;reason=operationCancelPending?"operation_cancel_failed":"cleanup_failed";Emit(ExecutionEventKind.ActivityFailed,null,reason);return;}
            State=ActivityState.Completed;Emit(ExecutionEventKind.ActivityCompleted,null,null);
        }
        private void Fail(string code,double now,GameTimeSnapshot time)
        {
            if(IsTerminal)return;reason=code;
            TryCancelOperation(now);
            if(occupied&&!operationCancelPending)PollPoseRestore(now);
            if(!occupied)Cleanup(now,time);
            State=ActivityState.Failed;Emit(ExecutionEventKind.ActivityFailed,null,operationCancelPending?"operation_cancel_failed":occupied?reason:resources.Count>0?"cleanup_failed":reason);
        }
        private bool TryCancelOperation(double now)
        {
            if(operation==null){operationCancelPending=false;return true;}
            bool cancelled=false;try{cancelled=ports.Cancel(operation,intent.NpcId,ExecutionId,now);}catch{cancelled=false;}
            if(!cancelled){operationCancelPending=true;reason="operation_cancel_failed";return false;}
            operation=null;operationCancelPending=false;return true;
        }
        private void Cleanup(double now,GameTimeSnapshot time)
        {
            State=ActivityState.CleaningUp;
            if(operationCancelPending)return;
            for(int i=resources.Count-1;i>=0;i--)
            {
                string token=resources[i];bool released=false;try{released=ports.Release(token,intent.NpcId,ExecutionId);}catch{released=false;}
                if(released){resources.RemoveAt(i);occupied=false;Emit(ExecutionEventKind.ResourceReleased,"release_slot",null);}
                else reason="cleanup_failed";
            }
        }
        private bool ReleaseCurrent(double now,GameTimeSnapshot time)
        {
            string token=CurrentToken();if(token==null)return true;
            bool released=false;try{released=ports.Release(token,intent.NpcId,ExecutionId);}catch{released=false;}
            if(!released){reason="cleanup_failed";return false;}resources.Remove(token);occupied=false;Emit(ExecutionEventKind.ResourceReleased,"release_slot",null);return true;
        }
        public bool RetryCleanup(double now,GameTimeSnapshot time)
        {
            if(!IsTerminal)return false;
            if(operationCancelPending&&!TryCancelOperation(now))return false;
            if(occupied)
            {
                OperationStatus restore=PollPoseRestore(now);
                if(restore!=OperationStatus.Succeeded)return false;
            }
            bool all=true;
            for(int i=resources.Count-1;i>=0;i--){string token=resources[i];bool released=false;try{released=ports.Release(token,intent.NpcId,ExecutionId);}catch{}if(released){resources.RemoveAt(i);occupied=false;Emit(ExecutionEventKind.ResourceReleased,"cleanup_retry",null);}else all=false;}
            if(all&&(reason=="cleanup_failed"||reason=="operation_cancel_failed"))reason="cleanup_recovered";
            return all;
        }
        private string CurrentToken(){return resources.Count==0?null:resources[resources.Count-1];}
        private OperationStatus PollPoseRestore(double now)
        {
            if(operation==null)
            {
                try{operation=ports.Begin("play_animation",intent.NpcId,ExecutionId,effectiveTarget,null,"stand_up",now);}
                catch{operation=null;}
                if(operation==null){reason="pose_restore_failed";return OperationStatus.Failed;}
            }
            ActivityOperation cleanup=operation;
            ActivityOperationResult result;
            try{result=ports.Poll(cleanup,intent.NpcId,ExecutionId,now);}
            catch{reason="pose_restore_failed";return OperationStatus.Running;}
            if(result==null){reason="pose_restore_failed";return OperationStatus.Running;}
            if(result.Status==OperationStatus.Running)return OperationStatus.Running;
            operation=null;
            if(result.Status!=OperationStatus.Succeeded){reason=result.ReasonCode??"pose_restore_failed";return OperationStatus.Failed;}
            occupied=false;
            return OperationStatus.Succeeded;
        }
        private static bool HasExpired(ActivityIntent i,GameTimeSnapshot time)
        {return time.AbsoluteDay>i.AbsoluteDay || time.AbsoluteDay==i.AbsoluteDay&&time.MinuteOfDay>=i.ValidUntil;}
        private ActivitySnapshot MakeSnapshot()
        {return new ActivitySnapshot{NpcId=intent.NpcId,ExecutionId=ExecutionId,IntendedActivityId=IntendedActivityId,ExecutedActivityId=ExecutedActivityId,TargetId=intent.TargetId,EffectiveTargetId=effectiveTarget,State=State,CurrentOperation=CurrentAction,ReasonCode=reason,StartedAtSeconds=startedSeconds,FinishedAtSeconds=IsTerminal?lastOperationalSeconds:0,ResourceId=CurrentToken(),ResidualOperationId=operation!=null&&(operationCancelPending||occupied)?operation.Id:null,IsFallback=IsFallback};}
        private void Emit(ExecutionEventKind kind,string step,string code)
        {if(sink==null)return;int day=lastGameMinute/1440+1;int minute=lastGameMinute%1440;sink.Emit(new ActivityExecutionEvent{Kind=kind,NpcId=intent.NpcId,ExecutionId=ExecutionId,IntendedActivityId=IntendedActivityId,ExecutedActivityId=ExecutedActivityId,TargetId=effectiveTarget??intent.TargetId,Step=step,ReasonCode=code,AbsoluteDay=day,MinuteOfDay=minute,OperationalSeconds=lastOperationalSeconds});}
    }

    public sealed class ActivityCoordinator
    {
        private readonly IDictionary<string,ActivityRecipe> recipes;
        private readonly IActivityPorts ports;
        private readonly IActivityEventSink sink;
        private readonly string npcId;
        private ActivityExecution active;
        private ActivityIntent activeIntent, pendingIntent;
        private string terminalOccurrence;
        private long nextExecution;
        private bool fallbackUsed, normalExitPending, cleanupBlocked, detached;
        public ActivitySnapshot Snapshot { get { return active==null?null:active.Snapshot; } }

        public ActivityCoordinator(string npcId,IDictionary<string,ActivityRecipe> recipes,IActivityPorts ports,IActivityEventSink sink)
        {this.npcId=npcId;this.recipes=recipes;this.ports=ports;this.sink=sink;}

        public ActivitySnapshot Tick(ActivityIntent intent,GameTimeSnapshot time,double now)
        {
            if(intent==null||intent.NpcId!=npcId)throw new ArgumentException("Intent NPC incompatível.");
            if(detached)return active==null?null:active.Snapshot;
            if(cleanupBlocked){pendingIntent=intent;return active==null?null:active.Snapshot;}
            if(active!=null&&active.IsTerminal)
            {
                if(pendingIntent!=null)pendingIntent=intent;
                if(active.Snapshot.HasResidualOwnership)
                {
                    if(!active.RetryCleanup(now,time)){cleanupBlocked=true;return active.Snapshot;}
                }
                if(active.State==ActivityState.Failed&&!fallbackUsed&&active.ExecutedActivityId!="idle"&&recipes.TryGetValue("idle",out var fallbackRecipe))
                {
                    fallbackUsed=true;active=new ActivityExecution(npcId+"-exec-"+(++nextExecution).ToString("D6"),activeIntent,fallbackRecipe,true,ports,sink);
                    active.Tick(now,time);return active.Snapshot;
                }
                terminalOccurrence=Occurrence(activeIntent);active=null;activeIntent=null;normalExitPending=false;
            }
            if(active!=null&&!active.IsTerminal)
            {
                if(normalExitPending)
                {
                    pendingIntent=intent;active.Tick(now,time);
                    if(active.IsTerminal)terminalOccurrence=Occurrence(activeIntent);
                    return active.Snapshot;
                }
                if(SameCommitment(activeIntent,intent)||ContinuesAtMidnight(activeIntent,intent))
                {activeIntent=intent;active.UpdateIntent(intent);active.Tick(now,time);if(active.IsTerminal)terminalOccurrence=Occurrence(intent);return active.Snapshot;}
                bool naturalExit=active.CurrentAction=="wait"&&HasExpired(activeIntent,time);
                pendingIntent=intent;
                if(naturalExit)
                {
                    normalExitPending=true;
                    // deixa a receita concluir a saída (por exemplo, levantar) com a intent antiga.
                    active.Tick(now,time);
                }
                else active.Cancel("intent_replaced",now,time);
                if(active.IsTerminal)terminalOccurrence=Occurrence(activeIntent);
                return active.Snapshot;
            }
            if(active!=null&&active.IsTerminal){terminalOccurrence=Occurrence(activeIntent);active=null;activeIntent=null;}
            if(pendingIntent!=null){intent=pendingIntent;pendingIntent=null;}
            string occurrence=Occurrence(intent);
            if(terminalOccurrence==occurrence)return null;
            ActivityRecipe recipe;bool fallback=false;
            if(!recipes.TryGetValue(intent.ActivityId,out recipe))
            {
                if(fallbackUsed || !recipes.TryGetValue("idle",out recipe)){terminalOccurrence=occurrence;return null;}
                fallback=true;fallbackUsed=true;
            }
            else fallbackUsed=false;
            activeIntent=intent;active=new ActivityExecution(npcId+"-exec-"+(++nextExecution).ToString("D6"),intent,recipe,fallback,ports,sink);
            active.Tick(now,time);return active.Snapshot;
        }

        public ActivitySnapshot Cancel(string reason,GameTimeSnapshot time,double now)
        {if(active!=null&&!active.IsTerminal){active.Cancel(reason,now,time);terminalOccurrence=Occurrence(activeIntent);pendingIntent=null;if(active.Snapshot.HasResidualOwnership)cleanupBlocked=true;}return active==null?null:active.Snapshot;}
        public void Remove(GameTimeSnapshot time,double now)
        {
            if(active!=null&&!active.IsTerminal)active.Cancel("npc_removed",now,time);
            pendingIntent=null;detached=true;
            if(active!=null&&active.Snapshot.HasResidualOwnership)cleanupBlocked=true;
            else{active=null;activeIntent=null;cleanupBlocked=false;}
        }
        public bool Resume(ActivityIntent current)
        {
            if(detached||cleanupBlocked||current==null||current.NpcId!=npcId||active!=null&&active.Snapshot.HasResidualOwnership)return false;
            if(active!=null&&!active.IsTerminal)return false;
            active=null;activeIntent=null;pendingIntent=null;terminalOccurrence=null;fallbackUsed=false;normalExitPending=false;return true;
        }
        public bool RetryResidualCleanup(GameTimeSnapshot time,double now)
        {
            if(active==null||!active.Snapshot.HasResidualOwnership)return false;
            if(!active.RetryCleanup(now,time)){cleanupBlocked=true;return false;}
            cleanupBlocked=false;terminalOccurrence=Occurrence(activeIntent);active=null;activeIntent=null;pendingIntent=null;normalExitPending=false;
            if(!detached)fallbackUsed=false;
            return true;
        }
        private static bool SameCommitment(ActivityIntent a,ActivityIntent b)
        {return a!=null&&b!=null&&a.NpcId==b.NpcId&&a.ActivityId==b.ActivityId&&a.TargetId==b.TargetId&&a.Origin==b.Origin&&a.SourceId==b.SourceId&&a.EntryId==b.EntryId&&a.Priority==b.Priority&&a.AbsoluteDay==b.AbsoluteDay;}
        private static bool ContinuesAtMidnight(ActivityIntent a,ActivityIntent b)
        {return a!=null&&b!=null&&a.NpcId==b.NpcId&&a.ActivityId==b.ActivityId&&a.TargetId==b.TargetId&&a.Origin==b.Origin&&a.SourceId==b.SourceId&&a.Priority==b.Priority&&b.AbsoluteDay==a.AbsoluteDay+1&&a.ValidUntil==1440&&b.ValidFrom==0;}
        private static string Occurrence(ActivityIntent i){return i.NpcId+"|"+i.ActivityId+"|"+i.TargetId+"|"+i.Origin+"|"+i.SourceId+"|"+i.EntryId+"|"+i.AbsoluteDay;}
        private static bool HasExpired(ActivityIntent i,GameTimeSnapshot t){return t.AbsoluteDay>i.AbsoluteDay||t.AbsoluteDay==i.AbsoluteDay&&t.MinuteOfDay>=i.ValidUntil;}
    }
}
