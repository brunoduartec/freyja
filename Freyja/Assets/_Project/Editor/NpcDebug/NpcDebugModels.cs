using System;
using System.Collections.Generic;

namespace Freyja.NpcDebug.Editor
{
    public enum NpcDebugDataStatus { available, unknown, unavailable, not_applicable, stale }
    public enum NpcDebugStateFilter { all, active, failed, fallback, reserving, occupied }

    [Serializable]
    public sealed class NpcDebugFilter
    {
        public string search = "";
        public NpcDebugStateFilter state = NpcDebugStateFilter.all;
        public int limit = 100;
    }

    [Serializable]
    public sealed class NpcDebugEventFilter
    {
        public string npcId = "";
        public string kind = "";
        public int limit = 100;
    }

    [Serializable]
    public sealed class NpcDebugSmartObjectFilter
    {
        public string objectId = "";
        public string state = "";
        public int limit = 100;
    }

    [Serializable]
    public sealed class NpcDebugSnapshot
    {
        public int schemaVersion = 1;
        public string sessionId;
        public string scenePath;
        public string mode;
        public string capturedAtUtc;
        public string clockStatus;
        public int absoluteDay;
        public int minuteOfDay;
        public string calendarTimeStatus;
        public float hour;
        public string hourStatus;
        public bool advancesAutomatically;
        public string advancesAutomaticallyStatus;
        public float clockRate;
        public string clockRateStatus;
        public int registeredNpcCount;
        public int configuredNpcCount;
        public int inactiveNpcCount;
        public string hostValidationStatus;
        public int activeNpcCount;
        public int readyNpcCount;
        public int failedNpcCount;
        public int slotAvailableCount;
        public int slotReservedCount;
        public int slotOccupiedCount;
        public int slotDisabledCount;
        public int returnedNpcCount;
        public int limit;
        public bool truncated;
        public int droppedEventCount;
        public NpcDebugFilter filter;
        public List<NpcDebugNpcSnapshot> npcs = new List<NpcDebugNpcSnapshot>();
        public List<NpcDebugSmartObjectSnapshot> smartObjects = new List<NpcDebugSmartObjectSnapshot>();
    }

    [Serializable]
    public sealed class NpcDebugNpcSnapshot
    {
        public string npcId;
        public string displayName;
        public string status;
        public string initializationError;
        public string intentStatus;
        public string intendedActivity;
        public string intendedTarget;
        public string intentSource;
        public string intentEntry;
        public int intentPriority;
        public int intentAbsoluteDay;
        public int intentValidFrom;
        public int intentValidUntil;
        public string intentIntervalStatus;
        public string resolutionReason;
        public string executionStatus;
        public string executionState;
        public string executionId;
        public string executedActivity;
        public string operation;
        public string effectiveTarget;
        public string reason;
        public bool fallback;
        public bool residualOwnership;
        public string navigationStatus;
        public string navigationReason;
        public string pathStatus;
        public NpcDebugVector3 requestedPosition;
        public NpcDebugVector3 effectivePosition;
        public float distanceToEffectivePosition;
        public string distanceStatus;
        public NpcDebugVector3 velocity;
        public string velocityStatus;
        public List<NpcDebugVector3> pathCorners = new List<NpcDebugVector3>();
        public string animationStatus;
        public string animationState;
        public int animationStateHash;
        public string animationStateHashStatus;
        public string lastCompletedStatus;
        public string lastCompletedEvent;
        public string lastCompletedExecution;
        public int lastCompletedDay;
        public int lastCompletedMinute;
        public string lastCompletedTimeStatus;
    }

    [Serializable]
    public sealed class NpcDebugSlotSnapshot
    {
        public string slotId;
        public string state;
        public string activity;
        public string ownerNpc;
        public string ownerObject;
        public NpcDebugVector3 approachPosition;
        public string approachStatus;
        public NpcDebugVector3 posePosition;
        public string poseStatus;
    }

    [Serializable]
    public sealed class NpcDebugSmartObjectSnapshot
    {
        public string objectId;
        public string regionId;
        public string displayName;
        public string status;
        public string regionBoundsStatus;
        public NpcDebugVector3 position = new NpcDebugVector3();
        public List<NpcDebugSlotSnapshot> slots = new List<NpcDebugSlotSnapshot>();
        public bool duplicateId;
    }

    [Serializable]
    public sealed class NpcDebugVector3
    {
        public float x;
        public float y;
        public float z;
        public string status;
    }

    [Serializable]
    public sealed class NpcDebugEvent
    {
        public long captureOrder;
        public long sourceSequence;
        public string sourceSequenceStatus;
        public string kind;
        public string npcId;
        public string executionId;
        public string intendedActivity;
        public string executedActivity;
        public string target;
        public string step;
        public string reason;
        public int day;
        public int minute;
        public double operationalSeconds;
    }

    [Serializable]
    public sealed class NpcDebugEventList
    {
        public int schemaVersion = 1;
        public int droppedCount;
        public int limit;
        public bool truncated;
        public NpcDebugEventFilter filter;
        public List<NpcDebugEvent> events = new List<NpcDebugEvent>();
    }

    [Serializable]
    public sealed class NpcDebugJsonEnvelope
    {
        public int schemaVersion = 1;
        public string sessionId;
        public string scenePath;
        public string capturedAtUtc;
        public string queryId;
        public string queryStatus;
        public NpcDebugSnapshot snapshot;
        public NpcDebugNpcSnapshot npc;
        public List<NpcDebugSmartObjectSnapshot> smartObjects;
        public NpcDebugEventList history;
    }

    [Serializable]
    public sealed class NpcDebugCommandResult
    {
        public string command;
        public string status;
        public string npcId;
        public string reason;
        public string result;
    }
}
