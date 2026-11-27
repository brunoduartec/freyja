# Implementation Plan: NPC Navigation System

**Branch**: `089-npc-navigation-system` | **Date**: 2026-11-27 | **Spec**: [spec.md](spec.md)

## Summary

Complete the existing NavMesh-based NPC movement path and its operation reporting, then exercise it through a removable three-NPC Play Mode harness. Reuse `IslandWaypointNetwork`, `IlhaWaypointNavigation`, `NavMeshAgent`, and `IslandWaypointWalker`; do not add a competing pathfinder or second agent owner. The separate Activity Core/Unity Integration worktree has uncommitted edits to `IslandWaypointWalker` and the shared `IActivityPorts` contract. Wait for that work to settle before changing overlapping files. Keep any harness/test code in feature-owned files and defer the adapter unless the Activity Core contract is stable and the existing adapter cannot consume the completed navigation.

## Technical Context

**Language/Version**: C# supported by Unity `6000.3.25f1`  
**Primary Dependencies**: Existing `UnityEngine.AI` module, current waypoint/NavMesh data, existing Smart Objects; no new package  
**Storage**: Unity assets and scene-authored fixture references; operation state is runtime only  
**Testing**: Unity Console compile state, focused Edit Mode tests, Play Mode movement through the connected Editor MCP  
**Target Platform**: Unity Editor / Built-in pipeline  
**Project Type**: Unity 3D game  
**Performance Goals**: Three agents operate concurrently with bounded polling/watchdogs and no global per-frame path recalculation  
**Constraints**: Preserve local and concurrent work; do not edit Activity Core or Smart Object shared contracts; no terrain/player-control changes, package upgrades, animation system, Tripo use, or automatic merge/commit  
**Scale/Scope**: A small, removable demonstration using three agents and existing navigation data; no island-wide re-bake unless a specific test proves it necessary

## Constitution Check

- **Reuse and narrow scope**: PASS — continue the existing Unity navigation path, no general pathfinding or system-wide refactor.
- **Preserve world and player systems**: PASS — use existing nav data and a removable demo/harness; no player controller, terrain, or aesthetic changes.
- **Keep responsibilities separate**: PASS — Activity Core owns operation policy and Smart Objects own reservations; navigation owns only physical movement state.
- **Verify actual evidence**: PASS — distinguish compilation/automated checks from physical Play Mode and mark unavailable scenarios explicitly.
- **Respect active work**: PASS WITH GATE — do not edit files currently changed in the Activity Core/Unity Integration worktree until that session is stable; use feature-owned files first.
- **Integration**: PASS — no commit/merge/push in this task; deliver the isolated branch for review because the request explicitly forbids automatic repository integration.

## Research Summary

See [research.md](research.md). In brief: the project already builds and loads `NavMeshData` through a waypoint network and has a NavMeshAgent walker; the AI Navigation UPM package is not installed. Unity's version-matched reference documents that path calculation may remain pending for multiple frames, an unknown remaining distance can be infinity, and complete/partial/invalid path status must be handled explicitly. Existing Activity Core adapter work is present but uncommitted elsewhere; shared method signatures are changing.

## Design

### Data and contracts

- See [data-model.md](data-model.md) for operation, region, and fixture state.
- See [contracts/navigation-operation.md](contracts/navigation-operation.md) for the minimum owner-scoped move/progress/cancel semantics.
- No change to `IActivityPorts`, Smart Object slots, location registry, or schedule/execution models.
- Reuse existing `IslandWaypointWalker` operation methods if the active integration work lands them. Confirm method names and semantics after handoff. Do not write a parallel `NavMeshAgent` driver.

### Source and test layout

```text
Freyja/Assets/_Project/Scripts/Prototype/IslandWaypointWalker.cs       # Existing movement owner; edit only after current owner finishes
Freyja/Assets/_Project/Scripts/Prototype/NpcNavigationDemoHarness.cs  # New removable test driver/diagnostics if no equivalent exists
Freyja/Assets/_Project/Scripts/Prototype/NpcNavigationDemoHarness.cs.meta
Freyja/Assets/_Project/Scenes/CadeiraSmartObject_Demo.unity            # Existing non-production demonstration scene only if required
Freyja/Assets/_Project/Scripts/Prototype/Tests/                         # Focused tests only if existing test assembly supports this code
specs/089-npc-navigation-system/                                       # Plan, evidence, instructions, matrix
```

The code layout is a target, not authorization to edit overlapping files. If the active integration implementation already supplies a required behavior, test and document it rather than duplicating it. Add no new assembly or package unless Unity proves one is needed.

## Runtime behavior decisions

- Resolve each destination once per operation; verify a complete path before accepting it. Do not project a remote or disconnected target onto a nearby surface and then call it reached.
- Keep requested target and effective target distinct. Arrival checks include horizontal and vertical tolerance and compare the actual transform to the effective target.
- Keep a monotonically increasing operation identity; stale poll/cancel calls cannot affect a replacement.
- Use unscaled runtime time for physical watchdog policy. Pause suspends the watchdog; game-clock jumps are handled by Activity Core cancellation.
- Track lack of physical progress only while a path is complete and the agent is expected to move; do not confuse path-pending, avoidance, or arrival with a stall.
- Regional points come from existing authored waypoints. Validate finitely; do not add an independent repeating wander scheduler.
- Smart Object consumers provide their already-selected slot approach transform and retain the reservation handle. Navigation stops at approach, not at the seated pose.
- Three test actors use shared existing avoidance settings. Do not add carving obstacles to all actors or implement crowd routing.
- Diagnostics update as live inspector state; structured logs occur on operation transitions and failures only.

## Verification Plan

1. Capture Unity Editor project/scene state, console cursor, and current compilation status before editing. Existing Console errors must be attributed before reporting a baseline.
2. Inspect actual loaded `NavMeshAgent`, `IslandWaypointNetwork`, waypoint asset, fixture positions, Smart Object slot count, and routes using the connected Unity MCP. Do not open a second Editor.
3. Compile after code changes and inspect new Console errors from the saved cursor.
4. Run focused Edit Mode tests and Smart Object/Schedule/Activity Core regressions only where the branch contains those test assemblies.
5. Run Play Mode in the existing removable demo context. Physically observe three agents, crossing/approach routes, cancel/reissue, invalid destination, obstacle/stall, disable/re-enable, and pause/resume. Save transition logs and a short sequence of screenshots/video when available.
6. Mark NAV-01–NAV-14 with state and evidence. A static capture cannot prove motion. Report real bench, stair, or regression validation as pending if its prerequisites are absent.

## Risks and gates

- **Concurrent Activity Core edit**: `IActivityPorts.Cancel` and pose-restoration members are being changed, and an adapter folder is untracked in another worktree. The navigation feature must not edit these files. Gate adapter work until that session finishes.
- **Existing walker overlap**: the same `IslandWaypointWalker.cs` is being changed to add operation identity and status. Reuse those changes after handoff and add only missing behavior.
- **Unity storage/checkout constraint**: a full duplicate worktree exhausted disk. This feature branch uses sparse checkout for planning. Perform Editor validation only on the already open project after placing only feature-owned files there; do not create another Unity instance.
- **Scene prerequisites**: active Editor has `CadeiraSmartObject_Demo`; current evidence shows one chair object, not a two-slot bench. Real multi-slot acceptance may remain blocked until such an object is available.

## Complexity Tracking

No constitution violations. The removable runtime harness is justified by physical validation needs and is smaller than adding a new test framework or replacing the island setup.
