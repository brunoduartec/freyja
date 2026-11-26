# Data Model: Smart Objects Foundation

## SmartObject

Authored Unity component attached to an object/prefab root.

| Field | Purpose | Rules |
|---|---|---|
| `objectId` | Stable serialized instance identity | Generated once, never position-derived; non-empty; duplicates are diagnosed and repaired explicitly in Inspector. |
| `regionId` | Optional logical grouping | Empty means unrestricted. Exact ordinal match when a query supplies a region. |
| `slots` | Configured interaction points | Local slot IDs unique within object; list order does not decide reservation priority. |

Object lifecycle registers active, enabled objects; disable/destroy unregisters and invalidates all runtime claims. Re-enable joins with all slots Available.

## InteractionSlot (serialized configuration)

| Field | Purpose | Rules |
|---|---|---|
| `slotId` | Stable local slot identity | Non-empty and unique within owner; may repeat on other objects. |
| `activityIds` | Supported future capabilities | Normalized ordinal strings such as `sit`, `collect_water`, `fish`, `watch_sea`; one slot may list multiple values. |
| `approach` | Navigable arrival Transform | Required for movement validation; separate from final pose. |
| `pose` | Final root position/orientation Transform | Required; used only after the harness reaches approach. |
| `enabled` | Authoring availability | Disabled slots cannot be offered or claimed. |

`Disabled` is derived from object inactive/disabled or slot configuration; no duplicated capacity count is stored.

## Runtime slot state

`Available → Reserved → Occupied → Available`; object/slot/owner disable or destruction invalidates the active claim and returns an enabled surviving slot to `Available`. Disabled configuration reports `Disabled` without erasing its activities/Transforms. Runtime state does not serialize.

| Runtime value | Purpose |
|---|---|
| `state` | Available, Reserved, Occupied or Disabled display/query state. |
| `owner` | Exact `SmartObjectReservationOwner` reference that claimed the slot. |
| `token` | New non-empty token per successful reserve; distinguishes old handles after release/reclaim. |
| `activityId` | The specific compatible activity selected for this claim. |

## ReservationHandle

Immutable runtime value containing Smart Object reference/ID, slot ID, owner reference and token. It is valid only when all four match the slot's current runtime claim. A value from another owner or an earlier token cannot occupy/release a new claim. Releasing an invalid/already-released handle is a safe no-op.

## SmartObjectRegistry

Lifecycle-managed index of active Smart Objects keyed by ordinal ID, with a separate set for duplicate diagnostics. Queries filter by object availability, optional region and enabled compatible slots, then return candidates sorted by distance, object ID and slot ID. `TryReserve` revalidates the candidate atomically on the Unity main thread and returns an explicit failure code when it lost a race or has invalid inputs.

## SmartObjectReservationOwner

Consumer-side lifecycle component. Tracks handles it owns; explicit release removes the matching handle, while disable/destroy releases all current handles. The demo harness may keep its handles in the owner component and request a release/cancel, but does not retain resource state independently.
