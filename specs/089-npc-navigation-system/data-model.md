# Data Model: NPC Navigation System

## Navigation Operation

One physical movement request owned by one NPC consumer.

| Field | Meaning | Lifetime |
|---|---|---|
| NPC/owner ID | Consumer identity used to reject unrelated calls | Operation |
| Operation ID | Monotonic or otherwise unique identity for this owner's current request | Operation |
| Requested destination | Original world point or selected Transform position | Operation |
| Effective destination | Resolved reachable point approved for this request | Operation |
| State | Idle, PathPending, Moving, Arrived, Failed, or Cancelled equivalent | Runtime |
| Path state | Pending/complete/partial/invalid and optional corners | Runtime |
| Position/velocity/direction | Actual movement telemetry for diagnosis and future animation | Runtime |
| No-progress duration | Physical runtime time without displacement during expected movement | Runtime |
| Terminal reason | Explicit failure/cancellation reason | Runtime |

Terminal results are immutable for that operation identity. Calls bearing a stale identity cannot change a later operation. No runtime operation state is serialized into NPC prefabs.

## Navigation Region

- **Region ID/name**: Human-authored test label only; not a general location registry.
- **Candidate points**: Finite existing waypoint Transform references.
- **Validation rules**: Candidate is within the configured region, samples to the correct navigable area/level, and has a complete route from the NPC.
- **Selection owner**: Activity/harness consumer. Navigation performs no repeating scheduler loop.

## Smart Object Approach

- **Source**: A specific slot selected by a Smart Object consumer.
- **Approach transform**: The position navigation is allowed to target.
- **Reservation**: Consumer-owned handle; never transferred into navigation state.
- **Interaction pose**: Separate pose owned by later alignment/animation work; navigation does not force the NPC there.

## Demonstration Fixture

- Three independently configured agent/walker references.
- A finite set of destination/region Transform markers.
- Optional real Smart Object slot references; temporary markers are labeled as fixtures.
- Test phase/command values may be serialized as authoring inputs; operation ID, status, telemetry, and last result are runtime-only.

