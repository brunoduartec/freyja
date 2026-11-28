# Feature Specification: NPC Integration Validation

**Feature Branch**: `093-npc-integration-validation`  
**Created**: 2026-11-27  
**Status**: Approved input; validation in progress  
**Input**: User-provided validation specification, preserved in [source-requirements.md](source-requirements.md).

## User Scenarios & Testing

### User Story 1 — Establish an evidence-based baseline (Priority: P1)

As the Freyja team, we need to know whether the six completed NPC routine systems are actually integrated in the current Unity project, so we can decide whether the next feature is safe to begin.

**Why this priority**: Every physical test depends on a compilable, correctly composed project and verified data.

**Independent Test**: Inspect the current scene, adapters, JSON, references, tests, and earlier W01–W19 results; rerun the available baseline suites and record their actual outputs.

**Acceptance Scenarios**:

1. **Given** the current checkout and open Unity project, **When** the baseline is inspected, **Then** observed systems, build version, scene, data, and tool availability are recorded without assuming prior claims are complete.
2. **Given** existing tests and reports, **When** relevant suites are rerun, **Then** counts and failures are separated by .NET, Edit Mode, and Play Mode.

### User Story 2 — Verify autonomous physical routines and failure recovery (Priority: P1)

As a player, I need three residents to follow their own JSON schedules in the real island scene, so their visible habits are stable and believable.

**Why this priority**: Real movement, animation, reservations, cancellation, and recovery are the central integration acceptance criteria.

**Independent Test**: Use the existing in-game clock and production routine host to observe a real scheduled trip through supported activity stages, conflicts, interruptions, and cleanup. Separate physically executed stages from logical simulation.

**Acceptance Scenarios**:

1. **Given** three NPCs with distinct JSON routines, **When** world time advances through activity windows, **Then** each follows its current commitment using real navigation, animation, and Smart Objects, with one active host/execution per NPC.
2. **Given** contention, schedule changes, inaccessible paths, stalls, or object invalidation, **When** the affected run reaches its documented resolution limit, **Then** it reports failure/fallback, cleans up correctly, and leaves other NPCs healthy.
3. **Given** an interrupted sit, **When** the NPC exits safely, **Then** the slot remains protected while physically occupied and is released after confirmed exit.

### User Story 3 — Assess duration, regressions, and runtime cost (Priority: P2)

As the team, we need reproducible short and long runs, a focused performance sample, and a final regression record before expanding the routine system.

**Why this priority**: A successful short sequence alone does not establish day-scale stability or reveal accumulating state.

**Independent Test**: Run the available seven-day logical harness twice where deterministic, run as many full physical days as practical without skipping physical work, and capture comparable Profiler samples.

**Acceptance Scenarios**:

1. **Given** the documented logical harness, **When** a week is simulated twice with the same seed/configuration, **Then** decisions and resource accounting are compared and no unexplained residual claims or deadlocks remain.
2. **Given** the real island and three NPCs, **When** a physical long run is performed, **Then** only fully observed days are counted and remaining coverage is explicitly partial or blocked.
3. **Given** idle, movement, and contention, **When** Profiler evidence is collected, **Then** available CPU, GC, memory, and navigation metrics are recorded without inventing an unapproved performance budget.

## Edge Cases

- A system, test assembly, test hook, reload mechanism, second compatible slot, dialogue hook, or profiler counter may not exist in the current integration; record the dependent cases as BLOCKED or NOT RUN rather than inventing replacements.
- World time can jump, pause, or accelerate while operations use a separate operational clock.
- Cancellation can arrive during navigation, sit-down, idle pose, or stand-up; stale callbacks must not affect a newer run.
- A routine can refer to invalid or duplicate identifiers, missing destinations, unavailable animation, a disabled slot, or an inaccessible destination.
- A Unity MCP operation can fail while the Editor remains open; do not launch a second Unity instance over the project.

## Requirements

- **FR-001**: Preserve current user changes and record branch, HEAD, Git status, Unity version, active scene, and available diagnostics before testing.
- **FR-002**: Verify the six reported systems and any `npc-unity-integration-code` adapters in the current checkout; identify actual runtime/test assemblies, three demo NPCs, source JSON, clock, targets, hosts, navigation, animation, Smart Objects, and existing debugger.
- **FR-003**: Map prior W01–W19 and valid evidence to V01–V35, then execute relevant existing .NET, Edit Mode, and Play Mode suites without duplicating already valid coverage unnecessarily.
- **FR-004**: Exercise production schedule-driven behavior with three distinct NPCs in the real island scene; manually issued activities or simulator fakes cannot prove this requirement.
- **FR-005**: For each defect, reproduce first, document the cause, apply the smallest in-scope fix, and run a focused regression before claiming resolution.
- **FR-006**: Verify schedule/time changes, contention, cancellation, inaccessible paths, stalls, Smart Object lifecycle, NPC lifecycle, and resource release using existing contracts and fixtures.
- **FR-007**: Separate logical simulation from physical Play Mode; distinguish time jumps from completed movement/animation and do not count skipped activity as physical success.
- **FR-008**: Perform the available full-day Play Mode and seven-day logical/physical runs as specified; clearly report partial, blocked, and not-run cases.
- **FR-009**: Use Unity Profiler where available to record comparable baseline/idle/movement/contention samples and available counters; do not apply speculative optimizations.
- **FR-010**: Recheck the playable protagonist, camera, input, interaction, and day/night clock for regressions.
- **FR-011**: Deliver a root-level Markdown report containing the full V01–V35 matrix, actual test outcomes, Play Mode evidence, defects/reproductions/fixes, performance measurements, limits, and a clear stability decision for `npc-debug-tools`.
- **FR-012**: Do not add new activities, schedule editors, narrative systems, AI frameworks, broad refactors, speculative performance budgets, or unsupported success claims.

## Success Criteria

- **SC-001**: Every V01–V35 case has an environment, expected result, observed result, status, and evidence path or an explicit NOT RUN/BLOCKED reason.
- **SC-002**: All existing relevant automated suites report actual pass/fail/skip counts, and final regression runs cover every changed production behavior.
- **SC-003**: Physical claims include correlated runtime evidence; simulator-only evidence is labelled logical.
- **SC-004**: Any fix has a reproduced cause and a passing focused regression, or remains an open defect.
- **SC-005**: Long-run duration records actual simulated days/minutes and physical wall time; a partial run never implies full requested coverage.
- **SC-006**: Performance records the Unity build/editor context, NPC count, time scale, sample type, and available CPU/GC/memory/navigation values; no unsupported budget assertion is made.
- **SC-007**: The final report states whether routine infrastructure is sufficiently stable for `npc-debug-tools` and lists any blockers.
