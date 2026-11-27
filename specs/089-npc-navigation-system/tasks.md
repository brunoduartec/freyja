# Tasks: NPC Navigation System

**Input**: Design documents from `specs/089-npc-navigation-system/`
**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/navigation-operation.md`, `quickstart.md`

## Phase 1: Setup and preflight

- [x] T001 Confirm feature branch and preserve concurrent work; record `Freyja/ProjectSettings/ProjectVersion.txt` and `Freyja/Packages/manifest.json` without changing dependencies. Evidence: branch `089-npc-navigation-system`, isolated sparse worktree; Unity 6000.3.25f1; manifest unchanged.
- [x] T002 Record current Unity scene, Console cursor/counts, loaded navigation network/data and agent settings in `specs/089-npc-navigation-system/validacao.md`.
- [x] T003 Audit existing walker/network/adapter behavior and document it in `research.md`. Stable Unity Integration handoff was commit `9c82ab1`; T006 was then completed before the walker extension.

## Phase 2: Foundational checks

- [x] T004 Verify compilation state and available Edit/Play Mode test assemblies; record observed Console state and test counts in `specs/089-npc-navigation-system/validacao.md`. No clean project baseline is claimed.
- [x] T005 Verify NavMesh samples and complete paths for demo start and available approach points using the current navigation asset; record tested points and blocked routes in `specs/089-npc-navigation-system/validacao.md`. Finite-region sequence was not run.
- [x] T006 Confirm ownership handoff (commit 9c82ab1) for Activity Core/Unity Integration before changing `IslandWaypointWalker.cs`, `ActivityExecutionModels.cs`, `NpcUnityActivityPorts.cs`, or any adapter. Do not modify any file that remains actively edited.

## Phase 3: User Story 1 — Reusable movement operation (Priority: P1)

**Goal**: Report truthful movement progress, arrival, failure, and cancellation through the existing movement owner.

**Independent Test**: Play Mode verifies reachable arrival, already-at-destination, pending path, invalid/partial/unreachable targets, disabled/off-surface agent, and a bounded stall.

- [ ] T007 [US1] Automated focused nav tests remain pending: current test assemblies cannot reference Prototype/Assembly-CSharp without architecture change. Runtime cases were exercised manually in Unity Play Mode; see validacao.md.
- [x] T008 [US1] Complete only missing move/progress/arrival semantics in `Freyja/Assets/_Project/Scripts/Prototype/IslandWaypointWalker.cs` after T003/T006 confirm handoff; preserve the operation API already added by Unity Integration.
- [x] T009 [US1] Verify real destination resolution and arrival tolerances through Unity MCP and record requested/effective points, elapsed runtime, and result in `specs/089-npc-navigation-system/validacao.md`.

## Phase 4: User Story 2 — Safe cancellation and recovery (Priority: P1)

**Goal**: Ensure cancellation, replacement, lifecycle, pause, and blockage cannot leave stale movement active.

**Independent Test**: Cancel a moving NPC repeatedly, start a new operation, then exercise target invalidation, disable/re-enable, pause/resume, and a blocked route.

- [ ] T010 [US2] Automated lifecycle tests remain pending for the same assembly reference limit. Cancellation, invalid target, disabled walker, off-surface, destroy-target, pause and watchdog were exercised manually in Play Mode; see validacao.md.
- [x] T011 [US2] Complete missing stale-operation isolation, idempotent cancellation, and disable/destroy cleanup in `Freyja/Assets/_Project/Scripts/Prototype/IslandWaypointWalker.cs` after stable handoff `9c82ab1`.

## Phase 5: User Story 3 — Regional movement and Smart Object approach (Priority: P2)

**Goal**: Reuse finite configured region points and reach the consumer-selected approach point without taking reservation ownership.

**Independent Test**: Walk a finite sequence of existing points and, when a real two-slot object exists, reach two distinct approaches; otherwise identify marker-only validation as blocked for real-object integration.

- [x] T012 [US3] Create `Freyja/Assets/_Project/Scripts/Prototype/NpcNavigationDemoHarness.cs` with finite candidate selection and selected-point/path gizmos; route requests remain owned by `IslandWaypointWalker`, with no scheduler or location registry.
- [x] T013 [US3] Add harness commands for region sequence and externally selected approach targets in `Freyja/Assets/_Project/Scripts/Prototype/NpcNavigationDemoHarness.cs`; no Smart Object reservation/occupancy/release is performed.
- [x] T014 [US3] Validate finite valid/no-candidate routes and any real selected Smart Object approaches in Play Mode; save operation evidence under `specs/089-npc-navigation-system/evidence/` and summarize in `specs/089-npc-navigation-system/validacao.md`.

## Phase 6: User Story 4 — Three-agent diagnostics demonstration (Priority: P2)

**Goal**: Observe three independently moving NPCs and their operation telemetry in a small removable test context.

**Independent Test**: Three agents simultaneously execute crossing, region, and approach/alternate destinations; inspect each operation and capture time-based evidence.

- [x] T015 [US4] Extend `Freyja/Assets/_Project/Scripts/Prototype/NpcNavigationDemoHarness.cs` to expose three NPC/operation IDs, state, requested/effective target, path state, velocity/direction, no-progress time, and last reason without per-frame logs.
- [x] T016 [US4] Configure three clearly labeled, runtime-only temporary fixtures in `CadeiraSmartObject_Demo.unity` after inspection found no suitable active residents; fixtures were not saved to the scene.
- [ ] T017 [US4] Run simultaneous routes/approach, invalid-target, cancel/reissue, and lifecycle sequences in Unity Play Mode; narrow-crossing/deadlock scenario remains pending. Save logs and captures under `specs/089-npc-navigation-system/evidence/`.

## Phase 7: Polish and regression

- [x] T018 Run focused tests and available Smart Objects, Schedule Core, and Activity Core regressions; record exact results and baseline limitations in `specs/089-npc-navigation-system/validacao.md`.
- [x] T019 Complete the NAV-01–NAV-14 matrix, setup instructions, adapter status, and `npc-animation-system` handoff in `specs/089-npc-navigation-system/validacao.md`.
- [x] T020 Review changed paths and post-test Console; confirm no PlayerController, terrain, package, animation-controller, or shared-contract changes in this feature worktree in `specs/089-npc-navigation-system/validacao.md`.

## Dependencies

- T001–T006 precede code changes.
- US1 precedes US2–US4 because later scenarios depend on its terminal movement semantics.
- US2 and US3 depend on the stable walker operation API; US4 depends on US1 and harness support from US3.
- T018–T020 complete final regressions and reporting.

## Parallel opportunities

- T001 and T002 may run independently.
- T004 and T005 may run independently after setup.
- Source changes remain sequential while `IslandWaypointWalker` or adapter files are owned by the other session. Tests must follow confirmed operation semantics.

## Implementation Strategy

1. Record baseline and confirm file ownership handoff.
2. Reuse the operation implementation in `IslandWaypointWalker`; fill only missing acceptance behavior.
3. Validate movement and cancellation for one NPC, then regions/approaches, then three simultaneous NPCs.
4. Run regressions and complete the evidence matrix. Do not report blocked or unrun cases as passed.
