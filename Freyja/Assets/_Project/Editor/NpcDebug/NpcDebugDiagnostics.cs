using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Freyja.Environment;
using Freyja.NpcScheduling;
using Freyja.NpcScheduling.Execution;
using Freyja.NpcScheduling.Unity;
using Freyja.Prototype;
using Freyja.SmartObjects;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace Freyja.NpcDebug.Editor
{
    public static class NpcDebugDiagnostics
    {
        private static string sessionId = Guid.NewGuid().ToString("N");
        private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;
        private static CoastalDayNight leasedClock;
        private static bool originalAdvance;
        private static bool appliedAdvance;
        private static float originalRate;
        private static float appliedRate;
        private static bool hasAdvanceLease;
        private static bool hasRateLease;
        private static NpcWorldRuntimeDriver[] cachedDrivers = Array.Empty<NpcWorldRuntimeDriver>();
        private static CoastalDayNight[] cachedClocks = Array.Empty<CoastalDayNight>();
        private static bool sourcesDirty = true;

        [InitializeOnLoadMethod]
        private static void RegisterLifecycle()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            EditorApplication.hierarchyChanged -= InvalidateSources;
            EditorApplication.hierarchyChanged += InvalidateSources;
            SceneManager.activeSceneChanged -= OnSceneChanged;
            SceneManager.activeSceneChanged += OnSceneChanged;
            AssemblyReloadEvents.beforeAssemblyReload -= RestoreSessionOverrides;
            AssemblyReloadEvents.beforeAssemblyReload += RestoreSessionOverrides;
        }

        public static NpcDebugSnapshot CaptureSnapshot(NpcDebugFilter filter = null)
        {
            filter = NormalizeFilter(filter);
            Scene scene = SceneManager.GetActiveScene();
            var snapshot = new NpcDebugSnapshot
            {
                sessionId = sessionId,
                scenePath = RelativeScenePath(scene.path),
                mode = EditorApplication.isPlaying ? "play" : "edit",
                hostValidationStatus = "unavailable",
                capturedAtUtc = DateTime.UtcNow.ToString("O", Invariant),
                clockStatus = "unavailable",
                calendarTimeStatus = "unavailable",
                hourStatus = "unavailable",
                advancesAutomaticallyStatus = "unavailable",
                clockRateStatus = "unavailable",
                filter = CopyFilter(filter),
                limit = filter.limit,
                droppedEventCount = NpcDebugEventBuffer.DroppedCount
            };

            var hosts = new List<NpcActivityController>();
            GameTimeSnapshot worldTime = null;
            NpcWorldRuntimeDriver[] drivers = GetDrivers();
            var clocks = new List<CoastalDayNight>();
            foreach (NpcWorldRuntimeDriver driver in drivers)
            {
                if (driver == null || !driver.isActiveAndEnabled) continue;
                snapshot.configuredNpcCount += driver.EditorConfiguredHostCount;
                snapshot.inactiveNpcCount += driver.EditorInactiveHostCount;
                snapshot.failedNpcCount += driver.EditorRejectedHostCount;
                if (!driver.EditorHostsValidated) snapshot.hostValidationStatus = "pending";
                else if (snapshot.hostValidationStatus != "pending") snapshot.hostValidationStatus = "available";
                hosts.AddRange(driver.EditorGetHosts());
                if (worldTime == null) worldTime = driver.CurrentGameTime;
                CoastalDayNight clock = driver.EditorClock;
                if (clock != null && !clocks.Contains(clock)) clocks.Add(clock);
            }
            if (clocks.Count == 0) clocks.AddRange(GetClocks());
            if (EditorApplication.isPlaying && clocks.Count == 1)
            {
                snapshot.clockStatus = "available";
                snapshot.hour = clocks[0].Hour;
                snapshot.hourStatus = IsFinite(snapshot.hour) ? "available" : "unavailable";
                snapshot.advancesAutomatically = clocks[0].AdvancesAutomatically;
                snapshot.advancesAutomaticallyStatus = "available";
                snapshot.clockRate = clocks[0].EditorCalendarRate;
                snapshot.clockRateStatus = "available";
            }
            if (EditorApplication.isPlaying && worldTime != null)
            {
                snapshot.absoluteDay = worldTime.AbsoluteDay;
                snapshot.minuteOfDay = worldTime.MinuteOfDay;
                snapshot.calendarTimeStatus = "available";
            }

            hosts.RemoveAll(host => host == null);
            hosts.Sort((a, b) => string.Compare(a.NpcId, b.NpcId, StringComparison.Ordinal));
            snapshot.registeredNpcCount = hosts.Count;
            var hostCounts = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (NpcActivityController host in hosts)
                if (!string.IsNullOrWhiteSpace(host.NpcId))
                {
                    hostCounts.TryGetValue(host.NpcId, out int count);
                    hostCounts[host.NpcId] = count + 1;
                }
            NpcDebugSmartObjectSnapshot[] smartObjects = CaptureSmartObjects();
            snapshot.smartObjects.AddRange(smartObjects);
            foreach (NpcActivityController host in hosts)
            {
                if (host.isActiveAndEnabled) snapshot.activeNpcCount++;
                if (host.IsReady) snapshot.readyNpcCount++;
                if (!string.IsNullOrEmpty(host.InitializationError)) snapshot.failedNpcCount++;
            }
            foreach (NpcDebugSmartObjectSnapshot smartObject in smartObjects)
                foreach (NpcDebugSlotSnapshot slot in smartObject.slots)
                {
                    if (slot.state == InteractionSlotState.Available.ToString()) snapshot.slotAvailableCount++;
                    else if (slot.state == InteractionSlotState.Reserved.ToString()) snapshot.slotReservedCount++;
                    else if (slot.state == InteractionSlotState.Occupied.ToString()) snapshot.slotOccupiedCount++;
                    else if (slot.state == InteractionSlotState.Disabled.ToString()) snapshot.slotDisabledCount++;
                }
            var capturedIds = new HashSet<string>(StringComparer.Ordinal);
            int validCount = 0;
            foreach (NpcActivityController host in hosts)
            {
                if (host == null || string.IsNullOrWhiteSpace(host.NpcId) || !capturedIds.Add(host.NpcId)) continue;
                NpcDebugNpcSnapshot npc = hostCounts[host.NpcId] > 1 ? AmbiguousHost(host.NpcId) : CaptureHost(host);
                if (!Matches(npc, filter, smartObjects)) continue;
                validCount++;
                if (snapshot.npcs.Count < filter.limit) snapshot.npcs.Add(npc);
            }
            snapshot.returnedNpcCount = snapshot.npcs.Count;
            snapshot.truncated = validCount > snapshot.npcs.Count;
            return snapshot;
        }

        public static NpcDebugNpcSnapshot CaptureNpc(string npcId)
        {
            if (string.IsNullOrWhiteSpace(npcId)) return null;
            NpcWorldRuntimeDriver[] drivers = GetDrivers();
            NpcActivityController match = null;
            foreach (NpcWorldRuntimeDriver driver in drivers)
            {
                if (driver == null || !driver.isActiveAndEnabled) continue;
                foreach (NpcActivityController host in driver.EditorGetHosts())
                    if (host != null && string.Equals(host.NpcId, npcId, StringComparison.Ordinal))
                    {
                        if (match != null) return null;
                        match = host;
                    }
            }
            return match != null ? CaptureHost(match) : null;
        }

        public static NpcDebugSmartObjectSnapshot[] CaptureSmartObjects(NpcDebugSmartObjectFilter filter = null)
        {
            SmartObject[] objects = SmartObjectRegistry.EditorGetRegisteredObjects();
            var counts = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (SmartObject item in objects)
                if (item != null) { counts.TryGetValue(item.ObjectId ?? "", out int count); counts[item.ObjectId ?? ""] = count + 1; }
            var result = new List<NpcDebugSmartObjectSnapshot>();
            foreach (SmartObject item in objects)
            {
                if (item == null) continue;
                counts.TryGetValue(item.ObjectId ?? "", out int count);
                var value = new NpcDebugSmartObjectSnapshot
                {
                    objectId = item.ObjectId,
                    regionId = item.RegionId,
                    displayName = item.name,
                    status = item.IsAvailable ? "available" : "unavailable",
                    regionBoundsStatus = "unavailable_no_bounds_contract",
                    duplicateId = count > 1,
                    position = ToVector(item.transform.position)
                };
                foreach (InteractionSlot slot in item.Slots)
                {
                    if (slot == null) continue;
                    value.slots.Add(new NpcDebugSlotSnapshot
                    {
                        slotId = slot.SlotId,
                        state = slot.State.ToString(),
                        activity = slot.ReservationOwner != null ? "reserved_activity_unknown" : JoinActivities(slot.ActivityIds),
                        ownerNpc = slot.ReservationOwner != null ? FindNpcIdForOwner(slot.ReservationOwner) : "",
                        ownerObject = slot.ReservationOwner != null ? "unknown" : "",
                        approachPosition = slot.Approach != null ? ToVector(slot.Approach.position) : null,
                        approachStatus = slot.Approach != null ? "available" : "unavailable",
                        posePosition = slot.Pose != null ? ToVector(slot.Pose.position) : null,
                        poseStatus = slot.Pose != null ? "available" : "unavailable"
                    });
                }
                value.slots.Sort((a, b) => string.Compare(a.slotId, b.slotId, StringComparison.Ordinal));
                result.Add(value);
            }
            result.Sort((a, b) => { int id = string.Compare(a.objectId, b.objectId, StringComparison.Ordinal); return id != 0 ? id : string.Compare(a.displayName, b.displayName, StringComparison.Ordinal); });
            if (filter == null) return result.ToArray();
            string objectId = filter.objectId ?? "";
            string state = filter.state ?? "";
            int limit = Mathf.Clamp(filter.limit, 0, 512);
            if (limit == 0) return Array.Empty<NpcDebugSmartObjectSnapshot>();
            var filtered = new List<NpcDebugSmartObjectSnapshot>();
            foreach (NpcDebugSmartObjectSnapshot item in result)
            {
                if (!string.IsNullOrEmpty(objectId) && !string.Equals(item.objectId, objectId, StringComparison.Ordinal)) continue;
                var slots = new List<NpcDebugSlotSnapshot>();
                foreach (NpcDebugSlotSnapshot slot in item.slots)
                    if (string.IsNullOrEmpty(state) || string.Equals(slot.state, state, StringComparison.Ordinal)) slots.Add(slot);
                if (!string.IsNullOrEmpty(state) && slots.Count == 0) continue;
                item.slots = slots;
                filtered.Add(item);
                if (filtered.Count >= limit) break;
            }
            return filtered.ToArray();
        }

        public static NpcDebugEvent[] GetRecentEvents(NpcDebugEventFilter filter = null) => NpcDebugEventBuffer.GetRecent(filter);

        public static NpcDebugEventList CaptureRecentEvents(NpcDebugEventFilter filter = null)
        {
            filter = filter ?? new NpcDebugEventFilter();
            int count = NpcDebugEventBuffer.CountFor(filter);
            NpcDebugEvent[] events = GetRecentEvents(filter);
            return new NpcDebugEventList
            {
                droppedCount = NpcDebugEventBuffer.DroppedCount,
                limit = Mathf.Clamp(filter.limit, 0, NpcDebugEventBuffer.Capacity),
                truncated = count > events.Length,
                filter = new NpcDebugEventFilter { npcId = filter.npcId ?? "", kind = filter.kind ?? "", limit = Mathf.Clamp(filter.limit, 0, NpcDebugEventBuffer.Capacity) },
                events = new List<NpcDebugEvent>(events)
            };
        }

        public static void ClearHistory() => NpcDebugEventBuffer.ClearHistory();

        public static string CaptureJson(NpcDebugFilter filter = null, bool includeEvents = true)
        {
            NpcDebugSnapshot snapshot = CaptureSnapshot(filter);
            return JsonConvert.SerializeObject(Envelope(snapshot, null, includeEvents, null, "available"), JsonSettings());
        }

        public static string CaptureNpcJson(string npcId, bool includeEvents = true)
        {
            NpcDebugNpcSnapshot npc = CaptureNpc(npcId);
            var filter = new NpcDebugFilter { search = npcId ?? "", limit = 1 };
            NpcDebugSnapshot snapshot = CaptureSnapshot(filter);
            snapshot.npcs.Clear();
            if (npc != null) snapshot.npcs.Add(npc);
            snapshot.returnedNpcCount = snapshot.npcs.Count;
            snapshot.truncated = false;
            string status = npc == null ? "not_found_or_ambiguous" : "available";
            return JsonConvert.SerializeObject(Envelope(snapshot, npc, includeEvents && npc != null, npcId, status), JsonSettings());
        }

        public static NpcDebugCommandResult RequestReevaluation(string npcId)
        {
            NpcActivityController host = FindHost(npcId);
            if (!EditorApplication.isPlaying) return Result("reevaluate", "rejected", npcId, "play_mode_required", null);
            if (host == null) return Result("reevaluate", "rejected", npcId, "npc_not_registered", null);
            bool accepted = host.EditorRequestReevaluation();
            return Result("reevaluate", accepted ? "accepted" : "rejected", npcId,
                accepted ? "queued_for_next_driver_tick" : "host_not_ready_or_inactive", null);
        }

        public static NpcDebugCommandResult CancelActivity(string npcId)
        {
            NpcActivityController host = FindHost(npcId);
            if (!EditorApplication.isPlaying) return Result("cancel", "rejected", npcId, "play_mode_required", null);
            if (host == null) return Result("cancel", "rejected", npcId, "npc_not_registered", null);
            ActivitySnapshot before = host.CurrentActivitySnapshot;
            if (before == null || before.State == ActivityState.Completed || before.State == ActivityState.Cancelled ||
                before.State == ActivityState.Failed || before.State == ActivityState.Pending)
                return Result("cancel", "rejected", npcId, "no_active_execution", before != null ? before.ExecutionId : null);
            ActivitySnapshot state = host.CancelExternal("debug_cancel");
            if (state == null) return Result("cancel", "rejected", npcId, "host_has_no_world_snapshot", null);
            string status = state.HasResidualOwnership ? "pending" : "accepted";
            return Result("cancel", status, npcId, state.HasResidualOwnership ? "cleanup_owned_by_activity_executor" : "cancel_forwarded_to_activity_coordinator", state.ExecutionId);
        }

        public static NpcDebugCommandResult RequestTemporaryOverride(string npcId, string activityId)
        {
            return Result("temporary_override", "unsupported", npcId, OverrideUnavailableReason, activityId);
        }

        public static NpcDebugCommandResult SetClockPaused(bool paused)
        {
            if (!EditorApplication.isPlaying) return Result("clock_pause", "rejected", null, "play_mode_required", null);
            CoastalDayNight clock = FindClock();
            if (clock == null) return Result("clock_pause", "rejected", null, "clock_unavailable_or_ambiguous", null);
            if (!hasAdvanceLease || leasedClock != clock)
            {
                leasedClock = clock;
                originalAdvance = clock.AdvancesAutomatically;
                hasAdvanceLease = true;
            }
            clock.AdvancesAutomatically = !paused;
            appliedAdvance = clock.AdvancesAutomatically;
            return Result("clock_pause", "accepted", null, paused ? "calendar_paused" : "calendar_resumed", null);
        }

        public static NpcDebugCommandResult SetClockRate(float rate)
        {
            if (!EditorApplication.isPlaying) return Result("clock_rate", "rejected", null, "play_mode_required", null);
            if (rate != 1f && rate != 5f && rate != 10f) return Result("clock_rate", "rejected", null, "rate_must_be_1_5_or_10", null);
            CoastalDayNight clock = FindClock();
            if (clock == null) return Result("clock_rate", "rejected", null, "clock_unavailable_or_ambiguous", null);
            if (!hasRateLease || leasedClock != clock)
            {
                leasedClock = clock;
                originalRate = clock.EditorCalendarRate;
                hasRateLease = true;
            }
            clock.EditorSetCalendarRate(rate);
            appliedRate = clock.EditorCalendarRate;
            return Result("clock_rate", "accepted", null, "calendar_rate_applied", appliedRate.ToString("0", Invariant) + "x");
        }

        public static NpcDebugCommandResult AdvanceClockMinutes(int minutes)
        {
            if (!EditorApplication.isPlaying) return Result("clock_advance", "rejected", null, "play_mode_required", null);
            CoastalDayNight clock = FindClock();
            if (clock == null) return Result("clock_advance", "rejected", null, "clock_unavailable_or_ambiguous", null);
            if (!TryCalculateClockAdvance(clock.Hour, minutes, out float requestedHour))
                return Result("clock_advance", "rejected", null, minutes < 1 || minutes > 1439 ? "minutes_must_be_1_to_1439" : "clock_projection_allows_midnight_wrap_only_from_18_to_06", null);
            clock.SetHour(requestedHour);
            return Result("clock_advance", "accepted", null, "positive_calendar_jump_requested", clock.Hour.ToString("0.00", Invariant));
        }

        public static bool CanAdvanceClockMinutes(float currentHour, int minutes) => TryCalculateClockAdvance(currentHour, minutes, out _);

        private static bool TryCalculateClockAdvance(float currentHour, int minutes, out float requestedHour)
        {
            requestedHour = currentHour;
            if (!IsFinite(currentHour) || currentHour < 0f || currentHour >= 24f || minutes < 1 || minutes > 1439) return false;
            requestedHour = currentHour + minutes / 60f;
            if (requestedHour >= 24f)
            {
                requestedHour -= 24f;
                return currentHour >= 18f && requestedHour <= 6f;
            }
            return requestedHour >= currentHour;
        }

        public static void RestoreSessionOverrides()
        {
            if (leasedClock != null)
            {
                if (hasAdvanceLease && leasedClock.AdvancesAutomatically == appliedAdvance)
                    leasedClock.AdvancesAutomatically = originalAdvance;
                if (hasRateLease && Mathf.Approximately(leasedClock.EditorCalendarRate, appliedRate))
                    leasedClock.EditorSetCalendarRate(originalRate);
            }
            leasedClock = null;
            hasAdvanceLease = false;
            hasRateLease = false;
        }

        public static NpcDebugCommandResult ExportJson(string npcIdOrNull, bool includeEvents = true)
        {
            string relative = "Temp/NpcDebugTools/" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff", Invariant) +
                (string.IsNullOrEmpty(npcIdOrNull) ? "-all.json" : "-" + SafeFileName(npcIdOrNull) + ".json");
            string json = string.IsNullOrEmpty(npcIdOrNull) ? CaptureJson(null, includeEvents) : CaptureNpcJson(npcIdOrNull, includeEvents);
            string repoRoot = Directory.GetParent(Directory.GetParent(Application.dataPath).FullName).FullName;
            string fullPath = Path.GetFullPath(Path.Combine(repoRoot, relative));
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                File.WriteAllText(fullPath, json, new UTF8Encoding(false));
                return Result("export", "completed", npcIdOrNull, "file_written", relative.Replace('\\', '/'));
            }
            catch (Exception exception)
            {
                return Result("export", "failed", npcIdOrNull, exception.GetType().Name, null);
            }
        }

        internal static bool IsSupportedOverride => false;
        internal static string OverrideUnavailableReason => "no_temporary_override_contract";

        private static NpcDebugNpcSnapshot CaptureHost(NpcActivityController host)
        {
            ActivityIntent intent = host.CurrentIntent;
            ActivitySnapshot execution = host.CurrentActivitySnapshot;
            var value = new NpcDebugNpcSnapshot
            {
                npcId = host.NpcId,
                displayName = host.gameObject.name,
                status = host.isActiveAndEnabled ? (host.IsReady ? "ready" : "initializing") : "inactive",
                initializationError = host.InitializationError ?? "",
                intentStatus = intent != null ? "available" : "unavailable",
                intendedActivity = intent != null ? intent.ActivityId : "",
                intendedTarget = intent != null ? intent.TargetId : "",
                intentSource = intent != null ? intent.SourceId : "",
                intentEntry = intent != null ? intent.EntryId : "",
                intentPriority = intent != null ? intent.Priority : 0,
                intentAbsoluteDay = intent != null ? intent.AbsoluteDay : 0,
                intentValidFrom = intent != null ? intent.ValidFrom : 0,
                intentValidUntil = intent != null ? intent.ValidUntil : 0,
                intentIntervalStatus = intent != null ? "available" : "unavailable",
                resolutionReason = host.EditorLastResolutionTrace != null ? host.EditorLastResolutionTrace.Reason : "unavailable",
                executionStatus = execution != null ? "available" : "unavailable",
                executionState = execution != null ? execution.State.ToString() : "",
                executionId = execution != null ? execution.ExecutionId : "",
                executedActivity = execution != null ? execution.ExecutedActivityId : "",
                operation = execution != null ? execution.CurrentOperation : "",
                effectiveTarget = execution != null ? execution.EffectiveTargetId : "",
                reason = execution != null ? execution.ReasonCode : "",
                fallback = execution != null && execution.IsFallback,
                residualOwnership = execution != null && execution.HasResidualOwnership
            };

            IslandWaypointWalker walker = host.GetComponent<IslandWaypointWalker>();
            if (walker != null)
            {
                value.navigationStatus = walker.MoveStatus.ToString();
                value.navigationReason = walker.LastReason ?? "";
                value.pathStatus = walker.PathStatus.ToString();
                bool hasRequestedDestination = walker.MoveStatus != WaypointMoveStatus.Unknown && walker.MoveStatus != WaypointMoveStatus.Cancelled;
                value.requestedPosition = hasRequestedDestination ? ToVector(walker.RequestedPosition) : null;
                bool hasEffectiveDestination = walker.PathStatus != UnityEngine.AI.NavMeshPathStatus.PathInvalid && hasRequestedDestination;
                value.effectivePosition = hasEffectiveDestination ? ToVector(walker.EffectivePosition) : null;
                value.distanceStatus = "unavailable_no_current_navmesh_distance";
                NavMeshAgent agent = host.GetComponent<NavMeshAgent>();
                if (agent != null && agent.enabled && agent.isOnNavMesh && agent.hasPath)
                {
                    value.distanceToEffectivePosition = agent.remainingDistance;
                    value.distanceStatus = IsFinite(value.distanceToEffectivePosition) ? "available" : "unavailable_non_finite";
                }
                value.velocity = ToVector(walker.Velocity);
                value.velocityStatus = "available";
                Vector3[] corners = walker.PathCorners ?? Array.Empty<Vector3>();
                foreach (Vector3 corner in corners) value.pathCorners.Add(ToVector(corner));
            }
            else
            {
                value.navigationStatus = "unavailable";
                value.navigationReason = "walker_not_found";
                value.pathStatus = "unavailable";
                value.distanceStatus = "unavailable";
                value.velocityStatus = "unavailable";
            }

            Animator animator = host.GetComponentInChildren<Animator>();
            if (animator != null && animator.isActiveAndEnabled && animator.layerCount > 0)
            {
                AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
                AnimatorClipInfo[] clips = animator.GetCurrentAnimatorClipInfo(0);
                value.animationStatus = "available";
                value.animationStateHash = state.fullPathHash;
                value.animationStateHashStatus = "available";
                value.animationState = clips.Length > 0 && clips[0].clip != null ? clips[0].clip.name : "clip_unavailable";
            }
            else { value.animationStatus = "unavailable"; value.animationState = "animator_not_found"; value.animationStateHashStatus = "unavailable"; }

            NpcDebugEvent completed = NpcDebugEventBuffer.FindLastCompleted(host.NpcId);
            if (completed != null)
            {
                value.lastCompletedStatus = "available";
                value.lastCompletedEvent = completed.kind;
                value.lastCompletedExecution = completed.executionId;
                value.lastCompletedDay = completed.day;
                value.lastCompletedMinute = completed.minute;
                value.lastCompletedTimeStatus = "available";
            }
            else { value.lastCompletedStatus = "unavailable_or_not_observed"; value.lastCompletedTimeStatus = "unavailable"; }
            return value;
        }

        private static NpcDebugNpcSnapshot AmbiguousHost(string npcId) => new NpcDebugNpcSnapshot
        {
            npcId = npcId,
            displayName = "ambiguous duplicate ID",
            status = "ambiguous_duplicate_id",
            initializationError = "duplicate_npc_id; runtime fields are not selected arbitrarily",
            intentStatus = "unavailable",
            executionStatus = "unavailable",
            navigationStatus = "unavailable",
            navigationReason = "ambiguous_duplicate_id",
            pathStatus = "unavailable",
            animationStatus = "unavailable",
            animationState = "ambiguous_duplicate_id",
            lastCompletedStatus = "unavailable"
        };

        private static NpcDebugJsonEnvelope Envelope(NpcDebugSnapshot snapshot, NpcDebugNpcSnapshot npc, bool includeEvents, string queryId, string queryStatus)
        {
            Scene scene = SceneManager.GetActiveScene();
            var envelope = new NpcDebugJsonEnvelope
            {
                sessionId = sessionId,
                scenePath = RelativeScenePath(scene.path),
                capturedAtUtc = DateTime.UtcNow.ToString("O", Invariant),
                queryId = queryId ?? "",
                queryStatus = queryStatus,
                snapshot = snapshot,
                npc = npc,
                smartObjects = snapshot != null ? new List<NpcDebugSmartObjectSnapshot>(snapshot.smartObjects) : new List<NpcDebugSmartObjectSnapshot>(CaptureSmartObjects())
            };
            if (includeEvents)
            {
                envelope.history = CaptureRecentEvents(new NpcDebugEventFilter
                {
                    npcId = npc != null ? npc.npcId : "",
                    kind = "",
                    limit = NpcDebugEventBuffer.Capacity
                });
            }
            return envelope;
        }

        private static NpcActivityController FindHost(string npcId)
        {
            if (string.IsNullOrWhiteSpace(npcId)) return null;
            NpcWorldRuntimeDriver[] drivers = GetDrivers();
            NpcActivityController match = null;
            foreach (NpcWorldRuntimeDriver driver in drivers)
                if (driver != null && driver.isActiveAndEnabled)
                    foreach (NpcActivityController host in driver.EditorGetHosts())
                        if (host != null && string.Equals(host.NpcId, npcId, StringComparison.Ordinal))
                        {
                            if (match != null) return null;
                            match = host;
                        }
            return match;
        }

        private static CoastalDayNight FindClock()
        {
            var clocks = new List<CoastalDayNight>();
            NpcWorldRuntimeDriver[] drivers = GetDrivers();
            foreach (NpcWorldRuntimeDriver driver in drivers)
                if (driver != null && driver.isActiveAndEnabled && driver.EditorClock != null && !clocks.Contains(driver.EditorClock)) clocks.Add(driver.EditorClock);
            if (clocks.Count == 0) clocks.AddRange(GetClocks());
            return clocks.Count == 1 ? clocks[0] : null;
        }

        internal static NpcActivityController[] GetCachedHosts()
        {
            var hosts = new List<NpcActivityController>();
            foreach (NpcWorldRuntimeDriver driver in GetDrivers())
                if (driver != null && driver.isActiveAndEnabled) hosts.AddRange(driver.EditorGetHosts());
            hosts.RemoveAll(host => host == null);
            hosts.Sort((a, b) => string.Compare(a.NpcId, b.NpcId, StringComparison.Ordinal));
            return hosts.ToArray();
        }

        private static NpcWorldRuntimeDriver[] GetDrivers()
        {
            if (!sourcesDirty) return cachedDrivers;
            cachedDrivers = UnityEngine.Object.FindObjectsByType<NpcWorldRuntimeDriver>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            Array.Sort(cachedDrivers, (a, b) => string.Compare(a != null ? a.name : null, b != null ? b.name : null, StringComparison.Ordinal));
            cachedClocks = UnityEngine.Object.FindObjectsByType<CoastalDayNight>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            Array.Sort(cachedClocks, (a, b) => string.Compare(a != null ? a.name : null, b != null ? b.name : null, StringComparison.Ordinal));
            sourcesDirty = false;
            return cachedDrivers;
        }

        private static CoastalDayNight[] GetClocks()
        {
            GetDrivers();
            return cachedClocks;
        }

        private static void InvalidateSources() => sourcesDirty = true;

        private static void OnSceneChanged(Scene previous, Scene next)
        {
            sourcesDirty = true;
            RestoreSessionOverrides();
        }

        private static JsonSerializerSettings JsonSettings()
        {
            var settings = new JsonSerializerSettings { Culture = Invariant, Formatting = Formatting.None };
            settings.Converters.Add(new FiniteNumberConverter());
            return settings;
        }

        private sealed class FiniteNumberConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType) => objectType == typeof(float) || objectType == typeof(double);
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                double number = Convert.ToDouble(value, Invariant);
                if (double.IsNaN(number) || double.IsInfinity(number)) writer.WriteNull();
                else writer.WriteValue(number);
            }
            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) => throw new NotSupportedException();
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            sourcesDirty = true;
            if (state == PlayModeStateChange.EnteredPlayMode) sessionId = Guid.NewGuid().ToString("N");
            if (state == PlayModeStateChange.ExitingPlayMode || state == PlayModeStateChange.EnteredEditMode) RestoreSessionOverrides();
        }

        private static bool Matches(NpcDebugNpcSnapshot npc, NpcDebugFilter filter, NpcDebugSmartObjectSnapshot[] smartObjects)
        {
            string query = filter.search.Trim();
            if (query.Length > 0 && npc.npcId.IndexOf(query, StringComparison.OrdinalIgnoreCase) < 0 &&
                npc.displayName.IndexOf(query, StringComparison.OrdinalIgnoreCase) < 0) return false;
            switch (filter.state)
            {
                case NpcDebugStateFilter.active: return npc.executionState != "" && !IsTerminal(npc.executionState);
                case NpcDebugStateFilter.failed: return npc.executionState == ActivityState.Failed.ToString() || !string.IsNullOrEmpty(npc.initializationError);
                case NpcDebugStateFilter.fallback: return npc.fallback;
                case NpcDebugStateFilter.reserving: return npc.executionState == ActivityState.Preparing.ToString() && npc.operation == "reserve_slot";
                case NpcDebugStateFilter.occupied: return HasOwner(npc.npcId, smartObjects);
                default: return true;
            }
        }

        private static bool HasOwner(string npcId, NpcDebugSmartObjectSnapshot[] objects)
        {
            foreach (NpcDebugSmartObjectSnapshot obj in objects)
                foreach (NpcDebugSlotSnapshot slot in obj.slots)
                    if (slot.state == InteractionSlotState.Occupied.ToString() && slot.ownerNpc == npcId) return true;
            return false;
        }

        private static string FindNpcIdForOwner(SmartObjectReservationOwner owner)
        {
            NpcWorldRuntimeDriver[] drivers = GetDrivers();
            foreach (NpcWorldRuntimeDriver driver in drivers)
                if (driver != null)
                    foreach (NpcActivityController host in driver.EditorGetHosts())
                        if (host != null && host.GetComponent<SmartObjectReservationOwner>() == owner) return host.NpcId;
            return "unknown";
        }

        private static bool IsTerminal(string value) => value == ActivityState.Completed.ToString() || value == ActivityState.Cancelled.ToString() || value == ActivityState.Failed.ToString();
        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        private static NpcDebugFilter NormalizeFilter(NpcDebugFilter filter) => filter == null ? new NpcDebugFilter() : new NpcDebugFilter { search = filter.search ?? "", state = filter.state, limit = Mathf.Clamp(filter.limit, 0, 512) };
        private static NpcDebugFilter CopyFilter(NpcDebugFilter filter) => new NpcDebugFilter { search = filter.search, state = filter.state, limit = filter.limit };
        private static NpcDebugCommandResult Result(string command, string status, string npcId, string reason, string result) => new NpcDebugCommandResult { command = command, status = status, npcId = npcId ?? "", reason = reason, result = result ?? "" };
        private static string RelativeScenePath(string path) => string.IsNullOrEmpty(path) ? "unknown" : path.Replace('\\', '/');
        private static NpcDebugVector3 ToVector(Vector3 v) => new NpcDebugVector3 { x = IsFinite(v.x) ? v.x : float.NaN, y = IsFinite(v.y) ? v.y : float.NaN, z = IsFinite(v.z) ? v.z : float.NaN, status = IsFinite(v.x) && IsFinite(v.y) && IsFinite(v.z) ? "available" : "unavailable_non_finite" };
        private static string JoinActivities(IReadOnlyList<string> activities) => activities == null ? "" : string.Join(",", activities);
        private static string SafeFileName(string value)
        {
            var builder = new StringBuilder(value.Length);
            foreach (char c in value) builder.Append(char.IsLetterOrDigit(c) || c == '-' || c == '_' ? c : '_');
            return builder.ToString();
        }
    }
}
