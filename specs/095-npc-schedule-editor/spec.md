# Feature Specification: NPC Schedule Editor

**Feature Branch**: `095-npc-schedule-editor`

**Created**: 2026-11-27

**Status**: Draft

**Input**: User request and `.codex-attachments/.../freyja-npc-schedule-editor.md`.

## User Scenarios & Testing

### User Story 1 - Edit and run an NPC schedule (Priority: P1)

As a developer, I can open a visual schedule editor, choose an NPC, adjust its activity, time, and compatible destination, validate and preview the change, then save it to the JSON that the game consumes.

**Why this priority**: This is the smallest complete authoring flow and proves that the editor changes real game behavior rather than maintaining a parallel representation.

**Independent Test**: Select one of the three active world NPCs, make a valid schedule change in the UI, confirm the source file is unchanged before Save, save and reopen it, then observe the NPC follow the changed schedule in Play Mode with the Debug Monitor.

**Acceptance Scenarios**:

1. **Given** Edit Mode and a valid configured NPC, **When** the editor opens, **Then** it shows the NPC's source file, current entries, and a 24-hour timeline without changing any file.
2. **Given** a selected entry, **When** the user edits its time, catalog activity, or compatible symbolic destination, **Then** the draft and timeline update and the real Schedule Core validator reports the result.
3. **Given** a valid draft, **When** the user previews a time/context and saves, **Then** preview uses the real resolver, the canonical JSON is updated, and a later Play Mode run demonstrates the changed NPC behavior.
4. **Given** an invalid overlap or invalid time, **When** the user validates or saves, **Then** the editor identifies the affected entry and blocks the save.

### User Story 2 - Reuse schedules and author supported conditions (Priority: P2)

As a developer, I can reuse existing profession templates, make an individual NPC customization, duplicate a schedule, and edit the Scheduler's supported conditional overrides without accidentally changing shared schedules.

**Why this priority**: Reuse and conditional schedules are part of the existing data model and are needed to author more than the three current world schedules.

**Independent Test**: Duplicate a loaded schedule, edit a template-dependent or individual schedule and one supported override, then validate and preview both matching and non-matching contexts while checking that unrelated source files remain unchanged.

**Acceptance Scenarios**:

1. **Given** a template or NPC document, **When** the user makes a supported change, **Then** the editor presents the effective schedule and its source/origin and preserves the original template unless that template itself was selected for editing.
2. **Given** a supported override, **When** the user changes its priority, conditions, or entries, **Then** validation and preview follow the Scheduler's existing condition matching and tie-breaking rules.
3. **Given** a catalog activity without a runtime recipe, **When** it is selected, **Then** the editor identifies it as catalogued but not currently executable and does not claim physical readiness.

### User Story 3 - Protect and diagnose authoring changes (Priority: P1)

As a developer, I can undo and redo draft edits, detect external file changes, recover from invalid data or failed saves, and compare authored intent with observed runtime activity.

**Why this priority**: Direct JSON authoring must not silently lose edits or overwrite another change, and runtime comparison is necessary to diagnose schedule versus execution differences.

**Independent Test**: Edit a draft, undo and redo, modify the same source outside the editor, attempt Save, and verify that the external file remains intact and the draft is recoverable; compare a saved schedule with its real Debug Monitor snapshot in Play Mode.

**Acceptance Scenarios**:

1. **Given** unsaved draft changes, **When** the user invokes undo, redo, or revert, **Then** only the draft changes and the disk file remains unchanged.
2. **Given** a source file changed after editor load, **When** the user saves, **Then** the editor detects the conflict and prevents silent overwrite while preserving the draft.
3. **Given** malformed or unsupported JSON, **When** the editor opens it, **Then** it reports the source and diagnostic and never replaces the file with defaults.
4. **Given** a running NPC, **When** the user compares authored schedule and runtime state, **Then** the editor uses Debug Monitor data and labels predicted intent separately from observed execution and fallback.

### Edge Cases

- Entries use half-open intervals; a start must be before its end, `24:00` is valid only as an end, and an overnight schedule must be split at midnight.
- Gaps are allowed by the existing resolver and must remain warnings with the actual fallback behavior visible in preview.
- Duplicate IDs, unknown activities, missing/incompatible targets, unsupported schema versions, invalid inheritance operations, and ambiguous location IDs must be diagnosed; unverified destination compatibility must be labeled as unverified.
- Concurrent file changes, deletion/rename, IO failure, failed validation, and Unity import failure must preserve both the disk original and recoverable draft.
- Editor authoring and preview must not reserve Smart Object slots, move NPCs, change animation state, mutate the world clock, or execute activities.
- Runtime data loaded by the active scene must be distinguishable from demo schedules, fixtures, and other JSON that is not currently loaded by that scene.
- While in Play Mode, editing and saving are read-only unless a supported safe reload already exists; a saved edit takes effect on the next supported runtime load.

## Requirements

### Functional Requirements

- **FR-001**: The editor MUST be available at `Freyja > NPC > Schedule Editor` and MUST list configured NPC data with stable IDs, source paths, template association, and whether each record is loaded by the active runtime, demo-only, incomplete, or invalid.
- **FR-002**: The editor MUST show and edit a 24-hour timeline and entry list using the actual Schedule Core interval rules, with explicit gap/overlap diagnostics and selectable entries.
- **FR-003**: Users MUST be able to add, edit, duplicate, and remove entries, including exact `HH:mm` times, catalog activity, and compatible symbolic target, without an implicit save.
- **FR-004**: The editor MUST enumerate activities from the real catalog and distinguish catalog presence from a recipe available to the current executor; future catalog entries MUST appear without editor code changes.
- **FR-005**: Destination choices MUST use IDs known to the real location/Smart Object contracts. The editor MUST distinguish resolved, ambiguous, unavailable, and unverified destinations and MUST NOT replace an incompatible target silently.
- **FR-006**: The editor MUST support the existing template, NPC customization, schedule duplication, and conditional override operations and fields that the real loader/schema supports. It MUST preserve unsupported or unedited data and identify any unsupported editing surface.
- **FR-007**: Validation MUST reuse the real Schedule Core loader/validator and report source file, NPC/template/override, entry, field, and actionable diagnostic. Errors block save; permitted gaps remain warnings.
- **FR-008**: Preview MUST use the real Schedule Resolver against an in-memory draft and show predicted activity, target, origin, condition/priority result, and available next boundary. It MUST NOT execute or mutate runtime/game state.
- **FR-009**: The editor MUST maintain a separate dirty draft and provide undo, redo, revert, and explicit save. Opening, validating, previewing, or reverting MUST NOT write schedule data.
- **FR-010**: Before saving, the editor MUST validate the exact candidate and detect source-file changes since load. Conflicts MUST block silent overwrite, preserve the external file, and keep the draft recoverable.
- **FR-011**: Save and reimport MUST update the canonical JSON data consumed by the runtime, preserve stable IDs and semantic data, avoid unnecessary rewrites, and report IO/import failures without false success.
- **FR-012**: In Play Mode the editor MUST be read-only unless an already-supported safe reload path is confirmed. It MUST state when saved data takes effect.
- **FR-013**: The editor MUST integrate with the existing NPC Debug Monitor APIs to compare authored/predicted schedule state with real intended and executed activity, fallback, and executor status without duplicating monitoring or runtime state.
- **FR-014**: A demonstration MUST exercise the three active NPCs in the main scene and show at least one UI-authored JSON change causing observable physical behavior in Play Mode. It MUST check isolation of the other NPCs and protagonists and preserve/restore any temporary test data.
- **FR-015**: The feature MUST NOT add professional activity implementations, narrative systems, island population systems, a second scheduler, duplicate activity catalog, parallel configuration format, or separate schedule database.
- **FR-016**: The delivery MUST document usage, actual before/after JSON, Play Mode evidence, verification matrix and limitations, modified files, and the handoff contract for `npc-activity-library`.

### Key Entities

- **Activity**: Catalog record identifying an activity and its target kind; runtime executability is a separate fact.
- **Schedule Entry**: Stable ID, time interval, activity ID, and optional symbolic target.
- **NPC Schedule**: An individual schedule that may use the existing template operations and overrides.
- **Template**: A reusable schedule document with independently validated dependents.
- **Conditional Override**: A prioritized schedule rule scoped to an NPC and evaluated using supported day, weekday, weather, and flag conditions.
- **Draft**: An unsaved authoring state associated with source fingerprints and undo history.
- **Destination**: A symbolic location or object identifier that may be resolved, ambiguous, unavailable, or unverified in a given world context.
- **Runtime Snapshot**: Existing Debug Monitor observation of intended schedule and effective executed activity/state.

## Success Criteria

### Measurable Outcomes

- **SC-001**: A developer can select one of the three active world NPCs, change an entry time/activity/target, validate, and save through the visual UI without editing code or JSON manually.
- **SC-002**: Opening the editor and all draft-only actions produce zero changes to source JSON; explicit Save changes only the selected canonical schedule document unless the user explicitly edits a shared template or override.
- **SC-003**: A valid UI-authored schedule change is confirmed in the intended JSON after reloading and in physical NPC behavior during Play Mode, with the Debug Monitor showing the corresponding observed state.
- **SC-004**: All tested schedule previews match the real resolver for base entries, supported conditions, priority conflicts, and permitted gaps.
- **SC-005**: An external file modification made during an edit is detected before Save, and the external content remains unchanged until the user resolves the conflict.
- **SC-006**: The three existing NPCs can be inspected and validated without changing the other two NPC files or protagonist assets during the demonstration.
- **SC-007**: A newly added catalog activity is listed automatically and its executability is derived from the current recipe/runtime data.
- **SC-008**: The final report identifies every acceptance case as pass, fail, blocked, or not run, with evidence type and path; no unobserved behavior is reported as validated.

## Assumptions

- The three active world NPCs mean `world_ivo`, `world_luana`, and `world_helena` in the main scene. The `demo_*` JSON schedules and test fixtures are separate authoring data and are not evidence of runtime NPC behavior.
- The current Schedule Core uses integer minutes, half-open intervals, `24:00` as an end-only value, supported template operations, and priorities 50/70/90. The implementation must confirm these in the active code before relying on them.
- The active scene's runtime data provider is the authority for declaring whether a schedule is loaded at runtime; the presence of another JSON under the data folder alone does not prove that it is consumed.
- Only `idle`, `wander`, and `sit` currently have Activity Core recipes. Other catalog IDs remain visible but are not represented as physically executable until the runtime confirms a recipe.
- Existing JSON remains the sole persisted schedule source. Draft recovery, if needed, is session/editor state and never becomes a second schedule store.
- Authorship, save, and validation happen in Edit Mode. No new hot reload is introduced.
- This feature authors existing data and contracts only. New professional behaviors, narrative content, island population, and activity recipes are reserved for later features.

## Baseline Observations (Read-only)

- Unity Editor connection: ready, Unity `6000.3.25f1`, project `Freyja/`, `Ilha_Principal` open, Play Mode stopped. Console baseline reports no compilation failure; historical Console counters include errors and need focused diagnosis during implementation before attributing them to this feature.
- Current branch before the feature: `094-npc-debug-tools`; local HEAD includes the Debug Tools and Integration Validation commits and is two commits ahead of `origin/main`. This spec is on sequential branch `095-npc-schedule-editor` created from that working baseline to retain the completed NPC integrations.
- Real contracts found: `ScheduleDataLoader.Load`, `ScheduleResolver.Resolve`, `ActivityRecipeLoader.Load`, `NpcScheduleTextAssetProvider.TryLoad`, `NpcLocationRegistry`, and `NpcDebugDiagnostics.CaptureSnapshot/CaptureNpc`.
- Current JSON data is under `Freyja/Assets/_Project/NpcScheduling/Data/`; the active world NPC documents live in `npcs/world_integration/`.
- The runtime provider is scene-configured with `TextAsset` inputs. The authoring editor must discover data without claiming that every discovered JSON is currently loaded by the active scene.
- Integration Validation reports its existing core/harness and physical Play Mode checks passed, with its stated partial coverage; Debug Tools reports D13 temporary override as blocked/unsupported and D25 domain suite coverage as partial. Those limitations do not prove or disprove editor behavior.

## Out of Scope

- New professional activities, recipes, animation clips, narrative systems, population systems, quests, or runtime schedule hot reload.
- New schema, scheduler, activity catalog, calendar/weather model, action/recipe editor, or parallel persisted schedule format.
- Multi-NPC timeline, bulk edit, advanced drag-and-drop, player-character behavior changes, and scene/prefab edits for routine saves.
- Commit, merge, push, reset, stash, or destructive cleanup as part of the feature workflow.

## Acceptance Matrix SE01–SE34

Each result must be recorded as PASS, FAIL, BLOCKED, or NOT RUN with evidence type and path. Source inspection or harness success is not Editor/UI/Play Mode evidence.

| ID | Scenario | Expected result |
|---|---|---|
| SE01 | Open in Edit Mode | Real documents listed; source files unchanged. |
| SE02 | Search, select, and locate NPC | Correct stable identity; absent association is explicit. |
| SE03 | Read the 24-hour timeline | Times, origin, gaps, overlaps, and selection agree with loaded schedule data. |
| SE04 | Add/edit/remove/duplicate an entry | Draft changes only; valid unique ID; no implicit save. |
| SE05 | Boundaries `00:00`, `23:59`, `24:00` | Existing interval semantics are preserved. |
| SE06 | Empty, inverted, or overnight interval | Clear error or explicit split; no invalid save. |
| SE07 | Overlapping base entries | Save is blocked. |
| SE08 | Allowed gap | Warning and actual resolver fallback appear in preview. |
| SE09 | Catalog versus executability | New/catalog-only ID appears but is not claimed executable without recipe. |
| SE10 | Change activity/target | Compatibility is re-evaluated; no silent target replacement. |
| SE11 | Unknown or incompatible target | Diagnostic appears; save is blocked when incompatibility is proven. |
| SE12 | Contextual `home` or unloaded-scene target | Scope is correct and unknown physical state is labeled unverified. |
| SE13 | Occupied Smart Object slot | Transient occupancy does not invalidate schedule authoring or alter reservations. |
| SE14 | Template and individual customization | Template stays unchanged when editing an individual schedule; result resolves correctly. |
| SE15 | Remove and restore inherited block | Supported operations round-trip correctly. |
| SE16 | Edit shared template | Dependent NPCs are listed and validated. |
| SE17 | Duplicate routine between NPCs | IDs and symbolic aliases remain valid; source remains unchanged. |
| SE18 | Create/associate template | References resolve without duplicating runtime schedule state. |
| SE19 | Supported conditional rule | Preview changes only for matching contexts. |
| SE20 | Equal-priority conflict | Resolver tie-break and diagnostic match core behavior. |
| SE21 | In-memory preview | Matches the real resolver without mutating world state. |
| SE22 | Undo/redo/revert | Draft returns to expected state; disk remains untouched. |
| SE23 | Switch NPC with dirty draft | Save/Discard/Cancel or documented recovery protects the draft. |
| SE24 | Round-trip without edits | JSON is a no-op or preserves semantics and supported fields. |
| SE25 | Save/reimport/reopen | Runtime reader sees saved values; asset GUID is preserved. |
| SE26 | Invalid JSON/unknown schema | File is not overwritten; diagnostic/read-only mode is shown. |
| SE27 | External file change while editing | Conflict is detected and external content remains intact. |
| SE28 | IO/validation failure during save | Original remains intact, draft survives, no false success. |
| SE29 | Play Mode editing/reload policy | Inspection-only policy is enforced; no unexpected runtime mutation. |
| SE30 | UI edit causes real NPC behavior | Physical change and real snapshot are captured after normal Play Mode load. |
| SE31 | Open Debug Monitor/diagnosis | Existing monitor and real NPC identity/data are used. |
| SE32 | Restart/reload Editor | Saved data persists; draft follows its documented recovery policy. |
| SE33 | Approximately 30 documents | Search and edit remain responsive; no repeated scene/folder scans per repaint. |
| SE34 | Regression and Player isolation | Existing tests pass; protagonist unchanged; editor code is excluded from Player build if build is run. |
