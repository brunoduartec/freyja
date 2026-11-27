using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Freyja.NpcScheduling;
using Freyja.NpcScheduling.Execution;

internal static class ExecutionTests
{
    private static int count;
    private static void Is(bool value,string name){count++;if(!value)throw new Exception("FAIL execution: "+name);}
    private static readonly GameTimeSnapshot AtStart=new GameTimeSnapshot(1,480,0);

    public static int Run(ResolvedScheduleData data)
    {
        count=0;var recipes=RecipeCatalog.Load(Program.DataPath,data.Activities);
        Is(recipes.Count==3&&recipes.ContainsKey("idle")&&recipes.ContainsKey("wander")&&recipes.ContainsKey("sit")&&!recipes.ContainsKey("fish"),"only idle wander sit have recipes");
        Is(RecipeCatalogRejectsUnknownOperation(data),"recipe whitelist rejects unknown operation");

        var sink=new RecordingActivitySink();var ports=new FakeActivityPorts();var coordinator=new ActivityCoordinator("npc_a",recipes,ports,sink);
        var idle=Intent("npc_a","idle",null,1,0,60,"idle_block");
        var first=coordinator.Tick(idle,new GameTimeSnapshot(1,1,0),0);var second=coordinator.Tick(idle,new GameTimeSnapshot(1,2,0),1);
        Is(first.ExecutionId==second.ExecutionId,"same intent does not restart");
        var refreshed=Intent("npc_a","idle",null,1,0,90,"idle_block");
        var third=coordinator.Tick(refreshed,new GameTimeSnapshot(1,50,0),2);
        Is(third.ExecutionId==first.ExecutionId&&third.State!=ActivityState.Completed,"validity refresh keeps running commitment");
        Is(ports.ActiveClaims==0,"idle without target does not reserve resources");
        var minuteSchedule=new ResolvedScheduleData(data.Activities,
            new Dictionary<string,IList<ScheduleEntry>>{{"minute_npc",new[]{new ScheduleEntry("all_morning",0,60,"idle",null)}}},
            new List<ScheduleOverride>());
        var minuteResolver=new ScheduleResolver(minuteSchedule);ports=new FakeActivityPorts();sink=new RecordingActivitySink();
        var minuteRunner=new ActivityCoordinator("minute_npc",recipes,ports,sink);string stableExecution=null;
        for(int minute=0;minute<10;minute++)
        {
            var resolved=minuteResolver.Resolve("minute_npc",new GameTimeSnapshot(1,minute,0),new WorldContextSnapshot("clear",null)).Intent;
            var observed=minuteRunner.Tick(resolved,new GameTimeSnapshot(1,minute,0),minute);
            if(stableExecution==null)stableExecution=observed.ExecutionId;
            Is(observed.ExecutionId==stableExecution,"resolver ValidFrom minute "+minute+" does not restart occurrence");
        }
        Is(sink.Events.Count(x=>x.Kind==ExecutionEventKind.ActivityStarted)==1,"one activity_started across per-minute resolver calls");

        var fish=Intent("npc_a","fish","pier",1,60,120,"fish");
        coordinator.Tick(fish,new GameTimeSnapshot(1,60,0),3);
        var unsupported=coordinator.Tick(fish,new GameTimeSnapshot(1,61,0),4);
        Is(unsupported.IsFallback&&unsupported.IntendedActivityId=="fish"&&unsupported.ExecutedActivityId=="idle","unsupported activity falls back without changing intent");
        var badClockRunner=new ActivityCoordinator("clock",recipes,new FakeActivityPorts(),new RecordingActivitySink());
        var clockIntent=Intent("clock","idle",null,1,0,60,"clock");badClockRunner.Tick(clockIntent,new GameTimeSnapshot(1,1,0),5);
        var regressed=badClockRunner.Tick(clockIntent,new GameTimeSnapshot(1,2,0),4);
        Is(regressed.State==ActivityState.Failed&&regressed.ReasonCode=="clock_regressed","monotonic time regression fails explicitly");

        TestSitSuccess(recipes);
        TestFullBankAndOtherBank(recipes);
        TestPathAndInvalidReservation(recipes);
        TestCleanupRetry(recipes);
        TestPoseRestoreRetry(recipes);
        TestOperationCancelFailure(recipes);
        TestReplacementAndNormalExit(recipes);
        TestWanderAndMovementExpiry(recipes);
        TestOperationToken(recipes);
        TestWeek(data);
        return count;
    }

    private static void TestSitSuccess(IDictionary<string,ActivityRecipe> recipes)
    {
        var ports=new FakeActivityPorts();var sink=new RecordingActivitySink();var runner=new ActivityCoordinator("sit_a",recipes,ports,sink);
        var intent=Intent("sit_a","sit","village_square",1,480,600,"sit_morning");
        var snapshot=DriveToAction(runner,intent,ports,"wait",AtStart,0);
        Is(snapshot.State==ActivityState.Executing&&ports.ActiveClaims==1,"sit reserves, moves, aligns, occupies and waits");
        Is(sink.Events.Any(x=>x.Kind==ExecutionEventKind.ResourceReserved)&&ports.ReservedSlotIds.Count==1,"reservation is correlated and exclusive");
        var ended=runner.Cancel("dialogue_interruption",AtStart,10);
        Is(ended.State==ActivityState.Cancelled&&ended.HasResidualOwnership&&ports.ActiveClaims==1,"cancel retains sit claim while stand_up runs");
        Is(ports.StandUpStarts==1&&ended.ResidualOperationId!=null,"cancel starts pollable stand_up cleanup");
        Is(!runner.RetryResidualCleanup(AtStart,10.25)&&ports.ActiveClaims==1,"claim remains while stand_up is running");
        Is(ports.ReleaseBeforeStandUp==0,"adapter never releases occupied claim before stand_up completion");
        var blocked=runner.Tick(Intent("sit_a","idle",null,1,600,660,"after_sit"),new GameTimeSnapshot(1,600,0),10.3);
        Is(blocked.ExecutionId==ended.ExecutionId&&ports.ActiveClaims==1,"replacement waits while stand_up cleanup owns the seat");
        Is(runner.RetryResidualCleanup(AtStart,10.5)&&ports.ActiveClaims==0,"confirmed stand_up releases exact claim");
        var replacement=runner.Tick(Intent("sit_a","idle",null,1,600,660,"after_sit"),new GameTimeSnapshot(1,600,0),10.6);
        Is(replacement!=null&&replacement.ExecutionId!=ended.ExecutionId,"replacement starts only after stand_up and release");

        foreach(string phase in new[]{"move_to","align_to","play_animation","wait"})
        {
            var phasePorts=new FakeActivityPorts();var phaseRunner=new ActivityCoordinator("phase_"+phase,recipes,phasePorts,new RecordingActivitySink());
            var phaseIntent=Intent("phase_"+phase,"sit","beach",1,480,600,"phase");
            DriveToAction(phaseRunner,phaseIntent,phasePorts,phase,AtStart,0);
            var cancelled=phaseRunner.Cancel("phase_cancel",AtStart,20);
            if(cancelled.HasResidualOwnership)CompleteResidual(phaseRunner,AtStart,20);
            Is(cancelled.State==ActivityState.Cancelled&&phasePorts.ActiveClaims==0,"cancel during "+phase+" releases resource");
        }

        var fullPorts=new FakeActivityPorts();var fullSink=new RecordingActivitySink();var a=new ActivityCoordinator("a",recipes,fullPorts,fullSink);var b=new ActivityCoordinator("b",recipes,fullPorts,fullSink);var c=new ActivityCoordinator("c",recipes,fullPorts,fullSink);
        a.Tick(Intent("a","sit","village_square",1,480,600,"seat"),AtStart,0);b.Tick(Intent("b","sit","village_square",1,480,600,"seat"),AtStart,0);c.Tick(Intent("c","sit","village_square",1,480,600,"seat"),AtStart,0);
        Is(fullPorts.ActiveClaims==2&&fullPorts.ReservedSlotIds.Distinct().Count()==2,"two bank A slots arbitrate exclusive claims");
        Is(fullSink.Events.Any(x=>x.NpcId=="c"&&x.Kind==ExecutionEventKind.ActivityFailed),"third claimant fails when bank A is full");
    }

    private static void TestFullBankAndOtherBank(IDictionary<string,ActivityRecipe> recipes)
    {
        var ports=new FakeActivityPorts();var sink=new RecordingActivitySink();
        var a=new ActivityCoordinator("a",recipes,ports,sink);var b=new ActivityCoordinator("b",recipes,ports,sink);var c=new ActivityCoordinator("c",recipes,ports,sink);
        a.Tick(Intent("a","sit","village_square",1,480,600,"seat"),AtStart,0);b.Tick(Intent("b","sit","village_square",1,480,600,"seat"),AtStart,0);c.Tick(Intent("c","sit","beach",1,480,600,"seat"),AtStart,0);
        Is(ports.ActiveClaims==3&&ports.ReservedSlotIds.Any(x=>x.StartsWith("bank_b",StringComparison.Ordinal)),"third NPC uses independent bank B");
    }

    private static void TestPathAndInvalidReservation(IDictionary<string,ActivityRecipe> recipes)
    {
        var ports=new FakeActivityPorts{FailPath=true};var sink=new RecordingActivitySink();var runner=new ActivityCoordinator("path",recipes,ports,sink);
        var result=runner.Tick(Intent("path","sit","beach",1,480,600,"path"),AtStart,0);
        Is(result.State==ActivityState.Failed&&result.ReasonCode=="path_unreachable"&&ports.ActiveClaims==0,"path failure cleans reservation");

        ports=new FakeActivityPorts{InvalidateNextReservation=true};sink=new RecordingActivitySink();runner=new ActivityCoordinator("invalid",recipes,ports,sink);
        result=DriveToTerminal(runner,Intent("invalid","sit","beach",1,480,600,"invalid"),ports,AtStart,0);
        Is(result.State==ActivityState.Failed&&result.ReasonCode=="reservation_invalid"&&ports.ActiveClaims==0,"invalid reservation rejected before occupy and cleaned");

        ports=new FakeActivityPorts{RejectAnimation=true};runner=new ActivityCoordinator("anim",recipes,ports,new RecordingActivitySink());
        result=DriveToTerminal(runner,Intent("anim","sit","beach",1,480,600,"anim"),ports,AtStart,0);
        Is(result.State==ActivityState.Failed&&result.ReasonCode=="animation_rejected"&&result.HasResidualOwnership&&ports.ActiveClaims==1,"sit animation rejection retains occupied claim for stand_up");
        ports.RejectAnimation=false;CompleteResidual(runner,AtStart,20);
        Is(ports.ActiveClaims==0,"confirmed stand_up releases claim after sit animation rejection");

        ports=new FakeActivityPorts{OperationDurationSeconds=10,OperationTimeoutSeconds=1};runner=new ActivityCoordinator("timeout",recipes,ports,new RecordingActivitySink());
        result=runner.Tick(Intent("timeout","sit","beach",1,480,600,"timeout"),AtStart,0);
        result=runner.Tick(Intent("timeout","sit","beach",1,480,600,"timeout"),AtStart,2);
        Is(result.State==ActivityState.Failed&&result.ReasonCode=="operation_timeout"&&ports.ActiveClaims==0,"operation timeout is deterministic and cleans resource");
    }

    private static void TestCleanupRetry(IDictionary<string,ActivityRecipe> recipes)
    {
        var ports=new FakeActivityPorts();var runner=new ActivityCoordinator("cleanup",recipes,ports,new RecordingActivitySink());
        var intent=Intent("cleanup","sit","home",1,480,600,"cleanup");DriveToAction(runner,intent,ports,"wait",AtStart,0);
        ports.FailCleanup=true;var failed=runner.Cancel("external",AtStart,10);
        Is(failed.ResourceId!=null&&ports.ActiveClaims==1,"cleanup failure remains owned");
        Is(!runner.Resume(intent),"resume is refused while the execution still owns a residual handle");
        runner.Remove(AtStart,11);
        Is(runner.Snapshot!=null&&runner.Snapshot.ResourceId==failed.ResourceId&&ports.ActiveClaims==1,"remove keeps detached cleanup pending and retains token");
        Is(runner.Tick(intent,AtStart,12)!=null&&ports.ActiveClaims==1,"detached NPC rejects new intents while retaining cleanup state");
        ports.FailCleanup=false;
        Is(runner.RetryResidualCleanup(AtStart,13)&&ports.ActiveClaims==0&&runner.Snapshot==null,"confirmed cleanup retry clears residual before discard");
        Is(runner.Tick(intent,AtStart,14)==null,"removed NPC remains detached after cleanup");
    }

    private static void TestPoseRestoreRetry(IDictionary<string,ActivityRecipe> recipes)
    {
        var ports=new FakeActivityPorts();var runner=new ActivityCoordinator("pose_cleanup",recipes,ports,new RecordingActivitySink());
        var intent=Intent("pose_cleanup","sit","home",1,480,600,"pose_cleanup");
        DriveToAction(runner,intent,ports,"wait",AtStart,0);
        ports.FailPoseRestore=true;
        var cancelled=runner.Cancel("external",AtStart,10);
        Is(cancelled.ResourceId!=null&&cancelled.ReasonCode=="pose_restore_failed"&&ports.ActiveClaims==1,
            "rejected stand_up keeps occupied reservation owned");
        Is(!runner.RetryResidualCleanup(AtStart,11)&&ports.ActiveClaims==1,
            "residual cleanup does not release before pose restore succeeds");
        ports.FailPoseRestore=false;
        Is(!runner.RetryResidualCleanup(AtStart,12.25)&&ports.ActiveClaims==1,"retry polls stand_up rather than releasing early");
        Is(!runner.RetryResidualCleanup(AtStart,12.5)&&ports.ActiveClaims==1&&runner.RetryResidualCleanup(AtStart,12.75)&&ports.ActiveClaims==0&&runner.Snapshot==null,
            "confirmed pose restore permits exact reservation cleanup");

        ports=new FakeActivityPorts{StandUpDurationSeconds=2,OperationTimeoutSeconds=1};
        runner=new ActivityCoordinator("pose_timeout",recipes,ports,new RecordingActivitySink());
        DriveToAction(runner,Intent("pose_timeout","sit","home",1,480,600,"timeout"),ports,"wait",AtStart,0);
        var timedOut=runner.Cancel("external",AtStart,10);
        Is(timedOut.HasResidualOwnership&&ports.ActiveClaims==1&&!runner.RetryResidualCleanup(AtStart,12),"stand_up timeout preserves residual claim");
        ports.StandUpDurationSeconds=.5;ports.OperationTimeoutSeconds=10;
        Is(!runner.RetryResidualCleanup(AtStart,13)&&ports.ActiveClaims==1&&runner.RetryResidualCleanup(AtStart,13.5)&&ports.ActiveClaims==0,"timed-out stand_up retries and releases after completion");
    }

    private static void TestOperationCancelFailure(IDictionary<string,ActivityRecipe> recipes)
    {
        var timedPorts=new FakeActivityPorts{SitAnimationDurationSeconds=30,OperationTimeoutSeconds=10};
        var timedRunner=new ActivityCoordinator("animation_cancel_timeout",recipes,timedPorts,new RecordingActivitySink());
        DriveToAction(timedRunner,Intent("animation_cancel_timeout","sit","home",1,480,600,"sit"),timedPorts,"play_animation",AtStart,0);
        var interruptedAnimation=timedPorts.LastOperation;
        var pendingStandUp=timedRunner.Cancel("external",AtStart,interruptedAnimation.StartedAt+.25);
        Is(pendingStandUp.ResidualOperationId==interruptedAnimation.Id&&timedPorts.StandUpStarts==0&&timedPorts.ActiveClaims==1,"running sit animation remains owned until bounded cancellation timeout");
        Is(!timedRunner.RetryResidualCleanup(AtStart,interruptedAnimation.StartedAt+10.1)&&timedPorts.LastOperation.Id!=interruptedAnimation.Id&&timedPorts.StandUpStarts==1&&timedPorts.ActiveClaims==1,"expired animation ticket advances to retained stand_up cleanup");
        Is(timedRunner.RetryResidualCleanup(AtStart,interruptedAnimation.StartedAt+10.6)&&timedPorts.ActiveClaims==0,"stand_up completion releases after expired operation is retired");

        var ports=new FakeActivityPorts{FailOperationCancel=true};var runner=new ActivityCoordinator("cancel_op",recipes,ports,new RecordingActivitySink());
        var sit=Intent("cancel_op","sit","home",1,480,600,"sit");
        var moving=runner.Tick(sit,AtStart,0);string oldOperation=ports.LastOperation.Id;
        Is(moving.CurrentOperation=="move_to"&&ports.ActiveOperationCount==1,"movement operation is active before external cancellation");
        var cancelled=runner.Cancel("external",AtStart,1);
        Is(cancelled.ResidualOperationId==oldOperation&&cancelled.HasResidualOwnership&&ports.ActiveOperationCount==1&&ports.ActiveClaims==1,"failed port cancellation remains an owned operation and keeps its slot");
        Is(!runner.Resume(sit),"resume refuses operation cancellation residue");
        runner.Remove(AtStart,2);
        Is(runner.Snapshot.ResidualOperationId==oldOperation&&ports.ActiveOperationCount==1,"remove does not discard a running operation");
        Is(runner.Tick(Intent("cancel_op","idle",null,1,481,600,"new"),new GameTimeSnapshot(1,481,0),3).ResidualOperationId==oldOperation,"detached coordinator rejects new intent during operation cleanup");
        ports.FailOperationCancel=false;
        Is(runner.RetryResidualCleanup(new GameTimeSnapshot(1,481,0),4)&&runner.Snapshot==null&&ports.ActiveOperationCount==0&&ports.ActiveClaims==0,"retry confirms operation cancel then releases slot before discard");
        Is(ports.Poll(ports.LastOperation,"cancel_op","old",5).Status==OperationStatus.Failed,"old operation token remains inert after cleanup");

        ports=new FakeActivityPorts{FailOperationCancel=true};runner=new ActivityCoordinator("replace_op",recipes,ports,new RecordingActivitySink());
        runner.Tick(Intent("replace_op","sit","home",1,480,600,"sit"),AtStart,0);var stale=ports.LastOperation;
        var next=Intent("replace_op","idle",null,1,481,600,"next");var nextTime=new GameTimeSnapshot(1,481,0);
        runner.Tick(next,nextTime,1);var blocked=runner.Tick(next,new GameTimeSnapshot(1,482,0),2);
        Is(blocked.ResidualOperationId==stale.Id&&!runner.Resume(next),"replacement remains blocked until active operation cancellation is confirmed");
        ports.FailOperationCancel=false;Is(runner.RetryResidualCleanup(nextTime,3),"replacement cleanup retry succeeds");
        var fresh=runner.Tick(next,nextTime,4);
        Is(fresh.ExecutionId!=blocked.ExecutionId&&ports.ActiveOperationCount==0,"new activity starts after old operation ownership is cleared");
        Is(ports.Poll(stale,"replace_op","old",5).Status==OperationStatus.Failed,"late old token cannot affect the replacement");
    }

    private static void TestReplacementAndNormalExit(IDictionary<string,ActivityRecipe> recipes)
    {
        var ports=new FakeActivityPorts();var sink=new RecordingActivitySink();var runner=new ActivityCoordinator("replace",recipes,ports,sink);
        var sit=Intent("replace","sit","village_square",1,480,483,"short_sit");
        DriveToAction(runner,sit,ports,"wait",AtStart,0);
        var idle=Intent("replace","idle",null,1,483,600,"next");var changedTime=new GameTimeSnapshot(1,483,0);
        runner.Tick(idle,changedTime,5);
        for(int i=1;i<8;i++)runner.Tick(idle,changedTime,5+i*.25);
        Is(sink.Events.Any(x=>x.Step=="play_animation"&&x.Kind==ExecutionEventKind.StepStarted),"normal expiry starts the sit exit animation");
        Is(ports.AnimationRequests.Contains("stand_up"),"normal expiry waits for stand_up adapter completion");
        Is(ports.ActiveClaims==0,"normal expiry releases sit slot after exit");
        var started=runner.Tick(idle,changedTime,8);Is(started!=null&&started.IntendedActivityId=="idle","replacement starts only after cleanup");
    }

    private static void TestWanderAndMovementExpiry(IDictionary<string,ActivityRecipe> recipes)
    {
        var ports=new FakeActivityPorts();var sink=new RecordingActivitySink();var runner=new ActivityCoordinator("wander",recipes,ports,sink);
        var intent=Intent("wander","wander","village_square",1,480,600,"wander");
        ActivitySnapshot snapshot=null;
        for(int i=0;i<30;i++)snapshot=runner.Tick(intent,AtStart,i*.25);
        Is(snapshot.ExecutedActivityId=="wander"&&ports.ActiveClaims==0&&sink.Events.Any(x=>x.Kind==ExecutionEventKind.StepCompleted&&x.Step=="move_to"),"wander moves over region waypoints without reserving slots");

        ports=new FakeActivityPorts();runner=new ActivityCoordinator("moving_cancel",recipes,ports,new RecordingActivitySink());
        var sit=Intent("moving_cancel","sit","home",1,480,483,"moving");
        var moving=runner.Tick(sit,AtStart,0);Is(moving.CurrentOperation=="move_to"&&ports.ActiveClaims==1,"sit is moving after reservation");
        moving=runner.Tick(sit,new GameTimeSnapshot(1,483,0),1);
        Is(moving.State==ActivityState.Cancelled&&ports.ActiveClaims==0,"schedule expiration during movement cancels and releases");
    }

    private static void TestOperationToken(IDictionary<string,ActivityRecipe> recipes)
    {
        var ports=new FakeActivityPorts();var old=ports.Begin("move_to","n","old","home","target",null,0);
        ports.Cancel(old,"n","old",1);var fresh=ports.Begin("move_to","n","new","home","target",null,0);
        var stale=ports.Poll(old,"n","old",1);var current=ports.Poll(fresh,"n","new",1);
        Is(stale.Status==OperationStatus.Failed&&stale.ReasonCode=="operation_cancelled"&&current.Status==OperationStatus.Succeeded,"stale operation token cannot affect current one");

        var r=recipes["wander"];var runner=new ActivityCoordinator("walk",recipes,ports,new RecordingActivitySink());
        var intent=Intent("walk","wander","missing_region",1,480,600,"bad_region");
        var result=runner.Tick(intent,AtStart,0);
        Is(result.State==ActivityState.Failed&&result.ReasonCode=="target_unavailable","unknown wander region fails explicitly");
    }

    private static void TestWeek(ResolvedScheduleData data)
    {
        string first,second;ExecutionWeekResult a,b;
        using(var w=new StringWriter())a=ExecutionWeekSimulator.Run(data,Program.DataPath,w);
        using(var w=new StringWriter()){b=ExecutionWeekSimulator.Run(data,Program.DataPath,w);second=w.ToString();}
        using(var w=new StringWriter()){ExecutionWeekSimulator.Run(data,Program.DataPath,w);first=w.ToString();}
        Is(first==second,"seven-day executor simulation repeats byte-for-byte");
        Is(first.Split('\n').Count(x=>x.StartsWith("SIM_INTENT ",StringComparison.Ordinal))>=84,"seven-day resolver log records schedule block transitions, not per-minute refreshes");
        Is(first.Contains("activity=repair")&&first.Contains("activity=secret_meeting"),"week passes real rain and narrative overrides through the resolver");
        Is(a.ActiveClaims==0&&b.ActiveClaims==0,"seven-day simulation shutdown releases all resources");
        Is(a.Reservations>=3&&a.Releases>=a.Reservations,"weekly scenario includes banks A/B and confirmed releases");
        Is(a.Cancellations>0&&a.Failures>0,"weekly scenario includes schedule changes, cancellation and injected failures");
        Is(first.Contains("SIM_SCENARIO full_bank target=village_square claimed=2 capacity=2 thirdFailed=True")&&first.Contains("SIM_WEEK days=7 npc=3"),"weekly output includes contention and seven full days");
    }

    private static ActivitySnapshot DriveToAction(ActivityCoordinator runner,ActivityIntent intent,FakeActivityPorts ports,string action,GameTimeSnapshot time,double start)
    {
        ActivitySnapshot snapshot=null;
        for(int i=0;i<40;i++){snapshot=runner.Tick(intent,time,start+i*.25);if(snapshot!=null&&snapshot.CurrentOperation==action)return snapshot;}
        throw new Exception("execution never reached "+action+"; last="+(snapshot==null?"null":snapshot.State+"/"+snapshot.CurrentOperation));
    }
    private static void CompleteResidual(ActivityCoordinator runner,GameTimeSnapshot time,double start)
    {for(int i=1;i<=8&&runner.Snapshot!=null&&runner.Snapshot.HasResidualOwnership;i++)runner.RetryResidualCleanup(time,start+i*.25);}
    private static ActivitySnapshot DriveToTerminal(ActivityCoordinator runner,ActivityIntent intent,FakeActivityPorts ports,GameTimeSnapshot time,double start)
    {
        ActivitySnapshot snapshot=null;for(int i=0;i<40;i++){snapshot=runner.Tick(intent,time,start+i*.25);if(snapshot!=null&&(snapshot.State==ActivityState.Failed||snapshot.State==ActivityState.Completed))return snapshot;}
        return snapshot;
    }
    private static ActivityIntent Intent(string npc,string activity,string target,int day,int from,int until,string entry)
    {return new ActivityIntent(npc,activity,target,day,from,until,30,ScheduleOrigin.Base,"tests",entry);}
    private static bool RecipeCatalogRejectsUnknownOperation(ResolvedScheduleData data)
    {
        string root=Path.Combine(Path.GetTempPath(),"npc-recipes-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(Path.Combine(root,"execution"));
        try{string json=File.ReadAllText(Path.Combine(Program.DataPath,"execution","recipes.json")).Replace("find_waypoints","run_script");File.WriteAllText(Path.Combine(root,"execution","recipes.json"),json);try{RecipeCatalog.Load(root,data.Activities);return false;}catch(InvalidDataException){return true;}}
        finally{Directory.Delete(root,true);}
    }
}
