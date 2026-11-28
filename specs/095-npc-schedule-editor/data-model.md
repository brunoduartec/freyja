# Data Model: NPC Schedule Editor

This is an editor-session model over existing schedule JSON. Only the source JSON remains persistent.

## Source Document

- **Fields**: relative asset path, document kind (`catalog`, `recipe`, `template`, `npc`, `override`), optional stable document ID/NPC ID, JSON text, SHA-256 fingerprint, schema status, and active-scene provider membership.
- **Rules**: source paths must remain under `Assets/_Project/NpcScheduling/Data/`; unsupported/malformed documents are inspect-only; provider membership is observed from the configured scene `TextAsset` references, not inferred from folder placement.

## Activity Definition and Execution Capability

- **Activity Definition**: `id`, `targetKind` (`none`, `location`, `object`), and optional label from `activities.json`.
- **Execution Capability**: activity ID with current recipe-present/recipe-invalid state from `execution/recipes.json`.
- **Rules**: catalog membership and physical executability are distinct. Activity changes never rewrite the target automatically.

## Schedule Entry

- **Fields**: stable `id`, start minute, end minute, activity ID, optional symbolic target ID.
- **Rules**: minutes are integers; ranges are `[start,end)`; start is `00:00–23:59`; end is `00:01–24:00`; start < end; entries in one effective base schedule cannot overlap. Existing gap behavior remains a warning. Overnight spans use entries split across midnight.

## NPC Schedule and Template

- **NPC document**: `schemaVersion`, `npcId`, optional `templateId`, optional direct `entries`, and supported `replaceEntries`, `removeEntries`, `addEntries` operations.
- **Template document**: `schemaVersion`, `id`, and ordered `entries`.
- **Rules**: only existing loader operations are supported. Preserve unedited source identities. Template changes require listing and validating dependent NPC documents before save. Existing runtime world NPCs are direct-entry schedules; demo templates are separately labeled.

## Conditional Override

- **Fields**: `schemaVersion`, stable override ID, NPC ID, supported priority (50/70/90), optional `conditions` (`days`, `weekdays`, `weather`, `flags`), and `entries`.
- **Rules**: existing condition matching governs preview. Fields differ by AND; values within a field list use the core's existing matching. Priority ties use the resolver's existing ordinal ID tie-break. Each override's own entries validate for IDs, times, and internal overlap.

## Editor Draft Session

- **Fields**: selected document, original fingerprint/text, draft text and parsed model, dirty flag, validation results, selected entry, undo/redo snapshots, preview request/result.
- **States**: clean → dirty → valid/invalid → save pending → saved; external source change transitions to conflict; parse/version errors transition to read-only error.
- **Rules**: undo/redo/revert change only the draft. Save is explicit, requires Edit Mode, exact-candidate validation, unchanged source fingerprint, successful replace, and targeted import. Failed/conflicted save preserves both source and draft.

## Destination Evidence

- **Fields**: symbolic ID, semantic kind (region, waypoint, Smart Object, contextual idle point), evidence source, matching capability/region when known, and confidence (`resolved`, `ambiguous`, `unavailable`, `unverified`).
- **Rules**: transient slot occupancy does not invalidate authoring; no reservations, path requests, or world mutations occur during editor refresh/preview.

## Preview and Runtime Snapshot

- **Preview**: selected NPC ID, day/minute and simulated supported context, resolved intent, source/entry, validity window, priority, trace, and warnings from the real resolver.
- **Runtime Snapshot**: existing Debug Monitor record of intended activity/target, executed activity/state, fallback/reason, and execution details.
- **Rules**: preview is predicted intent; runtime snapshot is observed Editor/Play Mode state. Never substitute one for the other.
