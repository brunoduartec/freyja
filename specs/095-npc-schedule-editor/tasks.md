---
description: "Dependency-ordered implementation tasks for NPC Schedule Editor"
---

# Tasks: NPC Schedule Editor

**Input**: Design artifacts from `specs/095-npc-schedule-editor/`.

**Prerequisites**: `spec.md`, `plan.md`, `research.md`, `data-model.md`, `contracts/editor-data-contract.md`, `quickstart.md`.

**Tests**: Explicitly requested; run harness and Unity MCP/editor validation and record evidence.

## Phase 1: Setup

**Purpose**: Capture pre-change state and prepare the Editor feature folder.

- [x] T001 Record the pre-feature hashes of the three `npcs/world_integration/*.json` files, Unity MCP Editor/Console status at baseline, and existing harness results in `specs/095-npc-schedule-editor/evidence/baseline.md`. Evidence: `evidence/baseline.md`; initial MCP state was unreachable and hashes/harness captured before code changes.
- [x] T002 Create `Freyja/Assets/_Project/Editor/NpcScheduleEditor/` and let Unity generate `.meta` files for added source files; do not modify scenes or packages. Evidence: folder and `.meta` pairs present; no scene/package edits.

## Phase 2: Foundational

**Purpose**: Provide data discovery, draft state, validation, and safe persistence shared by all stories.

- [x] T003 [P] Implement canonical JSON discovery, source path safety, provider-reference classification, and one-shot scene destination evidence in `Freyja/Assets/_Project/Editor/NpcScheduleEditor/NpcScheduleEditorRepository.cs` and `NpcScheduleEditorDestinations.cs`. Evidence: source review; scene data snapshot now refreshes outside `OnGUI`.
- [x] T004 Implement parsed document, dirty state, entry selection, bounded undo/redo, conflict state, and Save/Discard/Cancel document switching in `Freyja/Assets/_Project/Editor/NpcScheduleEditor/NpcScheduleEditorSession.cs`. Evidence: source review; Editor interaction remains unverified.
- [x] T005 Implement strict draft validation and resolver preview using `ScheduleDataLoader`, `ActivityRecipeLoader`, and `ScheduleResolver` in `Freyja/Assets/_Project/Editor/NpcScheduleEditor/NpcScheduleEditorPreview.cs`. Evidence: source review; preview/flags UI remains unverified.
- [x] T006 Implement fingerprint verification, exact candidate validation, single-file temporary replace/rollback, targeted reimport, and failure-safe draft retention in `Freyja/Assets/_Project/Editor/NpcScheduleEditor/NpcScheduleEditorRepository.cs`. Evidence: MCP Save probe exercised failed-write rollback and draft retention; failure specifically during AssetDatabase import remains untested.
- [ ] T007 Exercise strict parse/schema rejection, interval validation, candidate validation, source conflict, failed write/import recovery, and semantic no-op round-trip through Editor test harnesses or Unity MCP; record actual method and results in `specs/095-npc-schedule-editor/evidence/validation-matrix.md`. PARTIAL: Unity MCP probes passed malformed JSON, unsupported `schemaVersion: 99`, invalid time, no-op save, external conflict preservation, and failed-write rollback; import-failure recovery remains untested.

**Checkpoint**: Existing source JSON is discoverable, draft-only operations do not write, preview is the real core, and the save path is conflict-safe.

## Phase 3: User Story 1 - Edit and run an NPC schedule (Priority: P1)

**Goal**: The developer edits one of the active NPCs visually, saves its canonical JSON, and observes the change in Play Mode.

**Independent Test**: Open the actual menu, edit/save a valid block from the UI, reopen the source, run Play Mode and observe physical behavior plus Debug Monitor state.

- [x] T008 Implement `Freyja > NPC > Schedule Editor`, active/demo NPC selection, source summary, search/refresh, list/form state, status messages, and Play Mode read-only controls in `Freyja/Assets/_Project/Editor/NpcScheduleEditor/NpcScheduleEditorWindow.cs`. Evidence: Unity MCP opened menu and captured `evidence/editor-window.png`; field interaction remains pending.
- [x] T009 Implement the 24-hour timeline drawing, scale/tick labels, entry blocks, gaps, overlap visibility, selection, origin and activity labels/tooltips in `Freyja/Assets/_Project/Editor/NpcScheduleEditor/NpcScheduleEditorWindow.cs`. Evidence: `evidence/editor-window.png` shows tick labels and five effective blocks; gap/overlap marker interaction remains untested.
- [x] T010 Implement entry add/edit/duplicate/remove with exact time fields, catalog-backed activity selector, compatible symbolic destination options, dynamic IDs, validation feedback, and no implicit writes in `Freyja/Assets/_Project/Editor/NpcScheduleEditor/NpcScheduleEditorWindow.cs`. Evidence: source review; UI cases not exercised.
- [ ] T011 Validate the UI flow against the three active scene documents and exercise invalid interval, overlap, targetless, and catalog-only activity cases; save/reopen one valid schedule and record the JSON diff plus actual UI evidence under `specs/095-npc-schedule-editor/evidence/`. PARTIAL: Unity MCP probes validated all three runtime documents, rejected overlap/targetless drafts, and discovered an in-memory catalog activity; control interactions and visual save/reopen remain blocked by macOS CUA.

## Phase 4: User Story 2 - Reuse schedules and author supported conditions (Priority: P2)

**Goal**: Edit existing templates, individual inheritance operations, duplicate schedules, and supported conditional overrides without cross-document data loss.

**Independent Test**: Edit and validate a template/dependent, duplicate a schedule under a new NPC ID, and preview a supported override with matching and non-matching contexts.

- [x] T012 Implement template/override navigation, supported document fields, dependency impact list, per-document validation, and guarded shared-document edits in `Freyja/Assets/_Project/Editor/NpcScheduleEditor/NpcScheduleEditorWindow.cs` and `NpcScheduleEditorRepository.cs`. Evidence: source review; dependency flows not exercised.
- [x] T013 Implement UI editing and serialization of supported NPC template operations and override `priority`, `days`, `weekdays`, `weather`, `flags`, and entries without modifying unrelated documents in `Freyja/Assets/_Project/Editor/NpcScheduleEditor/`. Evidence: source review; matching/non-matching condition previews not exercised.
- [x] T014 Implement schedule duplication preserving symbolic aliases and entry semantics while assigning a unique NPC/document identity and preventing source/template mutation in `NpcScheduleEditorSession.cs` and `NpcScheduleEditorRepository.cs`. Evidence: source review; duplicate save/import not exercised.
- [ ] T015 Verify template dependent validation, remove/replace/add inheritance semantics, duplicate isolation, matching/non-matching conditions, tie-break behavior, and dynamic catalog/recipe status; record evidence in `specs/095-npc-schedule-editor/evidence/validation-matrix.md`. PARTIAL: Unity MCP candidate probe confirmed a future catalog activity is discovered dynamically and its missing recipe is detected; template/condition UI, tie-break, duplicate isolation, and dropdown behavior remain untested (CUA locked).

## Phase 5: User Story 3 - Protect and diagnose authoring changes (Priority: P1)

**Goal**: Compare draft/saved intent with observed execution and protect changes through undo, external conflict handling, and actionable diagnostics.

**Independent Test**: Exercise undo/redo/revert, concurrent external edit, invalid file, and compare a saved schedule with the existing NPC Debug Monitor in Play Mode.

- [x] T016 Add a focused open/select entry point to `Freyja/Assets/_Project/Editor/NpcDebug/NpcDebugMonitorWindow.cs` and use existing `NpcDebugDiagnostics` snapshots from the schedule editor; do not create a duplicate monitor or event store. Evidence: source review; runtime focus not exercised.
- [x] T017 Implement authored/resolver/runtime comparison, preview context controls, next boundary when exposed by the real resolver, diagnosis copy without absolute paths, and clear predicted-versus-observed labels in `NpcScheduleEditorWindow.cs`. Evidence: source review; flags, comparison and copied diagnosis not exercised.
- [ ] T018 Verify dirty-switch prompts, undo/redo/revert, malformed/unsupported JSON safe mode, external edit/delete conflict, failed IO/import recovery, Edit/Play Mode policy, and monitor focus for the selected NPC; record evidence. PARTIAL: Unity MCP probes passed undo/redo/revert, malformed session read-only state, core rejection of `schemaVersion: 99`, delete-conflict detection without file recreation, and `ReplaceDraft` refusing mutation while the Editor is in Play Mode. UI safe mode, dirty-switch prompt, import recovery, and Debug Monitor focus remain untested (CUA is blocked). External modification is covered by SE27.

## Phase 6: Polish and Cross-Cutting Validation

**Purpose**: Prove one real UI change affects physical runtime behavior, restore test data, and deliver the handoff.

- [x] T019 Run `dotnet run --project Tools/NpcScheduleSimulator -- --validate` and `--test`; compare with the baseline and investigate the first relevant failure. Evidence: `evidence/validation-matrix.md`; PASS, 174 assertions, same three baseline warnings.
- [x] T020 Recompile and inspect Console through Unity MCP; open and inspect the final EditorWindow, then run Edit Mode checks supported by the current project. Evidence: recompile passed, zero active errors and one warning with no message in the latest ground truth snapshot, menu/capture succeeded, 40/40 existing Edit Mode checks passed; see `evidence/validation-matrix.md`.
- [ ] T021 Make one UI-authored edit to an active `world_*` schedule, prove the JSON changed, re-enter Play Mode, capture physical NPC behavior and matching Debug Monitor snapshot, then restore that JSON and verify all three original hashes. BLOCKED: macOS CUA reports the session locked; no visual edit or physical behavior proof is claimed.
- [ ] T022 Verify the other two schedules and protagonist/camera state are unchanged; explicitly classify SE01–SE34 as PASS/FAIL/BLOCKED/NOT RUN with evidence type and path in `specs/095-npc-schedule-editor/evidence/validation-matrix.md`. PARTIAL: MCP validated all three active runtime documents and unchanged hashes; protagonist/camera verification remains blocked pending interactive Play Mode.
- [ ] T023 Write the required user guide, before/after example, results and `npc-activity-library` handoff in `specs/095-npc-schedule-editor/report.md`; summarize changed source/assets and unrun checks. PARTIAL: report/guide/handoff and actual pre-change JSON excerpt are recorded; post-change UI example and Play Mode evidence await the unlock.

## Dependencies

- Setup precedes Foundational; T003–T006 precede all user stories.
- User Story 1 depends on the foundational repository/session/preview and safe-save tasks.
- User Story 2 depends on User Story 1's document editor and validated persistence.
- User Story 3's monitor integration depends on existing capture APIs; final physical validation requires User Story 1 and the final Editor build.
- Polish starts after all three stories.

## Parallel Opportunities

- T003 (discovery/destination evidence) and T005 (resolver preview) can be developed independently after source contracts are confirmed.
- T008 (window shell) can begin after the session/repository contracts stabilize; T016 is isolated to the existing Debug Monitor file and should be scheduled separately from window edits.
- Avoid parallel edits to files owned by one implementer. The Game Designer review is complete; the Implementer owns editor code, then the Validator reviews independently.

## Implementation Strategy

1. Complete setup and shared data/session/validation/safe-save foundation.
2. Deliver Story 1 as the MVP and demonstrate visual save plus real Play Mode effect.
3. Add existing templates and conditional overrides, respecting what the core actually supports.
4. Integrate the Debug Monitor, then run final regression, physical evidence, restore temporary data, and report limitations.

## Notes

- `[P]` denotes independent file ownership only; this feature should be implemented by one Implementer agent at a time.
- Do not commit, merge, push, reset, stash, or clean the user's untracked attachments or other work.
- Any unconfirmed capability is reported as unverified; no test can be marked PASS without evidence.
