// Run with: unity command script eval --file Tools/Grass078/CheckManualGrassEditing.cs --project-path <Freyja>
var grass = UnityEngine.Object.FindFirstObjectByType<Freyja.Environment.CoastalGrass>();
if (!grass) throw new System.InvalidOperationException("Open Ilha_Principal and select the Coastal Grass component.");
var layout = grass.EditorLayout;
if (!layout) throw new System.InvalidOperationException("The active Coastal Grass component has no baked layout.");
var edits = layout.ManualEdits;
var ids = new System.Collections.Generic.HashSet<string>();
foreach (var edit in edits)
{
    if (string.IsNullOrWhiteSpace(edit.StableId) || !ids.Add(edit.StableId))
        throw new System.InvalidOperationException("Manual grass edit IDs must be present and unique.");
    if (edit.Kind != Freyja.Environment.CoastalGrassLayout.ManualKind.Remove && (edit.MeshIndex < 0 || edit.MeshIndex > 2 || edit.Scale <= 0))
        throw new System.InvalidOperationException("Manual placement has an invalid mesh or scale.");
}
var root = new UnityEngine.Vector3(12.3451f, 0, -67.8911f);
var keyA = new Freyja.Environment.CoastalGrassLayout.RootKey(Freyja.Environment.CoastalGrassLayout.ManualSource.Density, 2, root);
var keyB = new Freyja.Environment.CoastalGrassLayout.RootKey(Freyja.Environment.CoastalGrassLayout.ManualSource.Density, 2, root + new UnityEngine.Vector3(.0002f, 0, .0002f));
if (!keyA.Equals(keyB)) throw new System.InvalidOperationException("Root identity must be quantized to millimeters.");
return $"Manual grass harness passed: {edits.Length} edits, {ids.Count} unique IDs; active layout and millimeter key verified.";
