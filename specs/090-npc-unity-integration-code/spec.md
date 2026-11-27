# Feature Specification: NPC Unity Integration Code

**Feature Branch**: `090-npc-unity-integration-code`
**Created**: 2026-11-26
**Status**: Approved by user through the attached implementation request
**Input**: `.codex-attachments/.../freyja-npc-unity-integration-code.md`

## User Scenarios & Testing

### User Story 1 — Run a schedule-driven NPC (Priority: P1)

An NPC host consumes the existing schedule resolver's current intent and the existing activity coordinator, keeping schedule decisions and execution policy in their respective cores. It reads the existing catalog, schedule and recipe data through explicit runtime assets/providers, and rejects missing or invalid dependencies with diagnostics.

**Why this priority**: Without the host and data boundary, Unity cannot drive the existing cores.

**Independent Test**: Automated core tests continue to pass; static review confirms one coordinator per NPC, current snapshots only, no Unity dependencies in the cores, and no wall-clock schedule or duplicate resolver policy.

**Acceptance Scenarios**:

1. **Given** valid NPC data and a supplied real world-time/context source, **When** the host initializes and ticks, **Then** it resolves the current intent and gives it to one coordinator for that NPC.
2. **Given** no compatible world-time/context source or invalid data, **When** the host initializes, **Then** it reports a diagnostic and performs no fabricated schedule action.
3. **Given** a disabled or destroyed host, **When** teardown runs, **Then** it cancels/removes its coordinator synchronously and releases only resources it owns.

### User Story 2 — Move to authored locations and region points (Priority: P1)

The NPC moves to explicit symbolic destinations and configured region points through the project's existing movement/navigation behavior. Points are registered by lifecycle and duplicate or unavailable IDs fail visibly.

**Why this priority**: `idle` and `wander` require movement to authored destinations.

**Independent Test**: Static/API review confirms no teleports, global searches per frame, repeated `SetDestination`, or path-partial success; physical arrival remains a Unity validation step.

**Acceptance Scenarios**:

1. **Given** a unique enabled location ID with a complete reachable path, **When** a move operation begins, **Then** that operation is started once and succeeds only after actual arrival.
2. **Given** a missing destination, invalid agent, incomplete path, timeout, or destroyed target, **When** the operation is polled, **Then** it fails with a stable reason and never reports arrival.
3. **Given** a region with configured points, **When** `wander` requests waypoints, **Then** it receives only those points in deterministic order; an empty region fails without retrying every frame.

### User Story 3 — Reserve, reach and use a Smart Object (Priority: P1)

The activity ports adapter searches the existing Smart Object registry, reserves an available compatible slot atomically, approaches and aligns to its authored transforms, confirms occupancy, waits through the activity, restores pose and releases the exact reservation.

**Why this priority**: `sit` must represent a real occupied seat and must not be confused with schedule intent.

**Independent Test**: Core harness regressions confirm existing reservation/fallback policy; static review verifies the adapter preserves each `ReservationHandle` and its owner/execution association and never emits success for missing animation bindings.

**Acceptance Scenarios**:

1. **Given** a compatible available slot, **When** `sit` executes, **Then** the slot is atomically reserved, the NPC reaches and aligns to its authored approach/pose, occupancy is confirmed, and the mapped sit animation completes before the wait.
2. **Given** all slots are unavailable or the target disappears, **When** the adapter reports failure, **Then** the core decides fallback and no other NPC's reservation is released.
3. **Given** the intent expires or the NPC is removed while moving or sitting, **When** cleanup runs, **Then** only the current operation and exact reservation are canceled/released; residual cleanup failure remains visible.

### User Story 4 — Configure a minimal vertical slice later (Priority: P2)

When Unity is available, a test-only NPC can use the runtime host, explicit schedule data, symbolic destinations, configured region points, an Animator binding map and Smart Objects to execute `idle`, `wander`, and `sit` without changing official residents in this code-only delivery.

**Independent Test**: The implementation guide names the real scripts, fields, prerequisites and manual Play Mode acceptance sequence; no asset or Play Mode validation is claimed now.

**Acceptance Scenarios**:

1. **Given** the physical configuration and valid world clock become available, **When** the test NPC runs the accepted intent sequence, **Then** it reaches destinations, occupies a slot only after alignment, stays seated, stands and releases before the next intent.
2. **Given** clips, clock, data bindings, NavMesh or physical destinations are not configured, **When** the test is attempted, **Then** the missing prerequisite is reported and no false success is accepted.

## Edge Cases

- Duplicate location IDs, invalid scene object references, disabled/destroyed locations, and scene unload during an operation.
- NavMesh agent disabled/off-mesh, partial/invalid/pending paths, non-finite distance, unreachable destination, and cancellation while a move is active.
- Duplicate Smart Object/slot IDs, reservation race, invalidated reservation, object disable/destruction, and failed release.
- Missing Animator parameter/state/clip mapping, interrupted transition, Animator/agent ownership conflict, and sit without a confirmed seated pose.
- Clock pause/time-scale/fast-forward, midnight, large forward jump, backward reset, and scene/host disable; never replay skipped agenda entries.
- No real Unity clock/context provider was found. No schedule/clock is fabricated; runtime initialization must fail closed until supplied.

## Requirements

### Functional Requirements

- **FR-001**: The integration MUST reuse the real Schedule Resolver, Activity Coordinator, activity recipes and Smart Object registry; it MUST NOT duplicate their policies or registries.
- **FR-002**: One host/coordinator MUST own each NPC's active execution and consume the current game-time/context snapshot and monotonic operational time.
- **FR-003**: Runtime schedule and recipe data MUST come from explicit Unity-compatible references/providers and use the same JSON files as the harness; missing, malformed, duplicate or incompatible data MUST fail initialization with diagnostics.
- **FR-004**: The integration MUST expose lifecycle-managed symbolic point/region registration with deterministic waypoint results and duplicate-ID diagnostics.
- **FR-005**: Movement MUST use an existing movement controller or one NavMesh controller per NPC, start once per operation, and report success only after complete-path arrival; partial, pending, invalid and timed-out paths MUST fail.
- **FR-006**: Smart Object reservation MUST use `SmartObjectRegistry.TryReserve`, preserve exact `ReservationHandle` ownership, validate before occupy, and release only after confirmed cleanup.
- **FR-007**: Animation bindings MUST be explicit and verified; a trigger alone MUST NOT mean that a sit/stand animation completed. Missing configuration MUST fail clearly.
- **FR-008**: Disable, destroy, scene changes and cancellation MUST synchronously stop owned operations and release owned claims where the existing contracts allow; stale operations MUST NOT affect newer executions.
- **FR-009**: No production simulator, fabricated clock, teleport, absolute path, wall-clock timer, background task, per-frame data parsing/resolution, or per-frame diagnostic logging may be used.
- **FR-010**: This delivery MUST NOT modify scenes, prefabs, animation assets/controllers, NavMesh assets, project settings, packages or `.meta` files, and MUST NOT attach hosts to official residents.
- **FR-011**: The delivery MUST document the physical setup and acceptance steps for a test-only NPC, including clock/data, destinations, NavMesh, two-slot seat, Animator, lifecycle/failure cases and evidence.
- **FR-012**: Testing claims MUST distinguish core harness results, code review, Unity compilation and Play Mode; unavailable Unity validation MUST remain pending.

## Key Entities

- **Schedule input**: NPC ID, current `GameTimeSnapshot`, `WorldContextSnapshot`, and resolved `ActivityIntent` supplied by the actual world source.
- **Execution operation**: An `ActivityOperation` identified by NPC and `ExecutionId`, with one begin/poll/cancel lifecycle.
- **Authored location**: Stable symbolic point or region ID mapped to configured scene transforms.
- **Smart Object reservation**: Exact slot claim represented by the existing opaque `ReservationHandle` and its owner.
- **Animation binding**: Explicit mapping from recipe animation ID to a configured Animator state/parameter and observable completion condition.

## Success Criteria

### Measurable Outcomes

- **SC-001**: Existing Schedule and Activity Execution harness tests pass unchanged after integration.
- **SC-002**: Static review finds exactly one active coordinator per NPC and no duplicated core logic, Smart Object registry, fake production clock, or stale-operation ownership path.
- **SC-003**: Each operation begins at most once, polls without per-frame full agenda parsing/resolution, and reports success only on a verifiable completed condition.
- **SC-004**: The physical validation checklist is reproducible and explicitly identifies all unverified Unity, NavMesh, scene, asset and animation prerequisites.

## Assumptions

- Unity is 6000.3.25f1, Built-in renderer; C# must remain compatible with that project and the existing assembly layout.
- The Activity Execution Core, Schedule Core and Smart Objects Foundation are now present on local `main`; their source, contracts and test harness are available.
- There is no current real Unity world clock/context provider; this is a hard runtime dependency, not permission to create a parallel clock.
- There are no verified sit/stand clips, Animator states, real location IDs or scene destinations to assign during this code-only delivery.
- The existing `IslandWaypointWalker` and `IslandWaypointNetwork` are the movement/navigation implementation to inspect and reuse where compatible.
- The synthetic execution fixture is for the harness only; official NPC schedules remain unchanged.
