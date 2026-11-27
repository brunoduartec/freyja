using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Freyja.NpcScheduling
{
    public sealed class ScheduleTextDocument
    {
        public readonly string Name;
        public readonly string Json;
        public ScheduleTextDocument(string name, string json) { Name=name; Json=json; }
    }

    public sealed class ScheduleDataDocuments
    {
        public readonly string ActivitiesJson;
        public readonly IList<ScheduleTextDocument> Templates, Npcs, Overrides;
        public ScheduleDataDocuments(string activitiesJson, IList<ScheduleTextDocument> templates,
            IList<ScheduleTextDocument> npcs, IList<ScheduleTextDocument> overrides)
        { ActivitiesJson=activitiesJson; Templates=templates; Npcs=npcs; Overrides=overrides; }
    }

    public sealed class ScheduleDataLoadResult
    {
        public readonly ResolvedScheduleData Data;
        public readonly IList<string> Diagnostics;
        internal ScheduleDataLoadResult(ResolvedScheduleData data, IList<string> diagnostics)
        { Data=data; Diagnostics=new List<string>(diagnostics).AsReadOnly(); }
    }

    public static class ScheduleDataLoader
    {
        private sealed class ActivityFile { public int SchemaVersion { get; set; } public List<ActivityRow> Activities { get; set; } }
        private sealed class ActivityRow { public string Id { get; set; } public string TargetKind { get; set; } public string Label { get; set; } }
        private sealed class TemplateFile { public int SchemaVersion { get; set; } public string Id { get; set; } public List<EntryFile> Entries { get; set; } }
        private sealed class NpcFile { public int SchemaVersion { get; set; } public string NpcId { get; set; } public string TemplateId { get; set; } public List<EntryFile> Entries { get; set; } public List<EntryFile> ReplaceEntries { get; set; } public List<string> RemoveEntries { get; set; } public List<EntryFile> AddEntries { get; set; } }
        private sealed class OverrideFile { public int SchemaVersion { get; set; } public string Id { get; set; } public string NpcId { get; set; } public int Priority { get; set; } public ConditionFile Conditions { get; set; } public List<EntryFile> Entries { get; set; } }
        private sealed class ConditionFile { public List<int> Days { get; set; } public List<int> Weekdays { get; set; } public List<string> Weather { get; set; } public Dictionary<string,bool> Flags { get; set; } }
        private sealed class EntryFile { public string Id { get; set; } public string Start { get; set; } public string End { get; set; } public string ActivityId { get; set; } public string TargetId { get; set; } }

        public static ScheduleDataLoadResult Load(ScheduleDataDocuments documents)
        {
            if (documents==null) throw new ArgumentNullException("documents");
            var diagnostics=new List<string>();
            var activities=new Dictionary<string,ActivityDefinition>(StringComparer.Ordinal);
            var catalog=Read<ActivityFile>("activities.json",documents.ActivitiesJson);
            Version(catalog.SchemaVersion,"activities.json");
            if(catalog.Activities==null||catalog.Activities.Count==0)throw new InvalidDataException("activities.json: activities obrigatória e não vazia");
            foreach(var row in catalog.Activities)
            {
                if(row==null)throw new InvalidDataException("activities.json: definição nula");
                Id(row.Id,"activities.json");
                if(activities.ContainsKey(row.Id))throw new InvalidDataException("activities.json: ID duplicado "+row.Id);
                TargetKind kind;
                if(row.TargetKind=="none")kind=TargetKind.None;
                else if(row.TargetKind=="location")kind=TargetKind.Location;
                else if(row.TargetKind=="object")kind=TargetKind.Object;
                else throw new InvalidDataException("activities.json activity "+row.Id+": targetKind inválido");
                activities.Add(row.Id,new ActivityDefinition(row.Id,kind,row.Label));
            }

            var templates=new Dictionary<string,List<ScheduleEntry>>(StringComparer.Ordinal);
            foreach(var document in Ordered(documents.Templates))
            {
                var template=Read<TemplateFile>(document.Name,document.Json); Version(template.SchemaVersion,document.Name); Id(template.Id,document.Name);
                if(templates.ContainsKey(template.Id))throw new InvalidDataException(document.Name+": template duplicado");
                if(template.Entries==null||template.Entries.Count==0)throw new InvalidDataException(document.Name+": entries obrigatória e não vazia");
                var entries=Entries(template.Entries,document.Name); ValidateEntries(entries,document.Name,activities,true,diagnostics); templates.Add(template.Id,entries);
            }

            var schedules=new Dictionary<string,IList<ScheduleEntry>>(StringComparer.Ordinal);
            foreach(var document in Ordered(documents.Npcs))
            {
                var npc=Read<NpcFile>(document.Name,document.Json); Version(npc.SchemaVersion,document.Name); Id(npc.NpcId,document.Name);
                if(schedules.ContainsKey(npc.NpcId))throw new InvalidDataException(document.Name+": NPC duplicado");
                List<ScheduleEntry> entries;
                if(npc.TemplateId==null)entries=new List<ScheduleEntry>();
                else if(templates.TryGetValue(npc.TemplateId,out var templateEntries))entries=new List<ScheduleEntry>(templateEntries);
                else throw new InvalidDataException(document.Name+": template inexistente "+npc.TemplateId);
                var operations=new HashSet<string>(StringComparer.Ordinal);
                foreach(var id in npc.RemoveEntries??new List<string>())
                { Id(id,document.Name); if(!operations.Add(id))throw new InvalidDataException(document.Name+": operação conflitante "+id); if(entries.RemoveAll(e=>e.Id==id)!=1)throw new InvalidDataException(document.Name+": remoção inexistente "+id); }
                foreach(var entry in Entries(npc.ReplaceEntries,document.Name))
                { if(!operations.Add(entry.Id))throw new InvalidDataException(document.Name+": operação conflitante "+entry.Id); if(entries.RemoveAll(e=>e.Id==entry.Id)!=1)throw new InvalidDataException(document.Name+": substituição inexistente "+entry.Id); entries.Add(entry); }
                entries.AddRange(Entries(npc.AddEntries,document.Name)); entries.AddRange(Entries(npc.Entries,document.Name));
                if(entries.Count==0)throw new InvalidDataException(document.Name+": agenda resultante vazia");
                ValidateEntries(entries,document.Name,activities,true,diagnostics); schedules.Add(npc.NpcId,entries);
            }

            var overrides=new List<ScheduleOverride>(); var overrideIds=new HashSet<string>(StringComparer.Ordinal);
            foreach(var document in Ordered(documents.Overrides))
            {
                var item=Read<OverrideFile>(document.Name,document.Json); Version(item.SchemaVersion,document.Name); Id(item.Id,document.Name); Id(item.NpcId,document.Name);
                if(!overrideIds.Add(item.Id))throw new InvalidDataException(document.Name+": override duplicado "+item.Id);
                if(!schedules.ContainsKey(item.NpcId))throw new InvalidDataException(document.Name+": NPC inexistente "+item.NpcId);
                if(item.Priority!=50&&item.Priority!=70&&item.Priority!=90)throw new InvalidDataException(document.Name+": prioridade inválida");
                if(item.Entries==null||item.Entries.Count==0)throw new InvalidDataException(document.Name+": entries obrigatória e não vazia");
                var entries=Entries(item.Entries,document.Name); ValidateEntries(entries,document.Name,activities,false,diagnostics);
                overrides.Add(new ScheduleOverride(item.Id,item.NpcId,item.Priority,Conditions(item.Conditions,document.Name,diagnostics),entries));
            }

            foreach(var schedule in schedules)ValidateSchedule(schedule.Value,"NPC "+schedule.Key,activities,diagnostics);
            var orderedOverrides=overrides.OrderBy(x=>x.Id,StringComparer.Ordinal).ToList();
            for(int i=0;i<orderedOverrides.Count;i++)for(int j=i+1;j<orderedOverrides.Count;j++)
            {
                var left=orderedOverrides[i];var right=orderedOverrides[j];
                if(left.NpcId==right.NpcId&&left.Priority==right.Priority&&left.Entries.Any(a=>right.Entries.Any(b=>a.Start<b.End&&b.Start<a.End)))
                    diagnostics.Add("overrides: possível conflito de prioridade "+left.Priority+" entre "+left.Id+" e "+right.Id+" (condições avaliadas em runtime)");
            }
            return new ScheduleDataLoadResult(new ResolvedScheduleData(activities,schedules,orderedOverrides),diagnostics);
        }

        private static IEnumerable<ScheduleTextDocument> Ordered(IList<ScheduleTextDocument> documents)
        {
            if(documents==null)return Enumerable.Empty<ScheduleTextDocument>();
            if(documents.Any(x=>x==null||String.IsNullOrWhiteSpace(x.Name)))throw new InvalidDataException("Documento de agenda sem nome.");
            return documents.OrderBy(x=>x.Name,StringComparer.Ordinal);
        }
        private static T Read<T>(string source,string json)
        {
            try { return StrictJson.Deserialize<T>(json); }
            catch(Exception ex) { throw new InvalidDataException(source+": "+ex.Message,ex); }
        }
        private static ScheduleConditions Conditions(ConditionFile c,string source,IList<string> diagnostics)
        {
            if(c==null)return new ScheduleConditions(null,null,null,null);
            if(c.Days!=null&&c.Days.Count==0||c.Weekdays!=null&&c.Weekdays.Count==0||c.Weather!=null&&c.Weather.Count==0||c.Flags!=null&&c.Flags.Count==0)throw new InvalidDataException(source+": campos de condição presentes não podem ser vazios");
            if(c.Days?.Any(x=>x<1)==true)throw new InvalidDataException(source+": absolute day deve ser >= 1");
            if(c.Weekdays?.Any(x=>x<0||x>6)==true)throw new InvalidDataException(source+": weekday deve ser 0..6");
            if(c.Weather?.Any(String.IsNullOrWhiteSpace)==true)throw new InvalidDataException(source+": weather vazio");
            foreach(var flag in c.Flags??new Dictionary<string,bool>())Id(flag.Key,source);
            return new ScheduleConditions(c.Days?.ToArray(),c.Weekdays?.ToArray(),c.Weather?.ToArray(),c.Flags);
        }
        private static List<ScheduleEntry> Entries(List<EntryFile> source,string file)
        {
            var result=new List<ScheduleEntry>();
            foreach(var item in source??new List<EntryFile>())
            {
                if(item==null)throw new InvalidDataException(file+": entry nula");
                Id(item.Id,file); Id(item.ActivityId,file);
                result.Add(new ScheduleEntry(item.Id,EntryMinute(item.Start,false,file,item.Id,"start"),EntryMinute(item.End,true,file,item.Id,"end"),item.ActivityId,item.TargetId));
            }
            return result;
        }
        private static int EntryMinute(string value,bool allow24,string file,string entry,string field)
        { try{return GameTimeSnapshot.ParseMinute(value,allow24);}catch(FormatException ex){throw new InvalidDataException(file+": entry "+entry+" field "+field+": "+ex.Message,ex);} }
        private static void ValidateSchedule(IList<ScheduleEntry> entries,string file,IDictionary<string,ActivityDefinition> activities,IList<string> diagnostics)
        { ValidateEntries(entries,file,activities,true,diagnostics); }
        private static void ValidateEntries(IList<ScheduleEntry> entries,string file,IDictionary<string,ActivityDefinition> activities,bool checkGaps,IList<string> diagnostics)
        {
            var ids=new HashSet<string>(StringComparer.Ordinal);
            foreach(var entry in entries)
            {
                if(!ids.Add(entry.Id))throw new InvalidDataException(file+": entry duplicada "+entry.Id);
                if(entry.Start<0||entry.End>1440||entry.End<=entry.Start)throw new InvalidDataException(file+": intervalo inválido na entry "+entry.Id+" (divida à meia-noite)");
                if(!activities.TryGetValue(entry.ActivityId,out var activity))throw new InvalidDataException(file+": atividade desconhecida "+entry.ActivityId+" na entry "+entry.Id);
                if(activity.TargetKind!=TargetKind.None&&String.IsNullOrWhiteSpace(entry.TargetId))throw new InvalidDataException(file+": alvo obrigatório ausente na entry "+entry.Id);
                if(activity.TargetKind==TargetKind.None&&!String.IsNullOrWhiteSpace(entry.TargetId))throw new InvalidDataException(file+": atividade "+entry.ActivityId+" não aceita alvo na entry "+entry.Id);
                if(entry.TargetId!=null&&!System.Text.RegularExpressions.Regex.IsMatch(entry.TargetId,"^[a-z][a-z0-9_]*$"))throw new InvalidDataException(file+": destino simbólico inválido na entry "+entry.Id+": "+entry.TargetId);
            }
            var sorted=entries.OrderBy(x=>x.Start).ThenBy(x=>x.Id,StringComparer.Ordinal).ToArray();
            for(int i=1;i<sorted.Length;i++)if(sorted[i].Start<sorted[i-1].End)throw new InvalidDataException(file+": sobreposição entre "+sorted[i-1].Id+" e "+sorted[i].Id);
            if(checkGaps&&sorted.Length>0)
            {
                if(sorted[0].Start>0)diagnostics.Add(file+": lacuna 00:00–"+sorted[0].Start);
                for(int i=1;i<sorted.Length;i++)if(sorted[i].Start>sorted[i-1].End)diagnostics.Add(file+": lacuna após "+sorted[i-1].Id);
                if(sorted[sorted.Length-1].End<1440)diagnostics.Add(file+": lacuna até 24:00");
            }
        }
        private static void Id(string id,string file)
        { if(String.IsNullOrEmpty(id)||!System.Text.RegularExpressions.Regex.IsMatch(id,"^[a-z][a-z0-9_]*$"))throw new InvalidDataException(file+": ID inválido: "+id); }
        private static void Version(int version,string file)
        { if(version!=1)throw new InvalidDataException(file+": schemaVersion desconhecida "+version); }
    }
}
