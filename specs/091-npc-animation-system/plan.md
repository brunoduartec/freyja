# Implementation Plan: NPC Animation System

**Branch**: `091-npc-animation-system` | **Date**: 2026-11-27 | **Spec**: [spec.md](spec.md)

## Summary

Use one shared Humanoid Animator Controller/profile for resident idle, walk, run, sit-down, seated hold and stand-up actions. `IslandWaypointWalker`/NavMeshAgent remains the sole movement authority; locomotion reads actual walker velocity, in-place clips avoid root-motion conflicts, and Activity Execution Core retains exact operation, Smart Object claim, cancellation and cleanup ownership. Three existing residents and a real two-slot bench are exercised in an isolated demo scene.

The initial scan missed the user's local Quaternius Universal Animation Library. The project copy provides the Humanoid `Sitting_Enter`, `Sitting_Idle_Loop` and `Sitting_Exit` clips, now wired into semantic profile IDs. The seated pose is functional but visibly high/reads as falling; this visual polish is recorded for a later pass and the corrected facing must be preserved.

## Technical Context

**Language/Version**: C#; Unity 6000.3.25f1.  
**Primary Dependencies**: Built-in Animator, NavMeshAgent, Smart Objects Foundation, Schedule Core, Activity Execution Core, Navigation System.  
**Storage**: Unity assets and JSON demo schedule data under `Freyja/Assets/_Project/`.  
**Testing**: Unity Test Framework Edit Mode and Play Mode; Unity Editor Play Mode harness; .NET Activity Core simulator.  
**Target Platform**: Unity Editor, Built-in Render Pipeline.  
**Performance Goals**: One locomotion update per NPC per frame, no per-frame allocations or redundant state restarts.  
**Constraints**: Keep third-party source assets/GUIDs intact; no package changes, paid assets, Tripo credits, protagonist edits, definitive controller, full schedule/world integration, or profession activities.

## Constitution Check

- Preserve the low-poly resident silhouette and validate animation behavior in the actual Game View.
- Keep project-authored files under `Assets/_Project/` and third-party animation packages in `Assets/ThirdParty/`.
- Report only behavior supported by Test Runner results, logs, or real Editor evidence.
- Unity Editor is the validation authority. The Unity MCP endpoint was unavailable, so Editor UI/Test Runner were used directly.
- Document remaining pose quality and cyclic `sit_idle` uncertainty as known limitations.

## Architecture

```text
World/Schedule integration (next feature)
    └── Activity Core — intent execution, ActivityOperation identity, claims and cleanup
          ├── NpcUnityActivityPorts — Unity adapters and exact Smart Object handles
          ├── IslandWaypointWalker/NavMeshAgent — only position/velocity authority
          └── NpcAnimationDriver + shared NpcAnimationProfile/controller — visual states
```

Locomotion uses `IslandWaypointWalker.Velocity.magnitude` with profile thresholds, not destination or requested speed. The NPC Animator has root motion disabled. Finite action completion comes from the matching state's actual clip; `sit_idle` is a continuous hold. Smart Object alignment and occupancy occur only after Navigation reports arrival.

## Phased Implementation

1. Inventory rigs, avatars, controllers, motion clips and existing Navigation/Activity/Smart Object contracts.
2. Build and share one resident Humanoid profile/controller using verified Kevin Iglesias locomotion and Quaternius sitting clips.
3. Bind exact animation operations to Activity Core identity; keep occupied claims until `stand_up` cleanup succeeds.
4. Author a test-only three-resident scene with real Navigation and a two-slot Smart Object.
5. Run Edit Mode (40 tests), Play Mode (8 tests), simulator assertions, and manual Game View checks for idle, walk/run, two simultaneous seated residents, third contender, cancel and schedule-end cleanup.
6. Record contract, evidence, setup and limitations for `npc-world-integration`. User deferred sitting pose height/fall polish; preserve the front-facing orientation in follow-up work.

## Project Structure

```text
specs/091-npc-animation-system/
  spec.md, plan.md, research.md, data-model.md, quickstart.md, tasks.md
  contracts/npc-animation-contract.md, checklists/requirements.md, validacao.md
  asset-inventory.txt, evidence/
Freyja/Assets/ThirdParty/Quaternius/Universal Animation Library/
Freyja/Assets/_Project/NpcAnimation091/Runtime/ shared runtime profile/identities
Freyja/Assets/_Project/Art/NpcAnimation091/SharedResident.controller
Freyja/Assets/_Project/Resources/NpcAnimation091/SharedResidentProfile.asset
Freyja/Assets/_Project/Scenes/NpcAnimation_Demo.unity
```

## Complexity Tracking

None. A shared profile and controller provide reusable action bindings without changing the scheduler or creating a controller per resident.
