using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Freyja.NpcScheduling;

internal static class Program
{
    internal static string DataPath => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../Freyja/Assets/_Project/NpcScheduling/Data"));
    private static int Main(string[] args)
    {
        try
        {
            if (args.Contains("--test")) return Tests.Run();
            var data=Load(DataPath);
            if(args.Contains("--validate")){ Console.WriteLine("OK: " + data.NpcSchedules.Count + " NPCs, " + data.Activities.Count + " atividades, " + data.Overrides.Count + " overrides"); return 0; }
            if(args.Contains("--query")){ int i=Array.IndexOf(args,"--query"); var flags=new Dictionary<string,bool>(StringComparer.Ordinal);if(args.Length>i+5)foreach(var item in args[i+5].Split(',')){var pair=item.Split('=');if(pair.Length!=2||!Boolean.TryParse(pair[1],out var value))throw new ArgumentException("Flag esperada no formato nome=true|false: "+item);flags.Add(pair[0],value);}var r=new ScheduleResolver(data).Resolve(args[i+1],new GameTimeSnapshot(int.Parse(args[i+2]),GameTimeSnapshot.ParseMinute(args[i+3],false),0),new WorldContextSnapshot(args.Length>i+4?args[i+4]:"clear",flags)); Console.WriteLine("Day "+r.Intent.AbsoluteDay+" "+r.Intent.NpcId+" "+r.Intent.ActivityId+" "+(r.Intent.TargetId??"-")+" priority="+r.Intent.Priority+" valid="+r.Intent.ValidFrom+".."+r.Intent.ValidUntil+" "+r.Intent.Origin.ToString().ToLowerInvariant()+":"+r.Intent.SourceId+":"+r.Intent.EntryId+" trace="+r.Trace.Reason);foreach(var diagnostic in r.Trace.Diagnostics)Console.Error.WriteLine("WARN "+diagnostic); return 0; }
            if(args.Contains("--simulate-week")){ Simulate(data); return 0; }
            Console.WriteLine("Uso: --validate | --test | --query <npcId> <dia> <HH:mm> [clima] [flag=true,...] | --simulate-week"); return 2;
        }
        catch(Exception ex){ Console.Error.WriteLine("ERROR: "+ex.Message); return 1; }
    }
    private static T Read<T>(string path)
    {
        try { return StrictJson.Deserialize<T>(File.ReadAllText(path)); }
        catch(Exception ex) when (!(ex is InvalidDataException && ex.Message.StartsWith(path+":",StringComparison.Ordinal))) { throw new InvalidDataException(path+": "+ex.Message,ex); }
    }
    private static string[] Files(string path)=>Directory.GetFiles(path,"*.json").OrderBy(x=>x,StringComparer.Ordinal).ToArray();
    internal static ResolvedScheduleData Load(string root)
    {
        var acts=new Dictionary<string,ActivityDefinition>(StringComparer.Ordinal);
        var cat=Read<ActivityFile>(Path.Combine(root,"activities.json")); Version(cat.SchemaVersion,"activities.json"); if(cat.Activities==null||cat.Activities.Count==0)throw new InvalidDataException("activities.json: activities obrigatória e não vazia");
        foreach(var a in cat.Activities){if(a==null)throw new InvalidDataException("activities.json: definição nula");Id(a.Id,"activities.json"); if(acts.ContainsKey(a.Id)) throw new InvalidDataException("activities.json: ID duplicado "+a.Id);TargetKind k;if(a.TargetKind=="none")k=TargetKind.None;else if(a.TargetKind=="location")k=TargetKind.Location;else if(a.TargetKind=="object")k=TargetKind.Object;else throw new InvalidDataException("activities.json activity "+a.Id+": targetKind inválido"); acts.Add(a.Id,new ActivityDefinition(a.Id,k,a.Label)); }
        var templates=new Dictionary<string,List<ScheduleEntry>>(StringComparer.Ordinal);
        foreach(var f in Files(Path.Combine(root,"templates"))){var t=Read<TemplateFile>(f); Version(t.SchemaVersion,f); Id(t.Id,f); if(templates.ContainsKey(t.Id)) throw new InvalidDataException(f+": template duplicado"); if(t.Entries==null||t.Entries.Count==0)throw new InvalidDataException(f+": entries obrigatória e não vazia");var entries=Entries(t.Entries,f);ValidateEntries(entries,f,acts,true);templates[t.Id]=entries;}
        var schedules=new Dictionary<string,IList<ScheduleEntry>>(StringComparer.Ordinal);
        foreach(var f in Files(Path.Combine(root,"npcs")))
        {
            var n=Read<NpcFile>(f); Version(n.SchemaVersion,f); Id(n.NpcId,f); if(schedules.ContainsKey(n.NpcId)) throw new InvalidDataException(f+": NPC duplicado");
            var entries=n.TemplateId==null?new List<ScheduleEntry>(): templates.TryGetValue(n.TemplateId,out var t)?new List<ScheduleEntry>(t):throw new InvalidDataException(f+": template inexistente "+n.TemplateId);
            var ops=new HashSet<string>(StringComparer.Ordinal);
            foreach(var id in n.RemoveEntries??new List<string>()){Id(id,f);if(!ops.Add(id))throw new InvalidDataException(f+": operação conflitante "+id); if(entries.RemoveAll(e=>e.Id==id)!=1)throw new InvalidDataException(f+": remoção inexistente "+id);}
            foreach(var e in Entries(n.ReplaceEntries,f)){if(!ops.Add(e.Id))throw new InvalidDataException(f+": operação conflitante "+e.Id); if(entries.RemoveAll(x=>x.Id==e.Id)!=1)throw new InvalidDataException(f+": substituição inexistente "+e.Id); entries.Add(e);}
            entries.AddRange(Entries(n.AddEntries,f)); entries.AddRange(Entries(n.Entries,f)); if(entries.Count==0)throw new InvalidDataException(f+": agenda resultante vazia");ValidateSchedule(entries,f,acts); schedules.Add(n.NpcId,entries);
        }
        var overrides=new List<ScheduleOverride>(); var overrideIds=new HashSet<string>(StringComparer.Ordinal);
        foreach(var f in Files(Path.Combine(root,"overrides"))){var o=Read<OverrideFile>(f);Version(o.SchemaVersion,f);Id(o.Id,f);Id(o.NpcId,f);if(!overrideIds.Add(o.Id))throw new InvalidDataException(f+": override duplicado "+o.Id);if(!schedules.ContainsKey(o.NpcId))throw new InvalidDataException(f+": NPC inexistente "+o.NpcId);if(o.Priority!=50&&o.Priority!=70&&o.Priority!=90)throw new InvalidDataException(f+": prioridade inválida");if(o.Entries==null||o.Entries.Count==0)throw new InvalidDataException(f+": entries obrigatória e não vazia");var es=Entries(o.Entries,f);ValidateEntries(es,f,acts,false);overrides.Add(new ScheduleOverride(o.Id,o.NpcId,o.Priority,Conditions(o.Conditions,f),es));}
        foreach(var group in schedules){ValidateSchedule(group.Value,"NPC "+group.Key,acts);}
        var orderedOverrides=overrides.OrderBy(x=>x.Id,StringComparer.Ordinal).ToList();
        for(int i=0;i<orderedOverrides.Count;i++)for(int j=i+1;j<orderedOverrides.Count;j++)
        {
            var left=orderedOverrides[i];var right=orderedOverrides[j];if(left.NpcId!=right.NpcId||left.Priority!=right.Priority)continue;
            if(left.Entries.Any(a=>right.Entries.Any(b=>a.Start<b.End&&b.Start<a.End)))Console.Error.WriteLine("WARN overrides: possível conflito de prioridade "+left.Priority+" entre "+left.Id+" e "+right.Id+" (condições avaliadas em runtime)");
        }
        return new ResolvedScheduleData(acts,schedules,orderedOverrides);
    }
    private static ScheduleConditions Conditions(ConditionFile c,string f){if(c==null)return new ScheduleConditions(null,null,null,null); if(c.Days!=null&&c.Days.Count==0||c.Weekdays!=null&&c.Weekdays.Count==0||c.Weather!=null&&c.Weather.Count==0||c.Flags!=null&&c.Flags.Count==0)throw new InvalidDataException(f+": campos de condição presentes não podem ser vazios");if(c.Days?.Any(x=>x<1)==true)throw new InvalidDataException(f+": absolute day deve ser >= 1"); if(c.Weekdays?.Any(x=>x<0||x>6)==true)throw new InvalidDataException(f+": weekday deve ser 0..6");if(c.Weather?.Any(String.IsNullOrWhiteSpace)==true)throw new InvalidDataException(f+": weather vazio");foreach(var flag in c.Flags??new Dictionary<string,bool>())Id(flag.Key,f); return new ScheduleConditions(c.Days?.ToArray(),c.Weekdays?.ToArray(),c.Weather?.ToArray(),c.Flags);}
    private static List<ScheduleEntry> Entries(List<EntryFile> es,string f){var result=new List<ScheduleEntry>(); foreach(var e in es??new List<EntryFile>()){if(e==null)throw new InvalidDataException(f+": entry nula");Id(e.Id,f);Id(e.ActivityId,f);int start=EntryMinute(e.Start,false,f,e.Id,"start"),end=EntryMinute(e.End,true,f,e.Id,"end");result.Add(new ScheduleEntry(e.Id,start,end,e.ActivityId,e.TargetId));} return result;}
    private static int EntryMinute(string value,bool allow24,string file,string entry,string field){try{return GameTimeSnapshot.ParseMinute(value,allow24);}catch(FormatException ex){throw new InvalidDataException(file+": entry "+entry+" field "+field+": "+ex.Message,ex);}}
    private static void ValidateSchedule(IList<ScheduleEntry> es,string f,IDictionary<string,ActivityDefinition> acts)=>ValidateEntries(es,f,acts,true);
    private static void ValidateEntries(IList<ScheduleEntry> es,string f,IDictionary<string,ActivityDefinition> acts,bool checkGaps)
    {
        var ids=new HashSet<string>(StringComparer.Ordinal); foreach(var e in es){if(!ids.Add(e.Id))throw new InvalidDataException(f+": entry duplicada "+e.Id);if(e.Start<0||e.End>1440||e.End<=e.Start)throw new InvalidDataException(f+": intervalo inválido na entry "+e.Id+" (divida à meia-noite)"); if(!acts.TryGetValue(e.ActivityId,out var a))throw new InvalidDataException(f+": atividade desconhecida "+e.ActivityId+" na entry "+e.Id); if(a.TargetKind!=TargetKind.None&&String.IsNullOrWhiteSpace(e.TargetId))throw new InvalidDataException(f+": alvo obrigatório ausente na entry "+e.Id);if(a.TargetKind==TargetKind.None&&!String.IsNullOrWhiteSpace(e.TargetId))throw new InvalidDataException(f+": atividade "+e.ActivityId+" não aceita alvo na entry "+e.Id); if(e.TargetId!=null&&!System.Text.RegularExpressions.Regex.IsMatch(e.TargetId,"^[a-z][a-z0-9_]*$"))throw new InvalidDataException(f+": destino simbólico inválido na entry "+e.Id+": "+e.TargetId);}
        var sorted=es.OrderBy(e=>e.Start).ThenBy(e=>e.Id,StringComparer.Ordinal).ToArray(); for(int i=1;i<sorted.Length;i++)if(sorted[i].Start<sorted[i-1].End)throw new InvalidDataException(f+": sobreposição entre "+sorted[i-1].Id+" e "+sorted[i].Id); if(checkGaps && sorted.Length>0){if(sorted[0].Start>0)Console.Error.WriteLine("WARN "+f+": lacuna 00:00–"+sorted[0].Start); for(int i=1;i<sorted.Length;i++)if(sorted[i].Start>sorted[i-1].End)Console.Error.WriteLine("WARN "+f+": lacuna após "+sorted[i-1].Id);if(sorted[sorted.Length-1].End<1440)Console.Error.WriteLine("WARN "+f+": lacuna até 24:00");}
    }
    private static void Id(string x,string file=null){if(String.IsNullOrEmpty(x)||!System.Text.RegularExpressions.Regex.IsMatch(x,"^[a-z][a-z0-9_]*$"))throw new InvalidDataException((file==null?"":file+": ")+"ID inválido: "+x);}
    private static void Version(int v,string f){if(v!=1)throw new InvalidDataException(f+": schemaVersion desconhecida "+v);}
    internal static void Simulate(ResolvedScheduleData data){var resolver=new ScheduleResolver(data);foreach(var n in data.NpcSchedules.Keys.OrderBy(x=>x,StringComparer.Ordinal)){string prior=null;for(int day=1;day<=7;day++)for(int minute=0;minute<1440;minute++){var flags=new Dictionary<string,bool>();if(day==4)flags["demo_secret_meeting_unlocked"]=true;var world=new WorldContextSnapshot(day==2?"rain":"clear",flags);var intent=resolver.Resolve(n,new GameTimeSnapshot(day,minute,0),world).Intent;if(intent.Identity!=prior){Console.WriteLine("Day "+day+" "+(minute/60).ToString("00")+":"+(minute%60).ToString("00")+" "+n+" "+intent.ActivityId+" "+(intent.TargetId??"-")+" priority="+intent.Priority+" valid="+intent.ValidFrom+".."+intent.ValidUntil+" "+intent.Origin.ToString().ToLowerInvariant()+":"+intent.SourceId+":"+intent.EntryId);prior=intent.Identity;}}}}
    private sealed class ActivityFile{public int SchemaVersion{get;set;} public List<ActivityRow> Activities{get;set;}}
    private sealed class ActivityRow{public string Id{get;set;} public string TargetKind{get;set;} public string Label{get;set;}}
    private sealed class TemplateFile{public int SchemaVersion{get;set;} public string Id{get;set;} public List<EntryFile> Entries{get;set;}}
    private sealed class NpcFile{public int SchemaVersion{get;set;} public string NpcId{get;set;} public string TemplateId{get;set;} public List<EntryFile> Entries{get;set;} public List<EntryFile> ReplaceEntries{get;set;} public List<string> RemoveEntries{get;set;} public List<EntryFile> AddEntries{get;set;}}
    private sealed class OverrideFile{public int SchemaVersion{get;set;} public string Id{get;set;} public string NpcId{get;set;} public int Priority{get;set;} public ConditionFile Conditions{get;set;} public List<EntryFile> Entries{get;set;}}
    private sealed class ConditionFile{public List<int> Days{get;set;} public List<int> Weekdays{get;set;} public List<string> Weather{get;set;} public Dictionary<string,bool> Flags{get;set;}}
    private sealed class EntryFile{public string Id{get;set;} public string Start{get;set;} public string End{get;set;} public string ActivityId{get;set;} public string TargetId{get;set;}}
}
