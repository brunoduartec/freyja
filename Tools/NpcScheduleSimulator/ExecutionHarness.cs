using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Freyja.NpcScheduling;
using Freyja.NpcScheduling.Execution;

internal static class RecipeCatalog
{
    public static IDictionary<string,ActivityRecipe> Load(string dataRoot,IDictionary<string,ActivityDefinition> activities)
    {
        string path=Path.Combine(dataRoot,"execution","recipes.json");
        return ActivityRecipeLoader.Load(File.ReadAllText(path),activities,path);
    }
}

internal sealed class RecordingActivitySink : IActivityEventSink
{
    private long sequence;
    public readonly List<ActivityExecutionEvent> Events=new List<ActivityExecutionEvent>();
    public void Emit(ActivityExecutionEvent value){value.Sequence=++sequence;Events.Add(value);}
    public void Write(TextWriter writer)
    {
        foreach(var e in Events)writer.WriteLine("SIM day="+e.AbsoluteDay+" time="+(e.MinuteOfDay/60).ToString("D2")+":"+(e.MinuteOfDay%60).ToString("D2")+" npc="+e.NpcId+" exec="+e.ExecutionId+" event="+Camel(e.Kind)+" intended="+e.IntendedActivityId+" executed="+e.ExecutedActivityId+" step="+(e.Step??"-")+" reason="+(e.ReasonCode??"-")+" simulated=true");
    }
    private static string Camel(ExecutionEventKind k){return Char.ToLowerInvariant(k.ToString()[0])+k.ToString().Substring(1);}
}

internal sealed class FakeActivityPorts : IActivityPorts, ITargetlessActivityPointProvider
{
    private sealed class Claim { public ActivityCandidate Candidate; public string Npc,Execution; public bool Occupied,Invalid; }
    private readonly Dictionary<string,Claim> claims=new Dictionary<string,Claim>(StringComparer.Ordinal);
    private readonly HashSet<string> activeOps=new HashSet<string>(StringComparer.Ordinal);
    private readonly HashSet<string> stoodUpExecutions=new HashSet<string>(StringComparer.Ordinal);
    private int opId,tokenId;
    public double OperationDurationSeconds=0.2;
    public double OperationTimeoutSeconds=120.0;
    public double StandUpDurationSeconds=0.5;
    public double SitAnimationDurationSeconds=-1;
    public bool FailPath,RejectAnimation,FailCleanup,FailOperationCancel,FailPoseRestore,InvalidateNextReservation;
    public bool EnableTargetlessHomeMapping;
    public string IdleDestinationPointId="home_a";
    public int ActiveClaims { get { return claims.Count; } }
    public int ActiveOperationCount { get { return activeOps.Count; } }
    public IList<string> ActiveOperationIds { get { return activeOps.OrderBy(x=>x,StringComparer.Ordinal).ToList(); } }
    public ActivityOperation LastOperation { get; private set; }
    public int StandUpStarts { get; private set; }
    public int StandUpCompletions { get; private set; }
    public int ReleaseBeforeStandUp { get; private set; }
    public IList<string> ReservedSlotIds { get { return claims.Values.Select(x=>x.Candidate.Id).OrderBy(x=>x,StringComparer.Ordinal).ToList(); } }
    public IList<string> AnimationRequests { get; } = new List<string>();

    public IList<ActivityCandidate> FindTargets(string target,string capability,string npc)
    {
        var list=new List<ActivityCandidate>();if(capability!="sit")return list;
        string bank=target=="beach"?"bank_b":target=="village_square"||target=="home"?"bank_a":null;
        if(bank==null)return list;
        for(int i=0;i<2;i++){string id=bank+"_slot_"+(i+1);list.Add(new ActivityCandidate(id,target,id+"_approach",id+"_pose",(npc.Length+i)%4));}
        return list;
    }
    public bool TryGetTargetlessPoint(string activity,string action,string npc,out string point)
    {point=null;if(!EnableTargetlessHomeMapping||activity!="idle"||action!="move_to"||string.IsNullOrWhiteSpace(IdleDestinationPointId))return false;point=IdleDestinationPointId;return true;}
    public IList<string> FindWaypoints(string region,string npc)
    {
        if(region=="village_square")return new[]{"square_a","square_b","square_c"};
        if(region=="beach")return new[]{"shore_a","shore_b","shore_c"};
        if(region=="home")return new[]{"home_a","home_b"};
        return new string[0];
    }
    public bool TryReserve(ActivityCandidate candidate,string npc,string execution,out string token)
    {
        token=null;if(claims.Values.Any(x=>x.Candidate.Id==candidate.Id&&!x.Invalid))return false;
        token="claim_"+(++tokenId).ToString("D5");claims[token]=new Claim{Candidate=candidate,Npc=npc,Execution=execution,Invalid=InvalidateNextReservation};InvalidateNextReservation=false;return true;
    }
    public bool IsReservationValid(string token,string npc,string execution)
    {Claim c;return token!=null&&claims.TryGetValue(token,out c)&&!c.Invalid&&c.Npc==npc&&c.Execution==execution;}
    public bool Occupy(string token,string npc,string execution)
    {if(!IsReservationValid(token,npc,execution))return false;claims[token].Occupied=true;return true;}
    public bool Release(string token,string npc,string execution)
    {
        Claim c;if(!claims.TryGetValue(token,out c)||c.Npc!=npc||c.Execution!=execution)return false;
        if(c.Occupied&&!stoodUpExecutions.Contains(execution)){ReleaseBeforeStandUp++;return false;}
        if(FailCleanup)return false;
        claims.Remove(token);return true;
    }
    public ActivityOperation Begin(string action,string npc,string execution,string target,string point,string animation,double now)
    {string id="op_"+(++opId).ToString("D6");activeOps.Add(id);LastOperation=new ActivityOperation(id,action,target,point,animation,now);if(animation=="stand_up")StandUpStarts++;return LastOperation;}
    public ActivityOperationResult Poll(ActivityOperation operation,string npc,string execution,double now)
    {
        if(!activeOps.Contains(operation.Id))return ActivityOperationResult.Failed("operation_cancelled");
        double elapsed=now-operation.StartedAt;
        if(operation.Action=="move_to"&&FailPath){activeOps.Remove(operation.Id);return ActivityOperationResult.Failed("path_unreachable");}
        if(operation.Action=="move_to"&&operation.Point=="target"&&!new[]{"home","pier","village_square","beach","farm","fish_market","hill_ruins"}.Contains(operation.Target)){activeOps.Remove(operation.Id);return ActivityOperationResult.Failed("target_unavailable");}
        if(operation.Action=="play_animation"&&(RejectAnimation&&operation.Animation!="stand_up"||operation.Animation=="stand_up"&&FailPoseRestore)){activeOps.Remove(operation.Id);return ActivityOperationResult.Failed(operation.Animation=="stand_up"?"pose_restore_failed":"animation_rejected");}
        if(elapsed>OperationTimeoutSeconds){activeOps.Remove(operation.Id);return ActivityOperationResult.Failed("operation_timeout");}
        double duration=operation.Animation=="stand_up"?StandUpDurationSeconds:
            operation.Action=="play_animation"&&SitAnimationDurationSeconds>=0?SitAnimationDurationSeconds:OperationDurationSeconds;
        if(elapsed<duration)return ActivityOperationResult.Running;
        activeOps.Remove(operation.Id);if(operation.Action=="play_animation")AnimationRequests.Add(operation.Animation);if(operation.Animation=="stand_up"){StandUpCompletions++;stoodUpExecutions.Add(execution);}return ActivityOperationResult.Succeeded(operation.Target);
    }
    public bool Cancel(ActivityOperation operation,string npc,string execution,double now)
    {
        if(operation==null||!activeOps.Contains(operation.Id))return true;
        if(FailOperationCancel)throw new InvalidOperationException("cancel was not confirmed");
        double duration=operation.Animation=="stand_up"?StandUpDurationSeconds:
            operation.Action=="play_animation"&&SitAnimationDurationSeconds>=0?SitAnimationDurationSeconds:OperationDurationSeconds;
        if(operation.Action=="play_animation"&&now-operation.StartedAt<Math.Min(duration,OperationTimeoutSeconds))return false;
        return activeOps.Remove(operation.Id);
    }
    public void InvalidateOwnedReservations(string npc){foreach(var c in claims.Values.Where(x=>x.Npc==npc))c.Invalid=true;}
    public void ResetFailures(){FailPath=false;RejectAnimation=false;FailCleanup=false;FailOperationCancel=false;FailPoseRestore=false;InvalidateNextReservation=false;}
}
