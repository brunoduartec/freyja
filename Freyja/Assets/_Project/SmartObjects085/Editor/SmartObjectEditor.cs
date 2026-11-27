using System;
using System.Collections.Generic;
using System.Linq;
using Freyja.SmartObjects;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

namespace Freyja.SmartObjects.Editor
{
    [CustomEditor(typeof(SmartObject))]
    public sealed class SmartObjectEditor : UnityEditor.Editor
    {
        private SerializedProperty slotsProperty;

        private void OnEnable() => slotsProperty = serializedObject.FindProperty("slots");

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();
            serializedObject.ApplyModifiedProperties();

            var smartObject = (SmartObject)target;
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Diagnóstico Smart Object", EditorStyles.boldLabel);
            DrawObjectIdDiagnostic(smartObject);
            DrawSlotDiagnostics(smartObject);

            if (GUILayout.Button("Validar pontos de aproximação no NavMesh"))
                ValidateApproaches(smartObject);

            if (Application.isPlaying)
                DrawRuntimeState(smartObject);
        }

        private void DrawObjectIdDiagnostic(SmartObject smartObject)
        {
            List<SmartObject> duplicates = FindSceneObjectsWithId(smartObject.ObjectId);
            if (string.IsNullOrWhiteSpace(smartObject.ObjectId))
            {
                EditorGUILayout.HelpBox("O ID da instância está vazio.", MessageType.Error);
            }
            else if (duplicates.Count > 1)
            {
                string names = string.Join(", ", duplicates.Select(item => item.name));
                EditorGUILayout.HelpBox($"ID duplicado ({smartObject.ObjectId}). Instâncias: {names}. Gere um ID novo para esta instância; nenhum ID foi substituído automaticamente.", MessageType.Error);
                if (GUILayout.Button("Gerar ID novo para esta instância"))
                    RegenerateObjectId(smartObject);
            }
            else
            {
                EditorGUILayout.HelpBox($"ID estável: {smartObject.ObjectId}", MessageType.Info);
            }
        }

        private void DrawSlotDiagnostics(SmartObject smartObject)
        {
            if (slotsProperty == null || slotsProperty.arraySize == 0)
            {
                EditorGUILayout.HelpBox("O objeto ainda não possui slots configurados.", MessageType.Warning);
                return;
            }

            var seen = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < slotsProperty.arraySize; i++)
            {
                SerializedProperty slot = slotsProperty.GetArrayElementAtIndex(i);
                string id = slot.FindPropertyRelative("slotId").stringValue?.Trim();
                bool enabled = slot.FindPropertyRelative("enabled").boolValue;
                SerializedProperty activities = slot.FindPropertyRelative("activityIds");
                Transform approach = slot.FindPropertyRelative("approach").objectReferenceValue as Transform;
                Transform pose = slot.FindPropertyRelative("pose").objectReferenceValue as Transform;

                string prefix = $"Slot {i + 1}";
                if (string.IsNullOrEmpty(id))
                    EditorGUILayout.HelpBox($"{prefix}: ID local vazio.", MessageType.Error);
                else if (!seen.Add(id))
                    EditorGUILayout.HelpBox($"{prefix}: ID local duplicado ({id}). IDs de slot precisam ser únicos dentro deste objeto.", MessageType.Error);

                if (enabled && (activities == null || activities.arraySize == 0 ||
                    Enumerable.Range(0, activities.arraySize).All(index => string.IsNullOrWhiteSpace(activities.GetArrayElementAtIndex(index).stringValue))))
                    EditorGUILayout.HelpBox($"{prefix} ({id}): nenhuma atividade configurada.", MessageType.Error);
                if (enabled && approach == null)
                    EditorGUILayout.HelpBox($"{prefix} ({id}): Transform de aproximação ausente.", MessageType.Error);
                if (enabled && pose == null)
                    EditorGUILayout.HelpBox($"{prefix} ({id}): Transform de pose final ausente.", MessageType.Error);
            }

            if (smartObject.Slots.Count > 0 && !Application.isPlaying)
                EditorGUILayout.HelpBox("Estado de reserva só existe em runtime; a configuração do prefab não persiste reservas.", MessageType.None);
        }

        private static List<SmartObject> FindSceneObjectsWithId(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return new List<SmartObject>();
            return Resources.FindObjectsOfTypeAll<SmartObject>()
                .Where(item => item != null && !EditorUtility.IsPersistent(item) &&
                               item.gameObject.scene.IsValid() && item.gameObject.scene.isLoaded &&
                               string.Equals(item.ObjectId, id, StringComparison.Ordinal))
                .OrderBy(item => GlobalObjectId.GetGlobalObjectIdSlow(item).ToString(), StringComparer.Ordinal)
                .ToList();
        }

        private static void RegenerateObjectId(SmartObject smartObject)
        {
            Undo.RecordObject(smartObject, "Generate Smart Object ID");
            var serialized = new SerializedObject(smartObject);
            serialized.FindProperty("objectId").stringValue = Guid.NewGuid().ToString("N");
            serialized.ApplyModifiedProperties();
            PrefabUtility.RecordPrefabInstancePropertyModifications(smartObject);
            EditorUtility.SetDirty(smartObject);
            if (smartObject.gameObject.scene.IsValid())
                EditorSceneManager.MarkSceneDirty(smartObject.gameObject.scene);
        }

        private static void ValidateApproaches(SmartObject smartObject)
        {
            NavMeshTriangulation mesh = NavMesh.CalculateTriangulation();
            if (mesh.vertices == null || mesh.vertices.Length == 0)
            {
                Debug.LogWarning("Smart Object NavMesh check was not executable: no NavMesh is loaded in Edit Mode. Validate after IslandWaypointNetwork loads its data in Play Mode.", smartObject);
                return;
            }

            int checkedCount = 0;
            int invalidCount = 0;
            foreach (InteractionSlot slot in smartObject.Slots)
            {
                if (slot == null || !slot.IsEnabled || slot.Approach == null) continue;
                checkedCount++;
                if (!NavMesh.SamplePosition(slot.Approach.position, out _, 0.2f, NavMesh.AllAreas))
                {
                    invalidCount++;
                    Debug.LogError($"Slot '{slot.SlotId}' approach is not within 0.2 m of the loaded NavMesh.", smartObject);
                }
            }

            if (invalidCount == 0)
                Debug.Log($"Smart Object '{smartObject.ObjectId}': {checkedCount} approach point(s) sampled on the loaded NavMesh.", smartObject);
        }

        private static void DrawRuntimeState(SmartObject smartObject)
        {
            foreach (InteractionSlot slot in smartObject.Slots)
            {
                if (slot == null) continue;
                string owner = slot.ReservationOwner != null ? slot.ReservationOwner.name : "—";
                EditorGUILayout.LabelField($"{slot.SlotId} · {slot.State}", $"dono: {owner}");
            }
        }

        private void OnSceneGUI()
        {
            var smartObject = (SmartObject)target;
            var sceneView = SceneView.currentDrawingSceneView;
            Transform viewTransform = sceneView != null && sceneView.camera != null
                ? sceneView.camera.transform
                : null;
            Vector3 screenRight = viewTransform != null ? viewTransform.right : Vector3.right;
            Vector3 screenUp = viewTransform != null ? viewTransform.up : Vector3.up;
            var labelStyle = new GUIStyle(EditorStyles.whiteMiniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12,
                wordWrap = false,
                fontStyle = FontStyle.Bold
            };
            int slotIndex = 0;
            foreach (InteractionSlot slot in smartObject.Slots)
            {
                if (slot == null)
                {
                    slotIndex++;
                    continue;
                }

                Color stateColor = slot.State switch
                {
                    InteractionSlotState.Available => new Color(0.2f, 0.9f, 0.35f),
                    InteractionSlotState.Reserved => new Color(1f, 0.68f, 0.1f),
                    InteractionSlotState.Occupied => new Color(0.95f, 0.25f, 0.2f),
                    _ => Color.gray
                };

                if (slot.Approach != null)
                {
                    Handles.color = stateColor;
                    Handles.SphereHandleCap(0, slot.Approach.position, Quaternion.identity, 0.22f, EventType.Repaint);
                    Handles.ArrowHandleCap(0, slot.Approach.position, slot.Approach.rotation, 0.48f, EventType.Repaint);
                    float lane = (slotIndex - (smartObject.Slots.Count - 1) * 0.5f) * 3.8f;
                    Vector3 approachLabel = slot.Approach.position + screenRight * lane + screenUp * 0.48f;
                    Handles.Label(approachLabel, $"{slot.SlotId}\nAproximação", labelStyle);
                }

                if (slot.Pose != null)
                {
                    Handles.color = new Color(0.2f, 0.75f, 1f);
                    Handles.RectangleHandleCap(0, slot.Pose.position, slot.Pose.rotation, 0.28f, EventType.Repaint);
                    Handles.ArrowHandleCap(0, slot.Pose.position, slot.Pose.rotation, 0.42f, EventType.Repaint);
                    if (slot.Approach != null)
                    {
                        float lane = (slotIndex - (smartObject.Slots.Count - 1) * 0.5f) * 3.8f;
                        Vector3 poseLabel = slot.Pose.position + screenRight * lane - screenUp * 0.58f;
                        Handles.Label(poseLabel, $"{slot.SlotId}\nPose final", labelStyle);
                        Handles.color = new Color(stateColor.r, stateColor.g, stateColor.b, 0.65f);
                        Handles.DrawLine(slot.Approach.position, slot.Pose.position);
                    }
                }

                slotIndex++;
            }

            Handles.color = Color.white;
            Handles.BeginGUI();
            var idStyle = new GUIStyle(EditorStyles.helpBox)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 12,
                fontStyle = FontStyle.Bold
            };
            GUI.Label(new Rect(105f, 12f, 430f, 26f), $"Smart Object ID: {smartObject.ObjectId}", idStyle);
            Handles.EndGUI();
        }

        [MenuItem("Freyja/Smart Objects/Validar objetos na cena aberta")]
        private static void ValidateOpenScene()
        {
            SmartObject[] objects = Resources.FindObjectsOfTypeAll<SmartObject>()
                .Where(item => item != null && item.gameObject.scene.IsValid() && item.gameObject.scene.isLoaded)
                .OrderBy(item => item.ObjectId, StringComparer.Ordinal)
                .ToArray();

            var duplicateIds = objects.GroupBy(item => item.ObjectId, StringComparer.Ordinal)
                .Where(group => string.IsNullOrWhiteSpace(group.Key) || group.Count() > 1).ToArray();
            int invalidSlots = objects.Sum(item => item.Slots.Count(slot => slot == null ||
                string.IsNullOrWhiteSpace(slot.SlotId) || slot.Approach == null || slot.Pose == null ||
                !slot.IsEnabled || slot.ActivityIds.Count == 0));

            if (duplicateIds.Length > 0)
                foreach (var group in duplicateIds)
                    Debug.LogError($"Smart Object ID vazio/duplicado '{group.Key}': {string.Join(", ", group.Select(item => item.name))}. Corrija IDs pela ação explícita do Inspector.");

            NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();
            string navigation = triangulation.vertices != null && triangulation.vertices.Length > 0
                ? "NavMesh carregado; execute a validação de aproximação por objeto."
                : "NavMesh não carregado no Editor; checagem espacial não executável até o runtime carregar o asset.";
            Debug.Log($"Smart Objects: {objects.Length} objeto(s), {duplicateIds.Length} grupo(s) de ID inválido, {invalidSlots} slot(s) incompleto(s). {navigation}");
        }
    }
}
