# Pesquisa: NPC Debug Tools

## Clock and time controls

- **Decision**: call `CoastalDayNight.AdvancesAutomatically` and `SetHour`, which are the live clock's current APIs. Add an Editor-only transient rate field on that same component, defaulting to `1x`; don't use `Time.timeScale`.
- **Rationale**: the in-game `GameplayDebugPanel` already uses the same clock to pause and set hour. `Time.timeScale` also scales locomotion and operational time. A calendar rate changes only `CoastalDayNight` hour and leaves physical execution at normal Unity time.
- **Limit**: `WorldClockProjection` tracks day by observed hour rollover and rejects time regression. The monitor exposes positive advances only, at most one day per command. It won't claim a multi-day jump or time reversal.
- **Alternatives considered**: new debug clock (duplicates authority), global time scale (accelerates physical operation), direct scene-field editing (persists/debug bypass). All rejected.

## Execution decisions and events

- **Decision**: retain the actual `ActivityExecutionEvent` stream at the existing `NpcActivityController.Emit` boundary, using an Editor-only event and a bounded ring buffer.
- **Rationale**: events already carry sequence, NPC ID, execution ID, kind, step, target, reason and game time. The controller already receives `ResolutionResult.Trace`; keep that trace for snapshot rather than invoking the resolver a second time.
- **Limit**: only events after the Editor diagnostic service subscribes are retained. Subscribe from Editor initialization and reset/correctly reconnect on Play Mode, reload, and assembly reload. No per-frame logs.
- **Decision**: add an explicit “re-evaluate on next driver tick” request bit to the existing controller. This calls the existing resolver once; an identical active commitment is not restarted by the coordinator.
- **Cancel**: use the controller's existing `CancelExternal` and coordinator. Report residual cleanup while ownership exists. A canceled occurrence isn't automatically restarted by the coordinator; a later distinct commitment can run normally.

## Resource and navigation observations

- **Decision**: add an Editor-only copy-returning enumeration method to `SmartObjectRegistry`; take slot state, owner and configured interaction points from registered objects. Don't call candidate search for a diagnostic list because it omits claimed slots.
- **Navigation**: use the resident's existing `IslandWaypointWalker` current status/path data and the operation/intent target when exposed. Do not call NavMesh path calculations in capture or drawing.
- **Animation**: read the configured resident Animator's current state/clip, label operation-to-animation mapping unavailable when the binding doesn't expose it.
- **Decision**: current intent and its valid-until time are the supported next boundary; don't calculate future activity commitments because no public schedule-preview contract exists.

## Editor UI and export

- **Decision**: IMGUI `EditorWindow` in the existing `_Project/Editor` assembly; existing project EditorWindows use `OnGUI`.
- **Rationale**: this matches established project code and avoids a new package, assembly definition, or parallel UI pattern. Unity's EditorWindow scripting API documents `OnGUI` as the way to draw controls.
- **Docs read**: Unity 6 `EditorWindow.OnGUI()` and `EditorGUILayout.BeginHorizontal` scripting API; IMGUI guide's EditorWindow/layout/lifecycle rules. The `docs.unity.com` 6000.3 manual URLs in the installed guide were inaccessible from the docs reader, so the official Unity Scripting API pages were used as the fallback.
- **Polling**: manual refresh plus 2 Hz timer while visible; no scene discovery or DTO construction from every `OnGUI` repaint. Capture is read-only.
- **JSON**: deterministic schema `1`, explicitly ordered fields/arrays, invariant-culture numbers, custom compact writer to represent invalid numeric values as JSON `null` plus field availability. Export under repo `Temp/NpcDebugTools/` (outside `Assets`); curated samples/evidence live in feature docs.
- **API**: public static Editor methods for snapshot/filter/history/JSON; callable by harnesses and Unity MCP script evaluation without opening the window.
- **Override**: not implemented; the only schedule override currently described in the project is authored data, not a temporary debug-session command.

## Baseline and evidence

- Branch `094-npc-debug-tools`; current `HEAD` is inherited from the in-progress `092` integration path. Pre-existing modified lifecycle code/test and untracked 093 report/spec/attachments remain in the checkout.
- Unity MCP confirmed one ready Editor at `Freyja/`, version `6000.3.25f1`, scene `Assets/_Project/Scenes/Ilha_Principal.unity`, Play Mode stopped, scene not dirty, compilation not active.
- Console has 22 current errors/2 warnings per MCP ground truth at baseline; the 12 latest sampled entries are old Unity Pipeline transport errors (before this implementation). Do not clear the console or attribute these to the feature. No baseline compile failure.
- Report `npc-integration-validation-report.md` is **PARCIAL**. It documents three real residents, seat contention/cleanup, previous test counts and unresolved visual/lifecycle cases. Its capture doesn't validate the seated pose.
- Contract review is summarized in this file and the independent audit message; reference snapshots are [physical Play Mode](../093-npc-integration-validation/evidence/physical-playmode.md) and [tests](../093-npc-integration-validation/evidence/test-summary.md).
