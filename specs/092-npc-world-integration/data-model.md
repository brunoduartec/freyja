# Data Model: NPC World Integration

## NPC host configuration

One `NpcActivityController` on each configured resident is the stable runtime identity and owns exactly one resolver/coordinator. Its scene/prefab references identify the existing data provider, shared location registry, per-character Unity activity ports and animator bindings. A personal idle destination point is configured on that NPC's port because the current `idle` schedule activity has no JSON target.

Validation: non-empty unique ID; one active host per ID; all required references active; actor includes `SmartObjectReservationOwner`, `IslandWaypointWalker` and semantic animation bindings; its walker uses the registry's same waypoint network.

## World runtime driver

One scene driver references the existing `CoastalDayNight` and a serialized list of NPC controllers. It reads the clock once per frame, constructs one immutable `GameTimeSnapshot` and `WorldContextSnapshot`, then passes both and scaled operational seconds to each valid host. It does not search globally for NPCs, load JSON per frame or own activity state.

Its day number starts at 1 each Play Mode session and increments only when the observed clock wraps naturally across midnight. The current game supplies no persistent campaign day, weekday origin, weather or flags; demo routines do not require those conditions. A backward time reset that is not a midnight wrap is reported and does not replay prior agenda windows.

## Schedule documents

The shared provider loads existing activity catalog and recipes plus all referenced NPC JSON TextAssets. Each demonstration NPC has one distinct stable `npcId`; its entries reference only supported activity IDs (`idle`, `wander`, `sit`) and registered location/Smart Object region IDs. The loader and schema remain authoritative. Editing files requires Unity reimport and a Play Mode restart.

## Destination registry

`NpcLocationRegistry` owns explicit `pointId` and `regionId` mappings to active Transforms below one `IslandWaypointNetwork`. Shared square/beach wander points and per-NPC home points use explicit IDs. Bench Smart Objects use the sit region ID and have authored approach and pose Transforms per slot. No GameObject-name inference or frame-time lookup is allowed.

## Runtime intent and ownership

`ActivityIntent` is the resolver's current planned commitment, not evidence that the NPC arrived or performed it. `ActivitySnapshot` and execution events provide physical workflow status. Smart Object reservations are exact handles scoped to the owner and execution. Ownership persists through occupied pose restoration and approach movement; release happens only after the actor has left the seat or the NPC is destroyed and registry cleanup invalidates its owner.

## Lifecycle

`Disabled/Uninitialized → Ready → Executing → Completed | Cancelled | Failed`. A terminal snapshot may retain residual ownership while safe cleanup is pending. The controller retries cleanup when it receives subsequent valid world snapshots. Scene/host teardown invokes the existing owner's cleanup; runtime-day count and execution state are not saved.
