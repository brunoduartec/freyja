using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Freyja.NpcScheduling
{
    public enum TargetKind { None, Location, Object }
    public enum ScheduleOrigin { Base, Override, Fallback }

    public sealed class ActivityDefinition
    {
        public readonly string Id;
        public readonly TargetKind TargetKind;
        public readonly string Label;
        public ActivityDefinition(string id, TargetKind targetKind, string label) { Id = id; TargetKind = targetKind; Label = label; }
    }

    public sealed class ScheduleEntry
    {
        public readonly string Id, ActivityId, TargetId;
        public readonly int Start, End;
        public ScheduleEntry(string id, int start, int end, string activityId, string targetId) { Id=id; Start=start; End=end; ActivityId=activityId; TargetId=targetId; }
        public bool Contains(int minute) { return minute >= Start && minute < End; }
    }

    public sealed class ScheduleConditions
    {
        public readonly IList<int> Days;
        public readonly IList<int> Weekdays;
        public readonly IList<string> Weather;
        public readonly IDictionary<string, bool> Flags;
        public ScheduleConditions(int[] days, int[] weekdays, string[] weather, IDictionary<string, bool> flags) { Days=days==null?null:Array.AsReadOnly((int[])days.Clone()); Weekdays=weekdays==null?null:Array.AsReadOnly((int[])weekdays.Clone()); Weather=weather==null?null:Array.AsReadOnly((string[])weather.Clone()); Flags=new ReadOnlyDictionary<string,bool>(new Dictionary<string,bool>(flags ?? new Dictionary<string,bool>())); }
        public bool Matches(GameTimeSnapshot time, WorldContextSnapshot world)
        {
            if (Days != null && !Days.Contains(time.AbsoluteDay)) return false;
            if (Weekdays != null && !Weekdays.Contains(time.Weekday)) return false;
            if (Weather != null && !Weather.Contains(world.Weather)) return false;
            foreach (var pair in Flags) { bool value; if (!world.Flags.TryGetValue(pair.Key, out value) || value != pair.Value) return false; }
            return true;
        }
    }

    public sealed class ScheduleOverride
    {
        public readonly string Id, NpcId;
        public readonly int Priority;
        public readonly ScheduleConditions Conditions;
        public readonly IList<ScheduleEntry> Entries;
        public ScheduleOverride(string id, string npcId, int priority, ScheduleConditions conditions, IList<ScheduleEntry> entries) { Id=id; NpcId=npcId; Priority=priority; Conditions=conditions; Entries=Array.AsReadOnly(entries.ToArray()); }
    }

    public sealed class GameTimeSnapshot
    {
        public readonly int AbsoluteDay, MinuteOfDay, Weekday;
        public GameTimeSnapshot(int absoluteDay, int minuteOfDay, int weekStartDay)
        {
            if (absoluteDay < 1 || minuteOfDay < 0 || minuteOfDay > 1439 || weekStartDay < 0 || weekStartDay > 6) throw new ArgumentOutOfRangeException();
            AbsoluteDay=absoluteDay; MinuteOfDay=minuteOfDay; Weekday=(weekStartDay + absoluteDay - 1) % 7;
        }
        public static int ParseMinute(string value, bool allow24)
        {
            if(value==null||value.Length!=5||value[2]!=':'||!AsciiDigit(value[0])||!AsciiDigit(value[1])||!AsciiDigit(value[3])||!AsciiDigit(value[4]))throw new FormatException("Hora inválida (use HH:mm): "+value);
            int hour, minute;string[] parts=value.Split(':');
            if (!int.TryParse(parts[0], out hour) || !int.TryParse(parts[1], out minute) || minute < 0 || minute > 59 || hour < 0 || hour > (allow24 ? 24 : 23) || (hour == 24 && (!allow24 || minute != 0))) throw new FormatException("Hora inválida: " + value);
            return hour*60+minute;
        }
        private static bool AsciiDigit(char value){return value>='0'&&value<='9';}
    }

    public sealed class WorldContextSnapshot
    {
        public readonly string Weather;
        public readonly IDictionary<string, bool> Flags;
        public WorldContextSnapshot(string weather, IDictionary<string, bool> flags) { Weather=weather ?? "clear"; Flags=new ReadOnlyDictionary<string,bool>(new Dictionary<string,bool>(flags ?? new Dictionary<string,bool>())); }
    }

    public sealed class ResolvedScheduleData
    {
        public readonly IDictionary<string, ActivityDefinition> Activities;
        public readonly IDictionary<string, IList<ScheduleEntry>> NpcSchedules;
        public readonly IList<ScheduleOverride> Overrides;
        public ResolvedScheduleData(IDictionary<string, ActivityDefinition> activities, IDictionary<string, IList<ScheduleEntry>> schedules, IList<ScheduleOverride> overrides)
        {
            Activities=new ReadOnlyDictionary<string,ActivityDefinition>(new Dictionary<string,ActivityDefinition>(activities));
            var copied=new Dictionary<string,IList<ScheduleEntry>>(StringComparer.Ordinal);
            foreach(var pair in schedules) copied.Add(pair.Key,Array.AsReadOnly(pair.Value.ToArray()));
            NpcSchedules=new ReadOnlyDictionary<string,IList<ScheduleEntry>>(copied);
            Overrides=Array.AsReadOnly(overrides.ToArray());
        }
    }

    public sealed class ActivityIntent
    {
        public readonly string NpcId, ActivityId, TargetId, SourceId, EntryId;
        public readonly int AbsoluteDay, ValidFrom, ValidUntil, Priority;
        public readonly ScheduleOrigin Origin;
        public ActivityIntent(string npc, string activity, string target, int day, int from, int until, int priority, ScheduleOrigin origin, string source, string entry) { NpcId=npc; ActivityId=activity; TargetId=target; AbsoluteDay=day; ValidFrom=from; ValidUntil=until; Priority=priority; Origin=origin; SourceId=source; EntryId=entry; }
        public string Identity { get { return ActivityId+"|"+TargetId+"|"+Origin+"|"+SourceId+"|"+EntryId+"|"+Priority; } }
    }

    public sealed class ResolutionTrace
    {
        public readonly string Reason;
        public readonly IList<string> Diagnostics;
        public ResolutionTrace(string reason, IList<string> diagnostics) { Reason=reason; Diagnostics=diagnostics; }
    }
    public sealed class ResolutionResult
    {
        public readonly ActivityIntent Intent;
        public readonly ResolutionTrace Trace;
        public ResolutionResult(ActivityIntent intent, ResolutionTrace trace) { Intent=intent; Trace=trace; }
    }
}
