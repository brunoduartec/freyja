# Quickstart: Smart Objects Foundation

## Author a Smart Object prefab

1. Add `SmartObject` to the prefab root and give the instance a stable object ID. Run the Inspector validator; repair an ID only when the duplicate warning identifies it.
2. Add child Transforms for each slot's approach and final pose. Keep them outside the visual mesh and colliders; place approach on the NavMesh and orient both transforms toward the intended activity.
3. Add slots in the Inspector, assign local stable slot IDs, supported activity strings and both Transform references. Set enabled only when the slot is usable.
4. Reuse the root visual/physics arrangement. For the provisional bench, the local low-poly mesh is a replaceable visual child; collider and interaction transforms are separate children. The demo pose references sit at floor height just in front of the seat: without a sit animation the residents stand there, rather than intersecting or appearing to carry the bench.
5. Check Scene View Gizmos and the Inspector validation report for IDs, references, activities and slot state.

## Run the demonstration

1. Open `Freyja/Assets/_Project/Scenes/SmartObjects_Demo.unity` in Unity 6000.3.25f1.
2. Confirm `IslandWaypointNetwork` is ready and the three agents start on the loaded `IlhaWaypointNavigation` mesh.
3. Enter Play Mode. The harness requests `sit` for three agents: two distinct slots at bench A and fallback to bench B. Each agent must arrive at its assigned approach before the harness claims Occupied and aligns briefly to the final pose.
4. Press **F1** to cancel the first moving request, **F2** to probe an invalid/unreachable target for the first request, or **F3** to release all active owner handles. Confirm the slot can be claimed again.
5. In the same isolated scene, the existing `Cais de pedra` has future-only `fish_west` and `watch_sea_east` slots. Both approach and pose were sampled on NavMesh, and a complete resident route was checked. No suitable well asset exists. These tags do not run activity mechanics.

## Run tests

When the target project is not already open in the Editor, run from the repository root:

```sh
unity test Freyja --mode EditMode --filter Freyja.SmartObjects --output /tmp/freyja-smartobjects-edit.xml -- -nographics
unity test Freyja --mode PlayMode --filter Freyja.SmartObjects --output /tmp/freyja-smartobjects-play.xml -- -nographics
```

The completed connected Editor run used Unity `6000.3.25f1` on Pipeline port `7801`: recompilation reported zero errors; Edit Mode passed 12/12 and Play Mode passed 6/6. Visual evidence and the manual runtime state query are recorded in `validacao.md` and its evidence folder.

The harness lives in `Freyja/Assets/_Project/Scripts/Prototype/SmartObjectDemoHarness.cs` so it can reference the existing `IslandWaypointWalker` in Assembly-CSharp. No complete sit animation, IK, activity mechanics or NPC scheduler are included.
