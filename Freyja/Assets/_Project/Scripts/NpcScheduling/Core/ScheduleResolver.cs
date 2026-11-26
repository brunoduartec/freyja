using System;
using System.Collections.Generic;
using System.Linq;

namespace Freyja.NpcScheduling
{
    public sealed class ScheduleResolver
    {
        private readonly ResolvedScheduleData data;
        public ScheduleResolver(ResolvedScheduleData data) { this.data=data ?? throw new ArgumentNullException("data"); }
        public ResolutionResult Resolve(string npcId, GameTimeSnapshot time, WorldContextSnapshot world)
        {
            IList<ScheduleEntry> schedule;
            if (!data.NpcSchedules.TryGetValue(npcId, out schedule)) throw new ArgumentException("NPC desconhecido: "+npcId);
            var candidates = data.Overrides.Where(o=>o.NpcId==npcId && o.Conditions.Matches(time,world)).SelectMany(o=>o.Entries.Where(e=>e.Contains(time.MinuteOfDay)).Select(e=>new { Rule=o, Entry=e })).OrderByDescending(x=>x.Rule.Priority).ThenBy(x=>x.Rule.Id, StringComparer.Ordinal).ToList();
            ScheduleEntry selected; int priority; ScheduleOrigin origin; string source; var diagnostics=new List<string>();
            if (candidates.Count>0)
            {
                var best=candidates[0]; selected=best.Entry; priority=best.Rule.Priority; origin=ScheduleOrigin.Override; source=best.Rule.Id;
                if (candidates.Count>1 && candidates[1].Rule.Priority==priority) diagnostics.Add("Empate de overrides resolvido por ID ordinal: "+best.Rule.Id);
            }
            else
            {
                selected=schedule.FirstOrDefault(e=>e.Contains(time.MinuteOfDay)); priority=30; origin=ScheduleOrigin.Base; source="base";
            }
            if (selected==null)
            {
                int next=schedule.Where(e=>e.Start>time.MinuteOfDay).Select(e=>e.Start).DefaultIfEmpty(1440).Min();
                selected=new ScheduleEntry("idle_gap",time.MinuteOfDay,Math.Max(time.MinuteOfDay+1,next),"idle",null); priority=10; origin=ScheduleOrigin.Fallback; source="fallback";
                diagnostics.Add("Lacuna permitida; usando idle até a próxima fronteira da agenda-base.");
            }
            int until=selected.End;
            foreach (var rule in data.Overrides.Where(o=>o.NpcId==npcId && o.Entries.Any(e=>e.Start>time.MinuteOfDay && e.Start<until)))
                until=Math.Min(until,rule.Entries.Where(e=>e.Start>time.MinuteOfDay).Select(e=>e.Start).DefaultIfEmpty(until).Min());
            var intent=new ActivityIntent(npcId,selected.ActivityId,selected.TargetId,time.AbsoluteDay,time.MinuteOfDay,until,priority,origin,source,selected.Id);
            return new ResolutionResult(intent,new ResolutionTrace(origin==ScheduleOrigin.Fallback?"lacuna":"regra aplicável de maior prioridade",diagnostics));
        }
    }
}
