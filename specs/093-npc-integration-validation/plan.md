# Implementation Plan: NPC Integration Validation

## Goal and scope

Execute the approved requirements in [source-requirements.md](source-requirements.md) as a stabilization and verification delivery. Prefer existing reports, tests, fixtures, runtime adapters, diagnostics, and evidence. Change production code only after a defect is reproduced. No new game capability is in scope.

## Environment and current state

- Repository branch: `093-npc-integration-validation`, created from the current `092-npc-world-integration` checkout to preserve its unmerged work.
- Unity root: `Freyja/`; expected editor: `6000.3.25f1`; primary scene: `Freyja/Assets/_Project/Scenes/Ilha_Principal.unity`.
- A ready Unity Editor was observed for the expected project at the start of the task. Reconfirm the active scene before any test that mutates runtime state.
- Keep the user-supplied attachment and all preexisting untracked files intact. Do not reset, stash, clean, commit, push, or merge.
- The Unity MCP, Unity Test Runner, Profiler/diagnostics, and project CLI are the preferred validation surfaces; use only those actually available.

## Workflow

1. **Baseline**: map prior feature evidence and W01–W19 to V01–V35; inspect composition and JSON; inventory exact test suites and existing fixtures; record clean/dirty state and build version.
2. **Automated regression**: run existing Schedule, Activity, Smart Objects, Navigation, Animation, and World Integration test paths that actually exist. Capture failures before changes and distinguish historical failures.
3. **Physical smoke**: in the real island and via production hosts/clock, inspect three distinct scheduled NPCs. Capture one full real sit lifecycle and at least one real contention outcome if supported. Preserve the main scene and use the isolated test scene/harness for destructive cases.
4. **Fault and lifecycle cases**: reproduce supported schedule changes, invalid paths/stalls, slot contention and invalidation, phase-specific cancellation, disable/destroy/reactivate, stale callbacks, pause/time scaling, and cleanup. Assign unavailable hooks/fixtures BLOCKED or NOT RUN.
5. **Long runs**: execute the seven-day logical harness twice with the same existing deterministic inputs; separately execute physical days only as time allows while completing real operations. Record the actual day count and distinguish time skips from physical completion.
6. **Performance**: use Unity Profiler/diagnostic counters for comparable idle, movement, and contention observations. Record only available measurements and fix only an evident integration defect.
7. **Final regression and report**: rerun relevant suites and affected physical cases after any fix. Write the requested report at the repository root with the full V01–V35 matrix, paths to concise evidence, and an explicit readiness decision.

## Defect handling

The Validator leads baseline, reproduction, matrix, and retesting. The Implementer receives only reproduced defects and owns the smallest safe fix. The Game Designer reviews observable visual/behavioral coherence without expanding activity scope. Do not have two agents mutate the same scene, source files, or Editor session concurrently.

## Validation and evidence

- Logical .NET, Unity Edit Mode, Unity Play Mode, and real-scene Play Mode are separate evidence classes.
- Use Unity-generated `.meta` files only if a necessary regression artifact is added; do not hand-edit serialized scene/prefab YAML.
- Preserve original relevant logs for diagnosis while recording concise summaries and exact result artifact paths.
- Performance is a measured baseline, not a pass/fail budget because no formal budget exists.
- Full user acceptance requires honest V01–V35 status, final regression, and a candid `npc-debug-tools` readiness assessment. A blocked seven-day physical run remains partial.

## Non-goals

New activity types, schedule authoring UI, narrative/AI systems, new production clock, new duplicate runtime integration, broad refactors, third-party packages, and speculative optimization.
