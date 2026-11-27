# Implementation Plan: NPC Unity Integration Code

**Branch**: `090-npc-unity-integration-code` | **Date**: 2026-11-26 | **Spec**: `spec.md`

## Summary

Add a thin Unity integration assembly around the existing schedule and execution cores. Use explicit runtime data references, a per-NPC host, lifecycle-managed destination/waypoint registry, existing `IslandWaypointWalker`/NavMesh behavior, the real `SmartObjectRegistry`, and explicit Animator bindings. A real world clock/context provider and physical Animator/scene configuration are absent; no parallel clock or fabricated bindings will be added. Host initialization remains fail-closed until the game supplies a real snapshot source.

## Technical Context

- **Language/Version**: C# supported by Unity 6000.3.25f1; maintain conservative compatibility with existing core.
- **Primary Dependencies**: UnityEngine, UnityEngine.AI, existing `Freyja.SmartObjects.Runtime`; no new packages.
- **Storage**: Existing JSON in `Freyja/Assets/_Project/NpcScheduling/Data/` supplied as explicit `TextAsset` references; no copied JSON or new source of truth.
- **Testing**: Existing .NET harness `Tools/NpcScheduleSimulator` for shared data/core behavior; static review for Unity adapters. No Unity compile or Play Mode.
- **Target Platform**: Unity 6 Built-in project.
- **Project Type**: Unity runtime C# integration plus shared core/harness loader boundary.
- **Performance Goals**: Parse/load once at initialization; resolve on start, context/time boundary or provider change; operation ports poll only active work; no global search, repeated SetDestination, full schedule resolution or logs per frame.
- **Constraints**: No Unity MCP/editor/Blender, no scene/prefab/asset/controller/NavMesh/settings/package/.meta edits, no official NPC setup, no merges/pushes after the already authorized Activity Core synchronization.
- **Scope**: Only `idle`, `wander`, and `sit`; lifecycle, diagnostics and future physical setup guide.

## Constitution Check

- Core remains Unity-free: integration assembly depends on core, not vice versa.
- Runtime destination/data/animation IDs remain explicit and authored, with no invented scene references.
- Observed behavior, schedule intent and physical execution remain distinct.
- Validation claims will name actual commands/results and keep Editor/Play Mode pending.
- Pre-existing unrelated checkout changes remain outside the integration branch/worktree.

## Design Decisions

1. **Runtime JSON**: Extract the existing harness schedule loader into shared C# code that accepts named text documents; adapt the harness to pass files and a Unity provider to pass serialized `TextAsset`s. This keeps one loader and the same JSON in both environments.
2. **Recipe loading**: Expose recipe validation as shared code and reuse it in both harness and runtime; malformed or missing assets fail the host.
3. **World time**: Host consumes an explicit existing-world snapshot/provider contract. Since no production provider exists in source, host reports unavailable input and does not fabricate a day, clock, pause policy or intent.
4. **Movement**: Reuse `IslandWaypointWalker` where it can expose operation-owned completion/cancel information without taking control away from another actor. If this cannot satisfy the existing operation contract safely, use `NavMeshAgent` directly only when no `IslandWaypointWalker` owns that NPC; never install/configure components automatically.
5. **Smart Objects**: Adapt the current public registry API and keep returned `ReservationHandle` values in an adapter-owned token table keyed to NPC and execution ID. The core's string token remains opaque and cannot be confused with object IDs.
6. **Assembly**: Keep Core, Execution and integration scripts in the project's default `Assembly-CSharp`, which can reference the existing Smart Objects runtime asmdef. Do not add an asmdef: a nested asmdef would make `IslandWaypointWalker` inaccessible because it remains in `Assembly-CSharp`.
7. **Animation**: Scene-authored mapping uses configured Animator state/trigger IDs and verifies state transitions/exit within bounded time. No actual clip/state names will be assigned; unconfigured mapping fails.

## Project Structure

```text
Freyja/Assets/_Project/Scripts/NpcScheduling/
├── Core/                         # existing shared source; loader extraction only if needed
├── Execution/                    # existing shared source
└── Unity/                         # default Assembly-CSharp
    ├── NpcScheduleTextAssetProvider.cs
    ├── NpcLocationRegistry.cs
    ├── NpcActivityController.cs
    ├── NpcUnityActivityPorts.cs
    └── NpcAnimatorBindings.cs
Tools/NpcScheduleSimulator/       # same core and loader sources; no Unity references
specs/090-npc-unity-integration-code/
    plan.md, research.md, data-model.md, contracts/, quickstart.md, tasks.md, validation.md
```

## Phases

1. Verify shared loader boundaries, current movement and reservation API; preserve the existing harness.
2. Implement shared schedule/recipe loading from named text documents and runtime explicit TextAsset references; write no scene data.
3. Implement the location registry, explicit world snapshot boundary, one per-NPC host and diagnostics/lifecycle.
4. Implement IActivityPorts operations for waypoints, movement, Smart Object reservations, alignment and explicit Animator bindings.
5. Run existing .NET tests/validation/week simulation; review assembly references, token ownership, failure/teardown paths and produce the manual integration checklist.

## Risks / Open Limitations

- No production clock or world context source is present in code. This adapter remains a required external input and blocks autonomous runtime operation.
- Existing waypoint walker owns its own per-frame NavMeshAgent lifecycle but currently exposes no operation completion API. It may need a small backward-compatible extension; otherwise direct agent use must be guaranteed exclusive.
- Existing schedule data has no `sit` entry in `demo_resident`; a disposable runtime-schema fixture now supplies `idle`, `wander` and `sit` for explicit TextAsset assignment without changing official schedules. Physical validation still requires an actual world clock.
- Actual sit/stand Animator state/clip IDs and real home/square/beach/bench transforms are unknown; all require physical configuration and Play Mode validation.
- Unity compilation is not available; new runtime scripts are authored/reviewed only until Unity returns.
