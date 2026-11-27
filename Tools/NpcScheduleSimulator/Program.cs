using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Freyja.NpcScheduling;
using Freyja.NpcScheduling.Execution;

internal static class Program
{
    internal static string DataPath => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../Freyja/Assets/_Project/NpcScheduling/Data"));

    private static int Main(string[] args)
    {
        try
        {
            if(args.Contains("--test"))return Tests.Run();
            var data=Load(DataPath);
            if(args.Contains("--validate"))
            {
                var recipes=ActivityRecipeLoader.Load(File.ReadAllText(Path.Combine(DataPath,"execution","recipes.json")),data.Activities);
                Console.WriteLine("OK: "+data.NpcSchedules.Count+" NPCs, "+data.Activities.Count+" atividades, "+data.Overrides.Count+" overrides, "+recipes.Count+" receitas executáveis");
                return 0;
            }
            if(args.Contains("--query"))
            {
                int index=Array.IndexOf(args,"--query");var flags=new Dictionary<string,bool>(StringComparer.Ordinal);
                if(args.Length>index+5)foreach(string item in args[index+5].Split(',')){var pair=item.Split('=');if(pair.Length!=2||!Boolean.TryParse(pair[1],out var value))throw new ArgumentException("Flag esperada no formato nome=true|false: "+item);flags.Add(pair[0],value);}
                var resolution=new ScheduleResolver(data).Resolve(args[index+1],new GameTimeSnapshot(int.Parse(args[index+2]),GameTimeSnapshot.ParseMinute(args[index+3],false),0),new WorldContextSnapshot(args.Length>index+4?args[index+4]:"clear",flags));
                var intent=resolution.Intent;
                Console.WriteLine("Day "+intent.AbsoluteDay+" "+intent.NpcId+" "+intent.ActivityId+" "+(intent.TargetId??"-")+" priority="+intent.Priority+" valid="+intent.ValidFrom+".."+intent.ValidUntil+" "+intent.Origin.ToString().ToLowerInvariant()+":"+intent.SourceId+":"+intent.EntryId+" trace="+resolution.Trace.Reason);
                foreach(string diagnostic in resolution.Trace.Diagnostics)Console.Error.WriteLine("WARN "+diagnostic);
                return 0;
            }
            if(args.Contains("--simulate-week")){ExecutionWeekSimulator.Run(data,DataPath,Console.Out);return 0;}
            Console.WriteLine("Uso: --validate | --test | --query <npcId> <dia> <HH:mm> [clima] [flag=true,...] | --simulate-week");
            return 2;
        }
        catch(Exception ex){Console.Error.WriteLine("ERROR: "+ex.Message);return 1;}
    }

    internal static ResolvedScheduleData Load(string root,IEnumerable<string> additionalNpcDocuments=null)
    {
        List<ScheduleTextDocument> Documents(string folder)
        {
            return Directory.GetFiles(Path.Combine(root,folder),"*.json").OrderBy(path=>path,StringComparer.Ordinal)
                .Select(path=>new ScheduleTextDocument(path,File.ReadAllText(path))).ToList();
        }
        var npcDocuments=Documents("npcs");
        if(additionalNpcDocuments!=null)npcDocuments.AddRange(additionalNpcDocuments.Select(path=>new ScheduleTextDocument(path,File.ReadAllText(path))));
        var documents=new ScheduleDataDocuments(File.ReadAllText(Path.Combine(root,"activities.json")),Documents("templates"),npcDocuments,Documents("overrides"));
        ScheduleDataLoadResult result=ScheduleDataLoader.Load(documents);
        foreach(string diagnostic in result.Diagnostics)Console.Error.WriteLine("WARN "+diagnostic);
        return result.Data;
    }

    internal static void Simulate(ResolvedScheduleData data)
    {
        var resolver=new ScheduleResolver(data);
        foreach(string npc in data.NpcSchedules.Keys.OrderBy(x=>x,StringComparer.Ordinal))
        {
            string previous=null;
            for(int day=1;day<=7;day++)for(int minute=0;minute<1440;minute++)
            {
                var flags=new Dictionary<string,bool>();if(day==4)flags["demo_secret_meeting_unlocked"]=true;
                var world=new WorldContextSnapshot(day==2?"rain":"clear",flags);
                ActivityIntent intent=resolver.Resolve(npc,new GameTimeSnapshot(day,minute,0),world).Intent;
                if(intent.Identity!=previous)
                {
                    Console.WriteLine("Day "+day+" "+(minute/60).ToString("00")+":"+(minute%60).ToString("00")+" "+npc+" "+intent.ActivityId+" "+(intent.TargetId??"-")+" priority="+intent.Priority+" valid="+intent.ValidFrom+".."+intent.ValidUntil+" "+intent.Origin.ToString().ToLowerInvariant()+":"+intent.SourceId+":"+intent.EntryId);
                    previous=intent.Identity;
                }
            }
        }
    }
}
