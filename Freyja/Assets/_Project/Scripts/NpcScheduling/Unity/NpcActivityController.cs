using System;
using System.Collections.Generic;
using Freyja.NpcScheduling;
using Freyja.NpcScheduling.Execution;
using Freyja.SmartObjects;
using UnityEngine;

namespace Freyja.NpcScheduling.Unity
{
    [DisallowMultipleComponent]
    public sealed class NpcActivityController : MonoBehaviour, IActivityEventSink
    {
        [SerializeField] private string npcId;
        [SerializeField] private NpcScheduleTextAssetProvider dataProvider;
        [SerializeField] private NpcLocationRegistry locations;
        [SerializeField] private NpcUnityActivityPorts activityPorts;
        [SerializeField] private NpcAnimatorBindings animatorBindings;
        [SerializeField] private bool logTransitions = true;

        private ScheduleResolver resolver;
        private ActivityCoordinator coordinator;
        private GameTimeSnapshot lastTime;
        private WorldContextSnapshot lastContext;
        private ActivityIntent currentIntent;
        private double lastOperationalSeconds;
        private bool hasSnapshot;
        private bool initializationPending;
        private string initializationError;
        private string lastReportedError;

        public bool IsReady => coordinator != null;
        public string InitializationError => initializationError;
        public string NpcId => npcId;
        public ActivityIntent CurrentIntent => currentIntent;
        public ActivitySnapshot CurrentActivitySnapshot => coordinator == null ? null : coordinator.Snapshot;
        public GameTimeSnapshot CurrentWorldTime => hasSnapshot ? lastTime : null;
        public bool HasWorldSnapshot => hasSnapshot;
        public bool IsInitializationPending => initializationPending;

        private void OnEnable()
        {
            initializationPending=coordinator==null;
        }

        private void Update()
        {
            if(!initializationPending||coordinator!=null)return;
            initializationPending=false;
            Initialize();
        }

        private void OnDisable()
        {
            initializationPending=false;
            if(coordinator!=null&&hasSnapshot)
            {
                coordinator.Cancel("host_disabled",lastTime,lastOperationalSeconds);
                ActivitySnapshot snapshot=coordinator.Snapshot;
                if(snapshot!=null&&snapshot.HasResidualOwnership)
                {
                    coordinator.RetryResidualCleanup(lastTime,lastOperationalSeconds);
                    snapshot=coordinator.Snapshot;
                }
                if(snapshot!=null&&snapshot.HasResidualOwnership)
                    Debug.LogError("NPC cleanup remains pending for execution "+snapshot.ExecutionId+
                        ": "+snapshot.ReasonCode+"; the coordinator and exact resource ownership are retained for retry after the host receives another world snapshot.",this);
                else { coordinator=null; resolver=null; currentIntent=null; hasSnapshot=false; }
            }
            else { coordinator=null; resolver=null; currentIntent=null; hasSnapshot=false; }
        }

        public bool TickFromWorld(GameTimeSnapshot time,WorldContextSnapshot context,double operationalSeconds)
        {
            if(coordinator==null)return Fail("controller_not_initialized");
            if(time==null||context==null||Double.IsNaN(operationalSeconds)||Double.IsInfinity(operationalSeconds))return Fail("invalid_world_snapshot");
            if(hasSnapshot&&EarlierThan(lastTime,time))return Fail("world_clock_regressed");

            bool resolve=!hasSnapshot||lastContext==null||!SameContext(lastContext,context)||
                time.AbsoluteDay!=lastTime.AbsoluteDay||time.MinuteOfDay>=currentIntent.ValidUntil;
            if(resolve)
            {
                try
                {
                    ResolutionResult result=resolver.Resolve(npcId,time,context);
                    currentIntent=result.Intent;
                    if(logTransitions)
                    {
                        foreach(string diagnostic in result.Trace.Diagnostics)Debug.LogWarning("NPC "+npcId+" schedule: "+diagnostic,this);
                    }
                }
                catch(Exception exception){return Fail("schedule_resolution_failed: "+exception.Message);}
            }

            lastTime=time; lastContext=context; lastOperationalSeconds=operationalSeconds; hasSnapshot=true;
            ActivitySnapshot beforeTick=coordinator.Snapshot;
            if(IsTerminalResidual(beforeTick))RetryResidualCleanup(time,operationalSeconds);
            try { coordinator.Tick(currentIntent,time,operationalSeconds); return true; }
            catch(Exception exception){return Fail("activity_tick_failed: "+exception.Message);}
        }

        public ActivitySnapshot CancelExternal(string reason)
        {
            if(coordinator==null||!hasSnapshot)return null;
            return coordinator.Cancel(reason,lastTime,lastOperationalSeconds);
        }

        public bool RetryResidualCleanup(GameTimeSnapshot time,double operationalSeconds)
        {
            if(coordinator==null||time==null||Double.IsNaN(operationalSeconds)||Double.IsInfinity(operationalSeconds))return false;
            if(!IsTerminalResidual(coordinator.Snapshot))return false;
            bool cleared=coordinator.RetryResidualCleanup(time,operationalSeconds);
            ActivitySnapshot snapshot=coordinator.Snapshot;
            if(!cleared&&snapshot!=null&&snapshot.HasResidualOwnership)
                Fail("residual_cleanup_pending: "+(snapshot.ReasonCode??"unknown"));
            else if(cleared){lastReportedError=null;initializationError=null;}
            return cleared;
        }

        private static bool IsTerminalResidual(ActivitySnapshot snapshot)
        {
            return snapshot!=null&&snapshot.HasResidualOwnership&&
                (snapshot.State==ActivityState.Completed||snapshot.State==ActivityState.Cancelled||snapshot.State==ActivityState.Failed);
        }

        public bool Resume(ActivityIntent currentIntentToResume)
        {
            return coordinator!=null&&coordinator.Resume(currentIntentToResume);
        }

        public void Emit(ActivityExecutionEvent activityEvent)
        {
            if(!logTransitions||activityEvent==null)return;
            string target=String.IsNullOrEmpty(activityEvent.TargetId)?"-":activityEvent.TargetId;
            string reason=String.IsNullOrEmpty(activityEvent.ReasonCode)?"-":activityEvent.ReasonCode;
            Debug.Log("NPC_ACTIVITY event="+activityEvent.Kind+" npc="+activityEvent.NpcId+" exec="+activityEvent.ExecutionId+
                " intended="+activityEvent.IntendedActivityId+" executed="+activityEvent.ExecutedActivityId+
                " target="+target+" step="+(activityEvent.Step??"-")+" reason="+reason,this);
        }

        private void Initialize()
        {
            initializationError=null;
            if(String.IsNullOrWhiteSpace(npcId)){Fail("missing_npc_id");return;}
            if(dataProvider==null){Fail("missing_schedule_data_provider");return;}
            if(locations==null){Fail("missing_location_registry");return;}
            if(activityPorts==null){Fail("missing_activity_ports");return;}
            if(animatorBindings==null){Fail("missing_animator_bindings");return;}
            SmartObjectReservationOwner owner=GetComponent<SmartObjectReservationOwner>();
            if(owner==null){Fail("missing_smart_object_reservation_owner");return;}
            if(!dataProvider.TryLoad(out ResolvedScheduleData data,out IDictionary<string,ActivityRecipe> recipes,out string error))
            {Fail(error??"schedule_data_unavailable");return;}
            try
            {
                resolver=new ScheduleResolver(data);
                activityPorts.Initialize(npcId,owner,locations,animatorBindings,transform);
                coordinator=new ActivityCoordinator(npcId,recipes,activityPorts,this);
                if(logTransitions)Debug.Log("NPC "+npcId+" loaded schedule/recipe data; waiting for real world snapshots.",this);
            }
            catch(Exception exception){Fail("npc_initialization_failed: "+exception.Message);}
        }

        private bool Fail(string reason)
        {
            initializationError=reason;
            if(!String.Equals(lastReportedError,reason,StringComparison.Ordinal))
            {
                lastReportedError=reason;
                Debug.LogError("NPC "+(String.IsNullOrEmpty(npcId)?"<missing-id>":npcId)+" integration: "+reason,this);
            }
            return false;
        }

        private static bool EarlierThan(GameTimeSnapshot previous,GameTimeSnapshot current)
        { return current.AbsoluteDay<previous.AbsoluteDay||current.AbsoluteDay==previous.AbsoluteDay&&current.MinuteOfDay<previous.MinuteOfDay; }

        private static bool SameContext(WorldContextSnapshot left,WorldContextSnapshot right)
        {
            if(!String.Equals(left.Weather,right.Weather,StringComparison.Ordinal)||left.Flags.Count!=right.Flags.Count)return false;
            foreach(KeyValuePair<string,bool> flag in left.Flags)
                if(!right.Flags.TryGetValue(flag.Key,out bool value)||value!=flag.Value)return false;
            return true;
        }
    }
}
