# Tasks: NPC Integration Validation

**Input**: Design documents from `specs/093-npc-integration-validation/`  
**Prerequisites**: `spec.md`, `plan.md`, `research.md`, `quickstart.md`, and `source-requirements.md`

## Phase 1: Baseline

**Purpose**: Establish actual project and evidence state before interpreting test outcomes.

- [X] T001 [US1] Record initial branch, HEAD, Git status, Unity CLI status/version, active scene, and existing untracked changes in `npc-integration-validation-report.md`. **Evidence:** root report, Baseline.
- [X] T002 [US1] Map prior W01–W19 evidence and current adapter/scene/JSON/test paths to V01–V35 in `npc-integration-validation-report.md`. **Evidence:** root report, full V01–V35 matrix.
- [X] T003 [US1] Inventory and run existing relevant .NET, Unity Edit Mode, and Unity Play Mode tests; preserve concise output evidence under `specs/093-npc-integration-validation/evidence/`. **Evidence:** `evidence/test-summary.md`; suites absent for some systems are listed as partial.

**Independent Test**: Baseline identifies actual systems, fixtures, scenes, tools, test counts, and historical evidence limits.

## Phase 2: Production Play Mode integration

**Purpose**: Prove schedule-driven physical behavior with real scene objects and the game clock.

- [X] T004 [US2] Verify the three NPC identities, individual source JSON routines, production hosts, clock and real world references in `npc-integration-validation-report.md`. **Evidence:** root report, Baseline and V03–V04.
- [X] T005 [US2] Exercise supported physical idle, wander, sit, navigation, animation, and resource lifecycle through the production clock/host; capture concise evidence under `specs/093-npc-integration-validation/evidence/`. **Evidence:** `evidence/physical-playmode.md`; visual close-up remains partial.
- [X] T006 [US2] Exercise or explicitly classify contention, schedule/time changes, invalid/unreachable targets, stalls, cancellation phases, object/NPC lifecycle, stale callbacks, pause/time scale, and cleanup in `npc-integration-validation-report.md`. **Evidence:** root report matrix; unrun cases remain PARTIAL/NOT RUN.

**Independent Test**: Observed runtime state and correlated evidence prove each physical stage claimed; unavailable hooks or fixtures are marked `BLOCKED`/`NOT RUN`.

## Phase 3: Duration and performance

**Purpose**: Assess repeated logical behavior, feasible physical duration, and measured runtime cost.

- [X] T007 [US3] Run the existing seven-day logical harness twice with the same deterministic configuration; record outcomes and resource accounting in `npc-integration-validation-report.md`. **Evidence:** matching SHA-256 and counters in root report/test summary.
- [X] T008 [US3] Run the longest practical schedule-driven physical session without substituting clock jumps for physical completion; record actual days and duration in `npc-integration-validation-report.md`. **Evidence:** `evidence/physical-playmode.md`; >11 in-game days, physical wall duration not instrumented.
- [X] T009 [US3] Capture comparable idle, movement, and contention Unity Profiler/diagnostic samples and available counters under `specs/093-npc-integration-validation/evidence/`. **Evidence:** `evidence/profiler-summary.md`; nav request counters unavailable.

**Independent Test**: Logical and physical run lengths and profiler data are independently labelled and reproducible.

## Phase 4: Final assessment and report

**Purpose**: Verify defects were fixed with regressions and provide a complete readiness decision.

- [X] T010 [US1] For every reproduced defect, coordinate the minimum approved correction and focused regression with the Implementer; rerun relevant suites and affected Play Mode cases. **Evidence:** GameObject disable/reativação residual reproduzido; reset mínimo controller/adapter; regressão PlayMode 1/1, PlayMode completo 9/9, EditMode 40/40 e smoke físico passaram. Destroy físico em movimento segue parcial em V27.
- [X] T011 [US3] Recheck protagonist input/camera/interaction/day-night behavior and complete V01–V35 statuses in `npc-integration-validation-report.md`. **Evidence:** clock/camera context checked; input and interaction remain PARTIAL as they were not directly tested.
- [X] T012 [US3] Deliver the root-level report with evidence links, unresolved limits, measured performance, and a clear stability recommendation for `npc-debug-tools`. **Evidence:** `npc-integration-validation-report.md`; feature status remains PARCIAL.

**Independent Test**: Final report has a status, observed result, and evidence path or explicit reason for every V01–V35 case.

## Dependencies & Execution Order

- T001–T003 establish baseline and precede physical testing.
- T004–T006 depend on V01/V02 establishing a runnable project and test baseline.
- T007–T009 follow the smoke/fault pass so duration and performance use known fixtures.
- T010 follows each confirmed defect and precedes final assessment.
- T011–T012 require final regression evidence.
- No tasks are marked parallel: Unity MCP/editor work is serialized to protect the shared Editor session and scene.

## Implementation Strategy

This feature is validation-first. Complete baseline, then physical integration, then long runs and profiler. Only introduce a production code change after reproduction; assign it to the Implementer and add a regression at the narrowest existing test seam. Finish with an honest matrix even when some cases remain blocked.
