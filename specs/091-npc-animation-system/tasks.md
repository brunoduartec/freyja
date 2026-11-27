# Tasks: NPC Animation System

**Input**: `spec.md`, `plan.md`, `research.md`, `data-model.md`, `contracts/npc-animation-contract.md`  
**User approval**: spec approved before implementation; follow-up authorizes a final free Asset Store search.

## Phase 1: Baseline and shared locomotion (US1)

- [x] T001 Record branch, Unity version, feature base commit and project package/test assembly state in `validacao.md`; Unity baseline suites recorded as 40 Edit Mode and 8 Play Mode passing.
- [x] T002 Inventory resident prefab avatars, Animator assignments/controllers, clips, rig types, loops and root motion in `validacao.md` using the real Unity Editor.
- [x] T003 Add and run Edit Mode tests for duplicate/missing binding and actual velocity-to-speed resolution in `Freyja/Assets/_Project/NpcAnimation091/Tests/EditMode/`; all 40 project Edit Mode tests pass, XML in `evidence/editmode-results.xml`.
- [x] T004 Implement shared `NpcAnimationProfile` and locomotion driver in `Freyja/Assets/_Project/NpcAnimation091/Runtime/` and `Freyja/Assets/_Project/Scripts/Prototype/`.
- [x] T005 Update `Freyja/Assets/_Project/Scripts/Prototype/IslandWaypointWalker.cs` so animation writes do not compete with the shared driver; preserve Navigation API and player/protagonist assets.
- [x] T006 Create the shared locomotion controller/profile with Editor APIs and verified existing in-place idle/walk/run clips under `Freyja/Assets/_Project/Art/NpcAnimation091/`; also bind the verified Quaternius sitting lifecycle.
- [x] T007 Compile in Unity and verify stopped, walking and explicit run behavior with actual walker velocity. Idle and walk camera evidence is saved; run was seen live at 2.85–2.92 m/s and the current locomotion PNG captures walk.

## Phase 2: Activity operations and resource-safe interactions (US2)

- [x] T008 Add and run Edit Mode tests for exact animation operation identity, stale poll/cancel, idempotence and missing animation IDs in `Freyja/Assets/_Project/NpcAnimation091/Tests/EditMode/`; all 40 project Edit Mode tests pass.
- [x] T009 Update `Freyja/Assets/_Project/Scripts/NpcScheduling/Unity/NpcAnimatorBindings.cs` to consume shared profile bindings and track the exact Activity Operation through real state entry/completion.
- [x] T010 Update `Freyja/Assets/_Project/Scripts/NpcScheduling/Unity/NpcUnityActivityPorts.cs` for exact cancellation, arrival-before-alignment, occupied claim tracking and safe restoration ordering.
- [x] T011 Add focused regression tests for terminal cleanup retry while `stand_up` is Running; Activity Core unchanged because its public retry path already polls cleanup.
- [x] T012 Add and run Play Mode tests for two distinct slots, separate owners and reservation failure using real Smart Object components; all 8 project Play Mode tests pass, XML in `evidence/playmode-results.xml`.
- [x] T013 Import-audit the Quaternius Humanoid `Sitting_Enter`, `Sitting_Idle_Loop` and `Sitting_Exit` clips and bind them to real semantic actions. The seated pose is functional but visibly high/reads as falling; mark for later review and preserve current facing.

## Phase 3: Isolated demo and final validation (US3)

- [x] T014 Create Editor-authored `Freyja/Assets/_Project/Scenes/NpcAnimation_Demo.unity` using three resident prefabs, one two-slot Smart Object, real NavMesh, shared controller/profile and a test-only harness.
- [ ] T015 Play Mode checks recorded for idle/walk/run, two-seat reservation, third-NPC rejection, seated cancellation and normal schedule end. Validate movement-phase cancellation, pause/resume and return to a new Activity intent; XML and screenshots are in `evidence/`.
- [x] T016 Restore documented Asset Store candidates and compatibility findings in `validacao.md`; no purchase or import.
- [x] T017 Complete AN-01–AN-17 results, animation catalog, setup, limitations (including poor sitting pose), and `npc-world-integration` contract in `specs/091-npc-animation-system/validacao.md` and `quickstart.md`.
- [x] T018 Review `git diff --check`, feature-worktree status, all generated Unity `.meta` files and preserved original checkout changes; create durable recovery checkpoint. Evidence: diff check passes; no new Unity asset/source is missing `.meta`; the original `086-npc-schedule-core` checkout retains its pre-existing changes; refreshed patch and untracked-file checkpoint are saved outside the repository.

## Dependency order

## Independent review follow-up

- [ ] T019 Restore legacy walker animation behavior for existing actors without `NpcAnimationDriver`, and verify both legacy and shared-driver paths in Unity.
- [ ] T020 The added return-to-approach changes the existing recipe sequence and conflicts with the original spec; effects are documented in `validacao.md`, but reconciliation and consumer validation remain pending.

T001–T002 precede asset authoring. T003 drives T004–T007. T008 and T011 drive adapter changes T009–T010; Smart Object demonstration T012 depends on adapter contracts. T013 may block physical sit acceptance but does not block shipping explicit missing-binding behavior. T014 follows verified assets and runtime scripts. T015–T018 are final verification/documentation tasks. No merge/commit is part of this delivery.
