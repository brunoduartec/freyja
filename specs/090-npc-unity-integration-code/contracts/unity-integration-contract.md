# Unity Integration Contract

## Runtime host

- One host and `ActivityCoordinator` instance per NPC.
- Host input is an already-produced current world snapshot. It does not calculate schedule priority, call a second clock or replay skipped schedule windows.
- Data is loaded once from explicit references; resolver is re-run only on initial activation, relevant minute/entry boundary or context change.
- Operational ticks use the game's pause/time-scale policy; no wall-clock or background timer is allowed.
- On disable/destroy/scene exit, cancel synchronously while the snapshot/provider is available and stop only operations owned by this execution. If a seated NPC needs `stand_up`, retain the coordinator and exact claim until its bounded poll completes; after disable/re-enable, retry only when a fresh world snapshot is supplied. Preserve diagnostics if cleanup cannot finish.

## `IActivityPorts`

- `FindTargets`: adapt ordered `SmartObjectRegistry.FindCandidates` results; `ActivityCandidate` point IDs resolve to the exact authored slot's approach and pose transforms.
- `TryReserve`: call `SmartObjectRegistry.TryReserve` atomically for the selected candidate. Store the resulting exact `ReservationHandle` under an opaque adapter key bound to NPC/execution.
- `IsReservationValid` / `Occupy` / `Release`: check all three ownership fields; release mapping only when the public handle reports successful release.
- `FindWaypoints`: return configured region point IDs deterministically; empty/unknown region returns no candidates for the core to fail/fallback.
- `Begin` / `Poll` / `Cancel`: begin once, maintain a single operation owner, use complete NavMesh paths and arrival tolerances, return stable failure codes, and reject stale IDs. Cancellation receives the current operational time so an interrupted animation ticket can reach its configured timeout before `stand_up` cleanup takes ownership.
- Pose restoration: begin the explicit `stand_up` animation operation and poll the mapped Animator state to verified completion. Keep occupied ownership on rejection/timeout; retry cleanup before starting a replacement.

## Explicitly unresolved external providers

- Existing Freyja world-clock/context service (absolute day, minute, weekday origin, weather/flags, pause and fast-forward semantics).
- Authored scene points/region IDs and physical reachability.
- Verified Animator controller states/transitions/clips for `sit_down` and `stand_up`.
- Component ownership choice for NPC prefabs between `IslandWaypointWalker` and direct NavMeshAgent control.
