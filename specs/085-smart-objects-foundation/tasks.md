# Tasks: Smart Objects Foundation

**Input**: Design documents in `specs/085-smart-objects-foundation/`  
**Prerequisites**: `spec.md` approved; `plan.md`, `research.md`, `data-model.md`, `quickstart.md`  
**Tests**: Edit Mode and Play Mode tests are required by the specification.

## Phase 1: Setup

- [x] T001 Record existing movement, navigation data, resident prefabs, dock and test assembly references in `specs/085-smart-objects-foundation/research.md` and `specs/085-smart-objects-foundation/plan.md`.
- [x] T002 Create the feature source folders and separate runtime, editor, Edit Mode and Play Mode assembly definitions under `Freyja/Assets/_Project/SmartObjects085/`.

## Phase 2: Foundational

- [x] T003 [P] Create the editable provisional bench source `Source/Blender/SmartObjects085/BenchProvisional.blend` and export `Freyja/Assets/_Project/Art/SmartObjects085/BenchProvisional.fbx` with meter scale, bottom pivot and replaceable materials.
- [x] T004 Define the public authoring and reservation contracts in `Freyja/Assets/_Project/SmartObjects085/Runtime/` and update `specs/085-smart-objects-foundation/data-model.md` to match any implementation-level adjustment.
- [ ] T005 Configure Unity Pipeline Editor access for `Freyja/` and verify the scene/editing commands available through `unity command` without updating packages or CLI tooling.

## Phase 3: User Story 1 — Configure Smart Objects in the Inspector (P1)

**Goal**: Add a reusable two-slot bench prefab whose identities, activities, transforms and diagnostics are configurable without consumer-specific C#.

**Independent Test**: Duplicate and edit the prefab; verify distinct object IDs, stable slot IDs, valid approach/pose references and visible slot Gizmos.

- [x] T006 [US1] Implement serialized Smart Object/slot configuration and stable ID validation/repair in `Freyja/Assets/_Project/SmartObjects085/Runtime/SmartObject.cs` and `InteractionSlot.cs`.
- [x] T007 [US1] Implement Inspector diagnostics and Scene View Gizmos for IDs, references, activities, transforms, orientation and slot states in `Freyja/Assets/_Project/SmartObjects085/Editor/SmartObjectEditor.cs`.
- [ ] T008 [US1] Configure the two-slot `sit` bench prefab with separate visual, collider, approach and pose children in `Freyja/Assets/_Project/Prefabs/SmartObjects085/BenchProvisional.prefab`.
- [x] T009 [US1] Add Edit Mode tests for generated/duplicate IDs and prefab slot configuration in `Freyja/Assets/_Project/SmartObjects085/Tests/EditMode/SmartObjectAuthoringTests.cs` (round-trip de prefab, IDs estáveis e referências de approach/pose; execução pendente).

## Phase 4: User Story 2 — Discover and reserve compatible slots (P1)

**Goal**: Discover active Smart Objects and make owner-scoped, stale-safe slot claims without storing runtime state in assets.

**Independent Test**: Run Edit/Play Mode tests for discovery, deterministic candidate order, exclusivity, occupancy, stale handles and cleanup.

- [x] T010 [US2] Implement lifecycle-driven registration, ID/region/activity lookup, deterministic candidate sorting and runtime-state reset in `Freyja/Assets/_Project/SmartObjects085/Runtime/SmartObjectRegistry.cs`.
- [x] T011 [US2] Implement atomic main-thread reserve/occupy/release/validity transitions with immutable owner/token handles in `Freyja/Assets/_Project/SmartObjects085/Runtime/ReservationHandle.cs` and `SmartObjectRegistry.cs`.
- [x] T012 [US2] Implement consumer handle tracking and owner disable/destroy cleanup in `Freyja/Assets/_Project/SmartObjects085/Runtime/SmartObjectReservationOwner.cs`.
- [x] T013 [US2] Add Edit Mode registry tests for registration, lookup, enabled filtering, uniqueness, tie-breaking, incompatible/disabled slots, contention, occupancy, stale/foreign handles and repeated release in `Freyja/Assets/_Project/SmartObjects085/Tests/EditMode/SmartObjectRegistryTests.cs`.
- [x] T014 [US2] Add Play Mode lifecycle tests for object/slot/owner disable and destruction plus reactivation and Play Mode reset in `Freyja/Assets/_Project/SmartObjects085/Tests/PlayMode/SmartObjectLifecycleTests.cs`.

## Phase 5: User Story 3 — Demonstrate approach, fallback and release (P2)

**Goal**: Show three requests using two close benches, with existing residents and movement where valid, plus optional future-capability examples.

**Independent Test**: Play the demo; observe distinct reservations, arrival before pose alignment, fallback to bench B, cancel/release and unreachable-path cleanup.

- [x] T015 [US3] Implement the bounded three-agent reservation/movement harness, cancel/release action and unreachable timeout in `Freyja/Assets/_Project/Scripts/Prototype/SmartObjectDemoHarness.cs` (Assembly-CSharp, so it can reuse `IslandWaypointWalker`).
- [ ] T016 [US3] Create `Freyja/Assets/_Project/Scenes/SmartObjects_Demo.unity` from the approved small demonstration layout, reusing the existing island NavMesh, resident prefabs and dock assets without changing the main scene.
- [ ] T017 [US3] Place two independent bench prefab instances and three residents in `Freyja/Assets/_Project/Scenes/SmartObjects_Demo.unity`; configure deterministic fallback and validate approach/pose points against `IlhaWaypointNavigation.asset`.
- [ ] T018 [US3] Configure compatible future-only slots on the existing dock asset in `Freyja/Assets/_Project/Scenes/SmartObjects_Demo.unity` when points are navigable; record the absence of a well asset rather than creating one.
- [ ] T019 [US3] Add Play Mode integration tests for candidate fallback, arrival-before-occupancy, cancellation, invalid paths and reservation cleanup in `Freyja/Assets/_Project/SmartObjects085/Tests/PlayMode/SmartObjectDemoTests.cs`.
- [ ] T020 [US3] Run the harness in Play Mode and capture the agents, benches, Gizmos/slot states and Inspector diagnostics to `specs/085-smart-objects-foundation/evidence/`.

## Phase 6: Polish and Cross-Cutting Validation

- [ ] T021 Run feature Edit Mode and Play Mode suites, script recompilation, Console checks and `git diff --check`; record exact commands/results/limitations in `specs/085-smart-objects-foundation/validacao.md`.
- [x] T022 Document prefab authoring, ID repair, slot configuration, demo launch, release controls and future animation/navigation gaps in `specs/085-smart-objects-foundation/quickstart.md`.
- [ ] T023 Update checkboxes in `specs/085-smart-objects-foundation/tasks.md` with evidence and confirm no changes escaped the approved scope.

## Goal extension: chair in the square

- [x] T024 Create a replaceable low-poly chair prefab with collider separated from its visual and one Inspector-configured `sit` slot at `Freyja/Assets/_Project/Prefabs/SmartObjects085/CadeiraProvisoria.prefab`.
- [x] T025 Create isolated `Freyja/Assets/_Project/Scenes/CadeiraSmartObject_Demo.unity`, place the chair at `Largo`, and configure `Elisa Neves` with the existing `IslandWaypointWalker` and reservation owner.
- [x] T026 Apply a humanoid seated pose after arrival and restore standing pose when the handle is released in `Freyja/Assets/_Project/SmartObjects085/Runtime/SmartObjectSeatedPose.cs`.
- [x] T027 Validate Edit Mode (12/12), Play Mode lifecycle (3/3), navigation/occupancy/pose/release in the Editor, and save `specs/085-smart-objects-foundation/evidence/CadeiraSmartObject_Sentada.png`.

## Dependencies

```text
T001 → T002 → T004
T003 → T008
T004 → T006 → T007 → T008 → T009
T004 → T010 → T011 → T012 → T013 → T014
T008 + T012 → T015 → T016 → T017 → T018 → T019 → T020
T009 + T013 + T014 + T019 + T020 → T021 → T022 → T023
T005 is a prerequisite to editor scene authoring and validation (T007–T020).
```

## Parallel Opportunities

- T003 can run alongside T004 after T001; it owns only Blender source and FBX files.
- T006/T007 can split by Runtime vs Editor files after T004.
- T013 and T014 can be authored in parallel after runtime contracts T010–T012 stabilize.
- Scene/prefab authoring begins after the prefab and lifecycle components exist.

## Implementation Strategy

Build and test the authoring prefab first, then the registry/handle core, then the demonstration integration. Keep each increment independently verifiable. Do not begin the next phase until its tests and current Editor Console are understood. A blocked navigation test is reported as blocked; no movement workaround is added.
