# Feature Specification: NPC Animation System

**Feature Branch**: `091-npc-animation-system`  
**Created**: 2026-11-27  
**Status**: Approved by user; implementation in progress  
**Input**: Approved prompt `prompts/freyja-npc-animation-system.md`

## User scenarios and acceptance

### Story 1 — NPC locomotion (P1)
Residents use one reusable animation setup per compatible rig. Standing, waiting, walking and an explicit run request show the corresponding animation. Walk/run motion follows the Navigation System's effective velocity; stopping or a blocked/pending path returns to idle. Navigation remains the sole authority for movement, without root-motion double movement or changes to player characters.

**Independent test**: Use three real resident prefabs in an isolated Unity scene; inspect Animator parameter and NavMeshAgent velocity during moving and stopped frames.

### Story 2 — Smart Object sitting lifecycle (P1, gated by compatible clips)
Two residents reserve distinct seats on one two-place bench, navigate to approach points, align only after arrival, occupy, play `sit_down`, persist in a real seated pose/loop, then play `stand_up` before the Activity Execution Core releases ownership. A third NPC sees the full bench as unavailable. Cancellation and stale operations cannot release another execution's claim or resume movement while seated.

**Independent test**: Run the real Activity Core, ports, walker and Smart Object in Play Mode; assert exact slot owners, operation results, pose, cleanup, and return to movement. If compatible sit/stand clips are absent, report the physical acceptance as blocked and never fake success.

### Story 3 — Reusable authoring and future integration (P2)
An author can configure an NPC with a shared profile/controller, report missing or incompatible bindings, and add semantic actions without creating a controller per resident. The demo uses a temporary harness only. `npc-world-integration` can consume the public contracts without a second scheduler/controller or altered recipe semantics.

**Independent test**: Configure two resident prefabs against the same profile, validate bindings and document setup and operation lifecycle.

## Requirements

- **FR-001**: Inventory the existing resident rigs, avatars, clips, Animator Controllers and parameters before editing; distinguish Humanoid from Generic and retain asset paths/GUIDs.
- **FR-002**: Provide reusable semantic bindings for `idle`, `walk`, `run`, `sit_down`, `sit_idle`, `stand_up`; compatible NPCs must share a profile/controller rather than one controller per resident.
- **FR-003**: Drive locomotion from effective Navigation velocity and state, using the project's measured scale; in-place clips are preferred and Animator root motion must not compete with NavMeshAgent.
- **FR-004**: Route finite animation operations through existing Activity Core ports with exact NPC/execution/operation identity, bounded timeout, explicit result/reason and idempotent cancellation.
- **FR-005**: Keep Smart Object reservation/occupation/release in the Activity Core and its ports. Align after real navigation arrival, only within configured tolerance, while one component owns Transform. Restore standing posture before locomotion/release when cleanup requires it.
- **FR-006**: Provide a temporary isolated demonstration with three real NPCs and a two-slot Smart Object, using real navigation and execution components. Do not implement a definitive NPC controller, global routines, professions, or protagonist changes.
- **FR-007**: Do not invent or simulate missing animation clips. Record exact missing/Generic/incompatible assets. The user authorizes a read-only Asset Store search at the end for a free compatible sitting asset; no purchase or import is pre-authorized.
- **FR-008**: Run Unity compilation and relevant Edit/Play Mode tests; record actual results and evidence. Do not claim tests, poses, cancellations, or return paths that were not observed.

## Edge cases

- Animator absent, disabled, wrong layer, missing state or duplicate binding.
- Pending/failed NavMesh path, zero speed, stale walker ticket, invalidated slot, disabled actor.
- Cancel during movement, sit transition, seated wait or stand-up; repeated or stale cancellation.
- Missing `sit_down`/`sit_idle`/`stand_up` must fail explicitly and retain ownership until safe cleanup can complete.
- Two free slots occupied simultaneously; a third reservation must be rejected without overlap or pose execution.
- Pause/Animator speed changes must not create an unbounded timeout or false completion.

## Key entities

- **Animation Profile**: Shared semantic binding map, Animator controller and locomotion thresholds for one compatible rig family.
- **Animation Operation**: Exact owner/execution/ticket identity, semantic action, finite/continuous mode, start time, result and reason.
- **Smart Object Claim**: Existing opaque reservation handle owned and released only by the Activity Core adapter.
- **Demo Harness**: Feature-only driver for real components and explicit locomotion/cancel requests; not production scheduling.

## Success criteria

- **SC-001**: At least two compatible resident prefabs point to the same Animator Controller/profile; no protagonist controller/input/animation asset is modified.
- **SC-002**: During real movement, locomotion parameter follows measured effective walker velocity and returns to idle after arrival; root motion is disabled for those NPCs.
- **SC-003**: Sit lifecycle passes only when the resident rig shows an actual seated pose and safe stand-up/release. Otherwise report the exact missing clips and mark those scenarios blocked.
- **SC-004**: Three-NPC Play Mode test proves two distinct slot owners and third-NPC unavailability using the real Smart Object and Activity Core.
- **SC-005**: Unity compile and test results, evidence paths, setup steps, and future `npc-world-integration` contracts are recorded in `validacao.md`.

## Assumptions and limits

- Schedule data remains the source of activity/semantic animation IDs. Visual bindings do not add state names to JSON.
- Navigation owns path motion; the animation layer reads its velocity. The Activity Core owns ordering, cancellation and resource cleanup.
- Animation capabilities are gated by imported clips compatible with resident avatars. Generic protagonist clips cannot be silently retargeted as Humanoid.
- No package, asset purchase, Tripo credit, external download or dependency upgrade is part of the approved scope.
