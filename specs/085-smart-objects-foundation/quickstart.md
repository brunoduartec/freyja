# Quickstart: Smart Objects Foundation

## Author a Smart Object prefab

1. Add `SmartObject` to the prefab root and give the instance a stable object ID. Run the Inspector validator; repair an ID only when the duplicate warning identifies it.
2. Add child Transforms for each slot's approach and final pose. Keep them outside the visual mesh and colliders; place approach on the NavMesh and orient both transforms toward the intended activity.
3. Add slots in the Inspector, assign local stable slot IDs, supported activity strings and both Transform references. Set enabled only when the slot is usable.
4. Reuse the root visual/physics arrangement. For the provisional bench, the generated mesh is a replaceable visual child; collider and interaction transforms are separate children.
5. Check Scene View Gizmos and the Inspector validation report for IDs, references, activities and slot state.

## Run the chair demonstration

1. Open `Freyja/Assets/_Project/Scenes/CadeiraSmartObject_Demo.unity` in Unity 6000.3.25f1.
2. Enter Play Mode. `Elisa Neves` starts near the square. The harness reserves the chair's `sit` slot, follows the existing `IslandWaypointWalker` NavMesh route, aligns at the final pose and applies a humanoid seated pose.
3. The overlay reports `OccupiedAtPoseReference` while she sits. Press **F1** to cancel a moving request, **F2** to probe an invalid/unreachable target, or **F3** to stand and release all active owner handles. The slot should return to `Available`.
4. The seated pose is procedural and immediate; no transition clip is present in the installed resident motion set.

## Run tests

When the target project is not already open in the Editor, run from the repository root:

```sh
unity test Freyja --mode EditMode --filter Freyja.SmartObjects --output /tmp/freyja-smartobjects-edit.xml -- -nographics
unity test Freyja --mode PlayMode --filter Freyja.SmartObjects --output /tmp/freyja-smartobjects-play.xml -- -nographics
```

If using the connected Editor Pipeline, recompile, inspect Console, run both suites and capture Scene View at `specs/085-smart-objects-foundation/evidence/CadeiraSmartObject_Sentada.png`. Record the actual Editor endpoint, test outcome, and screenshot path in `validacao.md`. If the Editor, navigation prerequisites or visual capture are unavailable, record that exact blocker; test logs do not count as visual validation.

The harness lives in `Freyja/Assets/_Project/Scripts/Prototype/SmartObjectDemoHarness.cs` so it can reference the existing `IslandWaypointWalker` in Assembly-CSharp. No complete sit animation, IK, activity mechanics or NPC scheduler are included.
