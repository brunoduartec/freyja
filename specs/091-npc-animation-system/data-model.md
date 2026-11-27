# Data Model

## `NpcAnimationProfile` (shared asset)

- Compatible rig family / expected humanoid-avatar requirement.
- Shared `RuntimeAnimatorController` reference and direct `idleClip`/`walkClip`/`runClip` references used by its 1D Blend Tree.
- Locomotion float parameter name, stop/walk/run thresholds and damping, calibrated to the measured `IslandWaypointWalker` velocity scale.
- Semantic binding entries: animation ID, full state path, layer, finite or continuous mode, transition, bounded timeout, and completion policy.
- Missing or duplicate IDs remain validation failures; a profile cannot claim unavailable clips.

## Animation operation ticket

- Exact NPC ID, Activity Core execution ID and `ActivityOperation` ID.
- Semantic animation ID, state hash/layer, start time and timeout.
- Status `Running | Succeeded | Failed | Cancelled` and stable reason code.
- Only the currently owned ticket may poll/cancel the Animator; old tickets cannot affect new work.
- A finite animation/continuous pose hold suppresses locomotion parameter writes until its exact operation/hold is completed or safely cancelled.

## Navigation / interaction state

- Navigation remains `IslandWaypointWalker`'s state: operation ID, movement status, effective velocity/direction, path status and destination.
- Smart Object ownership remains the existing opaque reservation handle, claim owner and execution ID; animation profile stores no reservation data.
- Harness schedules/actor references are test-only and do not become production schedule/controller state.

## Relationship

One profile/controller can serve several compatible resident Humanoid avatars. Each actor has one walker, one Animator adapter and an independent operation/ticket state. A seated Smart Object slot has at most one exact owner. Activity Execution owns the transition sequence and releases that same handle after standing restoration succeeds.
