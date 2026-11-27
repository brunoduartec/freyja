# Research: NPC World Integration

## Existing contracts observed

- **Schedule Core**: `ScheduleResolver.Resolve(npcId, GameTimeSnapshot, WorldContextSnapshot)` yields an `ActivityIntent`; `ActivityIntent.Identity` tracks the complete commitment. JSON is loaded by `ScheduleDataLoader` and `ActivityRecipeLoader`. The Unity `NpcScheduleTextAssetProvider` loads explicit `TextAsset` references.
- **Execution Core**: `NpcActivityController` creates one resolver/coordinator per component and exposes `TickFromWorld(time, context, operationalSeconds)`, `CancelExternal` and residual cleanup. It does not source world time. `ActivityCoordinator` owns operation transitions and cleanup.
- **Unity ports**: `NpcUnityActivityPorts.Initialize` requires a `SmartObjectReservationOwner`, `NpcLocationRegistry`, `NpcAnimatorBindings`, actor and existing `IslandWaypointWalker`. It reserves through `SmartObjectRegistry` using exact `ReservationHandle`s, maps approach/pose transforms, polls owned movement/animation operations and rejects stale ownership.
- **Locations / navigation**: `NpcLocationRegistry` uses explicit point IDs and region IDs, requires registered points to be children of its configured `IslandWaypointNetwork`, and returns active region points in deterministic order. `IslandWaypointWalker` is the movement owner. Do not drive `NavMeshAgent` separately.
- **Smart Objects**: the global `SmartObjectRegistry` supplies ordered candidates and atomic tokenized reservation; `SmartObjectReservationOwner` releases only its claims. Existing Play Mode tests already cover two slots plus fallback, invalid paths and cleanup.
- **Animation**: `NpcAnimatorBindings` routes semantic animation operations to `NpcAnimationDriver` and `NpcAnimationProfile`. Existing animation profile tests require `sit_down`, `sit_idle` and `stand_up`; one existing Play Mode test covers claims, not a full sit/stand transition.

## Clock and world-context decision

`Freyja/Assets/_Project/Scripts/Environment/CoastalDayNight.cs` is the game clock currently used by the island: it exposes `Hour`, advances through `Time.deltaTime` over a configured 24-hour cycle, and supports `AdvancesAutomatically` and `SetHour`. It has no absolute-day counter, weekday, weather or narrative flags. No other runtime calendar provider was found in the project source search.

Use `CoastalDayNight` as the sole source of hour. The integration may track a transient session-day number only when observing its natural midnight wrap, so it can construct the Schedule Core's required `GameTimeSnapshot`; initialize it to day 1 on Play Mode and do not persist it. A deliberate backward `SetHour` is a clock reset/regression and must be diagnosed rather than replaying skipped windows. The routine fixtures will not depend on weekday, weather or flags; the real context defaults remain the Core's documented clear/no-flags values until the game supplies those conditions. This does not create a new clock/calendar or add campaign persistence.

## Idle-target compatibility

The catalog declares `idle` as `TargetKind.None` and the loader rejects a JSON `targetId` for that activity. The required recipe nevertheless runs `move_to` with point `target` before waiting. Consequently, personal home cannot be assigned through the schedule JSON without changing the established schema. Resolve this at the Unity composition/port boundary using a per-NPC configured home point ID for the recipe's targetless `idle`; keep the shared catalog, recipe and schema unchanged. This also makes JSON-only edits valid for all schedule fields the existing schema supports.

## Routine and Smart Object target compatibility

`sit` is declared as `TargetKind.Location`; the activity recipe searches `SmartObjectRegistry` by that target/region and capability `sit`. Therefore all demonstration bench objects must use the real target region ID selected for the routines and the same Smart Object registration/slot API. Individual bench slots are alternative candidates only within that requested region. `wander` uses the configured region waypoint IDs; points must be actual descendants of the selected network.

## Main-scene inventory observed in the feature Editor

The target Editor opened `Assets/_Project/Scenes/Ilha_Principal.unity` cleanly for read-only inspection. It contains `Coastal Sun` (`CoastalDayNight`) and `Waypoints da ilha` (`IslandWaypointNetwork`, using `IlhaWaypointNavigation.asset`). Existing `Moradores060` instances include `Ivo Batista`, `Helena Batista` and `Luana Batista`; the scene search found `IslandWaypointWalker` on each and `Animator` children on each. Ivo's Animator references the existing Humanoid `IvoBatista.fbx` Avatar. No `SmartObject`, `NpcAnimationDriver`, `NpcAnimatorBindings`, `NpcActivityController`, `NpcLocationRegistry` or NPC world driver exists in the scene yet. The current scene must therefore be composed additively with existing components/assets.

`Praça central` and `Praça — igreja` are existing island surfaces. The waypoint network already has explicit destination points including `Largo` at (-2.00, 6.99, -3.00), `Rua leste — praia` at (27.00, 3.88, -22.00) and `Rua leste — subida`. The main plaza renderer bounds center is (0.00, 6.06, -9.00), size (27, 0, 16). `BenchProvisional.prefab` contains two authored `sit` slots (`sit_left`, `sit_right`) with distinct approach and pose transforms; its prefab Object ID is shared by instances and its initial region ID is `Largo`, so each placed bench needs its own stable Object ID and a region matching the routine's sit target. Configure these instance values through Unity Editor, not by editing scene YAML.

Candidate resident IDs for this demonstration are `world_ivo`, `world_helena` and `world_luana`; their routines are technical fixtures and do not assert that these canonical characters live at their current transforms. Their inspected starting positions are technical return-anchor candidates: Ivo (11.00, 7.95, 2.80), Helena (18.30, 8.00, 3.20), Luana (15.00, 5.97, -12.80). The existing game camera is `Cais Gameplay Camera`, at (31.55, 18.05, 26.85), rotation (8, 90, 0), FOV 58. Plaza, paths and camera were visually inspected in Scene View. NavMesh sampling is unavailable in Edit Mode until the scene registers its NavMeshData; candidate markers remain subject to Play Mode path verification.

## Baseline checks before implementation

- Unity Editor initially reported `6000.3.25f1`, `compiling=false`, `compilationFailed=false`, zero current console errors and one warning while attached to `/Users/bruno.duartec/Documents/Projects/Freyja-091-npc-animation-system/Freyja`, whose HEAD is the same source commit as the new feature branch (`fabd711`). It is a distinct worktree and was not used for scene authoring.
- The feature Editor is open on the correct checkout and reachable through its Pipeline/MCP endpoint: Unity 6000.3.25f1, port 7800, PID 3373, `state=ready`; MCP `editor.status` reports `compiling=false`, `domainReloadInProgress=false`, project path `/Users/bruno.duartec/Documents/Projects/Freyja/Freyja`. `Ilha_Principal.unity` is loaded and clean before authoring. MCP reads and writes target this project.
- An initial headless Unity test run found CS0103 in `NpcWorldRuntimeDriver.Update` because `operationalSeconds` had no local declaration. The driver was fixed and its session projection now resets on each enable. After the fix, Unity 6000.3.25f1 EditMode tests passed 40/40; the interactive Editor reports `compilationFailed=false` and Console counts of 0 errors, 0 warnings, 0 logs before scene authoring.
- `dotnet run --project Tools/NpcScheduleSimulator -- --validate`: PASS, 5 existing NPC schedules, 15 activities, 3 overrides, 3 executable recipes.
- `dotnet run --project Tools/NpcScheduleSimulator -- --test`: PASS, 168 assertions. Existing `sleep_early` gap warnings remain in the sample fisherman routine.
- `--simulate-week`: 7 days / 3 NPCs / 1,047 events / 16 reservations / 16 releases / 0 residual claims, with two simulated movement/animation failures and `physicalValidation=false`; it remains logic-only evidence and does not validate physical behavior.

## Alternatives considered

- **New clock or calendar**: rejected because `CoastalDayNight` is already the game clock and the feature must not create a competing source of time.
- **A second activity workflow or direct NavMesh control**: rejected because Activity Core, Smart Object ownership and `IslandWaypointWalker` already own those responsibilities.
- **Putting home IDs in `idle` JSON**: rejected because the current catalog and loader explicitly forbid targets for `idle`; that would require changing a completed schema contract. Use per-NPC host configuration instead.
- **Using only the existing isolated NPC demo scene**: rejected as final acceptance; perform a small, safe integration on `Ilha_Principal` as required and retain isolated smoke tests where useful.
