# Implementation Plan: NPC World Integration

**Branch**: `092-npc-world-integration` | **Date**: 2026-11-27 | **Spec**: [spec.md](spec.md)

**Input**: Approved specification from `specs/092-npc-world-integration/spec.md`.

## Summary

Connect the existing `CoastalDayNight` cycle to the current `NpcActivityController` hosts and their Schedule, Activity, Smart Object, waypoint and animation ports. Add a scene-level driver and a session-only projection from the existing 0–24 hour cycle to the Schedule Core's absolute-day/minute snapshot. Configure three residents with individual home points and JSON routines containing only `idle`, `wander` and `sit`. Reuse the established ports and data loader; make only the smallest adapter changes required for per-NPC idle destinations and lifecycle diagnostics.

## Technical Context

**Language/Version**: C# for Unity 6000.3.25f1; shared scheduling/execution core remains Unity-free.  
**Primary Dependencies**: Unity Built-in, `CoastalDayNight`, `NpcActivityController`, `NpcUnityActivityPorts`, `NpcLocationRegistry`, `IslandWaypointWalker` / `IslandWaypointNetwork`, `SmartObjectRegistry`, `NpcAnimatorBindings`; no new package.  
**Storage**: Existing TextAsset JSON catalog/recipes/schedules; scene references, waypoint points and Smart Object configuration authored through Unity. Day counter is runtime-only and resets with Play Mode.  
**Testing**: Existing .NET `Tools/NpcScheduleSimulator` for shared core, Unity Edit/Play Mode suites for existing systems, new focused projection/integration checks, Editor compilation and real Play Mode on `Ilha_Principal`.  
**Target Platform**: Unity Editor 6000.3.25f1 and existing desktop prototype.  
**Project Type**: Unity game.  
**Performance Goals**: Resolve schedules on initialization, context change, day rollover or intent boundary; do not parse JSON or search the scene each frame. One bounded update per configured host per frame for ports.  
**Constraints**: Preserve existing GUIDs, main characters, input, camera and game clock controls. No second scheduler, calendar service, reservation registry, movement authority or activity workflow. No save/load, package changes, purchased assets, Tripo credits, professional activities, narrative systems or routine editor. Preserve unrelated branch/worktree edits.

## Constitution Check

| Gate | Status | Evidence / decision |
|---|---|---|
| The GDD remains the source of game decisions | PASS | Demo routines are technical fixtures; they do not declare canonical resident behavior or narrative evidence. |
| Unity MCP is used for Unity authoring and runtime checks | PASS | The Unity Pipeline endpoint is reachable at port 7800 for `/Users/bruno.duartec/Documents/Projects/Freyja/Freyja`; all scene authoring and Play Mode checks will target this Editor. |
| Visual direction and existing playable experience are preserved | PASS WITH VALIDATION | Reuse current resident models/rigs and main camera; no changes to protagonists, lighting or global navigation bake are planned. Verify real Play Mode. |
| Existing subsystem contracts are reused | PASS | Controller receives snapshots; Core owns execution/cleanup; Smart Objects own reservations; waypoint walker owns movement; animator bindings own presentation. |
| Tests/evidence permit resumption | PASS WITH VALIDATION | Record baseline and outcomes in `validacao.md` and keep focused logs/screenshots in this feature directory. |

## Phase 0 Research

See [research.md](research.md) for code contracts, baseline results, clock gaps and scope decisions.

## Phase 1 Design

See [data-model.md](data-model.md), [contracts/runtime-composition.md](contracts/runtime-composition.md), and [quickstart.md](quickstart.md).

## Project Structure

### Documentation

```text
specs/092-npc-world-integration/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/runtime-composition.md
├── tasks.md
└── evidence/
```

### Source and Unity Content

```text
Freyja/Assets/_Project/Scripts/NpcScheduling/Core/       # shared, Unity-free clock projection (if required)
Freyja/Assets/_Project/Scripts/NpcScheduling/Unity/      # scene driver and minimal port/configuration additions
Freyja/Assets/_Project/NpcScheduling/Data/npcs/          # three demonstration JSON schedules
Freyja/Assets/_Project/Scenes/Ilha_Principal.unity       # explicit registry, driver, NPC and bench references
Freyja/Assets/_Project/Prefabs/                           # only NPC/Smart Object prefabs if existing prefabs need additive components
Tools/NpcScheduleSimulator/                              # shared-core checks, no duplicated runtime implementation
```

**Structure Decision**: Keep the shared decision and execution cores as-is. Put world-to-snapshot projection logic in the existing shared Core folder only if it remains free of Unity dependencies; keep MonoBehaviour orchestration and serialized references in the existing `NpcScheduling/Unity` assembly context. Configure scene objects through Unity Editor/MCP. Do not copy Core classes into `Assets` or move code across assembly boundaries.

## Complexity Tracking

No new subsystem or dependency is planned. Any code change outside composition, adapter mapping, test hooks and scene/data authoring requires a documented contract defect directly blocking this feature.
