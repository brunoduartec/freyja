using System;
using System.Collections.Generic;
using System.Linq;
using Freyja.NpcScheduling.Execution;

namespace Freyja.NpcScheduling.Editor
{
    internal sealed class NpcScheduleEditorPreview
    {
        public int Day = 1;
        public int Minute = 8 * 60;
        public int WeekStartDay;
        public string Weather = "clear";
        public readonly Dictionary<string, bool> Flags = new Dictionary<string, bool>(StringComparer.Ordinal);
        public ResolutionResult Result { get; private set; }
        public NpcScheduleValidation Validation { get; private set; }
        public string Error { get; private set; }
        private string lastKey;

        public void Resolve(NpcScheduleValidation validation, int revision, string npcId)
        {
            string flags = string.Join(";", Flags.OrderBy(x => x.Key, StringComparer.Ordinal).Select(x => x.Key + "=" + x.Value));
            string key = revision + "|" + npcId + "|" + Day + "|" + Minute + "|" + WeekStartDay + "|" + Weather + "|" + flags;
            if (key == lastKey) return;
            lastKey = key;
            Result = null;
            Validation = validation;
            Error = Validation.Error;
            if (!Validation.IsValid || string.IsNullOrEmpty(npcId)) return;
            try
            {
                var time = new GameTimeSnapshot(Math.Max(1, Day), Math.Max(0, Math.Min(1439, Minute)), WeekStartDay);
                var world = new WorldContextSnapshot(Weather, Flags);
                Result = new ScheduleResolver(Validation.Data).Resolve(npcId, time, world);
            }
            catch (Exception ex) { Error = ex.Message; }
        }
    }
}
