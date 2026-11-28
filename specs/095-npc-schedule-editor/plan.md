# Implementation Plan: NPC Schedule Editor

**Branch**: `095-npc-schedule-editor` | **Date**: 2026-11-27 | **Spec**: [spec.md](spec.md)

## Summary

Build an editor-only IMGUI window at `Freyja > NPC > Schedule Editor`. It edits draft documents read from the existing JSON assets, uses `ScheduleDataLoader` and `ScheduleResolver` for validation and preview, uses `ActivityRecipeLoader` to report current executability, resolves destination evidence from the existing scene registries, and writes only to the selected source JSON after validation and a fingerprint conflict check. Runtime JSON remains the sole persistent schedule data. The tool will integrate through existing `NpcDebugDiagnostics` snapshots and offer an entry point into the existing monitor.

## Technical Context

**Language/Version**: C# supported by Unity 6.3 (`6000.3.25f1`), Built-in render pipeline.

**Primary Dependencies**: Unity Editor IMGUI, AssetDatabase, existing Newtonsoft.Json assembly already referenced by the NPC Debug Editor, existing `Freyja.NpcScheduling` core and Smart Objects APIs. No new package.

**Storage**: Existing text JSON assets at `Freyja/Assets/_Project/NpcScheduling/Data/` (`activities.json`, `execution/recipes.json`, `templates/`, `npcs/`, `overrides/`). Drafts and undo history exist only in the window session.

**Testing**: Existing `Tools/NpcScheduleSimulator --validate/--test`; Unity compilation/Console via Unity MCP; targeted Editor/EditMode validation where the project test assemblies allow it; main scene Play Mode physical behavior for `world_ivo`, `world_luana`, and `world_helena`.

**Target Platform**: Unity Editor on the Freyja project; editor code excluded from Player builds.

**Project Type**: Unity desktop game with C# core and Unity Editor authoring tools.

**Performance Goals**: No folder, scene, or runtime scans during repaint. Cache discovery until explicit refresh and invalidate on project/hierarchy/import changes. Preview is on demand and uses the actual resolver.

**Constraints**: Keep provider-driven runtime loading; no new runtime scheduler or hot reload. EditMode preview never mutates runtime state. Preserve JSON schema v1 and IDs. Save only after user action; detect concurrent changes. Use concrete source paths and distinguish all discovered files from those referenced by the active scene's `NpcScheduleTextAssetProvider`.

**Scale/Scope**: Current three active world NPCs plus configured/demo schedule documents and existing templates/overrides. Support up to approximately 30 configured NPC documents without per-frame scans.

## Constitution Check

| Principle | Gate | Plan |
|---|---|---|
| GDD is source of truth; specs are testable | PASS | This feature changes authoring workflow only; it adds no narrative fact or lore. |
| Unity MCP is implementation and test environment | PASS | Confirmed Editor `6000.3.25f1`, project root, main scene, and stopped Play Mode; implementation and final runtime validation use Unity MCP. |
| Visual style and readable evidence | PASS | No game visuals change; capture Game/Scene view only as runtime evidence of NPC behavior. |
| Preserve investigative distinction and island population | PASS | No narrative, protagonist, or population behavior changes. |
| Evidence and handoff | PASS | Quickstart, validation matrix, before/after JSON, and Play Mode evidence are required. |

No constitution violation is planned.

## Observed Contracts and Design Decisions

1. `ScheduleDataLoader.Load(ScheduleDataDocuments)` is the authoritative strict JSON loader and semantic validator. It validates schema version 1, activity references and target kind, template operations, ID syntax, interval validity, overlap, conditions, and override priorities. It produces warnings for schedule gaps. There is no existing schedule writer.
2. `ScheduleResolver.Resolve(npcId, GameTimeSnapshot, WorldContextSnapshot)` is the sole preview algorithm. The preview builds a candidate `ResolvedScheduleData` from the draft and queries this API; it does not implement its own selection/priority logic.
3. `ActivityRecipeLoader.Load` identifies recipes. The current catalog has 15 activities, while the current recipe file has only `idle`, `wander`, and `sit`. New catalog rows are therefore discovered dynamically and shown as not executable unless the matching recipe loads.
4. The runtime provider is scene-configured with `TextAsset` references for catalog, templates, NPC schedules, overrides, and recipes. Data folder presence alone does not mean runtime consumption. The editor will inspect configured references in the loaded scene and label discovered non-referenced files separately.
5. `NpcLocationRegistry` maps active point IDs to region IDs and exposes waypoint IDs by region. `NpcUnityActivityPorts.FindWaypoints` resolves `wander` by region; NPC `idle` uses its per-host configured `idleDestinationPointId`. `sit` asks `SmartObjectRegistry` for matching `sit` candidates in the schedule target (currently `village_square`) and reserves a slot dynamically during runtime. Editor validation will not reserve or path-test. Without an authoring-time contract proving a target, it reports `unverified` rather than rejecting it.
6. `NpcDebugDiagnostics.CaptureSnapshot`, `CaptureNpc`, and `CaptureSmartObjects` report runtime state. Existing monitor selection is window-private, so a small public focus/open entry point will be added to the monitor rather than duplicating monitoring state.
7. Existing schedule data is strict schema v1. Typed loader input rejects unknown fields and unsupported schema versions. Preserve all fields supported by the current loader and retain source files unchanged for parse/schema failures; do not attempt automatic migration.
8. Atomic authoring save uses a draft snapshot, captured SHA-256 source fingerprint, exact candidate validation, a same-directory temporary file with replace/rollback handling, and targeted `AssetDatabase.ImportAsset`. Temporary files are cleaned up before returning. A concurrent change blocks the write and leaves the draft intact.
9. Undo/redo is a bounded sequence of session-only document snapshots. It does not use Unity object Undo because the editable source is file data, not a scene object.
10. Save is enabled only in Edit Mode. Runtime data is picked up through the existing TextAsset load path on the next Play Mode entry; no new runtime reload is introduced.

## Project Structure

### Documentation

```text
specs/095-npc-schedule-editor/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/editor-data-contract.md
├── tasks.md
└── evidence/
```

### Source Code

```text
Freyja/Assets/_Project/Editor/NpcScheduleEditor/
├── NpcScheduleEditorWindow.cs       # Menu, IMGUI, tabs, timeline, forms, diagnostics
├── NpcScheduleEditorSession.cs      # Draft selection, dirty state, undo/redo
├── NpcScheduleEditorRepository.cs   # Discovery, fingerprints, safe JSON write/import
├── NpcScheduleEditorPreview.cs      # Real-loader/resolver preview and validation
└── NpcScheduleEditorDestinations.cs # One-shot scene-backed target evidence

Freyja/Assets/_Project/Editor/NpcDebug/NpcDebugMonitorWindow.cs
                                      # Add focused open/select hook for an NPC
Freyja/Assets/_Project/Editor/NpcScheduleEditor/Tests/EditMode/
                                      # Authoring draft, conflict, and round-trip tests
```

New Unity source files receive `.meta` through normal AssetDatabase import. No schedule JSON path, runtime core, scene, prefab, package, or schema changes are planned unless a concrete gap blocks the approved flow. If such a gap appears, keep changes minimal and document it.

## Complexity Tracking

None.

## Validation Plan

- Run the Schedule Simulator's current validation and assertions as regression baseline and after relevant changes.
- Compile in the connected Unity Editor and inspect compilation/Console errors, comparing against the recorded pre-feature error baseline before assigning attribution.
- Exercise Editor draft operations, strict validation, preview parity, round-trip, external file conflict, failure recovery, and dynamic catalog discovery.
- Open the actual editor in the connected Unity UI, inspect layout and menu, and use the three scene-configured world NPC documents.
- Save one UI-authored schedule edit, show the exact JSON diff, exit/re-enter Play Mode through Unity MCP, and capture the Debug Monitor and observed physical NPC behavior. Restore the original test schedule after captures; verify its hash equals baseline.
- Confirm the other two NPC schedule hashes and player/protagonist scene state remain unchanged.
- Report every SE01–SE34 as PASS, FAIL, BLOCKED, or NOT RUN with evidence. Do not claim standalone build exclusion unless a build is run.

## Operational Constraints

- Do not commit, merge, push, reset, stash, or discard concurrent/untracked work.
- Preserve the user's untracked attachment directory and the existing debug-tools branch history.
- Use the existing three runtime NPC IDs; do not turn test schedule changes into story canon.
- If Play Mode behavioral validation cannot be obtained from current time/world setup without modifying the scene, use the existing clock controls only when their operation is reversible and reported; never claim a preview as physical evidence.
