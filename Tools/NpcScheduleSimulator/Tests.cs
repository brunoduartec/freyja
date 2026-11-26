using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Freyja.NpcScheduling;

internal static class Tests
{
    private static int count;
    private static void Is(bool ok,string name){count++;if(!ok)throw new Exception("FAIL "+name);}
    public static int Run()
    {
        var data=Program.Load(Program.DataPath);var resolver=new ScheduleResolver(data);
        var t=new GameTimeSnapshot(1,0,0);Is(t.Weekday==0,"weekday derivation");
        Is(new GameTimeSnapshot(7,0,0).Weekday==6,"Sunday derives from Monday-based fixture calendar");
        Is(GameTimeSnapshot.ParseMinute("24:00",true)==1440,"24:00");
        bool start24=false;try{GameTimeSnapshot.ParseMinute("24:00",false);}catch(FormatException){start24=true;}Is(start24,"24:00 rejected as start");
        bool unpadded=false;try{GameTimeSnapshot.ParseMinute("6:00",false);}catch(FormatException){unpadded=true;}Is(unpadded,"HH:mm requires zero padding");
        var fullDay=new ScheduleEntry("full_day",0,1440,"sleep","home");Is(fullDay.Contains(0)&&fullDay.Contains(1439)&&!fullDay.Contains(1440),"full-day semi-open interval");
        Is(resolver.Resolve("demo_fisherman",t,new WorldContextSnapshot("clear",null)).Intent.ActivityId=="sleep","start boundary");
        Is(resolver.Resolve("demo_fisherman",new GameTimeSnapshot(1,360,0),new WorldContextSnapshot("clear",null)).Intent.ActivityId=="eat","next boundary");
        Is(resolver.Resolve("demo_fisherman",new GameTimeSnapshot(1,1439,0),new WorldContextSnapshot("clear",null)).Intent.ActivityId=="sleep","23:59");
        Is(resolver.Resolve("demo_fisherman",new GameTimeSnapshot(2,0,0),new WorldContextSnapshot("clear",null)).Intent.ActivityId=="sleep","midnight next day");
        var rain=resolver.Resolve("demo_fisherman",new GameTimeSnapshot(2,420,0),new WorldContextSnapshot("rain",null)).Intent;Is(rain.ActivityId=="repair"&&rain.Priority==50,"weather priority");
        Is(resolver.Resolve("demo_fisherman",new GameTimeSnapshot(2,420,0),new WorldContextSnapshot("clear",null)).Intent.ActivityId=="fish","same-minute context change");
        var story=resolver.Resolve("demo_fisherman",new GameTimeSnapshot(4,1380,0),new WorldContextSnapshot("clear",new Dictionary<string,bool>{{"demo_secret_meeting_unlocked",true}})).Intent;Is(story.ActivityId=="secret_meeting"&&story.Priority==90,"story priority");
        Is(resolver.Resolve("demo_fisherman",new GameTimeSnapshot(4,1380,0),new WorldContextSnapshot("clear",new Dictionary<string,bool>())).Intent.ActivityId!="secret_meeting","absent flag");
        Is(resolver.Resolve("demo_merchant",new GameTimeSnapshot(7,480,0),new WorldContextSnapshot("clear",null)).Intent.ActivityId=="rest","weekly override");
        Is(resolver.Resolve("demo_fisherman",new GameTimeSnapshot(4,1425,0),new WorldContextSnapshot("clear",new Dictionary<string,bool>{{"demo_secret_meeting_unlocked",true}})).Intent.ActivityId=="sleep","override end returns to base");
        Is(resolver.Resolve("demo_fisherman",new GameTimeSnapshot(2,720,0),new WorldContextSnapshot("clear",null)).Intent.ActivityId=="eat","weather condition AND time");
        Is(GameTimeSnapshot.ParseMinute("00:00",false)==0,"day start parser");
        bool invalid=false;try{GameTimeSnapshot.ParseMinute("24:01",true);}catch(FormatException){invalid=true;}Is(invalid,"invalid time rejected");
        Is(data.NpcSchedules.Count==5,"five demo NPCs");
        Is(data.Activities.Count==15&&new[]{"idle","wander","sit","sleep","eat","rest","chat","fish","sell_goods","water_crops","harvest","feed_animals","milk_cow","repair","secret_meeting"}.All(id=>data.Activities.ContainsKey(id)),"complete declared activity catalog");
        Is(data.Activities["milk_cow"].TargetKind==TargetKind.Object&&data.NpcSchedules["demo_livestock_keeper"].Single(e=>e.ActivityId=="milk_cow").TargetId=="demo_cow","logical object target type and alias");
        foreach(var schedule in data.NpcSchedules){var ordered=schedule.Value.OrderBy(e=>e.Start).ToArray();Is(ordered[0].Start==0&&ordered[ordered.Length-1].End==1440&&ordered.Zip(ordered.Skip(1),(a,b)=>a.End==b.Start).All(x=>x),schedule.Key+" covers contiguous 24 hours");}
        int templateCount=data.NpcSchedules["demo_fisherman"].Count;bool protectedData=false;try{data.NpcSchedules["demo_farmer"].Clear();}catch(NotSupportedException){protectedData=true;}Is(protectedData&&data.NpcSchedules["demo_fisherman"].Count==templateCount,"read-only expanded schedules");
        Is(resolver.Resolve("demo_fisherman",new GameTimeSnapshot(1,400,0),new WorldContextSnapshot("clear",null)).Intent.Identity==resolver.Resolve("demo_fisherman",new GameTimeSnapshot(1,401,0),new WorldContextSnapshot("clear",null)).Intent.Identity,"timestamp-independent identity");
        var tieA=new ScheduleOverride("a_rule","x",50,new ScheduleConditions(null,null,null,null),new List<ScheduleEntry>{new ScheduleEntry("a",0,60,"idle",null)});
        var tieB=new ScheduleOverride("b_rule","x",50,new ScheduleConditions(null,null,null,null),new List<ScheduleEntry>{new ScheduleEntry("b",0,60,"rest","home")});
        var tieData=new ResolvedScheduleData(data.Activities,new Dictionary<string,IList<ScheduleEntry>>{{"x",new List<ScheduleEntry>()}},new List<ScheduleOverride>{tieB,tieA});
        var tied=new ScheduleResolver(tieData).Resolve("x",new GameTimeSnapshot(1,0,0),new WorldContextSnapshot("clear",null));Is(tied.Intent.ActivityId=="idle"&&tied.Trace.Diagnostics.Count==1,"stable tie diagnostics independent of order");
        var gapData=new ResolvedScheduleData(data.Activities,new Dictionary<string,IList<ScheduleEntry>>{{"x",new List<ScheduleEntry>{new ScheduleEntry("later",60,120,"rest","home")}}},new List<ScheduleOverride>());
        var gap=new ScheduleResolver(gapData).Resolve("x",new GameTimeSnapshot(1,30,0),new WorldContextSnapshot("clear",null)).Intent;Is(gap.ActivityId=="idle"&&gap.ValidUntil==60,"idle gap bounded to next entry");
        Is(new ScheduleResolver(gapData).Resolve("x",new GameTimeSnapshot(1,60,0),new WorldContextSnapshot("clear",null)).Intent.ActivityId=="rest","fallback reevaluates at next base boundary");
        var temp=Path.Combine(Path.GetTempPath(),"npc-schedule-test-"+Guid.NewGuid().ToString("N"));Copy(Program.DataPath,temp);
        var f=Path.Combine(temp,"templates","fisherman.json");var text=File.ReadAllText(f).Replace("\"end\": \"06:00\"","\"end\": \"05:00\"");File.WriteAllText(f,text);
        var changed=Program.Load(temp);Is(new ScheduleResolver(changed).Resolve("demo_fisherman",new GameTimeSnapshot(1,300,0),new WorldContextSnapshot("clear",null)).Intent.ActivityId!="sleep","data edit affects result");Directory.Delete(temp,true);
        var invalidRoot=Path.Combine(Path.GetTempPath(),"npc-schedule-invalid-"+Guid.NewGuid().ToString("N"));Copy(Program.DataPath,invalidRoot);
        File.WriteAllText(Path.Combine(invalidRoot,"npcs","demo_farmer.json"),"{\"schemaVersion\":1,\"npcId\":\"demo_farmer\",\"templateId\":\"unknown\"}");
        bool broken=false;try{Program.Load(invalidRoot);}catch(InvalidDataException){broken=true;}Is(broken,"broken template reference rejected");Directory.Delete(invalidRoot,true);
        var opsRoot=Path.Combine(Path.GetTempPath(),"npc-schedule-ops-"+Guid.NewGuid().ToString("N"));Copy(Program.DataPath,opsRoot);var npcPath=Path.Combine(opsRoot,"npcs","demo_fisherman.json");
        File.WriteAllText(npcPath,"{\"schemaVersion\":1,\"npcId\":\"demo_fisherman\",\"templateId\":\"fisherman\",\"removeEntries\":[\"social\"],\"addEntries\":[{\"id\":\"new_social\",\"start\":\"17:00\",\"end\":\"20:00\",\"activityId\":\"chat\",\"targetId\":\"village_square\"}]}");var ops=Program.Load(opsRoot);Is(ops.NpcSchedules["demo_fisherman"].Any(e=>e.Id=="new_social")&&!ops.NpcSchedules["demo_fisherman"].Any(e=>e.Id=="social"),"template remove and add");
        File.WriteAllText(npcPath,"{\"schemaVersion\":1,\"npcId\":\"demo_fisherman\",\"templateId\":\"fisherman\",\"removeEntries\":[\"social\"],\"replaceEntries\":[{\"id\":\"social\",\"start\":\"17:00\",\"end\":\"20:00\",\"activityId\":\"chat\",\"targetId\":\"village_square\"}]}");bool conflict=false;try{Program.Load(opsRoot);}catch(InvalidDataException){conflict=true;}Is(conflict,"conflicting template ops rejected");Directory.Delete(opsRoot,true);
        bool malformed=false;try{StrictJson.Deserialize<ActivityFileProbe>("{\"x\":1,\"x\":2}");}catch(FormatException){malformed=true;}Is(malformed,"duplicate JSON property rejected");
        bool truncated=false;try{StrictJson.Deserialize<ActivityFileProbe>("{");}catch(FormatException){truncated=true;}Is(truncated,"malformed JSON rejected");
        var replaceRoot=Path.Combine(Path.GetTempPath(),"npc-schedule-replace-"+Guid.NewGuid().ToString("N"));Copy(Program.DataPath,replaceRoot);
        File.WriteAllText(Path.Combine(replaceRoot,"npcs","demo_fisherman.json"),"{\"schemaVersion\":1,\"npcId\":\"demo_fisherman\",\"templateId\":\"fisherman\",\"replaceEntries\":[{\"id\":\"social\",\"start\":\"17:00\",\"end\":\"20:00\",\"activityId\":\"rest\",\"targetId\":\"home\"}]}");
        var replaced=Program.Load(replaceRoot);Is(replaced.NpcSchedules["demo_fisherman"].Single(e=>e.Id=="social").ActivityId=="rest","template replacement succeeds");Is(replaced.NpcSchedules["demo_farmer"].Single(e=>e.Id=="social").ActivityId=="chat","same-template NPC remains isolated");Directory.Delete(replaceRoot,true);
        InvalidFixture("version", "templates/fisherman.json", s=>s.Replace("\"schemaVersion\": 1","\"schemaVersion\": 9"));
        InvalidFixture("unknown field", "templates/fisherman.json", s=>s.Replace("\"id\": \"fisherman\"","\"id\": \"fisherman\", \"typo\": true"));
        InvalidFixture("unknown activity", "templates/fisherman.json", s=>s.Replace("\"activityId\": \"fish\"","\"activityId\": \"not_real\""));
        InvalidFixture("invalid activity ID", "activities.json", s=>s.Replace("\"id\": \"sleep\"","\"id\": \"Sleep\""));
        InvalidFixture("duplicate activity ID", "activities.json", s=>s.Replace("\"id\": \"sleep\"","\"id\": \"fish\""));
        InvalidFixture("invalid activity target kind", "activities.json", s=>s.Replace("\"targetKind\": \"location\"","\"targetKind\": \"Location\""));
        InvalidFixture("duplicate entry ID", "templates/fisherman.json", s=>s.Replace("\"id\": \"sleep_night\"","\"id\": \"rest\""));
        InvalidFixture("malformed symbolic target", "templates/fisherman.json", s=>s.Replace("\"targetId\": \"pier\"","\"targetId\": \"Pier\""));
        InvalidFixture("missing required target", "templates/fisherman.json", s=>s.Replace(",\n      \"targetId\": \"pier\"",""));
        InvalidFixture("zero duration", "templates/fisherman.json", s=>s.Replace("\"start\": \"06:00\",\n      \"end\": \"07:00\"","\"start\": \"06:00\",\n      \"end\": \"06:00\""));
        InvalidFixture("overlap", "templates/fisherman.json", s=>s.Replace("\"start\": \"07:00\",\n      \"end\": \"12:00\"","\"start\": \"06:30\",\n      \"end\": \"12:00\""));
        InvalidFixture("inverted interval", "templates/fisherman.json", s=>s.Replace("\"start\": \"07:00\",\n      \"end\": \"12:00\"","\"start\": \"12:01\",\n      \"end\": \"12:00\""));
        InvalidFixture("invalid override priority", "overrides/demo_rain_repair.json", s=>s.Replace("\"priority\": 50","\"priority\": 55"));
        InvalidFixture("invalid weekday condition", "overrides/demo_sunday_market.json", s=>s.Replace("6","7"));
        InvalidFixture("overlap within one override", "overrides/demo_rain_repair.json", s=>s.Replace("\"targetId\": \"home\"\n    }\n  ]\n}","\"targetId\": \"home\"\n    },\n    {\"id\":\"overlap\",\"start\":\"08:00\",\"end\":\"09:00\",\"activityId\":\"repair\",\"targetId\":\"home\"}\n  ]\n}"));
        var duplicateNpcRoot=Path.Combine(Path.GetTempPath(),"npc-schedule-duplicate-"+Guid.NewGuid().ToString("N"));Copy(Program.DataPath,duplicateNpcRoot);File.Copy(Path.Combine(duplicateNpcRoot,"npcs","demo_farmer.json"),Path.Combine(duplicateNpcRoot,"npcs","z_duplicate.json"));bool duplicateNpc=false;try{Program.Load(duplicateNpcRoot);}catch(InvalidDataException){duplicateNpc=true;}finally{Directory.Delete(duplicateNpcRoot,true);}Is(duplicateNpc,"duplicate NPC IDs rejected");
        var conditionRule=new ScheduleOverride("and_or","x",70,new ScheduleConditions(new[]{2,3},new[]{0,1},new[]{"rain","storm"},new Dictionary<string,bool>{{"present_true",true},{"present_false",false}}),new[]{new ScheduleEntry("conditional",0,60,"repair","home")});
        Is(conditionRule.Conditions.Matches(new GameTimeSnapshot(2,0,0),new WorldContextSnapshot("rain",new Dictionary<string,bool>{{"present_true",true},{"present_false",false}})),"conditions AND fields and OR values");
        Is(!conditionRule.Conditions.Matches(new GameTimeSnapshot(2,0,0),new WorldContextSnapshot("rain",new Dictionary<string,bool>{{"present_true",true}})),"absent false flag does not match");
        var baseBlocks=new List<ScheduleEntry>{new ScheduleEntry("morning",0,60,"chat","home"),new ScheduleEntry("afternoon",60,120,"eat","home")};
        var longOverride=new ScheduleOverride("long_rule","x",70,new ScheduleConditions(null,null,null,null),new[]{new ScheduleEntry("long",30,90,"repair","home")});
        var longResolver=new ScheduleResolver(new ResolvedScheduleData(data.Activities,new Dictionary<string,IList<ScheduleEntry>>{{"x",baseBlocks}},new List<ScheduleOverride>{longOverride}));
        Is(longResolver.Resolve("x",new GameTimeSnapshot(1,45,0),new WorldContextSnapshot("clear",null)).Intent.ActivityId=="repair","override crosses base block boundary");
        Is(longResolver.Resolve("x",new GameTimeSnapshot(1,90,0),new WorldContextSnapshot("clear",null)).Intent.ActivityId=="eat","expired override returns to currently valid base block");
        var nightBase=new List<ScheduleEntry>{new ScheduleEntry("sleep_all_day",0,1440,"sleep","home")};
        var splitOverrides=new List<ScheduleOverride>{new ScheduleOverride("night_day_one","x",90,new ScheduleConditions(new[]{1},null,null,null),new[]{new ScheduleEntry("meeting_day_one",1380,1440,"secret_meeting","hill_ruins")}),new ScheduleOverride("night_day_two","x",90,new ScheduleConditions(new[]{2},null,null,null),new[]{new ScheduleEntry("meeting_day_two",0,30,"secret_meeting","hill_ruins")})};
        var splitResolver=new ScheduleResolver(new ResolvedScheduleData(data.Activities,new Dictionary<string,IList<ScheduleEntry>>{{"x",nightBase}},splitOverrides));
        Is(splitResolver.Resolve("x",new GameTimeSnapshot(1,1439,0),new WorldContextSnapshot("clear",null)).Intent.ActivityId=="secret_meeting","first explicit midnight exception block");Is(splitResolver.Resolve("x",new GameTimeSnapshot(2,0,0),new WorldContextSnapshot("clear",null)).Intent.ActivityId=="secret_meeting","next-day exception block has next absolute day condition");Is(splitResolver.Resolve("x",new GameTimeSnapshot(2,30,0),new WorldContextSnapshot("clear",null)).Intent.ActivityId=="sleep","second night block expires into current base");
        var priorityRules=new List<ScheduleOverride>{new ScheduleOverride("world","x",50,new ScheduleConditions(null,null,null,null),new[]{new ScheduleEntry("world",30,60,"rest","home")}),new ScheduleOverride("mission","x",70,new ScheduleConditions(null,null,null,null),new[]{new ScheduleEntry("mission",30,60,"repair","home")}),new ScheduleOverride("narrative","x",90,new ScheduleConditions(null,null,null,null),new[]{new ScheduleEntry("narrative",30,60,"secret_meeting","home")})};
        var priorityResolver=new ScheduleResolver(new ResolvedScheduleData(data.Activities,new Dictionary<string,IList<ScheduleEntry>>{{"x",baseBlocks}},priorityRules));
        Is(priorityResolver.Resolve("x",new GameTimeSnapshot(1,45,0),new WorldContextSnapshot("clear",null)).Intent.ActivityId=="secret_meeting","narrative priority 90 outranks mission and world");
        priorityRules.RemoveAt(2);Is(new ScheduleResolver(new ResolvedScheduleData(data.Activities,new Dictionary<string,IList<ScheduleEntry>>{{"x",baseBlocks}},priorityRules)).Resolve("x",new GameTimeSnapshot(1,45,0),new WorldContextSnapshot("clear",null)).Intent.ActivityId=="repair","mission priority 70 outranks world");
        var conflictRoot=Path.Combine(Path.GetTempPath(),"npc-schedule-rules-"+Guid.NewGuid().ToString("N"));Copy(Program.DataPath,conflictRoot);
        File.WriteAllText(Path.Combine(conflictRoot,"overrides","demo_rain_conflict.json"),"{\"schemaVersion\":1,\"id\":\"demo_rain_conflict\",\"npcId\":\"demo_fisherman\",\"priority\":50,\"conditions\":{\"weather\":[\"rain\"]},\"entries\":[{\"id\":\"conflict_block\",\"start\":\"08:00\",\"end\":\"09:00\",\"activityId\":\"repair\",\"targetId\":\"home\"}]}");
        var previousError=Console.Error;using(var diagnostics=new StringWriter()){try{Console.SetError(diagnostics);Program.Load(conflictRoot);Is(diagnostics.ToString().Contains("possível conflito de prioridade 50 entre demo_rain_conflict e demo_rain_repair",StringComparison.Ordinal),"static same-priority conflict warning is ordinal");}finally{Console.SetError(previousError);}}Directory.Delete(conflictRoot,true);
        var simA=CaptureSimulation(data);var simB=CaptureSimulation(data);Is(simA==simB,"two full week simulations have identical transitions");Is(simA.Split('\n').Count(x=>x.StartsWith("Day ",StringComparison.Ordinal))==296,"week emits transitions rather than minute ticks");
        count+=ExecutionTests.Run(data);
        Console.WriteLine("PASS: "+count+" assertions");return 0;
    }
    private static string CaptureSimulation(ResolvedScheduleData data){var saved=Console.Out;using(var output=new StringWriter()){try{Console.SetOut(output);Program.Simulate(data);return output.ToString();}finally{Console.SetOut(saved);}}}
    private static void InvalidFixture(string name,string relative,Func<string,string> edit){var root=Path.Combine(Path.GetTempPath(),"npc-schedule-invalid-"+Guid.NewGuid().ToString("N"));Copy(Program.DataPath,root);var path=Path.Combine(root,relative);File.WriteAllText(path,edit(File.ReadAllText(path)));bool rejected=false;try{Program.Load(root);}catch(Exception ex){rejected=ex is InvalidDataException||ex is FormatException;}finally{Directory.Delete(root,true);}Is(rejected,name+" rejected");}
    private static void Copy(string from,string to){Directory.CreateDirectory(to);foreach(var f in Directory.GetFiles(from)){File.Copy(f,Path.Combine(to,Path.GetFileName(f)));}foreach(var d in Directory.GetDirectories(from)){var target=Path.Combine(to,Path.GetFileName(d));Copy(d,target);}}
    private sealed class ActivityFileProbe { public int X {get;set;} }
}
