using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Freyja.NpcDebug.Editor;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Freyja.NpcScheduling.Editor
{
    public sealed class NpcScheduleEditorWindow : EditorWindow
    {
        private readonly NpcScheduleEditorRepository repository = new NpcScheduleEditorRepository();
        private readonly NpcScheduleEditorSession session = new NpcScheduleEditorSession();
        private readonly NpcScheduleEditorPreview preview = new NpcScheduleEditorPreview();
        private Vector2 scroll;
        private string search = "";
        private string status = "";
        private string arrayName = "entries";
        private string previewNpcId;
        private string targetText = "";
        private string removeIdText = "";
        private string previewFlags = "";
        private const float TimelineHeight = 78f;
        private string validationCacheKey;
        private NpcScheduleValidation validationCache;
        private NpcDebugNpcSnapshot runtimeSnapshot;
        private string runtimeSnapshotNpcId;
        private double nextRuntimeSnapshot;
        private bool destinationRefreshQueued;
        private JObject pendingInheritedEntry;
        private string pendingInheritedTemplate;

        [MenuItem("Freyja/NPC/Schedule Editor")]
        public static void Open()
        {
            NpcScheduleEditorWindow window = GetWindow<NpcScheduleEditorWindow>();
            window.titleContent = new GUIContent("NPC Schedule Editor");
            window.minSize = new Vector2(760f, 540f);
            window.Refresh(true);
            window.Show();
        }

        private void OnEnable()
        {
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update += OnEditorUpdate;
            EditorApplication.hierarchyChanged -= QueueDestinationRefresh;
            EditorApplication.hierarchyChanged += QueueDestinationRefresh;
            EditorApplication.projectChanged -= OnProjectChanged;
            EditorApplication.projectChanged += OnProjectChanged;
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
            if (repository.Documents.Count == 0) Refresh(true);
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.hierarchyChanged -= QueueDestinationRefresh;
            EditorApplication.projectChanged -= OnProjectChanged;
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            EditorApplication.delayCall -= RefreshDestinationsDelayed;
        }

        private void OnGUI()
        {
            bool readOnly = EditorApplication.isPlaying || session.Document == null || session.Document.ReadOnly;
            EditorGUILayout.LabelField("NPC Schedule Editor", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Rascunho local sobre JSON canônico · salvar é explícito · preview é intenção prevista", EditorStyles.miniLabel);
            if (EditorApplication.isPlaying) EditorGUILayout.HelpBox("Play Mode: inspeção e preview somente. Edição e gravação serão habilitadas no Edit Mode.", MessageType.Info);
            DrawDocumentToolbar(readOnly);
            if (session.Document == null) { EditorGUILayout.HelpBox("Nenhum documento de agenda foi encontrado.", MessageType.Warning); return; }
            DrawDocumentSummary();

            scroll = EditorGUILayout.BeginScrollView(scroll);
            DrawTimeline();
            DrawEntryList();
            DrawEntryForm(readOnly);
            DrawSupportedFields(readOnly);
            DrawValidationAndPreview();
            DrawRuntimeComparison();
            EditorGUILayout.EndScrollView();
            DrawFooter(readOnly);
        }

        private void DrawDocumentToolbar(bool readOnly)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            search = GUILayout.TextField(search, GUI.skin.FindStyle("ToolbarSeachTextField") ?? EditorStyles.toolbarTextField, GUILayout.MinWidth(170));
            if (GUILayout.Button("Atualizar", EditorStyles.toolbarButton, GUILayout.Width(74))) Refresh(false);
            if (repository.Documents.Count > 0)
            {
                NpcScheduleDocument[] visible = FilteredDocuments();
                string[] names = visible.Select(x => x.Label).ToArray();
                int current = Math.Max(0, Array.FindIndex(visible, x => x.AssetPath == session.Document?.AssetPath));
                using (new EditorGUI.DisabledScope(visible.Length == 0))
                {
                    int next = EditorGUILayout.Popup(current, names, EditorStyles.toolbarPopup, GUILayout.MinWidth(270));
                    if (next != current && next >= 0 && next < visible.Length) SelectDocument(visible[next]);
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            using (new EditorGUI.DisabledScope(readOnly || !session.Dirty))
            {
                if (GUILayout.Button("Desfazer")) session.Undo();
                if (GUILayout.Button("Refazer")) session.Redo();
                if (GUILayout.Button("Reverter")) Revert();
            }
            using (new EditorGUI.DisabledScope(readOnly || !session.Dirty))
                if (GUILayout.Button("Salvar JSON", GUILayout.Width(110))) Save();
            using (new EditorGUI.DisabledScope(readOnly || session.Document.Kind != NpcScheduleDocumentKind.Npc))
                if (GUILayout.Button("Duplicar NPC", GUILayout.Width(105))) DuplicateNpc();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawDocumentSummary()
        {
            NpcScheduleDocument doc = session.Document;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(doc.Label + (session.Dirty ? "  ·  NÃO SALVO" : ""), EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Origem: " + doc.AssetPath, EditorStyles.miniLabel);
            EditorGUILayout.LabelField("Estado da fonte: " + doc.ValidationStatus + " · uso: " + doc.UsageLabel + " · " + doc.ValidationReason, EditorStyles.wordWrappedMiniLabel);
            NpcScheduleValidation draftState = GetCurrentValidation();
            EditorGUILayout.LabelField("Rascunho: " + (draftState.IsValid ? "válido pelo loader/recipes" : "inválido · " + draftState.Error), EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.LabelField("Provider da cena ativa: " + (doc.RuntimeLoaded ? "referenciado" : "não referenciado"), EditorStyles.miniLabel);
            if (doc.Kind == NpcScheduleDocumentKind.Template)
            {
                string[] dependents = repository.Documents.Where(x => x.Kind == NpcScheduleDocumentKind.Npc && (string)Parse(x.Text)?["templateId"] == doc.Id).Select(x => x.Id).ToArray();
                EditorGUILayout.LabelField("NPCs dependentes: " + (dependents.Length == 0 ? "nenhum" : string.Join(", ", dependents)), EditorStyles.miniLabel);
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawTimeline()
        {
            bool npcSchedule = session.Document.Kind == NpcScheduleDocumentKind.Npc;
            EditorGUILayout.LabelField((IsTemplateBasedNpc() ? "Agenda efetiva herdada" : npcSchedule ? "Agenda efetiva do NPC" : "Agenda") + " · linha de 24 horas · intervalos [início, fim)", EditorStyles.boldLabel);
            JArray entries = npcSchedule ? EffectiveNpcEntries() : CurrentEntries();
            if (npcSchedule && entries == null)
            {
                EditorGUILayout.HelpBox("Agenda efetiva indisponível enquanto este rascunho não passar pelo loader. A operação local permanece separada abaixo.", MessageType.Error);
                return;
            }
            Rect rect = GUILayoutUtility.GetRect(100, TimelineHeight, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, new Color(.13f, .15f, .18f));
            for (int h = 0; h <= 24; h += 3)
            {
                float x = rect.x + rect.width * h / 24f;
                EditorGUI.DrawRect(new Rect(x, rect.y + 20, 1, 54), new Color(.34f, .37f, .41f));
                GUI.Label(new Rect(x + 3, rect.y + 2, 38, 18), h.ToString("00") + ":00", EditorStyles.miniLabel);
            }
            if (entries == null) return;
            NpcScheduleValidation validation = GetCurrentValidation();
            List<JObject> timelineItems = entries.OfType<JObject>().ToList();
            DrawGapMarkers(rect, timelineItems);
            foreach (JObject item in timelineItems)
            {
                if (!TryMinute((string)item["start"], false, out int start) || !TryMinute((string)item["end"], true, out int end)) continue;
                float x = rect.x + rect.width * start / 1440f;
                float width = Math.Max(3, rect.width * (end - start) / 1440f);
                bool selected = (string)item["id"] == session.SelectedEntryId;
                Rect block = new Rect(x, rect.y + 25, width, 42);
                Color color = selected ? new Color(.22f, .55f, .83f) : new Color(.28f, .42f, .54f);
                EditorGUI.DrawRect(block, color);
                string origin = EntryOrigin(item);
                string caption = (string)item["activityId"] + "  " + (string)item["start"] + "–" + (string)item["end"];
                if (block.width >= 120) caption += " · " + origin;
                string tooltip = origin + " · alvo " + ((string)item["targetId"] ?? "sem alvo");
                GUI.Label(new Rect(block.x + 3, block.y + 2, Math.Max(0, block.width - 6), block.height - 4), new GUIContent(caption, tooltip), EditorStyles.miniLabel);
                if (Event.current.type == EventType.MouseDown && block.Contains(Event.current.mousePosition))
                { SelectTimelineEntry(item, IsTemplateBasedNpc()); Event.current.Use(); Repaint(); }
                if (end > start && timelineItems.Any(other => other != item && TryMinute((string)other["start"], false, out int otherStart) && TryMinute((string)other["end"], true, out int otherEnd) && start < otherEnd && otherStart < end))
                    EditorGUI.DrawRect(new Rect(block.x, block.yMax - 3, block.width, 3), new Color(.9f, .22f, .18f));
            }
            if (validation.Diagnostics.Count > 0) EditorGUILayout.LabelField("Faixas âmbar = lacunas; marcadores vermelhos = sobreposição. O resolver continua sendo a fonte semântica.", EditorStyles.miniLabel);
        }

        private void DrawGapMarkers(Rect rect, List<JObject> entries)
        {
            var intervals = new List<Tuple<int, int>>();
            foreach (JObject item in entries)
            {
                if (TryMinute((string)item["start"], false, out int start) && TryMinute((string)item["end"], true, out int end) && end > start)
                    intervals.Add(Tuple.Create(start, end));
            }
            int cursor = 0;
            foreach (Tuple<int, int> interval in intervals.OrderBy(x => x.Item1))
            {
                if (interval.Item1 > cursor) DrawGap(rect, cursor, interval.Item1);
                cursor = Math.Max(cursor, interval.Item2);
            }
            if (cursor < 1440) DrawGap(rect, cursor, 1440);
        }

        private bool IsTemplateBasedNpc()
        {
            return session.Document != null && session.Document.Kind == NpcScheduleDocumentKind.Npc &&
                !string.IsNullOrWhiteSpace((string)Parse(session.Draft)?["templateId"]);
        }

        private JArray EffectiveNpcEntries()
        {
            NpcScheduleValidation validation = GetCurrentValidation();
            string npcId = CurrentNpcId();
            if (!validation.IsValid || string.IsNullOrEmpty(npcId) || !validation.Data.NpcSchedules.TryGetValue(npcId, out IList<ScheduleEntry> schedule)) return null;
            var entries = new JArray();
            foreach (ScheduleEntry entry in schedule.OrderBy(x => x.Start).ThenBy(x => x.Id, StringComparer.Ordinal))
            {
                var item = new JObject
                {
                    ["id"] = entry.Id,
                    ["start"] = FormatMinute(entry.Start),
                    ["end"] = FormatMinute(entry.End),
                    ["activityId"] = entry.ActivityId
                };
                if (!string.IsNullOrEmpty(entry.TargetId)) item["targetId"] = entry.TargetId;
                entries.Add(item);
            }
            return entries;
        }

        private string EntryOrigin(JObject item)
        {
            JObject root = Parse(session.Draft);
            if (session.Document.Kind == NpcScheduleDocumentKind.Npc && root != null)
            {
                string id = (string)item["id"];
                foreach (string operation in new[] { "replaceEntries", "addEntries", "entries" })
                    if (GetArray(root, operation, false)?.OfType<JObject>().Any(x => (string)x["id"] == id) == true)
                        return operation == "replaceEntries" ? "substituição local" : operation == "addEntries" ? "adição local" : "entrada local";
                string template = (string)root["templateId"];
                if (!string.IsNullOrEmpty(template)) return "template " + template;
                return "agenda NPC";
            }
            return session.Document.Kind == NpcScheduleDocumentKind.Template ? "template " + session.Document.Id :
                session.Document.Kind == NpcScheduleDocumentKind.Override ? "override " + session.Document.Id : "entrada local";
        }

        private void SelectTimelineEntry(JObject item, bool allowInherited)
        {
            string id = (string)item["id"];
            if (session.Document.Kind == NpcScheduleDocumentKind.Npc)
            {
                JObject root = Parse(session.Draft);
                foreach (string operation in new[] { "replaceEntries", "addEntries", "entries" })
                {
                    JObject local = GetArray(root, operation, false)?.OfType<JObject>().FirstOrDefault(x => (string)x["id"] == id);
                    if (local == null) continue;
                    arrayName = operation;
                    SelectEntry(local);
                    return;
                }
                if (allowInherited && IsTemplateBasedNpc())
                {
                    pendingInheritedEntry = (JObject)item.DeepClone();
                    pendingInheritedTemplate = (string)root["templateId"];
                    session.SelectedEntryId = id;
                    return;
                }
                status = "A origem local deste bloco não foi encontrada no documento.";
                return;
            }
            SelectEntry(item);
        }

        private void CreateInheritedReplacement()
        {
            if (pendingInheritedEntry == null) return;
            JObject root = JObject.Parse(session.Draft);
            JObject replacement = (JObject)pendingInheritedEntry.DeepClone();
            GetArray(root, "replaceEntries", true).Add(replacement);
            arrayName = "replaceEntries";
            pendingInheritedEntry = null;
            pendingInheritedTemplate = null;
            SelectEntry(replacement);
            session.ReplaceDraft(root.ToString(Formatting.Indented));
        }

        private void RemoveInheritedEntry()
        {
            if (pendingInheritedEntry == null) return;
            JObject root = JObject.Parse(session.Draft);
            string id = (string)pendingInheritedEntry["id"];
            JArray removed = GetArray(root, "removeEntries", true);
            if (!removed.Values<string>().Contains(id, StringComparer.Ordinal)) removed.Add(id);
            pendingInheritedEntry = null;
            pendingInheritedTemplate = null;
            session.SelectedEntryId = null;
            session.ReplaceDraft(root.ToString(Formatting.Indented));
        }

        private static void DrawGap(Rect rect, int start, int end)
        {
            float x = rect.x + rect.width * start / 1440f;
            float width = rect.width * (end - start) / 1440f;
            var band = new Rect(x, rect.yMax - 9, width, 7);
            EditorGUI.DrawRect(band, new Color(.82f, .58f, .16f));
            if (width > 56) GUI.Label(band, new GUIContent("lacuna", "Lacuna permitida na agenda-base; o resolver usa seu fallback."), EditorStyles.miniLabel);
        }

        private void DrawEntryList()
        {
            if (session.Document.Kind == NpcScheduleDocumentKind.Npc)
            {
                JArray effective = EffectiveNpcEntries();
                EditorGUILayout.LabelField(IsTemplateBasedNpc() ? "Agenda efetiva · template e operações deste NPC" : "Agenda efetiva do NPC", EditorStyles.boldLabel);
                if (effective == null)
                    EditorGUILayout.HelpBox("Lista efetiva indisponível: corrija a validação antes de interpretar a agenda herdada.", MessageType.Warning);
                else
                {
                    foreach (JObject item in effective.OfType<JObject>())
                    {
                        string origin = EntryOrigin(item);
                        string label = (string)item["start"] + "–" + (string)item["end"] + " · " + (string)item["activityId"] + " · " + origin;
                        if (GUILayout.Button(new GUIContent(label, "ID " + (string)item["id"] + " · origem " + origin + " · alvo " + ((string)item["targetId"] ?? "sem alvo")), "Button"))
                            SelectTimelineEntry(item, true);
                    }
                }
            }
            string[] arrays = EditableArrays();
            if (arrays.Length > 1)
            {
                int selected = Array.IndexOf(arrays, arrayName); if (selected < 0) selected = 0;
                int next = EditorGUILayout.Popup("Blocos da operação", selected, arrays);
                if (next >= 0 && next < arrays.Length && next != selected)
                {
                    arrayName = arrays[next];
                    pendingInheritedEntry = null; pendingInheritedTemplate = null; session.SelectedEntryId = null;
                }
            }
            JArray entries = CurrentEntries();
            EditorGUILayout.LabelField("Operação local " + arrayName + ": " + (entries?.Count ?? 0), EditorStyles.boldLabel);
            if (entries == null) return;
            if (arrayName == "removeEntries")
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    string id = (string)entries[i];
                    EditorGUILayout.BeginHorizontal(); EditorGUILayout.LabelField(id);
                    if (GUILayout.Button("Remover operação", GUILayout.Width(125))) { JObject root = JObject.Parse(session.Draft); GetArray(root, arrayName, false)?.RemoveAt(i); session.ReplaceDraft(root.ToString(Formatting.Indented)); }
                    EditorGUILayout.EndHorizontal();
                }
                return;
            }
            for (int i = 0; i < entries.Count; i++)
            {
                if (!(entries[i] is JObject item)) continue;
                string label = (string)item["id"] + "  ·  " + (string)item["start"] + "–" + (string)item["end"] + "  ·  " + (string)item["activityId"] +
                    (string.IsNullOrEmpty((string)item["targetId"]) ? "" : " @ " + (string)item["targetId"]);
                if (GUILayout.Toggle((string)item["id"] == session.SelectedEntryId, label, "Button")) SelectEntry(item);
            }
        }

        private void DrawEntryForm(bool readOnly)
        {
            JArray entries = CurrentEntries();
            EditorGUILayout.LabelField("Editar bloco", EditorStyles.boldLabel);
            if (pendingInheritedEntry != null)
            {
                EditorGUILayout.HelpBox("Este bloco vem do template '" + pendingInheritedTemplate + "'. A agenda efetiva é somente leitura aqui; crie uma substituição, remova o ID herdado ou abra o template.", MessageType.Info);
                using (new EditorGUI.DisabledScope(readOnly))
                {
                    if (GUILayout.Button("Criar substituição local")) CreateInheritedReplacement();
                    if (GUILayout.Button("Remover este bloco herdado")) RemoveInheritedEntry();
                    NpcScheduleDocument template = repository.Documents.FirstOrDefault(x => x.Kind == NpcScheduleDocumentKind.Template && x.Id == pendingInheritedTemplate);
                    using (new EditorGUI.DisabledScope(template == null))
                        if (GUILayout.Button("Abrir template")) SelectDocument(template);
                }
                return;
            }
            if (arrayName == "removeEntries")
            {
                JArray existing = entries ?? new JArray();
                using (new EditorGUI.DisabledScope(readOnly))
                {
                    removeIdText = EditorGUILayout.TextField("ID base a remover", removeIdText);
                    if (GUILayout.Button("Adicionar remoção") && !string.IsNullOrWhiteSpace(removeIdText) && !existing.Values<string>().Contains(removeIdText, StringComparer.Ordinal))
                    { JObject root = JObject.Parse(session.Draft); GetArray(root, arrayName, true).Add(removeIdText); session.ReplaceDraft(root.ToString(Formatting.Indented)); removeIdText = ""; }
                }
                return;
            }
            EditorGUILayout.BeginHorizontal();
            using (new EditorGUI.DisabledScope(readOnly))
            {
                if (GUILayout.Button("Adicionar", GUILayout.Width(90))) AddEntry();
                using (new EditorGUI.DisabledScope(entries == null || !entries.OfType<JObject>().Any(x => (string)x["id"] == session.SelectedEntryId)))
                {
                    if (GUILayout.Button("Duplicar bloco", GUILayout.Width(110))) DuplicateEntry();
                    if (GUILayout.Button("Remover", GUILayout.Width(90))) RemoveEntry();
                }
            }
            EditorGUILayout.EndHorizontal();
            JObject selected = SelectedEntry();
            if (selected == null) return;
            using (new EditorGUI.DisabledScope(readOnly))
            {
                string nextId = EditorGUILayout.DelayedTextField("ID", (string)selected["id"]);
                string nextStart = EditorGUILayout.DelayedTextField("Início (HH:mm)", (string)selected["start"]);
                string nextEnd = EditorGUILayout.DelayedTextField("Fim (HH:mm ou 24:00)", (string)selected["end"]);
                string[] activityIds = ActivityIds();
                int index = Array.IndexOf(activityIds, (string)selected["activityId"]);
                int nextActivity = EditorGUILayout.Popup("Atividade", Math.Max(0, index), activityIds);
                string activity = activityIds.Length == 0 ? (string)selected["activityId"] : activityIds[Mathf.Clamp(nextActivity, 0, activityIds.Length - 1)];
                EditorGUILayout.LabelField("Alvo simbólico", EditorStyles.miniLabel);
                NpcScheduleValidation destinationData = GetCurrentValidation();
                TargetKind targetKind = destinationData.Data != null && destinationData.Data.Activities.TryGetValue(activity, out ActivityDefinition definition) ? definition.TargetKind : TargetKind.None;
                NpcScheduleDestinationOption[] destinationOptions = repository.Destinations != null
                    ? repository.Destinations.Choices(activity, targetKind)
                    : Array.Empty<NpcScheduleDestinationOption>();
                string oldTarget = (string)selected["targetId"] ?? "";
                string destinationStatus = repository.Destinations?.StatusFor(activity, targetKind, oldTarget) ?? "unverified";
                string destinationEvidence = destinationOptions.FirstOrDefault(x => x.Id == oldTarget)?.Evidence ??
                    (destinationStatus == "ambiguous" ? "ID de Smart Object repetido na cena; runtime não pode desambiguar" :
                    destinationStatus == "unavailable" ? "o destino é conhecido, mas não está ativo/resolvível na cena capturada" :
                    destinationStatus == "invalid_target" ? "o tipo de atividade rejeita este destino" :
                    destinationStatus == "unverified" ? "o destino não foi comprovado no snapshot da cena" : "atividade sem destino obrigatório");
                if (destinationOptions.Length > 0)
                {
                    var choices = destinationOptions.ToList();
                    if (!string.IsNullOrEmpty(oldTarget) && !choices.Any(x => x.Id == oldTarget))
                        choices.Insert(0, new NpcScheduleDestinationOption { Id = oldTarget, Label = oldTarget + " · " + destinationStatus, Status = destinationStatus, Evidence = destinationEvidence });
                    string[] labels = choices.Select(x => x.Label).ToArray();
                    int targetIndex = choices.FindIndex(x => x.Id == oldTarget);
                    int nextTarget = EditorGUILayout.Popup(Math.Max(0, targetIndex), labels);
                    if (nextTarget >= 0 && nextTarget < choices.Count) targetText = choices[nextTarget].Id;
                }
                else targetText = EditorGUILayout.DelayedTextField(oldTarget);
                EditorGUILayout.LabelField("Destino: " + destinationStatus + " · snapshot " + (repository.Destinations?.CapturedAtUtc.ToString("HH:mm:ss 'UTC'") ?? "indisponível") +
                    (repository.Destinations == null ? "" : " · cena " + Path.GetFileNameWithoutExtension(repository.Destinations.CapturedScene)) + " · " + destinationEvidence, EditorStyles.miniLabel);
                if (nextId != (string)selected["id"] || nextStart != (string)selected["start"] || nextEnd != (string)selected["end"] || activity != (string)selected["activityId"] || targetText != oldTarget)
                {
                    JObject root = JObject.Parse(session.Draft);
                    JObject target = root;
                    JArray array = GetArray(target, arrayName, false);
                    JObject entry = array?.OfType<JObject>().FirstOrDefault(x => (string)x["id"] == session.SelectedEntryId);
                    if (entry != null)
                    {
                        entry["id"] = nextId; entry["start"] = nextStart; entry["end"] = nextEnd; entry["activityId"] = activity;
                        if (string.IsNullOrWhiteSpace(targetText)) entry.Remove("targetId"); else entry["targetId"] = targetText;
                        session.SelectedEntryId = nextId;
                        session.ReplaceDraft(root.ToString(Formatting.Indented));
                    }
                }
            }
        }

        private void DrawSupportedFields(bool readOnly)
        {
            if (session.Document.Kind == NpcScheduleDocumentKind.Npc)
            {
                JObject root = JObject.Parse(session.Draft);
                string template = (string)root["templateId"] ?? "";
                EditorGUILayout.BeginHorizontal();
                string next = EditorGUILayout.DelayedTextField("Template ID", template);
                if (next != template && !readOnly) { if (string.IsNullOrWhiteSpace(next)) root.Remove("templateId"); else root["templateId"] = next; session.ReplaceDraft(root.ToString(Formatting.Indented)); }
                EditorGUILayout.EndHorizontal();
                if (!string.IsNullOrEmpty(template)) EditorGUILayout.HelpBox("A linha e a lista mostram a agenda efetiva herdada. Selecione um bloco herdado para substituí-lo localmente ou removê-lo desta agenda.", MessageType.Info);
            }
            if (session.Document.Kind == NpcScheduleDocumentKind.Override)
            {
                JObject root = JObject.Parse(session.Draft);
                int[] priorities = { 50, 70, 90 };
                int current = Array.IndexOf(priorities, (int)((int?)root["priority"] ?? 50));
                using (new EditorGUI.DisabledScope(readOnly))
                {
                    int next = EditorGUILayout.Popup("Prioridade", Math.Max(0, current), new[] { "50 · baixa", "70 · média", "90 · alta" });
                    if (next != current) { root["priority"] = priorities[next]; session.ReplaceDraft(root.ToString(Formatting.Indented)); }
                }
                EditorGUILayout.LabelField("Condições suportadas (AND entre campos)", EditorStyles.boldLabel);
                JObject conditions = root["conditions"] as JObject ?? new JObject();
                string weather = String.Join(",", (conditions["weather"] as JArray ?? new JArray()).Values<string>());
                string nextWeather = EditorGUILayout.DelayedTextField("Clima (lista)", weather);
                if (!readOnly && nextWeather != weather)
                {
                    string[] values = nextWeather.Split(',').Select(x => x.Trim()).Where(x => x.Length > 0).ToArray();
                    if (values.Length == 0) conditions.Remove("weather"); else conditions["weather"] = new JArray(values);
                    StoreConditions(root, conditions); session.ReplaceDraft(root.ToString(Formatting.Indented));
                }
                string days = String.Join(",", (conditions["days"] as JArray ?? new JArray()).Values<int>());
                string nextDays = EditorGUILayout.DelayedTextField("Dias absolutos", days);
                if (!readOnly && nextDays != days && ParseInts(nextDays, out int[] parsedDays)) { if (parsedDays.Length == 0) conditions.Remove("days"); else conditions["days"] = new JArray(parsedDays); StoreConditions(root, conditions); session.ReplaceDraft(root.ToString(Formatting.Indented)); }
                string weekdays = String.Join(",", (conditions["weekdays"] as JArray ?? new JArray()).Values<int>());
                string nextWeekdays = EditorGUILayout.DelayedTextField("Dias da semana (0–6)", weekdays);
                if (!readOnly && nextWeekdays != weekdays && ParseInts(nextWeekdays, out int[] parsedWeekdays)) { if (parsedWeekdays.Length == 0) conditions.Remove("weekdays"); else conditions["weekdays"] = new JArray(parsedWeekdays); StoreConditions(root, conditions); session.ReplaceDraft(root.ToString(Formatting.Indented)); }
                DrawFlags(conditions, root, readOnly);
            }
        }

        private void DrawFlags(JObject conditions, JObject root, bool readOnly)
        {
            JObject flags = conditions["flags"] as JObject ?? new JObject();
            string flagsText = String.Join(",", flags.Properties().Select(x => x.Name + "=" + x.Value));
            string nextFlags = EditorGUILayout.DelayedTextField("Flags (nome=true/false)", flagsText);
            if (readOnly || flagsText == nextFlags || !ParseFlags(nextFlags, out JObject parsed)) return;
            if (!parsed.HasValues) conditions.Remove("flags"); else conditions["flags"] = parsed;
            StoreConditions(root, conditions); session.ReplaceDraft(root.ToString(Formatting.Indented));
        }

        private void DrawValidationAndPreview()
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Validação e preview do resolver", EditorStyles.boldLabel);
            preview.Day = EditorGUILayout.IntField("Dia absoluto", preview.Day);
            int hour = preview.Minute / 60, minute = preview.Minute % 60;
            EditorGUILayout.BeginHorizontal();
            hour = EditorGUILayout.IntSlider("Hora", hour, 0, 23);
            minute = EditorGUILayout.IntSlider("Minuto", minute, 0, 59);
            EditorGUILayout.EndHorizontal();
            preview.Minute = hour * 60 + minute;
            preview.WeekStartDay = EditorGUILayout.IntSlider("Primeiro dia da semana", preview.WeekStartDay, 0, 6);
            preview.Weather = EditorGUILayout.TextField("Clima", preview.Weather);
            previewFlags = EditorGUILayout.TextField("Flags (nome=true/false)", previewFlags);
            preview.Flags.Clear();
            if (ParseFlags(previewFlags, out JObject worldFlags))
                foreach (JProperty property in worldFlags.Properties()) preview.Flags[property.Name] = (bool)property.Value;
            NpcScheduleValidation draftValidation = GetCurrentValidation();
            if (draftValidation.Data != null)
            {
                string boundNpc = CurrentNpcId();
                string[] npcIds = draftValidation.Data.NpcSchedules.Keys.OrderBy(x => x, StringComparer.Ordinal).ToArray();
                if (string.IsNullOrEmpty(previewNpcId) || !npcIds.Contains(previewNpcId)) previewNpcId = boundNpc ?? npcIds.FirstOrDefault();
                if (session.Document.Kind != NpcScheduleDocumentKind.Npc && session.Document.Kind != NpcScheduleDocumentKind.Override && npcIds.Length > 0)
                {
                    int index = Math.Max(0, Array.IndexOf(npcIds, previewNpcId));
                    previewNpcId = npcIds[EditorGUILayout.Popup("NPC para preview", index, npcIds)];
                }
                else previewNpcId = boundNpc;
            }
            preview.Resolve(draftValidation, unchecked(repository.Revision * 397 ^ session.Revision), previewNpcId);
            if (!preview.Validation.IsValid) EditorGUILayout.HelpBox("Rascunho inválido: " + preview.Error, MessageType.Error);
            else
            {
                foreach (string diagnostic in preview.Validation.Diagnostics.Take(8)) EditorGUILayout.HelpBox(diagnostic, MessageType.Warning);
                ResolutionResult resolved = preview.Result;
                if (resolved != null)
                {
                    ActivityIntent intent = resolved.Intent;
                    bool executable = preview.Validation.Recipes.ContainsKey(intent.ActivityId);
                    EditorGUILayout.HelpBox("PREVISTO — " + intent.NpcId + ": " + intent.ActivityId + (string.IsNullOrEmpty(intent.TargetId) ? "" : " @ " + intent.TargetId) +
                        " · " + intent.Origin + "/" + intent.SourceId + " · até " + FormatMinute(intent.ValidUntil) + " · receita " + (executable ? "presente" : "ausente"), MessageType.Info);
                    foreach (string item in resolved.Trace.Diagnostics) EditorGUILayout.LabelField("• " + item, EditorStyles.miniLabel);
                }
            }
        }

        private void DrawRuntimeComparison()
        {
            string npcId = previewNpcId ?? CurrentNpcId();
            EditorGUILayout.LabelField("Observado no runtime", EditorStyles.boldLabel);
            if (!EditorApplication.isPlaying) { EditorGUILayout.LabelField("Indisponível fora do Play Mode; preview acima é apenas previsão.", EditorStyles.miniLabel); return; }
            NpcDebugNpcSnapshot snapshot = runtimeSnapshotNpcId == npcId ? runtimeSnapshot : null;
            if (snapshot == null) { EditorGUILayout.HelpBox("Snapshot do NPC indisponível ou ambíguo nesta sessão.", MessageType.Warning); return; }
            EditorGUILayout.LabelField("Executado: " + snapshot.executedActivity + " / " + snapshot.executionState + " · alvo efetivo: " + snapshot.effectiveTarget);
            EditorGUILayout.LabelField("Intenção runtime: " + snapshot.intendedActivity + " @ " + snapshot.intendedTarget + " · fallback: " + snapshot.fallback + " · razão: " + snapshot.reason, EditorStyles.miniLabel);
            if (preview.Result != null)
            {
                ActivityIntent intent = preview.Result.Intent;
                bool sameIntent = intent.ActivityId == snapshot.intendedActivity && (intent.TargetId ?? "") == (snapshot.intendedTarget ?? "");
                EditorGUILayout.LabelField("Comparação previsão/resolver × intenção observada: " + (sameIntent ? "coincide" : "diverge") +
                    " · execução física observada: " + (!string.IsNullOrEmpty(snapshot.executedActivity) ? "disponível" : "indisponível"), EditorStyles.miniLabel);
                if (GUILayout.Button("Copiar diagnóstico sem caminho absoluto"))
                {
                    EditorGUIUtility.systemCopyBuffer = "NPC " + npcId + " | fonte " + session.Document.AssetPath + " | previsto " + intent.ActivityId + " @ " +
                        (intent.TargetId ?? "-") + " até " + FormatMinute(intent.ValidUntil) + " | runtime intent " + snapshot.intendedActivity + " @ " +
                        snapshot.intendedTarget + " | executado " + snapshot.executedActivity + "/" + snapshot.executionState + " | fallback " +
                        snapshot.fallback + " | razão " + snapshot.reason;
                }
            }
            if (GUILayout.Button("Abrir Debug Monitor neste NPC")) NpcDebugMonitorWindow.FocusNpc(npcId);
        }

        private void DrawFooter(bool readOnly)
        {
            if (!string.IsNullOrEmpty(status)) EditorGUILayout.HelpBox(status, MessageType.None);
            if (!string.IsNullOrEmpty(session.Error)) EditorGUILayout.HelpBox(session.Error, MessageType.Error);
        }

        private void AddEntry()
        {
            pendingInheritedEntry = null;
            pendingInheritedTemplate = null;
            JObject root = JObject.Parse(session.Draft);
            JArray entries = GetArray(root, arrayName, true);
            var used = new HashSet<string>(entries.OfType<JObject>().Select(x => (string)x["id"]), StringComparer.Ordinal);
            string id = "new_entry"; int suffix = 1; while (used.Contains(id)) id = "new_entry_" + suffix++;
            var entry = new JObject { ["id"] = id, ["start"] = "08:00", ["end"] = "09:00", ["activityId"] = "idle" };
            entries.Add(entry); session.SelectedEntryId = id; targetText = ""; SelectEntry(entry); session.ReplaceDraft(root.ToString(Formatting.Indented));
        }

        private void DuplicateEntry()
        {
            JObject root = JObject.Parse(session.Draft); JArray entries = GetArray(root, arrayName, false); JObject selected = SelectedEntry(); if (entries == null || selected == null) return;
            JObject copy = (JObject)selected.DeepClone(); string baseId = ((string)copy["id"] ?? "entry") + "_copy"; string id = baseId; int suffix = 2;
            while (entries.OfType<JObject>().Any(x => (string)x["id"] == id)) id = baseId + suffix++;
            copy["id"] = id; entries.Add(copy); SelectEntry(copy); session.ReplaceDraft(root.ToString(Formatting.Indented));
        }

        private void RemoveEntry()
        {
            JObject root = JObject.Parse(session.Draft); JArray entries = GetArray(root, arrayName, false);
            JObject selected = entries?.OfType<JObject>().FirstOrDefault(x => (string)x["id"] == session.SelectedEntryId);
            if (selected == null) return; selected.Remove(); session.SelectedEntryId = null; session.ReplaceDraft(root.ToString(Formatting.Indented));
        }

        private void SelectEntry(JObject item)
        {
            pendingInheritedEntry = null;
            pendingInheritedTemplate = null;
            session.SelectedEntryId = (string)item["id"];
            targetText = (string)item["targetId"] ?? "";
        }

        private void Save()
        {
            if (EditorApplication.isPlaying || session.Document == null) return;
            if (!repository.TrySave(session.Document, session.Draft, session.OriginalFingerprint, out string error)) { status = error; return; }
            string path = session.Document.AssetPath; repository.Refresh(); NpcScheduleDocument refreshed = repository.FindByPath(path);
            session.Saved(refreshed); status = "Arquivo validado e salvo. O runtime lerá o TextAsset na próxima entrada normal em Play Mode.";
        }

        private void Revert()
        {
            if (session.Dirty && !EditorUtility.DisplayDialog("Reverter rascunho", "Descartar as alterações desta sessão e recarregar a origem?", "Reverter", "Cancelar")) return;
            string path = session.Document.AssetPath;
            repository.Refresh();
            NpcScheduleDocument latest = repository.FindByPath(path);
            if (latest == null) { status = "Conflito: arquivo removido. O rascunho continua disponível."; return; }
            if (latest.Fingerprint != session.OriginalFingerprint)
            {
                SelectSource(latest); status = "Rascunho descartado; conteúdo externo recarregado."; return;
            }
            session.Revert();
        }

        private void Refresh(bool initial)
        {
            string path = session.Document?.AssetPath;
            repository.Refresh();
            if (!string.IsNullOrEmpty(path))
            {
                NpcScheduleDocument same = repository.FindByPath(path);
                if (same != null && !session.Dirty) { SelectSource(same); return; }
            }
            if (initial || session.Document == null)
            {
                NpcScheduleDocument start = repository.Documents.FirstOrDefault(x => x.Kind == NpcScheduleDocumentKind.Npc && x.RuntimeLoaded) ??
                    repository.Documents.FirstOrDefault(x => x.Kind == NpcScheduleDocumentKind.Npc) ?? repository.Documents.FirstOrDefault();
                if (start != null) SelectSource(start);
            }
        }

        private void SelectDocument(NpcScheduleDocument document)
        {
            if (session.Dirty)
            {
                int choice = EditorUtility.DisplayDialogComplex("Rascunho não salvo", "Salvar as alterações antes de trocar de documento?", "Salvar", "Cancelar", "Descartar");
                if (choice == 0) { Save(); if (session.Dirty) return; }
                else if (choice == 1) return;
            }
            SelectSource(document); status = ""; arrayName = DefaultArray(document);
        }

        private void DuplicateNpc()
        {
            bool fromDraft = session.Dirty;
            JObject source = JObject.Parse(session.Draft);
            string original = (string)source["npcId"] ?? session.Document.Id;
            string id = original + "_copy"; int suffix = 2;
            while (repository.Documents.Any(x => x.Kind == NpcScheduleDocumentKind.Npc && x.Id == id)) id = original + "_copy" + suffix++;
            source["npcId"] = id;
            string folder = Path.GetDirectoryName(session.Document.AssetPath).Replace('\\', '/');
            string asset = folder + "/" + id + ".json";
            if (!repository.TryCreateNpc(asset, source.ToString(Formatting.Indented) + "\n", out string error)) { status = "Duplicação rejeitada: " + error; return; }
            NpcScheduleDocument clone = repository.FindByPath(asset);
            SelectSource(clone); status = "Criada cópia independente " + asset + (fromDraft ? " a partir do rascunho." : ".");
        }

        private NpcScheduleDocument[] FilteredDocuments() => repository.Documents.Where(x => string.IsNullOrEmpty(search) || x.Label.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 || x.AssetPath.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0).ToArray();
        private NpcScheduleValidation GetCurrentValidation()
        {
            string key = repository.Revision + ":" + session.Revision;
            if (key == validationCacheKey && validationCache != null) return validationCache;
            validationCacheKey = key;
            if (session.Document == null) return validationCache = new NpcScheduleValidation { Error = "nenhum documento selecionado" };
            validationCache = !session.Dirty && repository.SourceValidation != null
                ? repository.SourceValidation
                : repository.ValidateAll(session.Document, session.Draft);
            return validationCache;
        }

        private void OnEditorUpdate()
        {
            if (!EditorApplication.isPlaying)
            {
                runtimeSnapshot = null;
                runtimeSnapshotNpcId = null;
                return;
            }
            if (EditorApplication.timeSinceStartup < nextRuntimeSnapshot) return;
            nextRuntimeSnapshot = EditorApplication.timeSinceStartup + .5d;
            string npcId = previewNpcId ?? CurrentNpcId();
            runtimeSnapshotNpcId = npcId;
            runtimeSnapshot = string.IsNullOrEmpty(npcId) ? null : NpcDebugDiagnostics.CaptureNpc(npcId);
            Repaint();
        }

        private void OnActiveSceneChanged(Scene previous, Scene next) => QueueDestinationRefresh();

        private void OnProjectChanged()
        {
            EditorApplication.delayCall -= RefreshProjectDataDelayed;
            EditorApplication.delayCall += RefreshProjectDataDelayed;
        }

        private void RefreshProjectDataDelayed()
        {
            if (this == null) return;
            Refresh(false);
            QueueDestinationRefresh();
        }

        private void QueueDestinationRefresh()
        {
            if (destinationRefreshQueued) return;
            destinationRefreshQueued = true;
            EditorApplication.delayCall -= RefreshDestinationsDelayed;
            EditorApplication.delayCall += RefreshDestinationsDelayed;
        }

        private void RefreshDestinationsDelayed()
        {
            destinationRefreshQueued = false;
            if (this == null) return;
            repository.RefreshDestinations();
            validationCacheKey = null;
            Repaint();
        }

        private void SelectSource(NpcScheduleDocument document)
        {
            NpcScheduleValidation check = repository.ValidateAll(document, document.Text);
            string restriction = document.Kind == NpcScheduleDocumentKind.Catalog || document.Kind == NpcScheduleDocumentKind.Recipes
                ? "Catálogo/receitas são consultados por este editor e permanecem somente leitura."
                : check.IsValid ? null : check.Error ?? "Documento inválido segundo o loader; somente leitura.";
            session.Select(document, restriction);
            pendingInheritedEntry = null;
            pendingInheritedTemplate = null;
            validationCacheKey = null;
        }
        private string[] ActivityIds() => GetCurrentValidation().Data?.Activities.Keys.OrderBy(x => x, StringComparer.Ordinal).ToArray() ?? new[] { "idle" };
        private string CurrentNpcId()
        {
            if (session.Document == null) return null;
            if (session.Document.Kind == NpcScheduleDocumentKind.Npc) return (string)Parse(session.Draft)?["npcId"];
            if (session.Document.Kind == NpcScheduleDocumentKind.Override) return (string)Parse(session.Draft)?["npcId"];
            return repository.Documents.FirstOrDefault(x => x.Kind == NpcScheduleDocumentKind.Npc && x.RuntimeLoaded)?.Id;
        }
        private string[] EditableArrays()
        {
            JObject root = Parse(session.Draft); if (root == null) return new[] { "entries" };
            var arrays = new List<string>();
            foreach (string name in new[] { "entries", "replaceEntries", "addEntries", "removeEntries" }) if (root[name] is JArray) arrays.Add(name);
            if (session.Document.Kind == NpcScheduleDocumentKind.Npc && root["templateId"] != null)
                foreach (string name in new[] { "replaceEntries", "removeEntries", "addEntries" }) if (!arrays.Contains(name)) arrays.Add(name);
            if (arrays.Count == 0) arrays.Add(session.Document.Kind == NpcScheduleDocumentKind.Npc && root["templateId"] != null ? "addEntries" : "entries");
            return arrays.ToArray();
        }
        private string DefaultArray(NpcScheduleDocument document)
        {
            JObject root = Parse(document.Text);
            if (root?["entries"] is JArray) return "entries";
            if (document.Kind == NpcScheduleDocumentKind.Npc && root?["templateId"] != null) return "addEntries";
            return "entries";
        }
        private JArray CurrentEntries() => GetArray(Parse(session.Draft), arrayName, false);
        private JObject SelectedEntry() => CurrentEntries()?.OfType<JObject>().FirstOrDefault(x => (string)x["id"] == session.SelectedEntryId);
        private static JObject Parse(string value) { try { return value == null ? null : JObject.Parse(value); } catch { return null; } }
        private static JArray GetArray(JObject root, string name, bool create)
        {
            if (root == null) return null;
            if (root[name] is JArray found) return found;
            if (!create) return null;
            var array = new JArray(); root[name] = array; return array;
        }
        private static bool TryMinute(string text, bool allow24, out int minute)
        { try { minute = GameTimeSnapshot.ParseMinute(text, allow24); return true; } catch { minute = 0; return false; } }
        private static string FormatMinute(int value) => value >= 1440 ? "24:00" : (value / 60).ToString("00") + ":" + (value % 60).ToString("00");
        private static bool ParseInts(string source, out int[] values)
        { values = Array.Empty<int>(); if (string.IsNullOrWhiteSpace(source)) return true; string[] parts = source.Split(','); var list = new List<int>(); foreach (string part in parts) { if (!int.TryParse(part.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int n)) return false; list.Add(n); } values = list.ToArray(); return true; }
        private static bool ParseFlags(string source, out JObject flags)
        { flags = new JObject(); if (string.IsNullOrWhiteSpace(source)) return true; foreach (string part in source.Split(',')) { string[] pair = part.Trim().Split('='); if (pair.Length != 2 || !bool.TryParse(pair[1].Trim(), out bool value) || string.IsNullOrWhiteSpace(pair[0])) return false; flags[pair[0].Trim()] = value; } return true; }
        private static void StoreConditions(JObject root, JObject conditions)
        { if (conditions.HasValues) root["conditions"] = conditions; else root.Remove("conditions"); }
    }
}
