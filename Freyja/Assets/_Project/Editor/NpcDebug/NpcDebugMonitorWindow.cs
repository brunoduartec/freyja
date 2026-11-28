using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Freyja.NpcDebug.Editor
{
    public sealed class NpcDebugMonitorWindow : EditorWindow
    {
        private NpcDebugFilter filter = new NpcDebugFilter();
        private NpcDebugSnapshot snapshot;
        private NpcDebugSmartObjectSnapshot[] smartObjects = Array.Empty<NpcDebugSmartObjectSnapshot>();
        private NpcDebugNpcSnapshot selected;
        private Vector2 scroll;
        private string selectedNpcId;
        private bool showAllOverlay;
        private bool overlayEnabled;
        private double nextRefresh;
        private string notice = "";

        [MenuItem("Freyja/NPC/Debug Monitor")]
        public static void Open()
        {
            NpcDebugMonitorWindow window = GetWindow<NpcDebugMonitorWindow>();
            window.titleContent = new GUIContent("NPC Debug Monitor");
            window.minSize = new Vector2(460f, 360f);
            window.Show();
        }

        private void OnEnable()
        {
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update += OnEditorUpdate;
            Refresh();
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
            NpcDebugSceneOverlay.Disable();
            NpcDebugDiagnostics.RestoreSessionOverrides();
        }

        private void OnEditorUpdate()
        {
            if (overlayEnabled && !NpcDebugSceneOverlay.IsEnabled) overlayEnabled = false;
            if (EditorApplication.timeSinceStartup >= nextRefresh)
            {
                nextRefresh = EditorApplication.timeSinceStartup + 0.5d;
                Refresh();
                Repaint();
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("NPC Debug Monitor", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Leitura passiva do runtime; controles encaminhados aos donos existentes.", EditorStyles.wordWrappedMiniLabel);
            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("Dados ao vivo indisponíveis fora de Play Mode. Comandos estão desabilitados.", MessageType.Info);
            }

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            string query = GUILayout.TextField(filter.search, GUI.skin.FindStyle("ToolbarSeachTextField") ?? EditorStyles.toolbarTextField);
            NpcDebugStateFilter state = (NpcDebugStateFilter)EditorGUILayout.EnumPopup(filter.state, EditorStyles.toolbarPopup, GUILayout.Width(120));
            if (query != filter.search || state != filter.state)
            {
                filter.search = query;
                filter.state = state;
                Refresh();
            }
            if (GUILayout.Button("Atualizar", EditorStyles.toolbarButton, GUILayout.Width(70))) Refresh();
            EditorGUILayout.EndHorizontal();

            if (snapshot != null)
            {
                string clock = snapshot.clockStatus == "available"
                    ? "Relógio: " + snapshot.hour.ToString("00.00") + "h | dia " + (snapshot.calendarTimeStatus == "available" ? snapshot.absoluteDay + " " + FormatMinute(snapshot.minuteOfDay) : "indisponível") + " | avanço " + (snapshot.advancesAutomatically ? "ativo" : "pausado") + " | taxa " + snapshot.clockRate.ToString("0") + "x"
                    : "Relógio: indisponível";
                EditorGUILayout.LabelField(clock, EditorStyles.miniLabel);
                EditorGUILayout.LabelField("NPC válidos/configurados/inativos: " + snapshot.registeredNpcCount + "/" + snapshot.configuredNpcCount + "/" + snapshot.inactiveNpcCount +
                    " | ativos/prontos/com falha: " + snapshot.activeNpcCount + "/" + snapshot.readyNpcCount + "/" + snapshot.failedNpcCount + " | validação " + snapshot.hostValidationStatus + " | eventos descartados: " + snapshot.droppedEventCount, EditorStyles.miniLabel);
                EditorGUILayout.LabelField("Slots livres/reservados/ocupados/desativados: " + snapshot.slotAvailableCount + "/" + snapshot.slotReservedCount + "/" + snapshot.slotOccupiedCount + "/" + snapshot.slotDisabledCount,
                    EditorStyles.miniLabel);
            }

            using (new EditorGUI.DisabledScope(!EditorApplication.isPlaying)) DrawControls();
            scroll = EditorGUILayout.BeginScrollView(scroll);
            DrawNpcList();
            DrawSmartObjects();
            DrawSelection();
            DrawHistory();
            EditorGUILayout.EndScrollView();
            if (!string.IsNullOrEmpty(notice)) EditorGUILayout.HelpBox(notice, MessageType.None);
        }

        private void DrawControls()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Pausar calendário")) Command(NpcDebugDiagnostics.SetClockPaused(true));
            if (GUILayout.Button("Retomar calendário")) Command(NpcDebugDiagnostics.SetClockPaused(false));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+15 min")) Command(NpcDebugDiagnostics.AdvanceClockMinutes(15));
            if (GUILayout.Button("+60 min")) Command(NpcDebugDiagnostics.AdvanceClockMinutes(60));
            if (GUILayout.Button("1x")) Command(NpcDebugDiagnostics.SetClockRate(1f));
            if (GUILayout.Button("5x")) Command(NpcDebugDiagnostics.SetClockRate(5f));
            if (GUILayout.Button("10x")) Command(NpcDebugDiagnostics.SetClockRate(10f));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            using (new EditorGUI.DisabledScope(selected == null))
            {
                if (GUILayout.Button("Reavaliar no próximo tick")) Command(NpcDebugDiagnostics.RequestReevaluation(selectedNpcId));
                if (GUILayout.Button("Cancelar atividade")) Command(NpcDebugDiagnostics.CancelActivity(selectedNpcId));
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Exportar JSON global"))
            {
                NpcDebugCommandResult result = NpcDebugDiagnostics.ExportJson(null);
                Command(result);
            }
            using (new EditorGUI.DisabledScope(selected == null))
                if (GUILayout.Button("Exportar JSON selecionado")) Command(NpcDebugDiagnostics.ExportJson(selectedNpcId));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Copiar resumo")) EditorGUIUtility.systemCopyBuffer = Summary();
            if (GUILayout.Button("Limpar histórico observado"))
            {
                NpcDebugDiagnostics.ClearHistory();
                notice = "Histórico limpo; estado de jogo inalterado.";
                Refresh();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            bool nextOverlay = EditorGUILayout.ToggleLeft("Overlay Scene View", overlayEnabled, GUILayout.Width(150));
            showAllOverlay = EditorGUILayout.ToggleLeft("Mostrar todos", showAllOverlay, GUILayout.Width(110));
            if (nextOverlay != overlayEnabled)
            {
                overlayEnabled = nextOverlay;
                if (overlayEnabled) NpcDebugSceneOverlay.Enable(selectedNpcId, showAllOverlay);
                else NpcDebugSceneOverlay.Disable();
            }
            if (overlayEnabled) NpcDebugSceneOverlay.SetSelection(selectedNpcId, showAllOverlay);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField("Override temporário: não suportado (" + NpcDebugDiagnostics.OverrideUnavailableReason + ")", EditorStyles.miniLabel);
        }

        private void DrawNpcList()
        {
            EditorGUILayout.LabelField("NPCs", EditorStyles.boldLabel);
            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("Controllers de runtime serão listados quando o mundo iniciar.", MessageType.Info);
                return;
            }
            if (snapshot == null || snapshot.npcs.Count == 0)
            {
                EditorGUILayout.LabelField("Nenhum NPC corresponde ao filtro.", EditorStyles.miniLabel);
                return;
            }
            foreach (NpcDebugNpcSnapshot npc in snapshot.npcs)
            {
                EditorGUILayout.BeginHorizontal();
                string label = npc.npcId + " — " + npc.status + " — previsão: " + (Empty(npc.intendedActivity) ? "desconhecida" : npc.intendedActivity) +
                    " → " + (Empty(npc.intendedTarget) ? "alvo indisponível" : npc.intendedTarget) + " — execução: " + (Empty(npc.executedActivity) ? "indisponível" : npc.executedActivity + "/" + npc.executionState) +
                    (npc.fallback ? " — FALLBACK" : "") + (npc.residualOwnership ? " — OWNERSHIP" : "") + (Empty(npc.reason) ? "" : " — " + npc.reason);
                if (GUILayout.Toggle(selectedNpcId == npc.npcId, label, "Button") && selectedNpcId != npc.npcId)
                {
                    selectedNpcId = npc.npcId;
                    selected = NpcDebugDiagnostics.CaptureNpc(selectedNpcId);
                    if (overlayEnabled) NpcDebugSceneOverlay.SetSelection(selectedNpcId, showAllOverlay);
                }
                if (GUILayout.Button("Localizar " + npc.npcId, EditorStyles.miniButton)) Locate(npc.npcId);
                EditorGUILayout.EndHorizontal();
            }
            if (snapshot.truncated) EditorGUILayout.HelpBox("Lista truncada pelo limite " + snapshot.limit + ".", MessageType.Warning);
        }

        private void DrawSmartObjects()
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Smart Objects e slots", EditorStyles.boldLabel);
            if (smartObjects == null || smartObjects.Length == 0)
            {
                EditorGUILayout.LabelField("Registry indisponível ou sem objetos registrados.", EditorStyles.miniLabel);
                return;
            }
            EditorGUILayout.LabelField("Bounds de região: indisponíveis no contrato observado; exibidos somente IDs e pontos configurados.", EditorStyles.miniLabel);
            foreach (NpcDebugSmartObjectSnapshot item in smartObjects)
            {
                string duplicate = item.duplicateId ? " — ID duplicado/ambíguo" : "";
                EditorGUILayout.LabelField(item.objectId + " | " + item.displayName + " | região " + (Empty(item.regionId) ? "unknown" : item.regionId) + duplicate, EditorStyles.boldLabel);
                foreach (NpcDebugSlotSnapshot slot in item.slots)
                    EditorGUILayout.LabelField("  " + slot.slotId + " | " + slot.state + " | atividades " + (Empty(slot.activity) ? "unknown" : slot.activity) +
                        " | owner " + (Empty(slot.ownerNpc) ? "none" : slot.ownerNpc) + " | aproximação " + VectorText(slot.approachPosition, slot.approachStatus) +
                        " | pose " + VectorText(slot.posePosition, slot.poseStatus), EditorStyles.miniLabel);
            }
        }

        private void DrawSelection()
        {
            if (selected == null) return;
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Detalhe: " + selected.displayName + " (" + selected.npcId + ")", EditorStyles.boldLabel);
            Line("Previsto", Describe(selected.intendedActivity, selected.intentStatus) + " → " + Describe(selected.intendedTarget, selected.intentStatus));
            Line("Origem", Describe(selected.intentSource, selected.intentStatus) + " / " + Describe(selected.intentEntry, selected.intentStatus));
            Line("Prioridade / dia", selected.intentIntervalStatus == "available" ? selected.intentPriority + " / " + selected.intentAbsoluteDay : "indisponível");
            Line("Validade", selected.intentIntervalStatus == "available" ? FormatMinute(selected.intentValidFrom) + "–" + FormatMinute(selected.intentValidUntil) : "indisponível");
            Line("Trace", Describe(selected.resolutionReason, selected.intentStatus));
            Line("Efetivo", Describe(selected.executedActivity, selected.executionStatus) + " / " + Describe(selected.executionState, selected.executionStatus));
            Line("Etapa / destino", Describe(selected.operation, selected.executionStatus) + " / " + Describe(selected.effectiveTarget, selected.executionStatus));
            Line("Falha / fallback", Describe(selected.reason, selected.executionStatus) + " / " + selected.fallback.ToString());
            Line("Navegação", selected.navigationStatus + " | path " + selected.pathStatus + " | " + Describe(selected.navigationReason, selected.navigationStatus));
            Line("Posições requested/effective", VectorText(selected.requestedPosition, selected.requestedPosition == null ? "unavailable" : selected.requestedPosition.status) +
                " / " + VectorText(selected.effectivePosition, selected.effectivePosition == null ? "unavailable" : selected.effectivePosition.status));
            Line("Distância / velocidade", (selected.distanceStatus == "available" ? selected.distanceToEffectivePosition.ToString("R") : selected.distanceStatus) +
                " / " + VectorText(selected.velocity, selected.velocityStatus));
            Line("Animação", selected.animationStatus == "available" ? selected.animationState + " (state hash " + selected.animationStateHash + "; sem vínculo causal)" : selected.animationState);
            Line("Reserva", selected.residualOwnership ? "ownership residual ativo" : "sem ownership residual; estado dos slots na seção Smart Objects");
            Line("Última conclusão", selected.lastCompletedStatus == "available" ? selected.lastCompletedEvent + " / " + selected.lastCompletedExecution + " em " + selected.lastCompletedDay + " " + FormatMinute(selected.lastCompletedMinute) : selected.lastCompletedStatus);
            if (!string.IsNullOrEmpty(selected.initializationError)) EditorGUILayout.HelpBox(selected.initializationError, MessageType.Warning);
        }

        private void DrawHistory()
        {
            NpcDebugEvent[] events = NpcDebugDiagnostics.GetRecentEvents(new NpcDebugEventFilter { npcId = selectedNpcId ?? "", limit = 32 });
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Histórico observado nesta sessão", EditorStyles.boldLabel);
            if (events.Length == 0) { EditorGUILayout.LabelField("Nenhuma transição observada desde a inscrição.", EditorStyles.miniLabel); return; }
            foreach (NpcDebugEvent item in events.Reverse())
                EditorGUILayout.LabelField("#" + item.captureOrder + " D" + item.day + " " + FormatMinute(item.minute) + " | " + item.npcId + " | " + item.kind + " | " + item.step + " | " + item.reason, EditorStyles.miniLabel);
        }

        private void Refresh()
        {
            snapshot = NpcDebugDiagnostics.CaptureSnapshot(filter);
            smartObjects = snapshot.smartObjects.ToArray();
            if (!EditorApplication.isPlaying)
            {
                selected = null;
                selectedNpcId = null;
                if (overlayEnabled) NpcDebugSceneOverlay.SetSelection(null, showAllOverlay);
                return;
            }
            selected = !string.IsNullOrEmpty(selectedNpcId) ? NpcDebugDiagnostics.CaptureNpc(selectedNpcId) : null;
            if (selected == null && snapshot.npcs.Count > 0)
            {
                selectedNpcId = snapshot.npcs[0].npcId;
                selected = snapshot.npcs[0];
            }
            if (overlayEnabled) NpcDebugSceneOverlay.SetSelection(selectedNpcId, showAllOverlay);
        }

        private void Locate(string npcId)
        {
            NpcDebugSceneOverlay.Locate(npcId);
            if (overlayEnabled) NpcDebugSceneOverlay.SetSelection(npcId, showAllOverlay);
            selectedNpcId = npcId;
            selected = NpcDebugDiagnostics.CaptureNpc(npcId);
        }

        private void Command(NpcDebugCommandResult result) => notice = result.command + ": " + result.status + " — " + result.reason + (string.IsNullOrEmpty(result.result) ? "" : " (" + result.result + ")");
        private string Summary() => snapshot == null ? "NPC Debug Monitor: indisponível" :
            "NPC Debug Monitor | cena " + snapshot.scenePath + " | modo " + snapshot.mode + " | NPC " + snapshot.registeredNpcCount +
            " (ativos " + snapshot.activeNpcCount + ", prontos " + snapshot.readyNpcCount + ", falhas " + snapshot.failedNpcCount + ") | slots livres/reservados/ocupados " +
            snapshot.slotAvailableCount + "/" + snapshot.slotReservedCount + "/" + snapshot.slotOccupiedCount + " | capturado " + snapshot.capturedAtUtc;
        private static bool Empty(string value) => string.IsNullOrEmpty(value);
        private static string Describe(string value, string status) => status == "available" && !string.IsNullOrEmpty(value) ? value : status;
        private static string FormatMinute(int minute) => (minute / 60).ToString("00") + ":" + (minute % 60).ToString("00");
        private static string VectorText(NpcDebugVector3 value, string status) => status != "available" || value == null ? status : value.x.ToString("R") + "," + value.y.ToString("R") + "," + value.z.ToString("R");
        private static void Line(string label, string value) => EditorGUILayout.LabelField(label, value, EditorStyles.miniLabel);
    }
}
