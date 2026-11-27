# Tasks: NPC Unity Integration Code

**Branch**: `090-npc-unity-integration-code`
**Input**: `spec.md`, `plan.md`, `research.md`, `data-model.md`, `contracts/unity-integration-contract.md`, `quickstart.md`

## Phase 1: Setup

- [x] T001 Confirm active branch, `main` base, feature folder and Unity project version in this worktree; record current `git status` in `specs/090-npc-unity-integration-code/validation.md`.
- [x] T002 [P] Inspect current assembly layout, `IslandWaypointWalker`, Smart Object APIs and data-loader boundaries; record allowed source paths in `specs/090-npc-unity-integration-code/research.md`.

## Phase 2: Foundational

- [x] T003 Extract or expose the shared named-document schedule loader for Unity TextAssets and `Tools/NpcScheduleSimulator` without duplicating parsing/expansion semantics in `Freyja/Assets/_Project/Scripts/NpcScheduling/Core/` and the harness.
- [x] T004 Expose shared recipe parsing/validation for both runtime TextAssets and the existing harness in `Freyja/Assets/_Project/Scripts/NpcScheduling/Execution/`.
- [x] T005 Reuse the existing harness source globs (no project-file change required); preserve Unity-free Core/Execution and one-way dependencies in `Tools/NpcScheduleSimulator/NpcScheduleSimulator.csproj`.

## Phase 3: User Story 1 — Schedule-driven NPC host (P1)

**Goal**: Feed current game snapshots and existing schedule intent to one activity coordinator per NPC, with strict runtime data loading and teardown.

**Independent Test**: Existing .NET tests pass; the host loads the same data/recipes from explicit TextAsset references and does not fabricate world input when no real provider is configured.

- [x] T006 [P] [US1] Create explicit runtime data provider for existing schedule/recipe TextAssets and diagnostics in `Freyja/Assets/_Project/Scripts/NpcScheduling/Unity/NpcScheduleTextAssetProvider.cs`.
- [x] T007 [US1] Implement one per-NPC host that owns resolver/coordinator lifecycle, caches schedule boundary/context, accepts current world snapshots, cancels synchronously and retains residual cleanup ownership across disable/re-enable in `Freyja/Assets/_Project/Scripts/NpcScheduling/Unity/NpcActivityController.cs`.
- [x] T008 [US1] Implement transition/failure diagnostics and stable reason reporting without per-frame logs in the host event sink and deduplicated failure logger in `NpcActivityController.cs`.
- [x] T009 [US1] Document the absent real world clock/context provider, pause policy and host input contract in `specs/090-npc-unity-integration-code/contracts/unity-integration-contract.md`.

## Phase 4: User Story 2 — Authored locations and movement (P1)

**Goal**: Resolve only explicit symbolic point and region references and use one existing movement owner per NPC.

**Independent Test**: Review unique lifecycle IDs, deterministic configured waypoints, one operation begin, complete-path arrival, fail-closed errors and stale operation isolation.

- [x] T010 [P] [US2] Add lifecycle-managed location/region point registration with duplicate/disabled/destroyed reference diagnostics in `Freyja/Assets/_Project/Scripts/NpcScheduling/Unity/NpcLocationRegistry.cs`.
- [x] T011 [US2] Implement operation-owned movement polling/cancel through existing `IslandWaypointWalker`, with complete path, arrival, timeout and failure checks in `Freyja/Assets/_Project/Scripts/NpcScheduling/Unity/NpcUnityActivityPorts.cs`.
- [x] T012 [US2] Implement deterministic `FindWaypoints` and symbolic point resolution without coordinate/name fallback or frame-global searches in `Freyja/Assets/_Project/Scripts/NpcScheduling/Unity/NpcUnityActivityPorts.cs`.

## Phase 5: User Story 3 — Smart Object and Animator ports (P1)

**Goal**: Reserve/approach/align/occupy/sit/stand/release using exact existing ownership and explicit animation bindings.

**Independent Test**: Static review of exact handle ownership, reservation races, token scoping, alignment tolerance, verified animation completion and residual cleanup. Existing core tests exercise retry/fallback policy.

- [x] T013 [US3] Adapt deterministic `SmartObjectRegistry.FindCandidates`/atomic `TryReserve` to opaque per-execution port tokens that retain exact `ReservationHandle` ownership in `Freyja/Assets/_Project/Scripts/NpcScheduling/Unity/NpcUnityActivityPorts.cs`.
- [x] T014 [US3] Implement reservation validity, occupancy, idempotent exact release, object loss and stale execution checks in `Freyja/Assets/_Project/Scripts/NpcScheduling/Unity/NpcUnityActivityPorts.cs`.
- [x] T015 [P] [US3] Add explicit Animator ID/state binding validation and bounded animation completion polling for recipe animation IDs in `Freyja/Assets/_Project/Scripts/NpcScheduling/Unity/NpcAnimatorBindings.cs`.
- [x] T016 [US3] Implement bounded alignment and retain the exact claim through pollable `stand_up` cleanup; preserve/retry residual ownership after failure, timeout and host disable/re-enable. Evidence: 153 execution assertions and seven-day simulation in `validation.md`.

## Phase 6: User Story 4 — Physical validation preparation (P2)

**Goal**: Prepare the first test-only vertical slice and its repeatable Unity checklist without changing assets/scenes.

**Independent Test**: Checklist names exact new scripts/fields, prerequisites and expected sequence; all physical steps remain unchecked.

- [x] T017 [P] [US4] Document data-provider, host, location, movement, seat and animation setup by exact fields and sequence in `specs/090-npc-unity-integration-code/integration-checklist.md`.
- [x] T018 [US4] Add a disposable runtime-schema test-NPC schedule for explicit TextAsset assignment with `idle`, `wander` and `sit`; verify shared loader/resolver and document field setup. Evidence: `Data/test-fixtures/npc_activity_execution_test.json`, loader assertions, and `integration-checklist.md`.

## Phase 7: Polish and validation

- [x] T019 Existing portable harness assertions exercise schedule load/resolve and recipe load through the shared loaders; no separate duplicated test layer was added.
- [x] T020 Run .NET build, harness tests, data/recipe validation and seven-day simulation; exact results and limits are in `validation.md`.
- [x] T021 Review `git diff --check`, asmdef/dependency direction, no forbidden asset changes, token/operation ownership and final Git status; evidence is in `validation.md`.

## Dependencies

- T003–T005 are foundational for US1 runtime data and all existing shared-source validation.
- US1 (T006–T009) is required before any NPC can use the ports.
- US2 (T010–T012) and US3 (T013–T016) depend on the existing host/ports contracts and can be developed sequentially within the same port file; do not parallel-edit `NpcUnityActivityPorts.cs`.
- US4 documents the final Inspector/Play Mode setup after implementation surfaces are known; the runtime fixture is verified by the shared loader and resolver.
- T019–T021 finish after all implementation changes.

## MVP Strategy

First complete shared data boundaries and the host input/lifecycle. Then implement authored location and one movement operation. Add the Smart Object and Animator port only after exact ownership/failure behavior is clear. Finish with tests, code review and an explicitly pending Unity checklist. Unity build/Play Mode are not claimed in this environment.

## Phase 8: Convergence

- [x] T022 [US3] Replace synchronous pose restoration with bounded, pollable `stand_up` cleanup; retain the exact Smart Object claim until the mapped Animator state completes and preserve/retry residual ownership on rejection or timeout. Harness covers confirmed completion, failure, timeout of both cleanup and interrupted animation tickets, retry, release ordering and blocked replacement per US3/AC3, FR-007/FR-008.
- [x] T023 [US4] Add and verify a disposable runtime-schema test-NPC JSON for `idle`, `wander`, and `sit`; document explicit TextAsset assignment. Official NPC schedules, scenes, prefabs and `.meta` files remain untouched per US4/AC1, FR-003.

## Phase 9: Unity Physical Integration

- [ ] T024 [US1] Locate the existing production world clock/context provider and document its day/minute/weekday, pause, fast-forward and reset semantics; wire its current snapshots into `NpcActivityController.TickFromWorld` only if that provider exists. If none exists, leave the host fail-closed and record the external dependency; do not invent a second clock per US1/AC1–2 and FR-002/FR-009.
- [ ] T025 [US4] Open the project in Unity `6000.3.25f1`, import `Data/test-fixtures/npc_activity_execution_test.json` as a TextAsset, assign it to `NpcScheduleTextAssetProvider.npcSchedules[]`, and compile/check Console plus Editor/Player data loading. Preserve official NPC assets and report any Unity-only compile errors per US4/AC2 and FR-003/FR-010/FR-012.
- [ ] T026 [US4] Configure a disposable, unsaved test-only NPC setup using the runtime host, provider, existing movement owner and `SmartObjectReservationOwner`; assign real available symbolic locations/region waypoints, a complete NavMesh path and a compatible two-slot `sit` Smart Object. Do not modify the main scene, official NPCs, prefabs or commit test-only scene assets; record unavailable authored references as blockers per US2/US3/US4, FR-004–006/FR-010.
- [ ] T027 [US3] Inspect the actual Animator controller and bind `sit_down` and `stand_up` only to verified states, layers and completion behavior; exercise missing/rejected/looping/timeout cases. If suitable states or clips do not exist, stop physical `sit` validation and record the required animation asset work without fabricating IDs per US3/AC1/3 and FR-007.
- [ ] T028 [US1–US4] Run Unity Edit Mode checks where supported, then Play Mode `idle → wander → sit → wander → idle`; verify path arrival/alignment, exact slot ownership, stand-up completion before release, schedule changes, cancellation, failure/retry, host disable/re-enable, stale-ticket isolation and zero residual claims. Capture Console/test results and mark each physical checklist item only from observed Editor evidence per US1–US4, ACs and SC-003/SC-004.
- [ ] T029 [US4] Record Unity version, project path, provider and asset references used, Edit/Play Mode outcomes, remaining external blockers and evidence paths in `validation.md`; rerun .NET build, all harness tests, data validation, seven-day simulation and `git diff --check` after any code change per FR-012/SC-001/SC-004.
