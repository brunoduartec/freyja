# Data Model — NPC Unity Integration

- **Runtime schedule documents**: Existing catalog, templates, NPC schedules and overrides passed by explicit named TextAsset references. Loader output is the existing immutable `ResolvedScheduleData`; no copied JSON is authored.
- **Runtime recipe document**: Existing `ActivityRecipeFile`, validated once and passed to the existing `ActivityCoordinator` as recipes keyed by `ActivityId`.
- **World snapshot**: Existing `GameTimeSnapshot` and `WorldContextSnapshot`, supplied by an actual game provider for every resolve/tick. This feature does not create calendar or context state.
- **Location point**: Serialized stable `locationId` plus Transform. A region may expose an ordered list of child point references with stable IDs; no transform-name or coordinate fallback.
- **Operation identity**: `(npcId, executionId, operationId)`; adapter rejects polls/cancel calls that do not match its active operation owner.
- **Smart Object claim**: Adapter's opaque port token maps to the exact existing `ReservationHandle`, `SmartObjectReservationOwner`, NPC ID and execution ID. Mapping is removed only after release confirmation.
- **Animation binding**: Recipe animation ID maps to an explicitly configured Animator state/trigger and bounded completion state/exit rule. Missing binding is an operation failure.
- **Activity diagnostics**: Existing event/snapshot identifiers, source/entry, effective target, step, operation ID and stable reason code; transition/failure only, never per-frame output.
