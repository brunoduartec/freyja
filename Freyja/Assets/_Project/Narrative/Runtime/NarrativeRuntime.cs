using System;
using System.Collections.Generic;

namespace Freyja.Narrative
{
    public sealed class NarrativeRuntime : IDisposable
    {
        public NarrativeProgress Progress { get; }
        public INarrativeClock Clock { get; }
        private readonly bool persistDemo;
        public NarrativeRuntime(NarrativeProgress progress = null, INarrativeClock clock = null, bool persistDemo = true)
        {
            Progress = progress ?? NarrativeProgress.LoadDemo();
            Clock = clock ?? new FixedNarrativeClock(9);
            if (Progress.clockHour > 0) Clock.Restore(Progress.clockHour);
            this.persistDemo = persistDemo;
        }
        public void Save() { if (persistDemo) { Progress.clockHour = Clock.Hour; Progress.SaveDemo(); } }
        public bool Discover(string clueId) => Progress.Discover(clueId);
        public bool Discover(string clueId, string kind, string source) => Progress.Discover(clueId, kind, source);
        public bool Communicate(string npcId, string clueId) => Progress.Communicate(npcId, clueId);
        public bool Knows(string npcId, string clueId) => Progress.Knows(npcId, clueId);
        public bool Confirm(string storyletId, RepeatPolicy policy) => Progress.Confirm(storyletId, policy, Clock.Hour);
        public void Dispose() { }
    }
}
