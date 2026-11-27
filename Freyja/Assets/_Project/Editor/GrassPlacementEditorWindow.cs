using System;
using System.Collections.Generic;
using System.Linq;
using Freyja.Environment;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Freyja.Editor
{
    public sealed class GrassPlacementEditorWindow : EditorWindow
    {
        private enum EditMode { Select, Add, Erase }
        private sealed class Root
        {
            public CoastalGrassLayout.RootKey Key;
            public Vector3 Position;
            public float Yaw;
            public float Scale = 1;
            public int Mesh;
            public string ManualId;
        }

        private static GrassPlacementEditorWindow active;
        private CoastalGrass grass;
        private CoastalGrassLayout layout;
        private readonly List<Root> roots = new();
        private GrassExclusionVolume[] exclusionVolumes = Array.Empty<GrassExclusionVolume>();
        private Root selected;
        private EditMode mode;
        private float radius = 0.8f;
        private float spacing = 0.24f;
        private float scale = 1;
        private float scaleVariance = 0.15f;
        private float yaw;
        private float yawVariance = 180f;
        private int meshIndex;
        private int seed = 78084;
        private string status = "Clique em um tufo para selecionar; use Adicionar ou Apagar na Scene View.";
        private Label statusLabel;
        private Slider scaleSlider;
        private Slider yawSlider;
        private bool cacheReady;

        [MenuItem("Freyja/Vegetação/Editar Grama", false, 10)]
        public static void Open()
        {
            var window = GetWindow<GrassPlacementEditorWindow>();
            window.titleContent = new GUIContent("Editar Grama");
            window.minSize = new Vector2(300, 390);
            window.Show();
            window.Focus();
        }

        public static void OpenAndActivate()
        {
            Open();
            if (Selection.activeGameObject && Selection.activeGameObject.GetComponent<Terrain>())
                ToolManager.SetActiveTool<GrassPlacementTool>();
        }

        private void OnEnable()
        {
            active = this;
            SceneView.duringSceneGui += OnSceneGUI;
            EditorApplication.hierarchyChanged += Invalidate;
            Undo.undoRedoPerformed += OnUndoRedo;
            BindGrass();
        }

        private void OnDisable()
        {
            if (active == this) active = null;
            SceneView.duringSceneGui -= OnSceneGUI;
            EditorApplication.hierarchyChanged -= Invalidate;
            Undo.undoRedoPerformed -= OnUndoRedo;
        }

        private void CreateGUI()
        {
            var root = rootVisualElement;
            root.style.paddingLeft = root.style.paddingRight = 10;
            root.style.paddingTop = root.style.paddingBottom = 8;
            root.Add(new Label("Edite a grama baked sem criar objetos por tufo. Clique no terreno para adicionar; clique/arraste para selecionar ou apagar. Cmd/Ctrl+Z desfaz."));
            var modes = new List<string> { "Selecionar / mover", "Adicionar", "Apagar" };
            var modeField = new PopupField<string>("Modo", modes, (int)mode);
            modeField.RegisterValueChangedCallback(evt => { mode = (EditMode)modes.IndexOf(evt.newValue); SceneView.RepaintAll(); });
            root.Add(modeField);
            root.Add(new Slider("Raio", 0.1f, 5f) { value = radius });
            root.Q<Slider>().RegisterValueChangedCallback(evt => radius = evt.newValue);
            var spacingField = new Slider("Espaçamento", 0.12f, 1f) { value = spacing };
            spacingField.RegisterValueChangedCallback(evt => spacing = evt.newValue);
            root.Add(spacingField);
            var mesh = new IntegerField("Mesh (0–2)") { value = meshIndex };
            mesh.RegisterValueChangedCallback(evt => meshIndex = Mathf.Clamp(evt.newValue, 0, 2));
            root.Add(mesh);
            scaleSlider = new Slider("Escala", 0.55f, 1.7f) { value = scale };
            scaleSlider.RegisterValueChangedCallback(evt => { scale = evt.newValue; ChangeSelectedTransform(); });
            root.Add(scaleSlider);
            var varianceField = new Slider("Variação de escala", 0f, 0.5f) { value = scaleVariance };
            varianceField.RegisterValueChangedCallback(evt => scaleVariance = evt.newValue);
            root.Add(varianceField);
            yawSlider = new Slider("Yaw", 0, 360) { value = yaw };
            yawSlider.RegisterValueChangedCallback(evt => { yaw = evt.newValue; ChangeSelectedTransform(); });
            root.Add(yawSlider);
            var yawVarianceField = new Slider("Variação de yaw", 0f, 180f) { value = yawVariance };
            yawVarianceField.RegisterValueChangedCallback(evt => yawVariance = evt.newValue);
            root.Add(yawVarianceField);
            var seedField = new IntegerField("Seed") { value = seed };
            seedField.RegisterValueChangedCallback(evt => seed = evt.newValue);
            root.Add(seedField);
            var transform = new Vector3Field("Posição selecionada");
            root.Add(transform);
            transform.RegisterValueChangedCallback(evt => MoveSelected(evt.newValue));
            var save = new Button(() => { AssetDatabase.SaveAssets(); status = "Overlay salvo no layout baked."; RefreshStatus(); }) { text = "Salvar overlay" };
            root.Add(save);
            root.Add(new HelpBox("O overlay fica no CoastalGrassLayout atribuído. Terreno, máscaras, caminhos, construções, inclinação e volumes de exclusão bloqueiam colocações.", HelpBoxMessageType.Info));
            statusLabel = new Label(status);
            root.Add(statusLabel);
        }

        private void BindGrass()
        {
            grass = Selection.activeGameObject ? Selection.activeGameObject.GetComponentInParent<CoastalGrass>() : null;
            if (!grass) grass = FindFirstObjectByType<CoastalGrass>();
            layout = grass ? grass.EditorLayout : null;
            Invalidate();
        }

        private void Invalidate()
        {
            cacheReady = false;
            roots.Clear();
            SceneView.RepaintAll();
        }

        private void OnUndoRedo()
        {
            Invalidate();
            if (grass) grass.EditorRebuildPlacement();
        }

        private void BuildCache()
        {
            roots.Clear();
            exclusionVolumes = FindObjectsByType<GrassExclusionVolume>(FindObjectsSortMode.None);
            if (!grass || !layout) BindGrass();
            if (!layout) { cacheReady = true; return; }
            AddLayout(layout, CoastalGrassLayout.ManualSource.Layout, false);
            AddLayout(grass.EditorAuthoredLayout, CoastalGrassLayout.ManualSource.Authored, true);
            GrassDensityLayout density = grass.EditorDensityLayout;
            if (density && density.Matches(layout))
                for (int c = 0; c < layout.Cells.Length; c++)
                    foreach (GrassDensityLayout.Tuft tuft in density.GetTufts(c))
                        roots.Add(new Root { Key = new CoastalGrassLayout.RootKey(CoastalGrassLayout.ManualSource.Density, tuft.MeshIndex, tuft.Root), Position = tuft.Root, Yaw = tuft.Yaw, Scale = tuft.Scale, Mesh = tuft.MeshIndex });
            foreach (CoastalGrassLayout.ManualEdit edit in layout.ManualEdits.Where(edit => edit.Kind != CoastalGrassLayout.ManualKind.Remove))
                roots.Add(new Root { Position = edit.Root, Yaw = edit.Yaw, Scale = edit.Scale, Mesh = edit.MeshIndex, ManualId = edit.StableId });
            var hidden = new HashSet<CoastalGrassLayout.RootKey>(layout.ManualEdits.Where(edit => edit.Kind != CoastalGrassLayout.ManualKind.Add).Select(edit => edit.Source));
            roots.RemoveAll(root => hidden.Contains(root.Key));
            cacheReady = true;
        }

        private void AddLayout(CoastalGrassLayout source, CoastalGrassLayout.ManualSource layer, bool authored)
        {
            if (!source) return;
            var pending = new List<Root>();
            foreach (CoastalGrassLayout.Cell cell in source.Cells)
            foreach (CoastalGrassLayout.Batch batch in cell.Batches)
            foreach (Matrix4x4 matrix in batch.Instances)
            {
                Vector3 p = matrix.GetColumn(3);
                pending.Add(new Root { Key = new CoastalGrassLayout.RootKey(layer, batch.MeshIndex, p), Position = p, Yaw = matrix.rotation.eulerAngles.y, Scale = matrix.lossyScale.x, Mesh = batch.MeshIndex });
            }
            Terrain terrain = FindFirstObjectByType<Terrain>();
            if (!terrain) return;
            bool[] valid;
            try { valid = CoastalGrassAuthoring.FilterGrassSites(FindIsland(terrain), pending.Select(r => r.Position).ToArray(), authored); }
            catch (Exception ex) { status = ex.Message; RefreshStatus(); return; }
            for (int i = 0; i < pending.Count; i++)
                if (valid[i] && !IsExcluded(pending[i].Position) && (authored || layer != CoastalGrassLayout.ManualSource.Layout || !grass.EditorIsBlocked(pending[i].Position))) roots.Add(pending[i]);
        }

        private void OnSceneGUI(SceneView view)
        {
            if (!cacheReady) BuildCache();
            if (!grass || !layout) return;
            Event e = Event.current;
            if (e.type == EventType.Layout && mode != EditMode.Select)
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            Terrain terrain = FindFirstObjectByType<Terrain>();
            RaycastHit terrainHit = default;
            bool hit = terrain && terrain.GetComponent<TerrainCollider>() && terrain.GetComponent<TerrainCollider>().Raycast(ray, out terrainHit, 10000);
            Vector3 point = hit ? terrainHit.point : default;
            if (hit)
            {
                Handles.color = mode == EditMode.Erase ? new Color(1, .25f, .15f, .85f) : new Color(.35f, .9f, .3f, .85f);
                Handles.DrawWireDisc(point, terrainHit.normal, radius);
            }
            if (selected != null)
            {
                Handles.color = Color.yellow;
                Handles.DrawWireDisc(selected.Position, Vector3.up, .22f);
                EditorGUI.BeginChangeCheck();
                Vector3 moved = Handles.PositionHandle(selected.Position, Quaternion.identity);
                if (EditorGUI.EndChangeCheck()) MoveSelected(moved);
            }
            bool press = e.type == EventType.MouseDown && e.button == 0;
            bool stroke = e.type == EventType.MouseDrag && e.button == 0 && mode != EditMode.Select;
            if ((!press && !stroke) || !hit || EditorGUIUtility.editingTextField) return;
            if (EditorApplication.isPlayingOrWillChangePlaymode) { status = "Edite em Edit Mode."; RefreshStatus(); return; }
            if (mode == EditMode.Add) AddBrush(point, terrain);
            else if (mode == EditMode.Erase && stroke)
            {
                Root erased = FindNearest(point, radius);
                if (erased != null) Remove(erased);
            }
            else
            {
                selected = FindNearest(point, radius);
                if (mode == EditMode.Erase && selected != null) Remove(selected);
                else if (selected != null)
                {
                    rootVisualElement.Q<Vector3Field>()?.SetValueWithoutNotify(selected.Position);
                    scale = selected.Scale; yaw = selected.Yaw;
                    scaleSlider?.SetValueWithoutNotify(scale); yawSlider?.SetValueWithoutNotify(yaw);
                }
                RefreshStatus();
            }
            e.Use();
        }

        private Root FindNearest(Vector3 point, float range)
        {
            Root best = null;
            float distance = range * range;
            foreach (Root root in roots)
            {
                float d = (root.Position - point).sqrMagnitude;
                if (d < distance) { distance = d; best = root; }
            }
            return best;
        }

        private void AddBrush(Vector3 center, Terrain terrain)
        {
            exclusionVolumes = FindObjectsByType<GrassExclusionVolume>(FindObjectsSortMode.None);
            var sites = new List<Vector3>();
            float step = Mathf.Max(.12f, spacing);
            int count = Mathf.CeilToInt(radius * 2 / step);
            for (int z = 0; z <= count; z++)
            for (int x = 0; x <= count; x++)
            {
                uint h = (uint)(seed + x * 73856093 ^ z * 19349663);
                float jx = ((h & 1023) / 1023f - .5f) * step;
                float jz = (((h >> 10) & 1023) / 1023f - .5f) * step;
                Vector3 p = center + new Vector3((x - count * .5f) * step + jx, 0, (z - count * .5f) * step + jz);
                p.y = terrain.SampleHeight(p) + terrain.transform.position.y;
                if ((p - center).sqrMagnitude <= radius * radius) sites.Add(p);
            }
            bool[] valid;
            try { valid = CoastalGrassAuthoring.FilterGrassSites(FindIsland(terrain).transform, sites.ToArray(), false); }
            catch (Exception ex) { status = ex.Message; RefreshStatus(); return; }
            var edits = layout.ManualEdits.ToList();
            int undo = Undo.GetCurrentGroup(); Undo.SetCurrentGroupName("Add grass tufts"); Undo.RecordObject(layout, "Add grass tufts");
            int added = 0;
            for (int i = 0; i < sites.Count; i++)
            {
                if (!valid[i] || IsExcluded(sites[i]) || grass.EditorIsBlocked(sites[i]) || roots.Any(root => (root.Position - sites[i]).sqrMagnitude < step * step * .45f)) continue;
                Vector3 p = sites[i]; p.y = terrain.SampleHeight(p) + terrain.transform.position.y;
                var edit = new CoastalGrassLayout.ManualEdit { StableId = Guid.NewGuid().ToString("N"), Kind = CoastalGrassLayout.ManualKind.Add, Root = p, MeshIndex = meshIndex, Scale = scale, Yaw = yaw };
                uint randomHash = (uint)(seed + i * 73856093);
                float random = ((randomHash >> 10) & 0xfff) / 4095f;
                edit.Scale = scale * Mathf.Lerp(1f - scaleVariance, 1f + scaleVariance, random);
                edit.Yaw = yaw + Mathf.Lerp(-yawVariance, yawVariance, random);
                edits.Add(edit); roots.Add(new Root { ManualId = edit.StableId, Position = p, Mesh = meshIndex, Scale = edit.Scale, Yaw = edit.Yaw }); added++;
            }
            layout.SetManualEdits(edits.ToArray());
            EditorUtility.SetDirty(layout); Undo.CollapseUndoOperations(undo);
            grass.EditorRebuildPlacement();
            status = $"Adicionados: {added}. Pontos inválidos ou sobrepostos ignorados."; RefreshStatus();
        }

        private bool IsExcluded(Vector3 p)
        {
            foreach (GrassExclusionVolume volume in exclusionVolumes)
                if (volume && volume.isActiveAndEnabled && volume.EditorContains(p)) return true;
            return false;
        }

        private static Transform FindIsland(Terrain terrain)
        {
            Transform current = terrain.transform;
            while (current && current.name != "Island023") current = current.parent;
            return current ? current : terrain.transform;
        }

        private void Remove(Root root)
        {
            var edits = layout.ManualEdits.ToList(); Undo.RecordObject(layout, "Remove grass tuft");
            if (!string.IsNullOrEmpty(root.ManualId))
            {
                int index = edits.FindIndex(edit => edit.StableId == root.ManualId);
                if (index >= 0 && edits[index].Kind == CoastalGrassLayout.ManualKind.Replace)
                {
                    var replacement = edits[index]; replacement.Kind = CoastalGrassLayout.ManualKind.Remove; edits[index] = replacement;
                }
                else edits.RemoveAll(edit => edit.StableId == root.ManualId);
            }
            else
            {
                edits.RemoveAll(edit => edit.Kind != CoastalGrassLayout.ManualKind.Add && edit.Source.Equals(root.Key));
                edits.Add(new CoastalGrassLayout.ManualEdit { StableId = Guid.NewGuid().ToString("N"), Kind = CoastalGrassLayout.ManualKind.Remove, Source = root.Key });
            }
            layout.SetManualEdits(edits.ToArray()); EditorUtility.SetDirty(layout); roots.Remove(root); selected = null;
            grass.EditorRebuildPlacement(); status = "Tufo removido. Cmd/Ctrl+Z desfaz."; RefreshStatus();
        }

        private void MoveSelected(Vector3 position)
        {
            if (selected == null || !layout) return;
            Terrain terrain = FindFirstObjectByType<Terrain>();
            if (!terrain) return;
            Ray ray = new(new Vector3(position.x, terrain.transform.position.y + terrain.terrainData.size.y + 20, position.z), Vector3.down);
            if (!terrain.GetComponent<TerrainCollider>().Raycast(ray, out RaycastHit hit, 10000)) return;
            bool[] valid;
            try { valid = CoastalGrassAuthoring.FilterGrassSites(FindIsland(terrain), new[] { hit.point }, selected.Key.Source == CoastalGrassLayout.ManualSource.Authored); }
            catch { return; }
            if (!valid[0] || IsExcluded(hit.point) || grass.EditorIsBlocked(hit.point)) { status = "Destino inválido: fora da superfície permitida."; RefreshStatus(); return; }
            var edits = layout.ManualEdits.ToList(); Undo.RecordObject(layout, "Move grass tuft");
            int index = !string.IsNullOrEmpty(selected.ManualId) ? edits.FindIndex(e => e.StableId == selected.ManualId) : -1;
            if (index >= 0) { var edit = edits[index]; edit.Root = hit.point; edits[index] = edit; selected.Position = hit.point; }
            else
            {
                edits.RemoveAll(edit => edit.Kind != CoastalGrassLayout.ManualKind.Add && edit.Source.Equals(selected.Key));
                edits.Add(new CoastalGrassLayout.ManualEdit { StableId = Guid.NewGuid().ToString("N"), Kind = CoastalGrassLayout.ManualKind.Replace, Source = selected.Key, Root = hit.point, MeshIndex = selected.Mesh, Yaw = selected.Yaw, Scale = selected.Scale });
                roots.Remove(selected); selected = new Root { ManualId = edits[^1].StableId, Position = hit.point, Mesh = selected.Mesh, Yaw = selected.Yaw, Scale = selected.Scale };
                roots.Add(selected);
            }
            layout.SetManualEdits(edits.ToArray()); EditorUtility.SetDirty(layout); grass.EditorRebuildPlacement(); status = "Posição atualizada."; RefreshStatus();
        }

        private void ChangeSelectedTransform()
        {
            if (selected == null || !layout) return;
            var edits = layout.ManualEdits.ToList();
            int index = !string.IsNullOrEmpty(selected.ManualId) ? edits.FindIndex(e => e.StableId == selected.ManualId) : -1;
            Undo.RecordObject(layout, "Adjust grass tuft");
            if (index >= 0)
            {
                var edit = edits[index]; edit.Yaw = yaw; edit.Scale = scale; edits[index] = edit;
            }
            else
            {
                edits.RemoveAll(edit => edit.Kind != CoastalGrassLayout.ManualKind.Add && edit.Source.Equals(selected.Key));
                var edit = new CoastalGrassLayout.ManualEdit { StableId = Guid.NewGuid().ToString("N"), Kind = CoastalGrassLayout.ManualKind.Replace, Source = selected.Key, Root = selected.Position, MeshIndex = selected.Mesh, Yaw = yaw, Scale = scale };
                edits.Add(edit); selected.ManualId = edit.StableId;
            }
            selected.Yaw = yaw; selected.Scale = scale;
            layout.SetManualEdits(edits.ToArray()); EditorUtility.SetDirty(layout); grass.EditorRebuildPlacement();
        }

        private void RefreshStatus() { if (statusLabel != null) statusLabel.text = status; rootVisualElement.MarkDirtyRepaint(); SceneView.RepaintAll(); }
    }

    [EditorTool("Editar Grama", typeof(Terrain))]
    public sealed class GrassPlacementTool : EditorTool
    {
        public override GUIContent toolbarIcon => new("🌱", "Abrir a ferramenta de edição manual de grama");
        public override void OnActivated() => GrassPlacementEditorWindow.Open();
    }
}
