# Editor Data Contract

## Scope

This contract describes the authoring boundary for the existing schedule data. It does not introduce runtime APIs, a new schema, or a second scheduler.

## Canonical sources

| Document | Source path | Reader/validator | Runtime status |
|---|---|---|---|
| Activity catalog | `Freyja/Assets/_Project/NpcScheduling/Data/activities.json` | `ScheduleDataLoader` | Scene provider reference |
| Activity recipes | `Freyja/Assets/_Project/NpcScheduling/Data/execution/recipes.json` | `ActivityRecipeLoader` | Scene provider reference |
| Templates | `.../Data/templates/*.json` | `ScheduleDataLoader` | Only provider-referenced files are runtime-loaded |
| NPC schedules | `.../Data/npcs/**/*.json` | `ScheduleDataLoader` | Active scene provider references identify loaded schedules |
| Overrides | `.../Data/overrides/*.json` | `ScheduleDataLoader` | Only provider-referenced files are runtime-loaded |

All write paths must resolve under the canonical data root. A file found by asset discovery is an authoring document, not automatically a runtime schedule. Show active-provider membership separately.

## Current schema v1

- Times are `HH:mm`, stored as minute intervals `[start,end)`; `24:00` is permitted only as an end.
- Activity targets follow catalog `targetKind`: targetless `none`, symbolic target required for `location`/`object`.
- Base schedule entries cannot overlap. Gaps are accepted with diagnostics and resolve through the real fallback behavior.
- NPC template customization uses only `entries`, `replaceEntries`, `removeEntries`, and `addEntries` as accepted by `ScheduleDataLoader`.
- Overrides use priorities 50, 70, or 90 and the existing `days`, `weekdays`, `weather`, and `flags` condition fields. Resolver matching and ID tie-break are authoritative.
- Strict JSON and schema version validation are authoritative. Parse failure or unsupported versions are read-only and never defaulted or migrated.

## Preview contract

The editor provides an NPC ID, `GameTimeSnapshot`, `WorldContextSnapshot`, and candidate `ScheduleDataDocuments` to `ScheduleDataLoader.Load`, then uses the resulting `ScheduleResolver.Resolve`. Preview must not change clock, hosts, ports, reservations, animation, or scene state. A preview result is an intended action, not proof of physical execution.

## Destination contract

- Persist symbolic IDs only.
- `wander` resolves waypoint IDs through a region in `NpcLocationRegistry`.
- `sit` matches registered Smart Objects with `sit` capability and region/target, then reserves a slot dynamically at runtime.
- `idle` uses the per-NPC configured idle destination point.
- If current scene data cannot prove a choice, label it unverified. Current slot occupancy and Edit Mode path availability are not authored schedule validity.

## Save contract

1. Save is explicit and Edit Mode only.
2. Validate the exact serialized candidate with the real loader and recipe loader.
3. Compare the current target fingerprint to the fingerprint recorded at load. A missing or changed file is a conflict; keep both external content and the draft.
4. Write a temporary sibling, replace the target, remove temporary files, and import only the target asset. If replace or import fails, restore original content and leave the draft dirty.
5. A successful file save is not immediate runtime reload. The existing provider reads TextAssets on normal Play Mode initialization.

## Debug Monitor contract

Use `NpcDebugDiagnostics.CaptureNpc` and existing snapshot data. Compare saved/draft resolver intent with observed runtime intent and execution only when the scene/session/data version is identified. Open/focus the existing monitor; do not create another snapshot service or history buffer.
