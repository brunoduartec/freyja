# Research: NPC Schedule Editor

## Decisions

### Use an editor-only IMGUI window

- **Decision**: Follow the existing `NpcDebugMonitorWindow` and add the editor under `Assets/_Project/Editor/NpcScheduleEditor/` with menu path `Freyja/NPC/Schedule Editor`.
- **Rationale**: The project already uses `EditorWindow` and IMGUI for its NPC monitor. The UI request is a desktop authoring tool; adding UI Toolkit setup or packages would not improve this delivery.
- **Alternatives considered**: UI Toolkit (no established pattern required here), runtime UI (wrong lifecycle and risks shipping authoring controls).

### Keep JSON assets canonical and build an in-memory draft

- **Decision**: Discover schedule JSON assets under the existing `NpcScheduling/Data` root, use `AssetDatabase` paths to associate assets with source files, and inspect the scene's configured `NpcScheduleTextAssetProvider` references to label runtime-loaded documents. Edit the JSON document through a draft model and write back only after explicit save.
- **Rationale**: The runtime reads `TextAsset`s assigned to the provider; it does not scan folders. Folder discovery gives the author access to current NPC/template/override data without misrepresenting demo documents as active. The provider reference set gives the necessary runtime status.
- **Alternatives considered**: Treat every JSON in the folder as active (incorrect), add a new registry asset (parallel schedule source), or modify runtime loading to scan all files (unapproved runtime behavior change).

### Reuse strict core loading and resolution

- **Decision**: Build `ScheduleDataDocuments` from disk/draft text and invoke `ScheduleDataLoader.Load`; construct `ScheduleResolver` from the returned `ResolvedScheduleData` for point-in-time preview. Derive executability from `ActivityRecipeLoader.Load` against `execution/recipes.json`.
- **Rationale**: These APIs own the semantics for strict schema validation, inheritance operations, conditions, overlap, warnings, priority selection, and recipe availability.
- **Alternatives considered**: UI-only validation/resolution (would drift), or a second schedule data schema (forbidden).

### Distinguish destination identity from physical verification

- **Decision**: Present the `targetKind` from the activity catalog. For `wander`, derive choices from active `NpcLocationRegistry` point/region data; for `sit`, derive known candidates from active scene `SmartObject` data and its supported capabilities/region; for `idle`, expose the NPC-specific configured idle point when known. Keep logical JSON targets symbolic. If a compatible destination cannot be proven from the loaded scene, show unverified status rather than blocking it as invalid.
- **Rationale**: Runtime contracts resolve `wander` regions to waypoint IDs, `sit` through dynamic Smart Object slot reservation, and `idle` through each NPC host's configured fallback point. A slot's current occupancy is transient and cannot be used as an authoring error.
- **Alternatives considered**: Store transforms/GUIDs/slot IDs (doesn't match runtime JSON), reserve/test paths during authoring (mutates runtime), or invent a global static destination list (parallel registry).

### Preserve existing document fields and reject unsafe schemas

- **Decision**: Use the Editor's existing Newtonsoft.Json support for document-level JSON edits/formatting, validate candidates through `ScheduleDataLoader`, write stable UTF-8, and block editing/saving of malformed or unsupported documents. Keep unedited supported fields and IDs intact; do not migrate schema versions.
- **Rationale**: The core has a strict deserializer but no writer. Newtonsoft is already present in the NPC Debug Editor and allows structural edits without a new persisted format. The core remains the semantic authority.
- **Alternatives considered**: `JsonUtility` (does not preserve the required JSON structure/optional fields), hand-built loose serialization for a parallel schema, or replacing invalid files with defaults (data loss).

### Guard concurrent edits and file failure

- **Decision**: Capture a SHA-256 fingerprint at load. Before save, verify the target still exists with the same fingerprint; then write to a temporary sibling file and replace atomically, reimport only that asset, and restore the original content if replacement/import fails. Keep draft and undo snapshots in memory; clean temporary files in all paths.
- **Rationale**: This prevents silent overwrites and limits the write to one selected document at a time. It can be verified with an injected conflict and failure path.
- **Alternatives considered**: Direct truncate/write (can lose original data), global refresh (unnecessary project work), and multi-file atomic save (not required for the first implementation).

### Use session-scoped undo/redo snapshots

- **Decision**: Maintain a bounded, in-memory snapshot history per edited document. A document switch with unsaved changes prompts Save, Discard, or Cancel. Save clears the undo branch only after successful import; Revert reloads the source only when no conflict is unresolved.
- **Rationale**: Schedule data is file-backed, not a Unity object; `UnityEditor.Undo` cannot reverse an external JSON write. Session snapshots match the explicit draft semantics.
- **Alternatives considered**: Unity object Undo (does not own file contents), auto-save on edit (unsafe), or persistent draft database (a second source).

### Keep Play Mode inspection-only

- **Decision**: Disable mutation and saving in Play Mode. Apply edited TextAsset values on the next normal Play Mode entry. Integrate selected-NPC comparison via `NpcDebugDiagnostics` and a small focus method on the existing monitor window.
- **Rationale**: No supported safe schedule hot reload was found; host state and slot cleanup are tied to current execution. Monitor snapshots already distinguish intended from executed activity.
- **Alternatives considered**: New hot reload, mutation of live hosts, or duplicating monitor state (explicitly outside scope).

## Environment Findings

- Unity MCP reports Editor `6000.3.25f1`, project root `Freyja/`, main scene `Ilha_Principal`, Edit Mode, no compile failure. The Console's historic counts are high; compare specific entries and timestamps during implementation.
- Schedule Core is plain C# and the Unity provider loads configured `TextAsset` documents. The loader has no write/round-trip API.
- Current world NPC JSON files are independent direct-entry documents. Current profession templates and `demo_*` schedules are separate data and are not proof of active world NPC behavior.
- `NpcLocationRegistry` currently uses serialized point/region/Transform rows. `SmartObjectRegistry` exposes editor-visible registered objects; sit reservations are dynamic.
- Debug Monitor's selected NPC state is private to its window; the diagnostics snapshot/capture APIs are public and reusable.
- Integration Validation and NPC Debug evidence show real movement/sitting, debug state, and scene operation. Debug Tools D13 temporary override is explicitly unsupported; D25 schedule/execution domain suites are partial. No editor authoring behavior has been validated yet.
