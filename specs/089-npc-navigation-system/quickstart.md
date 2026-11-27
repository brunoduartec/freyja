# Quickstart: NPC Navigation Validation

## Preconditions

1. Open the existing Unity Editor for `Freyja/` (Unity `6000.3.25f1`); do not open a second Editor.
2. Confirm the current scene and compilation state in Unity Console. Record the Console cursor and any existing errors before feature validation.
3. Confirm the active scene has a ready `IslandWaypointNetwork`, the `IlhaWaypointNavigation` data, three eligible agent/walker fixtures, and the intended destination markers. Confirm each start and target with `NavMesh.SamplePosition` and a complete path check.
4. Confirm whether the Smart Object has two real approach slots. If it does not, use clearly labeled temporary approach markers and report NAV-10 real-object integration blocked.

## Run the demonstration

1. Select the removable navigation harness in the demo scene.
2. Use its Inspector context actions/buttons to start the configured route sequence for NPC A, B, and C; no script edits should be required.
3. Observe the runtime inspector state for each owner and operation ID. Record physical starting and ending positions, state transitions, path status, requested/effective destinations, and elapsed runtime.
4. While one operation is moving, cancel it twice and start a new destination for that NPC. Verify the old operation cannot stop the replacement.
5. Start an unreachable target and a blocked-route case. Verify each ends with a bounded failure and an explicit reason, with no teleport or transform snap.
6. Disable and re-enable one test NPC; verify the previous operation is cleared. Pause and resume the Editor and verify the watchdog follows the documented policy.
7. Capture Console transition logs plus a short video or sequential screenshots showing the movement over time. A still image alone does not prove arrival.

## Expected outcomes

- All three agents run independently through the existing navigation owner and shared agent settings.
- Valid routes end at their measured target tolerances; invalid routes fail explicitly.
- Cancellation, replacement, pause, and lifecycle events do not create stale operations.
- Region selection is finite and points remain within the configured test region.
- Smart Object validation stops at the selected approach point and leaves reservation cleanup to the consumer.

## Evidence capture

Save reports and captures under `specs/089-npc-navigation-system/evidence/`. Include Unity version, scene path, operation IDs, NPC IDs, start/end world positions, actual elapsed time, test results, and any routes not run. Keep the original Console log available when diagnosing failures. Fill the NAV-01–NAV-14 matrix in the delivery report; mark missing prerequisites `BLOCKED` and unexecuted routes `NOT RUN`.

