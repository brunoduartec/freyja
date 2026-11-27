# Research — NPC Unity Integration Code

## Confirmed contracts

- `Freyja.NpcScheduling.ScheduleResolver.Resolve(npcId, GameTimeSnapshot, WorldContextSnapshot)` creates a current `ActivityIntent` and trace. Core source is Unity-free.
- `ActivityCoordinator(npcId, recipes, IActivityPorts, sink)` exposes `Tick(intent,time,operationalSeconds)`, `Cancel(reason,time,now)`, `Remove(time,now)`, `Resume(current)` and residual-cleanup operations. It owns retries, fallback, transitions and cleanup policy.
- `IActivityPorts` methods: `FindTargets`, `FindWaypoints`, `TryReserve`, `IsReservationValid`, `Occupy`, `Release`, `Begin`, `Poll`, and time-aware `Cancel`. Pose restoration reuses the bounded animation operation with the explicit `stand_up` ID; no synchronous pose shortcut is exposed. `ActivityCandidate` exposes symbolic candidate/target/approach/pose IDs; `ActivityOperation` has one operation ID and action/target/point/animation.
- The recipe file has executable `idle`, `wander`, and `sit`; `sit` requires find → reserve → move → align → occupy → play `sit_down` → wait → play `stand_up` → release.
- Smart Objects expose `SmartObjectRegistry.FindCandidates`, `TryReserve`, and `ReservationHandle`; owner cleanup is delegated to `SmartObjectReservationOwner`.
- `IslandWaypointWalker.TryMoveTo(Transform)` owns a NavMeshAgent, stops it on arrival/failure/disable, but does not expose a completion result or operation ID. It must not be driven concurrently with a second agent controller.
- `IslandWaypointNetwork` registers its authored `NavMeshData` in `OnEnable` and removes it in `OnDisable`; no global NavMesh setup is needed.

## Missing production contracts

- No game clock, weather/context provider, or clock-change event exists in checked scripts. The host must require explicit current snapshots and report that the real provider is still unconnected.
- No verified animation state/trigger IDs or sit/stand clips exist in the checked code.
- No authorized real home, square, beach, or bench IDs/Transforms can be selected from source alone. Registry components will accept explicit authored references and reject duplicates.

## Baseline verification

- .NET SDK 11.0.100.
- `dotnet build Tools/NpcScheduleSimulator/NpcScheduleSimulator.csproj --no-restore`: passed, 0 warnings, 0 errors (after assets restore/build had been run).
- `dotnet run --project Tools/NpcScheduleSimulator -- --test`: Schedule Core `PASS: 68 assertions`; Activity Core `PASS: 135 assertions`.
- `--validate`: Schedule-only worktree reports 5 NPC / 15 activities / 3 overrides; Activity Core worktree also validates 3 executable recipes.
- Schedule `--simulate-week`: 296 transitions; Activity `--simulate-week`: 7 days, 1,335 simulated events, 2 simulated failures, 21 reservations/21 releases, 0 residual claims, `physicalValidation=false`.
- Smart Objects Edit/Play Mode tests were not run; Unity is explicitly out of scope/unavailable.

## Decisions

- Keep existing `.NET` harness as the non-Unity test boundary; no fake Unity types or runtime simulators.
- Prefer explicit TextAsset references for runtime data and the same named-document loader implementation for harness/runtime.
- A port token is an adapter-owned lookup key only; retain the actual `ReservationHandle` struct and owner/execution tuple until confirmed release.
- Calendar state is supplied by the game. Operational timeout uses monotonic Unity game time that respects the approved pause policy; absent that policy, external injection remains pending and the adapter must not invent wall-clock timing.
