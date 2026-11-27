# Feature Specification: NPC Navigation System

**Feature Branch**: `089-npc-navigation-system`
**Created**: 2026-11-27
**Status**: Approved for planning
**Input**: User-approved specification in `.codex-attachments/.../freyja-npc-navigation-system.md`.

## User Scenarios & Testing

### User Story 1 — Reusable destination movement (Priority: P1)

As the activity executor, I need an NPC to move to a resolved world point and report truthful progress and completion, so activities can request physical movement without controlling the character directly.

**Why this priority**: Every other scenario depends on reliable movement, arrival, and failure reporting.

**Independent Test**: In Play Mode, a fixture NPC moves from a reachable start to a configured point, reports arrival only inside the configured horizontal and vertical tolerance, and reports an unreachable target as failure.

**Acceptance Scenarios**:

1. **Given** an enabled agent on a valid navigation surface, **When** a complete path is requested, **Then** it moves and reports arrival only after physical displacement and destination checks pass.
2. **Given** a new request while path calculation is pending, **When** progress is queried, **Then** no stale or premature success is reported.
3. **Given** an invalid, partial, unreachable, disabled, or off-surface request, **When** movement is requested, **Then** a finite explicit failure is reported without teleporting or falsely claiming arrival.
4. **Given** the NPC is already inside the arrival tolerances, **When** it is ordered to that point, **Then** it may complete without displacement because its current position satisfies the same arrival checks.

### User Story 2 — Cancel and recover safely (Priority: P1)

As an activity executor, I need to cancel the current movement and replace it without an old operation stopping the new one, so schedule changes and interruptions remain safe.

**Independent Test**: Cancel a moving NPC twice, issue another operation, and verify only the current operation controls its agent. Exercise disable/re-enable, target destruction, pause/resume, and a blocked route.

**Acceptance Scenarios**:

1. **Given** an NPC is moving, **When** its current operation is cancelled, **Then** its path is cleared and repeated cancellation is safe.
2. **Given** a new operation starts after cancellation, **When** an old operation later receives cancellation, **Then** the new operation continues unaffected.
3. **Given** the agent or target becomes invalid, or progress stops, **When** the operation is polled, **Then** it terminates with a diagnostic failure within its configured physical-time limit.
4. **Given** the simulation is paused under the chosen pause policy, **When** no physical time elapses, **Then** the watchdog does not time out solely because of the pause.
5. **Given** the NPC is disabled, destroyed, or its scene unloads, **When** lifecycle cleanup runs, **Then** no active movement or callback remains.

### User Story 3 — Regional movement and Smart Object approach (Priority: P2)

As an activity or Smart Object consumer, I need to move among configured points or reach a selected slot's approach point, while retaining ownership of selection and reservations.

**Independent Test**: Exercise a finite configured region sequence; direct two agents to distinct approach points on a configured Smart Object with two slots. If no real suitable object is configured, use identified temporary markers and report real-object integration as pending.

**Acceptance Scenarios**:

1. **Given** a region with valid points, **When** a consumer requests the next movement, **Then** the chosen point is finite, belongs to the configured region, and is reachable before movement begins.
2. **Given** a region without usable points, **When** movement is requested, **Then** an explicit failure is returned without starting a competing wander loop.
3. **Given** a consumer has selected and reserved a Smart Object slot, **When** it supplies that slot's approach point, **Then** navigation arrives at the approach point and does not select, occupy, release, or reposition onto the seated pose.
4. **Given** the selected target is invalidated during movement, **When** progress is polled, **Then** navigation fails safely and the consumer remains responsible for reservation cleanup.

### User Story 4 — Observe concurrent movement (Priority: P2)

As a developer, I need a small removable demonstration with three NPCs and movement diagnostics, so I can observe crossings, shared-space avoidance, arrival, cancellation, and failure in the Unity Editor.

**Independent Test**: In Play Mode, run three agents simultaneously through a reachable route, a region sequence, and distinct Smart Object approaches or a crossing route. Capture operation transition logs and runtime state; a static screenshot alone is insufficient.

**Acceptance Scenarios**:

1. **Given** three configured agents, **When** their independent routes run simultaneously, **Then** each completes or fails independently without permanent control contention or gross overlap.
2. **Given** a fixture harness is configured, **When** its routes run, **Then** selected/requested/effective destinations, operation ID, state, path status, velocity, lack-of-progress time, and last failure/cancellation reason are inspectable.
3. **Given** a waypoint or approach point is selected, **When** the object is inspected, **Then** gizmos show configured points, tolerances, and available path corners.

## Edge Cases

- The agent starts at the destination, but on a different level or outside vertical tolerance.
- A new order arrives while the previous path is pending or while cancellation is running.
- A destination projects to a nearby but disconnected surface, or a path is partial.
- The navigation surface, agent, selected slot, or target is disabled or destroyed.
- A temporary obstacle blocks progress; distinguish pending path calculation, legitimate avoidance, and a real stall.
- No valid regional candidate exists, or a candidate lies in water, outside the region, or on another level.
- Several NPCs converge on a narrow route or nearby approach points.
- Time scale is paused or accelerated; movement watchdogs use runtime physical time, not game-world minutes.
- Stair/ramp/causeway validation is conditional on a usable route already existing in the scene.

## Requirements

### Functional Requirements

- **FR-001**: The system MUST reuse the project's existing navigation data, waypoint network, and agent configuration where suitable, and MUST NOT introduce a duplicate pathfinding system or a navigation surface on every NPC.
- **FR-002**: A movement operation MUST carry an owner/NPC identity and unique operation identity, expose progress, and return terminal success, failure, or cancellation.
- **FR-003**: Arrival MUST require a complete non-pending path, measured physical position within configured horizontal and vertical tolerances, and valid operation ownership. Being stationary alone MUST NOT count as arrival.
- **FR-004**: Invalid, partial, unreachable, off-surface, or invalidated-target requests MUST fail explicitly; navigation MUST NOT warp or move an NPC by direct transform assignment.
- **FR-005**: Cancellation MUST be idempotent and MUST affect only the matching current operation; disabling or destroying an NPC MUST clear its active navigation state.
- **FR-006**: Stall detection and any retries MUST be bounded and configurable, and MUST NOT duplicate Activity Core retry ownership if that policy is already active.
- **FR-007**: Watchdog time MUST follow a documented pause policy and MUST be based on physical runtime time, not the simulated schedule clock.
- **FR-008**: A consumer MAY provide a world point, transform, configured waypoint, or selected Smart Object approach point. Navigation MUST NOT own Smart Object reservation/occupancy/release or impose the final seated pose.
- **FR-009**: Regional candidate selection and movement MUST be finite; no valid candidate MUST produce explicit failure. Navigation MUST NOT add a second scheduler or an unbounded wander loop.
- **FR-010**: Three NPCs MUST use the same reusable navigation implementation and agent settings in a simultaneous Play Mode demonstration.
- **FR-011**: Runtime diagnostics MUST expose operation/NPC ID, state, requested and effective destination, path status/corners when available, real velocity/direction, lack-of-progress time, and the last terminal reason without per-frame logging.
- **FR-012**: A removable harness MUST support starting and cancelling test movement without C# edits and MUST not populate or materially alter the definitive island scene.
- **FR-013**: The feature MUST NOT implement animation controllers, final NPC controllers, schedule/activity orchestration, a general location registry, or changes to playable-character controls.
- **FR-014**: Any Activity Core adapter MUST use an existing stable contract and MUST live in feature-owned files. If the contract is changing or unavailable, physical movement remains testable through the isolated harness and the adapter is explicitly marked pending.
- **FR-015**: Logs and evidence MUST distinguish physical Play Mode behavior from compilation, automated/simulated tests, and untested integration. NPC navigation state MUST NOT be written as a narrative fact or player deduction.

### Acceptance Matrix

| ID | Scenario | Required result |
|---|---|---|
| NAV-01 | Reachable destination | Complete physical movement and valid arrival |
| NAV-02 | Already at destination | Success only when position tolerances pass |
| NAV-03 | Pending path/new request | No premature success or stale path state |
| NAV-04 | Invalid/partial/unreachable path | Explicit failure, no false arrival or teleport |
| NAV-05 | Disabled/off-surface agent | Safe failure and diagnosis |
| NAV-06 | Repeated cancellation/new operation | Old cancellation cannot affect the new operation |
| NAV-07 | Obstacle/stall | Bounded failure or recovery without infinite loop |
| NAV-08 | Region with/without candidates | Finite valid move or explicit failure |
| NAV-09 | Three simultaneous NPCs | Independent operation and no permanent deadlock |
| NAV-10 | Smart Object approaches | Distinct approach arrival; consumer owns reservations |
| NAV-11 | Disable/destroy/scene unload | No stale operation or callback |
| NAV-12 | Pause/resume | No timeout while paused according to documented policy |
| NAV-13 | Stair/ramp/causeway | Test a real available route or record blocked/not run |
| NAV-14 | Core regression | No newly attributable failures in Smart Objects, Schedule, or Activity Core tests |

## Key Entities

- **Navigation operation**: One owner-scoped request with operation ID, requested/effective destination, state, timing, path status, movement telemetry, and terminal reason.
- **Navigation region**: A finite collection of configured points with candidate-validity rules; it does not own scheduling.
- **Approach point**: A location selected by a Smart Object consumer for a reserved slot; it is separate from the final interaction pose.
- **Demonstration fixture**: Removable agent, destination/region markers, and test driver used for isolated Play Mode evidence.

## Success Criteria

### Measurable Outcomes

- **SC-001**: A reachable movement ends only after the NPC physically occupies the configured arrival tolerance; report requested/effective point, elapsed runtime, and terminal state.
- **SC-002**: Cancellation followed immediately by a new movement leaves the new operation active and completes/fails independently.
- **SC-003**: An unreachable destination and a stalled route terminate explicitly within configured finite limits, without warp or false success.
- **SC-004**: Three NPCs execute simultaneously through the shared implementation and each reaches its own terminal result.
- **SC-005**: Every NAV-01–NAV-14 criterion has a real evidence reference or is clearly marked `BLOCKED` or `NOT RUN`; no unexecuted behavior is called validated.
- **SC-006**: Unity compilation reports no new errors attributable to this feature, and relevant existing-core tests show no attributable regression.

## Assumptions

- The current `IslandWaypointNetwork`, `IslandWaypointWalker`, and `IlhaWaypointNavigation` data are the first reuse candidates; final values and route availability will be verified in Unity before configuring fixtures.
- The installed project version is Unity `6000.3.25f1`; no AI Navigation package upgrade or other dependency change is authorized by this feature.
- A prior Unity Integration worktree contains uncommitted Activity Core and Unity adapter edits. This feature will not alter those files or shared contracts; direct adapter integration depends on that contract stabilizing.
- A real bench with two approach slots may not exist in the active test scene; use markers only for movement proof and report Smart Object integration pending if real slots are unavailable.
- Test NPCs may be existing rigged residents or clearly labeled temporary fixtures; locomotion animation is not part of the acceptance evidence.
- Navigation telemetry is technical runtime state and does not itself establish narrative knowledge, testimony, or evidence.
