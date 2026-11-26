using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Freyja.NpcScheduling;
using Freyja.NpcScheduling.Execution;

internal sealed class ExecutionWeekResult
{
    public readonly int ActiveClaims, EventCount, Failures, Cancellations, Reservations, Releases;
    public ExecutionWeekResult(int activeClaims,int eventCount,int failures,int cancellations,int reservations,int releases)
    {ActiveClaims=activeClaims;EventCount=eventCount;Failures=failures;Cancellations=cancellations;Reservations=reservations;Releases=releases;}
}

internal static class ExecutionWeekSimulator
{
    private static readonly string[] Npcs={"demo_farmer","demo_fisherman","demo_resident"};
    private sealed class FixtureFile { public int SchemaVersion { get; set; } public List<FixtureNpc> NpcSchedules { get; set; } }
    private sealed class FixtureNpc { public string NpcId { get; set; } public List<FixtureEntry> Entries { get; set; } }
    private sealed class FixtureEntry { public string Id { get; set; } public string Start { get; set; } public string End { get; set; } public string ActivityId { get; set; } public string TargetId { get; set; } }

    public static ExecutionWeekResult Run(ResolvedScheduleData source,string dataRoot,TextWriter output)
    {
        var recipes=RecipeCatalog.Load(dataRoot,source.Activities);
        var scenario=CreateScenario(source,dataRoot);
        RunFullBankScenario(recipes,output);
        var resolver=new ScheduleResolver(scenario);var ports=new FakeActivityPorts();var sink=new RecordingActivitySink();
        var coordinators=Npcs.ToDictionary(n=>n,n=>new ActivityCoordinator(n,recipes,ports,sink),StringComparer.Ordinal);
        var prior=new Dictionary<string,string>(StringComparer.Ordinal);double now=0;
        for(int day=1;day<=7;day++)
        {
            for(int minute=0;minute<1440;minute++)
            {
                now=((day-1)*1440+minute)*60.0;
                var time=new GameTimeSnapshot(day,minute,0);
                var flags=new Dictionary<string,bool>();if(day==4)flags["demo_secret_meeting_unlocked"]=true;
                var world=new WorldContextSnapshot(day==2?"rain":"clear",flags);
                foreach(string npc in Npcs.OrderBy(x=>x,StringComparer.Ordinal))
                {
                    ActivityIntent intent=resolver.Resolve(npc,time,world).Intent;
                    // ValidFrom é o minuto atual da consulta, não parte da ocorrência do bloco.
                    string identity=intent.Identity+"|"+intent.AbsoluteDay;
                    string old;if(!prior.TryGetValue(npc,out old)||old!=identity)
                    {output.WriteLine("SIM_INTENT day="+day+" minute="+minute+" npc="+npc+" activity="+intent.ActivityId+" target="+(intent.TargetId??"-")+" source="+intent.SourceId+" entry="+intent.EntryId);prior[npc]=identity;}
                    if(day==2&&minute==481&&npc=="demo_farmer")ports.FailPath=true;
                    else if(day==2&&minute==481&&npc!="demo_farmer"||day==2&&minute==482)ports.FailPath=false;
                    if(day==5&&minute==483&&npc=="demo_fisherman")ports.RejectAnimation=true;
                    else if(day==5&&minute==483&&npc!="demo_fisherman"||day==5&&minute==484)ports.RejectAnimation=false;
                    if(day==3&&minute==485&&npc=="demo_resident")
                    {coordinators[npc].Cancel("demo_external_cancel",time,now);output.WriteLine("SIM_CONTROL day=3 minute=485 npc="+npc+" action=cancel reason=demo_external_cancel");}
                    if(day==3&&minute==486&&npc=="demo_resident")coordinators[npc].Resume(intent);
                    coordinators[npc].Tick(intent,time,now);
                }
            }
        }
        var shutdownTime=new GameTimeSnapshot(7,1439,0);
        foreach(string npc in Npcs){var current=resolver.Resolve(npc,shutdownTime,new WorldContextSnapshot("clear",null)).Intent;coordinators[npc].Cancel("simulation_shutdown",shutdownTime,now);}
        sink.Write(output);
        output.WriteLine("SIM_WEEK days=7 npc=3 events="+sink.Events.Count+" failures="+sink.Events.Count(x=>x.Kind==ExecutionEventKind.ActivityFailed)+" cancellations="+sink.Events.Count(x=>x.Kind==ExecutionEventKind.ActivityCancelled)+" reservations="+sink.Events.Count(x=>x.Kind==ExecutionEventKind.ResourceReserved)+" releases="+sink.Events.Count(x=>x.Kind==ExecutionEventKind.ResourceReleased)+" residualClaims="+ports.ActiveClaims+" physicalValidation=false");
        return new ExecutionWeekResult(ports.ActiveClaims,sink.Events.Count,
            sink.Events.Count(x=>x.Kind==ExecutionEventKind.ActivityFailed),sink.Events.Count(x=>x.Kind==ExecutionEventKind.ActivityCancelled),
            sink.Events.Count(x=>x.Kind==ExecutionEventKind.ResourceReserved),sink.Events.Count(x=>x.Kind==ExecutionEventKind.ResourceReleased));
    }

    private static void RunFullBankScenario(IDictionary<string,ActivityRecipe> recipes,TextWriter output)
    {
        var ports=new FakeActivityPorts();var sink=new RecordingActivitySink();
        var coordinators=new List<ActivityCoordinator>();var time=new GameTimeSnapshot(1,480,0);
        for(int i=0;i<3;i++)
        {
            string npc=Npcs[i];var coordinator=new ActivityCoordinator(npc,recipes,ports,sink);coordinators.Add(coordinator);
            var intent=Intent(npc,"sit","village_square",1,480,600);
            coordinator.Tick(intent,time,0);
        }
        for(int tick=1;tick<=5;tick++){var t=new GameTimeSnapshot(1,480+tick,0);foreach(var c in coordinators)c.Tick(IntentFor(c.Snapshot,1,480,600),t,tick);}
        int held=ports.ActiveClaims;
        output.WriteLine("SIM_SCENARIO full_bank target=village_square claimed="+held+" capacity=2 thirdFailed="+sink.Events.Any(x=>x.Kind==ExecutionEventKind.ActivityFailed)+" exclusive=true");
        var cleanup=new GameTimeSnapshot(1,490,0);foreach(var c in coordinators)c.Cancel("scenario_cleanup",cleanup,10);
        output.WriteLine("SIM_SCENARIO full_bank_cleanup residualClaims="+ports.ActiveClaims);
    }

    private static ActivityIntent IntentFor(ActivitySnapshot snapshot,int day,int start,int end)
    {
        if(snapshot==null)return Intent("demo_resident","sit","village_square",day,start,end);
        return Intent(snapshot.NpcId,"sit","village_square",day,start,end);
    }

    private static ResolvedScheduleData CreateScenario(ResolvedScheduleData source,string dataRoot)
    {
        string path=Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,"../../../../../Tools/NpcScheduleSimulator/Fixtures/execution-week.json"));
        try
        {
            var fixture=StrictJson.Deserialize<FixtureFile>(File.ReadAllText(path));
            if(fixture.SchemaVersion!=1||fixture.NpcSchedules==null)throw new InvalidDataException("fixture requer schemaVersion 1 e npcSchedules");
            var schedules=new Dictionary<string,IList<ScheduleEntry>>(StringComparer.Ordinal);
            foreach(var npc in fixture.NpcSchedules)
            {
                if(npc==null||!Npcs.Contains(npc.NpcId)||schedules.ContainsKey(npc.NpcId)||npc.Entries==null)throw new InvalidDataException("NPC de fixture ausente, duplicado ou fora da lista de demonstração");
                var entries=new List<ScheduleEntry>();var ids=new HashSet<string>(StringComparer.Ordinal);
                foreach(var row in npc.Entries)
                {
                    if(row==null||String.IsNullOrWhiteSpace(row.Id)||!ids.Add(row.Id)||!source.Activities.TryGetValue(row.ActivityId,out var activity))throw new InvalidDataException(npc.NpcId+": entry ou atividade inválida");
                    int start=GameTimeSnapshot.ParseMinute(row.Start,false),end=GameTimeSnapshot.ParseMinute(row.End,true);
                    if(start<0||end>1440||start>=end)throw new InvalidDataException(npc.NpcId+"/"+row.Id+": intervalo inválido");
                    if(activity.TargetKind==TargetKind.None&&!String.IsNullOrEmpty(row.TargetId)||activity.TargetKind!=TargetKind.None&&String.IsNullOrEmpty(row.TargetId))throw new InvalidDataException(npc.NpcId+"/"+row.Id+": target incompatível com catálogo");
                    entries.Add(new ScheduleEntry(row.Id,start,end,row.ActivityId,row.TargetId));
                }
                var ordered=entries.OrderBy(x=>x.Start).ToArray();for(int i=1;i<ordered.Length;i++)if(ordered[i].Start<ordered[i-1].End)throw new InvalidDataException(npc.NpcId+": blocos sobrepostos");
                if(entries.Count==0||ordered[0].Start!=0||ordered[ordered.Length-1].End!=1440||ordered.Where((x,i)=>i>0&&x.Start!=ordered[i-1].End).Any())throw new InvalidDataException(npc.NpcId+": fixture precisa cobrir o dia sem lacunas");
                schedules.Add(npc.NpcId,entries);
            }
            if(Npcs.Any(n=>!schedules.ContainsKey(n)))throw new InvalidDataException("fixture precisa conter os três NPCs configurados");
            var overrides=source.Overrides.Where(x=>Npcs.Contains(x.NpcId)).ToList();
            return new ResolvedScheduleData(source.Activities,schedules,overrides);
        }
        catch(Exception ex) when(!(ex is InvalidDataException&&ex.Message.StartsWith(path+":",StringComparison.Ordinal)))
        {throw new InvalidDataException(path+": "+ex.Message,ex);}
    }
    private static ActivityIntent Intent(string npc,string activity,string target,int day,int start,int end)
    {return new ActivityIntent(npc,activity,target,day,start,end,30,ScheduleOrigin.Base,"execution_fixture","demo_"+activity);}
}
