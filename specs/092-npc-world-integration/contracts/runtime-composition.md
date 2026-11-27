# Runtime Composition Contract

## Existing public boundaries

The implementation MUST preserve these existing contracts:

- `NpcActivityController.TickFromWorld(GameTimeSnapshot, WorldContextSnapshot, double)` receives authoritative snapshots and scaled operational seconds.
- `NpcScheduleTextAssetProvider.TryLoad(...)` loads the activity catalog, templates, NPC schedule documents, overrides and recipes from explicit TextAsset references.
- `NpcUnityActivityPorts.Initialize(...)` binds one NPC's owner, location registry, semantic animation bindings and actor transform. Operations are started once, polled by the Activity Core and matched to NPC/execution ownership.
- `NpcLocationRegistry.TryGetPoint(pointId, out Transform)` and `FindWaypointIds(regionId)` resolve explicit active point IDs and regions.
- `IslandWaypointWalker` is the sole movement authority; `SmartObjectRegistry` is the sole discovery/reservation authority; animation bindings are the pose/action authority.

## New composition behavior

One scene-level runtime driver MUST receive a serialized `CoastalDayNight` reference and an explicit list of enabled `NpcActivityController` components. On each Unity update it reads `Hour`, projects it to one session-only `GameTimeSnapshot`, provides the documented default world context (`clear`, empty flags) and calls each unique, initialized host once with `Time.timeAsDouble` (or equivalent time-scale-aware operational seconds). Invalid clock values and regressing time MUST yield a stable diagnostic. The driver MUST NOT create or advance a second clock, use wall time/background callbacks, replay skipped agenda windows, or resolve all schedules on every frame.

The projection starts at absolute day 1 with minute `floor(Hour * 60)`. An observed natural wrap from the late evening to early morning increments the absolute day. A backward jump outside that wrap is a reset/regression and is diagnosed. Weekday remains the Core's zero-origin default because the current game clock has no weekday source; no demonstration schedule depends on weekdays.

## Per-NPC idle destination

Each ports component MUST accept a serialized personal home point ID. When the established `idle` recipe requests point `target` and the current intent has no target, the adapter resolves that NPC's configured home point. This preserves `TargetKind.None` and avoids changing the Core's JSON schema or recipe. Missing/inactive home points return an explicit operation failure.

## Host registration and diagnostics

Before ticking, the driver detects null/disabled controllers, duplicate component references, blank IDs, duplicate NPC IDs and failed initialization; it logs each invalid item once and skips it. It does not silently select one duplicate host. Inspector/runtime diagnostics expose current NPC ID, projected day/time, current intent, activity snapshot/execution ID, result/reason and last resolved target; active seat owner/slot comes from existing Smart Object diagnostics/events.

## Interruption and cleanup

External interruption forwards to `NpcActivityController.CancelExternal(reason)` and retries residual cleanup only with the current valid snapshot. The runtime driver does not directly release reservations or cancel `NavMeshAgent`. If no real dialogue begin/end event exists, a test-only explicit interruption path may exercise this contract; it is not represented as dialogue integration.
