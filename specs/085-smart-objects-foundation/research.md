# Research: Smart Objects Foundation

## Existing project capabilities

- **NPC movement and navigation** — `IslandWaypointWalker` uses a resident's existing `NavMeshAgent`, validates a complete path and can stop/reset it. `IslandWaypointNetwork` loads `Freyja/Assets/_Project/Data/IlhaWaypointNavigation.asset` into the runtime NavMesh. The `Ilha_Principal` scene references this network and existing resident prefabs include agents. Reuse these systems. Confirm reachability from the actual chosen demo positions before claiming navigation acceptance.
- **Test infrastructure** — `com.unity.test-framework` is already present in the package lock and WorldBuilder has Edit Mode and Play Mode test assemblies. Add feature-specific assemblies without changing packages.
- **Unity editor tooling** — Unity 6000.3.25f1, Built-in rendering, `com.unity.pipeline` 1.0.0. Unity MCP scene-editing tools are absent from this agent session. The Unity CLI reports the project Editor/Pipeline endpoint, so use its structured commands for scene work, tests and captures if the endpoint responds. Do not claim MCP-based editing if unavailable.
- **Bench search** — no standalone bench is present among named Unity assets or ThirdParty low-poly model filenames. `Cais_Iteracao01.blend` contains `Banco_barco_0..2`, but these are boat-integrated seats and do not form a freestanding park/shore bench. They are not suitable for the two-slot standalone interaction prefab. Create a deliberately simple provisional bench locally in Blender and keep the editable `.blend` source and replaceable FBX separate from interaction transforms.
- **Well and dock** — no standalone well/pump prefab has been found, so do not create one. Existing dock sources are available; author future `fish`/`watch_sea` slots only if the chosen pier surface is visibly navigable and clear of obstacles.

## Decisions

1. **Lifecycle registry**: use a static active-object index registered from component lifecycle and reset at `RuntimeInitializeOnLoadMethod(SubsystemRegistration)`. This avoids per-frame scene scans and stale static state when Enter Play Mode skips domain reload.
2. **Runtime state**: each Smart Object owns per-slot state for the active session; authoring availability/activities stay serialized. Reservation changes never write into prefab/scene configuration.
3. **Owner cleanup**: a small `SmartObjectReservationOwner` component explicitly stores its current handles and releases them on disable/destroy. `TryReserve` takes this owner component rather than an untracked arbitrary object.
4. **Stable identity**: create a GUID at authoring/reset time, preserve it through moves/saves/reopens, and expose a clear Inspector action to generate a replacement. Detect duplicate IDs across loaded scene objects/prefab instances and display a validation warning plus an explicit repair action; never silently choose which serialized ID to overwrite. Slot IDs are local to their Smart Object.
5. **Deterministic discovery**: candidate list sorts by squared world distance from origin, then ordinal Smart Object ID, then slot ID. Consumers may retry the next slot/object if a candidate becomes unavailable before claim.
6. **Interaction distinction**: `Approach` is a navigable target; `Pose` is a final root transform. The harness confirms occupancy only after arrival and makes only a short explicit alignment to the pose. No sit animation or IK is implied.
7. **Provisional art**: model one simple low-poly wooden bench using local Blender mesh geometry (no paid generation). Keep its material/mesh visual under a replaceable child; collider and named interaction Transforms stay separate in the prefab.

## Alternatives considered

- **Existing `Banco_barco_*` meshes**: rejected as a freestanding interaction asset because they belong to boat construction and do not provide the required stable, unobstructed two-sided seating footprint.
- **Scene-wide `FindObjectsByType` at query time**: rejected because it repeats discovery work and can include disabled/stale objects; lifecycle registration provides the same query without per-frame/global scans.
- **One static capacity counter per object**: rejected because capacity is the number of compatible enabled slots, and per-slot claims are the arbitration unit.
- **Using world coordinates in future routine data**: rejected; consumers refer to object/slot IDs and resolve current transforms at runtime.
- **Adding AI Navigation or a movement stack**: rejected because the island already has a baked `NavMeshData`, a network loader and a waypoint walker.
