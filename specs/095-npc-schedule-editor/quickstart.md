# NPC Schedule Editor Validation Quickstart

## Prerequisites

- Unity project: `Freyja/`, Unity `6000.3.25f1`, main scene `Assets/_Project/Scenes/Ilha_Principal.unity`.
- Connected Unity MCP and Editor open in Edit Mode.
- Existing schedules in `Freyja/Assets/_Project/NpcScheduling/Data/npcs/world_integration/`.
- Keep temporary test edits isolated and restore original JSON after runtime evidence is captured.

## Harness regression

From repository root:

```sh
dotnet run --project Tools/NpcScheduleSimulator -- --validate
dotnet run --project Tools/NpcScheduleSimulator -- --test
```

Expected: data/recipe validation succeeds and all existing assertions pass.

## Editor smoke

1. Open `Freyja > NPC > Schedule Editor` in Edit Mode.
2. Confirm the window lists `world_ivo`, `world_luana`, and `world_helena` as active-scene data, and separates demo/fixture documents.
3. Select an NPC and compare the form/list with the 24-hour timeline. Check that opening the window leaves all JSON hashes unchanged.
4. Add/edit/duplicate/remove a block in the draft. Verify overlap, invalid `24:00` start, zero duration, catalog-only activity, and destination diagnostics. Undo and redo; confirm disk remains unchanged.
5. Preview a time before and after a draft change. Compare both results against `ScheduleResolver` behavior for the same NPC, world context, and time. Confirm world state is unchanged.
6. Externally modify the selected JSON after load and attempt Save. Expect a conflict message, unchanged external content, and recoverable draft.
7. Reload from source, edit a valid block to a different interval, validate, save, inspect the JSON diff, reopen, and verify the saved value.

## Physical Play Mode proof

1. Record baseline hashes for all three world NPC JSON files and capture Edit Mode monitor baseline.
2. Change one `world_*` schedule through the editor to make a `wander`/`sit` transition occur at a known future game-clock boundary using a currently valid symbolic target. Save and verify only the intended JSON changed.
3. Enter Play Mode through Unity MCP. Observe the selected NPC's transform/activity over time and capture `NpcDebugDiagnostics.CaptureNpc` / NPC Debug Monitor values after the boundary. A preview or clock jump alone is insufficient.
4. Capture the Game/Scene view showing physical activity, plus the monitor snapshot and JSON before/after diff.
5. Stop Play Mode, restore the test schedule through the editor or captured original file with conflict checks, reimport, and prove its original hash is restored. Verify the other two NPC hashes and the protagonist scene state are unchanged.

## Evidence and reporting

Store implementation evidence under `specs/095-npc-schedule-editor/evidence/`: test summaries, validation matrix, JSON diff, Debug Monitor snapshots, and actual Unity captures. Mark SE01–SE34 individually as PASS/FAIL/BLOCKED/NOT RUN with evidence type and path. Do not report Editor build isolation without a successful Player build.
