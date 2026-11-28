# Research Notes: NPC Integration Validation

## Decisions

- Use the existing source JSON and runtime composition as the authority. The NpcScheduling README expressly describes earlier CLI fixtures as synthetic; do not count them as physical evidence.
- Reuse existing Unity test assemblies, MCP diagnostics, demo harnesses, and prior evidence. Do not add another simulation system or duplicate runtime host.
- Keep logical-week simulation, Editor/Edit Mode tests, Play Mode tests, and real-scene physical evidence as separate result classes.
- Let the configured Validator own baseline and defect reproduction. Route only confirmed defects to the Implementer; request Game Designer review after observed runtime behavior is available.
- The project provides no formal performance budget. Capture comparable metrics and report trends/limits without inventing a pass threshold.

## Existing evidence to recheck

- `specs/087-npc-activity-execution-core/evidencias.md`: .NET/fake integration evidence only; it explicitly does not prove Unity movement, animation, or Smart Objects.
- `specs/089-npc-navigation-system/validacao.md`: three simultaneous navigation paths, invalid destination, cancellation, lifecycle, and pause/watchdog evidence; prior scene was `CadeiraSmartObject_Demo.unity`, so it does not establish schedule-driven routines in the main island.
- `specs/091-npc-animation-system/validacao.md`: earlier Play Mode and Test Runner evidence, with known partial animation cases (including idle sit loop and several cancellation boundaries).
- `specs/092-npc-world-integration/validacao.md`: latest W01–W19 world-composition evidence. The current branch is based on that work; results must be rechecked against the open editor and actual current code.

## Unknowns to resolve in baseline

- Whether production scene contains three independent schedule-driven NPC hosts and supports physical schedule reload.
- Which Unity tests cover runtime composition and Smart Object lifecycle, and whether reruns fit the available session.
- Whether integrated week harness output is deterministic and records resource ownership through cancellations.
- Whether a physical seven-day run fits the environment; duration must not be replaced by logical clock jumps.
- Which Unity Profiler counters are exposed by the current MCP/editor integration.
