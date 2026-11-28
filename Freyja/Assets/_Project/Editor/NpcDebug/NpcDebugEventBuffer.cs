using System;
using System.Collections.Generic;
using Freyja.NpcScheduling.Execution;
using Freyja.NpcScheduling.Unity;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace Freyja.NpcDebug.Editor
{
    [InitializeOnLoad]
    internal static class NpcDebugEventBuffer
    {
        internal const int Capacity = 512;
        private static readonly NpcDebugEvent[] Buffer = new NpcDebugEvent[Capacity];
        private static int start;
        private static int count;
        private static int dropped;
        private static long nextSequence;

        static NpcDebugEventBuffer()
        {
            NpcActivityController.EditorActivityEvent -= OnActivityEvent;
            NpcActivityController.EditorActivityEvent += OnActivityEvent;
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            SceneManager.activeSceneChanged -= OnSceneChanged;
            SceneManager.activeSceneChanged += OnSceneChanged;
            AssemblyReloadEvents.beforeAssemblyReload -= Clear;
            AssemblyReloadEvents.beforeAssemblyReload += Clear;
        }

        internal static int DroppedCount => dropped;
        internal static int CountFor(NpcDebugEventFilter filter)
        {
            string npcId = filter == null ? null : filter.npcId;
            string kind = filter == null ? null : filter.kind;
            int matches = 0;
            for (int i = 0; i < count; i++)
            {
                NpcDebugEvent item = Buffer[(start + i) % Capacity];
                if (Matches(item, npcId, kind)) matches++;
            }
            return matches;
        }

        internal static void ClearHistory()
        {
            Array.Clear(Buffer, 0, Buffer.Length);
            start = 0;
            count = 0;
            dropped = 0;
        }

        internal static NpcDebugEvent[] GetRecent(NpcDebugEventFilter filter)
        {
            int limit = ClampLimit(filter == null ? 100 : filter.limit);
            string npcId = filter == null ? null : filter.npcId;
            string kind = filter == null ? null : filter.kind;
            var result = new List<NpcDebugEvent>(Math.Min(limit, count));
            for (int i = count - 1; i >= 0 && result.Count < limit; i--)
            {
                NpcDebugEvent item = Buffer[(start + i) % Capacity];
                if (Matches(item, npcId, kind))
                    result.Add(Clone(item));
            }
            result.Reverse();
            return result.ToArray();
        }

        internal static NpcDebugEvent FindLastCompleted(string npcId)
        {
            for (int i = count - 1; i >= 0; i--)
            {
                NpcDebugEvent item = Buffer[(start + i) % Capacity];
                if (item != null && item.kind == ExecutionEventKind.ActivityCompleted.ToString() &&
                    string.Equals(item.npcId, npcId, StringComparison.Ordinal)) return Clone(item);
            }
            return null;
        }

        private static void OnActivityEvent(ActivityExecutionEvent source)
        {
            if (source == null) return;
            var item = new NpcDebugEvent
            {
                captureOrder = ++nextSequence,
                sourceSequence = source.Sequence,
                sourceSequenceStatus = source.Sequence > 0 ? "available" : "unknown_not_emitted_by_source",
                kind = source.Kind.ToString(),
                npcId = source.NpcId,
                executionId = source.ExecutionId,
                intendedActivity = source.IntendedActivityId,
                executedActivity = source.ExecutedActivityId,
                target = source.TargetId,
                step = source.Step,
                reason = source.ReasonCode,
                day = source.AbsoluteDay,
                minute = source.MinuteOfDay,
                operationalSeconds = source.OperationalSeconds
            };
            if (count == Capacity)
            {
                Buffer[start] = item;
                start = (start + 1) % Capacity;
                dropped++;
            }
            else
            {
                Buffer[(start + count) % Capacity] = item;
                count++;
            }
        }

        private static NpcDebugEvent Clone(NpcDebugEvent source) => new NpcDebugEvent
        {
            captureOrder = source.captureOrder, sourceSequence = source.sourceSequence,
            sourceSequenceStatus = source.sourceSequenceStatus, kind = source.kind, npcId = source.npcId, executionId = source.executionId,
            intendedActivity = source.intendedActivity, executedActivity = source.executedActivity, target = source.target,
            step = source.step, reason = source.reason, day = source.day, minute = source.minute,
            operationalSeconds = source.operationalSeconds
        };

        private static int ClampLimit(int value) => Math.Max(0, Math.Min(Capacity, value));
        private static bool Matches(NpcDebugEvent item, string npcId, string kind) => item != null &&
            (string.IsNullOrEmpty(npcId) || string.Equals(item.npcId, npcId, StringComparison.Ordinal)) &&
            (string.IsNullOrEmpty(kind) || string.Equals(item.kind, kind, StringComparison.Ordinal));

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode || state == PlayModeStateChange.ExitingPlayMode || state == PlayModeStateChange.EnteredEditMode) Clear();
        }

        private static void OnSceneChanged(Scene previous, Scene next) => Clear();

        private static void Clear()
        {
            ClearHistory();
            nextSequence = 0;
        }
    }
}
