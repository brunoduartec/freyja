# Research: NPC Navigation System

## Decisions

### Reuse existing NavMesh and movement

- **Decision**: Continue from `IslandWaypointNetwork`, `IlhaWaypointNavigation.asset`, and `IslandWaypointWalker`; do not add another NavMesh asset, surface, pathfinder, or `NavMeshAgent` owner.
- **Evidence**: `IslandWaypointNetwork` registers the serialized `NavMeshData` on enable. The existing editor setup collects collider geometry, builds a single island asset, samples destinations, checks connected paths, and configures agents. `IslandWaypointWalker` already requests movement and owns the agent lifecycle.
- **Rationale**: The user asks for a reusable layer on top of the project's current infrastructure and explicitly prohibits duplicates.
- **Alternative rejected**: Add AI Navigation `NavMeshSurface` to each NPC or rebake the production island. The package `com.unity.ai.navigation` is absent, existing navigation is built by project code, and no need for a new package or island rebake is proven.

### Do not edit the in-flight adapter/contract

- **Decision**: Do not change `IActivityPorts`, `ActivityExecutionModels`, `NpcUnityActivityPorts`, or the currently edited `IslandWaypointWalker` until the active Unity Integration session is handed off. Prefer its landed operation API; otherwise leave adapter integration pending instead of forking a second movement owner.
- **Evidence**: The existing `090-npc-unity-integration-code` worktree contains uncommitted edits to `ActivityExecution.cs`, `ActivityExecutionModels.cs`, `IslandWaypointWalker.cs`, test harness code, and an untracked `NpcScheduling/Unity/` adapter directory. In the shared contract, `Cancel` changed to take `nowSeconds` and `RestorePose` was removed. The untracked adapter files include `NpcUnityActivityPorts.cs`, `NpcLocationRegistry.cs`, `NpcActivityController.cs`, `NpcAnimatorBindings.cs`, and `NpcScheduleTextAssetProvider.cs`.
- **Rationale**: These are active changes and overlap exactly with movement/adapter ownership. Do not modify shared contracts or duplicate adapters.
- **Alternative rejected**: Implement a second `IActivityPorts` adapter or another controller that directly sets agent destinations.

### Completion checks use path state and physical position

- **Decision**: Treat pending paths and unknown remaining distance as non-terminal. Accept arrival only when the current operation remains valid, the path is no longer pending and complete, and the actual NPC position satisfies horizontal and vertical tolerances relative to its resolved target.
- **Evidence**: Unity 6000.3 `NavMeshAgent.destination` documentation says requested paths can take multiple frames and points to `pathPending`; it says destination can resolve to a nearby valid surface point and recommends checking `SetDestination`'s return for immediate request failure. `remainingDistance` can be infinity when unknown. The agent exposes `pathStatus`, `isOnNavMesh`, velocity, and corners via path APIs. Sources: [`NavMeshAgent.destination`](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/AI.NavMeshAgent-destination.html), [`NavMeshAgent`](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/AI.NavMeshAgent.html), [`NavMeshAgent.remainingDistance`](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/AI.NavMeshAgent-remainingDistance.html).
- **Rationale**: Avoid false arrival when the closest NavMesh point differs from the requested point or when a previous path result is still exposed.
- **Alternative rejected**: `remainingDistance == 0`, stopped velocity, transform warp, or the walker merely disabling its agent as sole arrival proof.

### Use finite authored region points and existing approach slots

- **Decision**: Use existing authored waypoint Transform references for region demonstrations; use the consumer-selected Smart Object approach Transform. Navigation does not query for targets or own reservation handles.
- **Evidence**: Existing waypoint setup creates authored points and identifies disconnected points. Smart Objects define slots with separate approach and interaction poses.
- **Rationale**: Selection/scheduling and reservation ownership belong to other systems.
- **Alternative rejected**: Add a general location registry, procedural wander planner, or reservation policy.

### Test in existing Editor and isolate fixture content

- **Decision**: Use the connected Unity Editor instance only; use the existing removable `CadeiraSmartObject_Demo` as the first candidate. Add at most a small demo harness/fixture when physical inspection confirms there is no suitable three-agent setup.
- **Evidence**: Unity MCP reports one ready Editor at the project's `Freyja/` path, Unity `6000.3.25f1`. The active scene is `CadeiraSmartObject_Demo`. Its hierarchy contains `Waypoints da ilha`, a `SmartObject` chair and `SmartObjectDemoHarness`.
- **Rationale**: The user forbids a second Editor and unnecessary production-scene changes.
- **Alternative rejected**: Copy the full Unity project for a second Editor. A full worktree checkout exhausted available disk; the feature branch is now sparse.

## Unresolved implementation gates

- Confirm final `IslandWaypointWalker` operation API and whether it meets NAV-01–NAV-07 before editing movement code.
- Confirm whether the active demo scene exposes three NPC fixtures and a real Smart Object with two approach slots.
- Establish a trustworthy Console compile baseline. The current MCP console reports no compile-failure flag but has accumulated errors, several from pipeline socket disconnects after an oversized hierarchy response; this is not evidence of a clean baseline.
