# Implementation Plan: Smart Objects Foundation

**Branch**: `085-smart-objects-foundation` | **Date**: 2026-11-26 | **Spec**: [spec.md](spec.md)

**Input**: Approved specification at `specs/085-smart-objects-foundation/spec.md`; full acceptance detail remains in the attachment named by that spec.

## Summary

Deliver reusable scene components for Smart Objects, interaction slots, active discovery and owner-bound reservations. Keep configuration serialized and reservation state runtime-only. Add Editor diagnostics/Gizmos, an independent provisional low-poly bench prefab with two `sit` slots, a bounded demonstration on the existing island/NavMesh, and Edit/Play Mode coverage. Configure future-only capabilities for the existing well/dock only where their geometry supports clear, walkable interaction points. No scheduler or activity implementation.

## Technical Context

**Language/Version**: C# for Unity 6000.3.25f1; Blender 5.2.2 LTS for the editable placeholder source.

**Primary Dependencies**: Existing Unity Built-in pipeline, `UnityEngine.AI`, existing `Unity.Pipeline` editor integration, and the installed Unity Test Framework. No package changes.

**Storage**: Serialized Smart Object/slot authoring data in Unity prefabs and scenes; IDs persist in authoring assets. Active registrations, owners, state and tokens exist only for the current runtime session. Bench source in `Source/Blender/SmartObjects085/`; FBX and Unity prefab/meta under `Freyja/Assets/_Project/Art/SmartObjects085/` and `Prefabs/SmartObjects085/`.

**Testing**: Unity Test Framework Edit Mode and Play Mode. Exercise real navigation with existing `NavMeshData` and `IslandWaypointWalker` where the demonstration points validate; run editor compilation and Console checks through Unity Pipeline CLI.

**Target Platform**: Unity Editor and current Freyja desktop prototype; no platform-specific APIs.

**Project Type**: Unity 3D game project, Built-in Render Pipeline.

**Performance Goals**: Registry work occurs on lifecycle/query calls, not per frame; queries allocate only bounded candidate results; reservation state transitions are constant time per slot. No new per-frame scene-wide search.

**Constraints**: Preserve current user edits and GUIDs; no package upgrades, commercial dependency, paid generation, Tripo, routine/scheduler/AI, new movement system, or large main-scene rearrangement. Prefab authoring stays script-free for consumers. Registry resets across Play Mode/domain reload.

**Scale/Scope**: Tens to low hundreds of active Smart Objects, a small number of slots per object, one focused demonstration and test fixtures.

## Constitution Check

| Gate | Result | Evidence/decision |
|---|---|---|
| Spec is an independently verifiable delivery and preserves approved decisions | PASS | Approved spec has three user stories, measurable criteria and explicit scope exclusions. |
| Unity/Blender MCP workflow | PARTIAL / BLOCKED | Blender background MCP created the provisional source. Unity Editor tools are not exposed; `unity` CLI reports Editors, but the target project's Editor is already open and the editor Pipeline endpoint is unresponsive while the Mac is locked. No claim of Unity MCP use or Editor validation will be made. |
| Low-poly visual direction and evidence | PARTIAL | A simple replaceable Blender bench mesh exists. Prefab/scene placement and camera-scale review require Unity Editor access. |
| Investigation lore and evidence boundaries | PASS | Feature adds no narrative facts or deductions. Record observed blockers separately from assumptions. |
| Artifacts/tests and retomável handoff | PARTIAL | Runtime, Inspector, bench source/export, registry/lifecycle tests and instructions exist. Prefab, scene, demo integration tests, test execution and visual evidence remain pending. |
| No unnecessary dependencies or future scope | PASS | Uses existing NavMesh, NPC prefabs, Unity Test Framework, and no dependency changes. |

## Phase 0: Research Decisions

See [research.md](research.md). Existing residents have `NavMeshAgent`; `IslandWaypointWalker` and `IslandWaypointNetwork` provide movement and load `IlhaWaypointNavigation.asset`. Reuse them. Existing dock source contains boat-integrated `Banco_barco_*` pieces, not a freestanding bench. There is no standalone low-poly bench asset in the inspected asset names; create an independent simple Blender placeholder so it can be replaced. Existing `Cais_Iteracao01.blend` contains a dock; use the existing dock/scene locations for a future-capability showcase only if the walkable NavMesh points are clear. No named well prefab has been found; do not invent one.

## Phase 1: Design

See [data-model.md](data-model.md) and [quickstart.md](quickstart.md). No external API contracts are required; the public C# contract is internal to the Unity project and documented in the data model.

### Constitution Check (post-design)

PASS. The design remains lifecycle-driven, package-free, confined to `_Project/`, and separates authoring from runtime state. Navigation remains the existing project system; unavailable spatial validation is recorded rather than hidden with substitute movement.

## Project Structure

### Documentation (this feature)

```text
specs/085-smart-objects-foundation/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
└── tasks.md
```

### Source Code (repository root)

```text
Freyja/Assets/_Project/SmartObjects085/
├── Runtime/
│   ├── SmartObject.cs
│   ├── InteractionSlot.cs
│   ├── SmartObjectRegistry.cs
│   ├── ReservationHandle.cs
│   └── SmartObjectReservationOwner.cs
├── Editor/
│   └── SmartObjectEditor.cs
├── Tests/EditMode/
│   ├── Freyja.SmartObjects.EditModeTests.asmdef
│   └── SmartObjectRegistryTests.cs
└── Tests/PlayMode/
    ├── Freyja.SmartObjects.PlayModeTests.asmdef
    └── SmartObjectLifecycleTests.cs

Source/Blender/SmartObjects085/BenchProvisional.blend
Freyja/Assets/_Project/Art/SmartObjects085/BenchProvisional.fbx
Freyja/Assets/_Project/Prefabs/SmartObjects085/BenchProvisional.prefab
Freyja/Assets/_Project/Scenes/SmartObjects_Demo.unity
Freyja/Assets/_Project/Scripts/Prototype/SmartObjectDemoHarness.cs
specs/085-smart-objects-foundation/evidence/
```

**Structure Decision**: Keep one isolated `_Project/SmartObjects085/` code and asset slice with separate Runtime, Editor and tests assemblies. Reuse current NPC, NavMesh and dock assets by references; do not edit their source prefabs or broadly modify `Ilha_Principal`.

## Complexity Tracking

No constitution violations. No new package, service or runtime framework is introduced.
