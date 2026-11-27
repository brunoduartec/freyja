using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Freyja.Narrative
{
    [Serializable]
    public sealed class NarrativeProgress
    {
        public const int CurrentVersion = 1;
        public int version = CurrentVersion;
        public int clockHour;
        public List<string> discovered = new List<string>();
        public List<NarrativeDiscovery> discoveryDetails = new List<NarrativeDiscovery>();
        public List<NpcKnowledge> npcKnowledge = new List<NpcKnowledge>();
        public List<string> effects = new List<string>();
        public List<StoryletHistory> history = new List<StoryletHistory>();

        public bool Discover(string clueId) => Discover(clueId, "", "");
        public bool Discover(string clueId, string kind, string source)
        {
            bool changed = AddUnique(discovered, clueId);
            if (string.IsNullOrWhiteSpace(clueId) ||
                (string.IsNullOrWhiteSpace(kind) && string.IsNullOrWhiteSpace(source))) return changed;
            foreach (var detail in discoveryDetails)
            {
                if (detail.clueId != clueId) continue;
                if (string.IsNullOrEmpty(detail.kind) && !string.IsNullOrEmpty(kind)) { detail.kind = kind; changed = true; }
                if (string.IsNullOrEmpty(detail.source) && !string.IsNullOrEmpty(source)) { detail.source = source; changed = true; }
                return changed;
            }
            discoveryDetails.Add(new NarrativeDiscovery { clueId = clueId, kind = kind ?? "", source = source ?? "" });
            return true;
        }
        public bool Communicate(string npcId, string clueId)
        {
            if (!discovered.Contains(clueId)) return false;
            foreach (var item in npcKnowledge)
                if (item.npcId == npcId && item.clueId == clueId) return false;
            npcKnowledge.Add(new NpcKnowledge { npcId = npcId, clueId = clueId });
            return true;
        }
        public bool Knows(string npcId, string clueId)
        {
            foreach (var item in npcKnowledge)
                if (item.npcId == npcId && item.clueId == clueId) return true;
            return false;
        }
        public bool ApplyEffectOnce(string effectId) => AddUnique(effects, effectId);
        public bool IsAvailable(string storyletId, RepeatPolicy policy, int now)
        {
            var last = int.MinValue;
            var plays = 0;
            foreach (var item in history)
                if (item.storyletId == storyletId) { plays++; last = item.gameHour; }
            return policy.kind == RepeatKind.Repeatable || plays == 0 ||
                   (policy.kind == RepeatKind.Cooldown && now - last >= policy.hours);
        }
        public bool Confirm(string storyletId, RepeatPolicy policy, int now)
        {
            if (!IsAvailable(storyletId, policy, now)) return false;
            if (policy.kind != RepeatKind.Repeatable)
                history.Add(new StoryletHistory { storyletId = storyletId, gameHour = now });
            return true;
        }
        public string ToJson() => UnityEngine.JsonUtility.ToJson(this);
        public static NarrativeProgress FromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return new NarrativeProgress();
            var value = UnityEngine.JsonUtility.FromJson<NarrativeProgress>(json);
            if (value == null || value.version != CurrentVersion) return new NarrativeProgress();
            value.discovered ??= new List<string>();
            value.discoveryDetails ??= new List<NarrativeDiscovery>();
            value.npcKnowledge ??= new List<NpcKnowledge>();
            value.effects ??= new List<string>();
            value.history ??= new List<StoryletHistory>();
            return value;
        }
        public static NarrativeProgress LoadDemo()
        {
            var path = Path.Combine(Application.persistentDataPath, "demo-narrative-progress.json");
            return File.Exists(path) ? FromJson(File.ReadAllText(path)) : new NarrativeProgress();
        }
        public void SaveDemo()
        {
            var path = Path.Combine(Application.persistentDataPath, "demo-narrative-progress.json");
            File.WriteAllText(path, ToJson());
        }
        private static bool AddUnique(List<string> list, string id)
        {
            if (string.IsNullOrWhiteSpace(id) || list.Contains(id)) return false;
            list.Add(id); return true;
        }
    }
    [Serializable] public sealed class NarrativeDiscovery { public string clueId; public string kind; public string source; }
    [Serializable] public sealed class NpcKnowledge { public string npcId; public string clueId; }
    [Serializable] public sealed class StoryletHistory { public string storyletId; public int gameHour; }
    public enum RepeatKind { Once, Repeatable, Cooldown }
    [Serializable] public struct RepeatPolicy
    {
        public RepeatKind kind; public int hours;
        public static RepeatPolicy Once => new RepeatPolicy { kind = RepeatKind.Once };
        public static RepeatPolicy Repeatable => new RepeatPolicy { kind = RepeatKind.Repeatable };
        public static RepeatPolicy Cooldown(int hours) => new RepeatPolicy { kind = RepeatKind.Cooldown, hours = hours };
    }
    public interface INarrativeClock { int Hour { get; } void Restore(int hour); }
    public sealed class FixedNarrativeClock : INarrativeClock
    {
        public int Hour { get; set; }
        public void Restore(int hour) => Hour = hour;
        public FixedNarrativeClock(int hour) { Hour = hour; }
    }
}
