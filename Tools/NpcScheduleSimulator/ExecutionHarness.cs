using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Freyja.NpcScheduling;
using Freyja.NpcScheduling.Execution;

internal static class RecipeCatalog
{
    private sealed class RecipeFile { public int SchemaVersion { get; set; } public List<ActivityRecipe> Recipes { get; set; } }

    public static IDictionary<string,ActivityRecipe> Load(string dataRoot,IDictionary<string,ActivityDefinition> activities)
    {
        string path=Path.Combine(dataRoot,"execution","recipes.json");
        try
        {
            var document=StrictJson.Deserialize<RecipeFile>(File.ReadAllText(path));
            if(document.SchemaVersion!=1||document.Recipes==null)throw new InvalidDataException("schemaVersion 1 e recipes são obrigatórios");
            var result=new Dictionary<string,ActivityRecipe>(StringComparer.Ordinal);
            foreach(var recipe in document.Recipes)
            {
                if(recipe==null||String.IsNullOrWhiteSpace(recipe.ActivityId)||!System.Text.RegularExpressions.Regex.IsMatch(recipe.ActivityId,"^[a-z][a-z0-9_]*$"))throw new InvalidDataException("activityId inválido");
                if(!activities.ContainsKey(recipe.ActivityId))throw new InvalidDataException("atividade ausente do catálogo: "+recipe.ActivityId);
                if(result.ContainsKey(recipe.ActivityId))throw new InvalidDataException("ID de receita duplicado: "+recipe.ActivityId);
                if(recipe.Steps==null||recipe.Steps.Count==0)throw new InvalidDataException(recipe.ActivityId+": steps vazio");
                for(int i=0;i<recipe.Steps.Count;i++)ValidateStep(recipe.ActivityId,recipe.Steps[i],i);
                ValidateOrder(recipe);
                result.Add(recipe.ActivityId,recipe);
            }
            foreach(string required in new[]{"idle","wander","sit"})if(!result.ContainsKey(required))throw new InvalidDataException("receita obrigatória ausente: "+required);
            return result;
        }
        catch(Exception ex) when(!(ex is InvalidDataException&&ex.Message.StartsWith(path+":",StringComparison.Ordinal)))
        {throw new InvalidDataException(path+": "+ex.Message,ex);}
    }

    private static void ValidateStep(string recipe,ActivityStep step,int index)
    {
        string prefix=recipe+" step["+index+"]: ";
        if(step==null)throw new InvalidDataException(prefix+"passo nulo");
        var known=new HashSet<string>(new[]{"find_target","find_waypoints","reserve_slot","move_to","align_to","occupy_slot","play_animation","wait","release_slot"},StringComparer.Ordinal);
        if(!known.Contains(step.Action))throw new InvalidDataException(prefix+"operação desconhecida: "+step.Action);
        if(step.Action=="find_target"&&String.IsNullOrWhiteSpace(step.Capability))throw new InvalidDataException(prefix+"capability obrigatória");
        if(step.Action=="find_target"&&String.IsNullOrWhiteSpace(step.Target))throw new InvalidDataException(prefix+"target obrigatório");
        if(step.Action=="move_to"&&String.IsNullOrWhiteSpace(step.Point))throw new InvalidDataException(prefix+"point obrigatório");
        if(step.Action=="play_animation"&&String.IsNullOrWhiteSpace(step.Animation))throw new InvalidDataException(prefix+"animation obrigatória");
        if(step.Action=="wait")
        {
            if(step.Until!="schedule_end"&&step.Until!="duration")throw new InvalidDataException(prefix+"until deve ser schedule_end ou duration");
            if(step.Until=="duration"&&step.DurationSeconds<=0)throw new InvalidDataException(prefix+"durationSeconds deve ser positivo");
        }
    }
    private static void ValidateOrder(ActivityRecipe recipe)
    {
        var actions=recipe.Steps.Select(x=>x.Action).ToList();
        if(recipe.ActivityId=="sit")
        {
            string[] required={"find_target","reserve_slot","move_to","align_to","occupy_slot","play_animation","wait","play_animation","release_slot"};
            int cursor=0;foreach(string action in actions)if(cursor<required.Length&&action==required[cursor])cursor++;
            if(cursor!=required.Length)throw new InvalidDataException("sit: sequência precisa encontrar/reservar/mover/alinha/ocupar/entrar/esperar/sair/liberar");
        }
        if(recipe.ActivityId=="wander"&&(!actions.Contains("find_waypoints")||!actions.Contains("move_to")||!actions.Contains("wait")))throw new InvalidDataException("wander: find_waypoints, move_to e wait são obrigatórios");
        int reserve=actions.IndexOf("reserve_slot"),occupy=actions.IndexOf("occupy_slot"),move=actions.IndexOf("move_to"),align=actions.IndexOf("align_to"),release=actions.LastIndexOf("release_slot");
        if(reserve>=0&&(actions.IndexOf("find_target")<0||actions.IndexOf("find_target")>reserve||move<reserve||occupy>=0&&align>=0&&(align<move||occupy<align)||occupy>=0&&occupy<move))throw new InvalidDataException(recipe.ActivityId+": ordem de busca/reserva/movimento/alinhamento/ocupação inválida");
        if(occupy>=0&&reserve<0)throw new InvalidDataException(recipe.ActivityId+": occupy_slot sem reserva");
        if(release>=0&&reserve<0)throw new InvalidDataException(recipe.ActivityId+": release_slot sem reserva");
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

internal sealed class FakeActivityPorts : IActivityPorts
{
    private sealed class Claim { public ActivityCandidate Candidate; public string Npc,Execution; public bool Occupied,Invalid; }
    private readonly Dictionary<string,Claim> claims=new Dictionary<string,Claim>(StringComparer.Ordinal);
    private readonly HashSet<string> activeOps=new HashSet<string>(StringComparer.Ordinal);
    private int opId,tokenId;
    public double OperationDurationSeconds=0.2;
    public double OperationTimeoutSeconds=120.0;
    public bool FailPath,RejectAnimation,FailCleanup,FailOperationCancel,InvalidateNextReservation;
    public int ActiveClaims { get { return claims.Count; } }
    public int ActiveOperationCount { get { return activeOps.Count; } }
    public IList<string> ActiveOperationIds { get { return activeOps.OrderBy(x=>x,StringComparer.Ordinal).ToList(); } }
    public ActivityOperation LastOperation { get; private set; }
    public int RestorePoseCalls { get; private set; }
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
        Claim c;if(FailCleanup||!claims.TryGetValue(token,out c)||c.Npc!=npc||c.Execution!=execution)return false;
        claims.Remove(token);return true;
    }
    public ActivityOperation Begin(string action,string npc,string execution,string target,string point,string animation,double now)
    {string id="op_"+(++opId).ToString("D6");activeOps.Add(id);LastOperation=new ActivityOperation(id,action,target,point,animation,now);return LastOperation;}
    public ActivityOperationResult Poll(ActivityOperation operation,string npc,string execution,double now)
    {
        if(!activeOps.Contains(operation.Id))return ActivityOperationResult.Failed("operation_cancelled");
        double elapsed=now-operation.StartedAt;
        if(operation.Action=="move_to"&&FailPath){activeOps.Remove(operation.Id);return ActivityOperationResult.Failed("path_unreachable");}
        if(operation.Action=="move_to"&&operation.Point=="target"&&!new[]{"home","pier","village_square","beach","farm","fish_market","hill_ruins"}.Contains(operation.Target)){activeOps.Remove(operation.Id);return ActivityOperationResult.Failed("target_unavailable");}
        if(operation.Action=="play_animation"&&RejectAnimation){activeOps.Remove(operation.Id);return ActivityOperationResult.Failed("animation_rejected");}
        if(elapsed>OperationTimeoutSeconds){activeOps.Remove(operation.Id);return ActivityOperationResult.Failed("operation_timeout");}
        if(elapsed<OperationDurationSeconds)return ActivityOperationResult.Running;
        activeOps.Remove(operation.Id);if(operation.Action=="play_animation")AnimationRequests.Add(operation.Animation);return ActivityOperationResult.Succeeded(operation.Target);
    }
    public bool Cancel(ActivityOperation operation,string npc,string execution)
    {
        if(operation==null||!activeOps.Contains(operation.Id))return true;
        if(FailOperationCancel)throw new InvalidOperationException("cancel was not confirmed");
        return activeOps.Remove(operation.Id);
    }
    public void RestorePose(string npc,string execution,string target){RestorePoseCalls++;}
    public void InvalidateOwnedReservations(string npc){foreach(var c in claims.Values.Where(x=>x.Npc==npc))c.Invalid=true;}
    public void ResetFailures(){FailPath=false;RejectAnimation=false;FailCleanup=false;FailOperationCancel=false;InvalidateNextReservation=false;}
}
