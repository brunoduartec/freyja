# NPC Integration Validation Quickstart

Run from the repository root. Do not start a second Unity Editor for the already-open project.

## Baseline

1. Record `git status --short --branch`, HEAD, Unity CLI status, Unity version, and active scene.
2. Inspect `Tools/MCP/README.md`, the current `specs/092-npc-world-integration/validacao.md`, and the prior 087/089/091 validation reports.
3. Discover actual test assemblies and use Unity MCP Test Runner `list` before selecting EditMode/PlayMode suites.
4. Run the existing harness using the actual commands documented by its README or prior reports. Keep this output labelled as simulation evidence.

## Physical validation

1. Confirm the open Editor is `/Users/bruno.duartec/Documents/Projects/Freyja/Freyja`, on Unity `6000.3.25f1`.
2. Confirm active scene and actual runtime references before entering Play Mode.
3. Use the production schedule host and game clock. Do not invoke individual activities manually to claim agenda integration.
4. Capture concise logs/state snapshots and a small number of camera/scene images at meaningful stages. Include the V-case IDs in the report.
5. Restore any temporary fixture through the existing harness/lifecycle; do not save destructive runtime setup into the main island scene.

## Long run and profiler

- Repeat the same deterministic logical week input twice and compare decisions/resource counts.
- Run physical time at a pace that permits movement and animations to finish. Report actual completed physical days and wall duration.
- If supported, sample Profiler in idle, simultaneous movement, and Smart Object contention with the same NPC count and time scale. Record counters actually exposed; no project performance budget exists.

The full acceptance matrix, including explicit `BLOCKED` and `NOT RUN` rules, is in [source-requirements.md](source-requirements.md).
