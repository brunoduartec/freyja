# Navigation Operation Contract

This feature-owned contract describes expected behavior. It does not edit or replace `Freyja.NpcScheduling.Execution.IActivityPorts`.

## Begin

Input: NPC/owner identity, unique operation identity, and one resolved world point or Transform.  
Output: accepted/running, or a terminal failure with a reason.

- Validate component and agent lifecycle, target validity, and ownership before starting.
- Resolve the effective target once and require a complete path; keep the requested and effective targets separately diagnosable.
- Starting a replacement invalidates the previous operation before it can affect the new one.
- Never use Warp or direct transform movement to hide path failure.

## Poll

Input: exact owner and operation identity.  
Output: state, available path/progress/locomotion diagnostics, and optional terminal reason.

- A stale identity returns a stale/not-owned result and has no side effects.
- Pending path computation remains non-terminal.
- Arrival requires a completed route, a valid active operation, and physical position inside configured horizontal and vertical tolerances. Stopped velocity or zero/unknown remaining distance alone is insufficient.
- Invalid/partial path, invalidated target, disabled/off-surface agent, or bounded stall ends explicitly as failure.
- A no-progress watchdog runs only while physical movement should be possible. Pause behavior is controlled by the documented runtime-time policy.

## Cancel

Input: exact owner and operation identity.  
Output: whether that operation was cancelled or was already terminal.

- Repeated cancellation is safe.
- A stale identity cannot stop another operation.
- Clear the existing walker's path only when the same operation remains current.
- Lifecycle shutdown clears local state and callbacks without waiting for another frame.

## Consumer responsibilities

- Activity Core decides when movement begins/cancels and applies any retry/fallback policy.
- The Smart Object consumer chooses and reserves the slot and releases its own handle after completion, cancellation, or failure.
- A future Animation System consumes velocity/direction and controls any later pose alignment; navigation does not set Animator state or seated pose.

## Existing Activity Core mapping (conditional)

The committed `IActivityPorts` contract maps movement through `Begin("move_to", ..., point, ...)`, `Poll(ActivityOperation, ...)`, and `Cancel(ActivityOperation, ...)`. However, the active Unity Integration worktree has uncommitted signature changes and an untracked `NpcUnityActivityPorts` adapter. Do not implement or edit that adapter until the activity session hands off its final contract. If the current adapter can consume the completed walker operation API, reuse it and add no second adapter.

