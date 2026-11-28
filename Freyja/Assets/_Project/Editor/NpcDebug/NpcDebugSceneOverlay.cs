using System;
using System.Collections.Generic;
using Freyja.NpcScheduling.Unity;
using Freyja.SmartObjects;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Freyja.NpcDebug.Editor
{
    internal static class NpcDebugSceneOverlay
    {
        private struct SceneLabel
        {
            public GUIContent content;
            public Vector2 guiPosition;
            public Color color;
        }

        private sealed class NpcDrawData
        {
            public string id;
            public Vector3 position;
            public List<Vector3> path = new List<Vector3>();
        }

        private sealed class ObjectDrawData
        {
            public sealed class PointDrawData
            {
                public string label;
                public Vector3 position;
            }
            public string id;
            public string region;
            public Vector3 position;
            public readonly List<PointDrawData> points = new List<PointDrawData>();
        }

        private static readonly List<NpcDrawData> Npcs = new List<NpcDrawData>();
        private static readonly List<ObjectDrawData> Objects = new List<ObjectDrawData>();
        private static readonly List<SceneLabel> Labels = new List<SceneLabel>(24);
        private static readonly List<Rect> PlacedLabelRects = new List<Rect>(24);
        private static GUIStyle sceneLabelStyle;
        private static string selectedNpcId;
        private static bool showAll;
        private static bool enabled;
        private static double nextRefresh;

        [InitializeOnLoadMethod]
        private static void RegisterLifecycle()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            SceneManager.activeSceneChanged -= OnSceneChanged;
            SceneManager.activeSceneChanged += OnSceneChanged;
        }

        internal static bool IsEnabled => enabled;

        internal static void Enable(string npcId, bool all)
        {
            selectedNpcId = npcId;
            showAll = all;
            if (enabled) return;
            enabled = true;
            SceneView.duringSceneGui -= OnSceneGui;
            SceneView.duringSceneGui += OnSceneGui;
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update += OnEditorUpdate;
            Refresh();
            SceneView.RepaintAll();
        }

        internal static void SetSelection(string npcId, bool all)
        {
            if (string.Equals(selectedNpcId, npcId, StringComparison.Ordinal) && showAll == all) return;
            selectedNpcId = npcId;
            showAll = all;
            SceneView.RepaintAll();
        }

        internal static void Disable()
        {
            enabled = false;
            SceneView.duringSceneGui -= OnSceneGui;
            EditorApplication.update -= OnEditorUpdate;
            Npcs.Clear();
            Objects.Clear();
            SceneView.RepaintAll();
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode || state == PlayModeStateChange.EnteredEditMode) Disable();
        }

        private static void OnSceneChanged(Scene previous, Scene next) => Disable();

        internal static void Locate(string npcId)
        {
            foreach (NpcActivityController host in NpcDebugDiagnostics.GetCachedHosts())
            {
                if (host == null || !string.Equals(host.NpcId, npcId, StringComparison.Ordinal)) continue;
                Selection.activeGameObject = host.gameObject;
                EditorGUIUtility.PingObject(host.gameObject);
                SceneView.lastActiveSceneView?.FrameSelected();
                return;
            }
        }

        private static void OnEditorUpdate()
        {
            if (EditorApplication.timeSinceStartup < nextRefresh) return;
            nextRefresh = EditorApplication.timeSinceStartup + 0.5d;
            Refresh();
            SceneView.RepaintAll();
        }

        private static void Refresh()
        {
            Npcs.Clear();
            Objects.Clear();
            NpcDebugSnapshot snapshotSet = NpcDebugDiagnostics.CaptureSnapshot(new NpcDebugFilter { limit = 512 });
            NpcActivityController[] hosts = NpcDebugDiagnostics.GetCachedHosts();
            foreach (NpcDebugNpcSnapshot snapshot in snapshotSet.npcs)
            {
                NpcActivityController host = Array.Find(hosts, item => string.Equals(item.NpcId, snapshot.npcId, StringComparison.Ordinal));
                if (host == null) continue;
                var value = new NpcDrawData { id = host.NpcId, position = host.transform.position };
                foreach (NpcDebugVector3 corner in snapshot.pathCorners)
                    if (corner != null && corner.status == "available") value.path.Add(new Vector3(corner.x, corner.y, corner.z));
                Npcs.Add(value);
            }
            NpcDebugSmartObjectSnapshot[] objects = snapshotSet.smartObjects.ToArray();
            SmartObject[] liveObjects = SmartObjectRegistry.EditorGetRegisteredObjects();
            foreach (NpcDebugSmartObjectSnapshot snapshot in objects)
            {
                if (snapshot.duplicateId) continue;
                SmartObject item = null;
                int idMatches = 0;
                foreach (SmartObject candidate in liveObjects)
                    if (candidate != null && string.Equals(candidate.ObjectId, snapshot.objectId, StringComparison.Ordinal))
                    {
                        item = candidate;
                        idMatches++;
                    }
                if (idMatches != 1 || item == null) continue;
                var value = new ObjectDrawData { id = snapshot.objectId, region = snapshot.regionId, position = item.transform.position };
                foreach (InteractionSlot slot in item.Slots)
                {
                    if (slot == null) continue;
                    if (slot.Approach != null) value.points.Add(new ObjectDrawData.PointDrawData { label = slot.SlotId + " approach", position = slot.Approach.position });
                    if (slot.Pose != null) value.points.Add(new ObjectDrawData.PointDrawData { label = slot.SlotId + " pose", position = slot.Pose.position });
                }
                Objects.Add(value);
            }
        }

        private static void OnSceneGui(SceneView sceneView)
        {
            if (!enabled || Event.current.type != EventType.Repaint || sceneView.camera == null) return;
            Color previous = Handles.color;
            try
            {
                Labels.Clear();
                foreach (NpcDrawData npc in Npcs)
                {
                    if (!showAll && !string.Equals(npc.id, selectedNpcId, StringComparison.Ordinal)) continue;
                    bool selected = string.Equals(npc.id, selectedNpcId, StringComparison.Ordinal);
                    Color color = selected ? Color.cyan : new Color(0.3f, 0.85f, 1f, 0.95f);
                    AddLabel("NPC " + npc.id, npc.position + Vector3.up * 1.8f, color, sceneView);
                    Handles.color = color;
                    for (int i = 1; i < npc.path.Count; i++) Handles.DrawLine(npc.path[i - 1], npc.path[i]);
                    foreach (Vector3 point in npc.path) Handles.DrawWireDisc(point, Vector3.up, 0.12f);
                }
                Handles.color = new Color(1f, 0.78f, 0.25f, 0.8f);
                foreach (ObjectDrawData item in Objects)
                {
                    AddLabel("SmartObject " + item.id + " | região " + (string.IsNullOrEmpty(item.region) ? "unknown" : item.region) + " | bounds indisponíveis", item.position + Vector3.up * 0.5f, new Color(1f, 0.78f, 0.25f, 1f), sceneView);
                    Handles.DrawWireDisc(item.position, Vector3.up, 0.18f);
                    foreach (ObjectDrawData.PointDrawData point in item.points)
                    {
                        Handles.DrawWireDisc(point.position, Vector3.up, 0.1f);
                        AddLabel(point.label, point.position + Vector3.up * 0.15f, new Color(1f, 0.9f, 0.55f, 1f), sceneView);
                    }
                }
                DrawLabels(sceneView);
            }
            finally { Handles.color = previous; }
        }

        private static void AddLabel(string text, Vector3 worldPosition, Color color, SceneView sceneView)
        {
            Vector3 viewport = sceneView.camera.WorldToViewportPoint(worldPosition);
            if (viewport.z <= 0f || viewport.x < 0f || viewport.x > 1f || viewport.y < 0f || viewport.y > 1f) return;
            Labels.Add(new SceneLabel
            {
                content = new GUIContent(text),
                guiPosition = HandleUtility.WorldToGUIPoint(worldPosition),
                color = color
            });
        }

        private static void DrawLabels(SceneView sceneView)
        {
            if (Labels.Count == 0) return;
            Labels.Sort(CompareLabelY);
            PlacedLabelRects.Clear();
            Rect view = new Rect(6f, 32f, Mathf.Max(1f, sceneView.position.width - 12f), Mathf.Max(1f, sceneView.position.height - 64f));
            GUIStyle style = GetSceneLabelStyle();
            Color previousTextColor = style.normal.textColor;
            Handles.BeginGUI();
            try
            {
                foreach (SceneLabel label in Labels)
                {
                    GUIContent content = label.content;
                    Vector2 size = style.CalcSize(content);
                    size.x = Mathf.Min(size.x + 8f, Mathf.Min(320f, Mathf.Max(48f, view.width)));
                    size.y = Mathf.Max(18f, style.CalcHeight(content, size.x - 8f) + 4f);
                    Rect preferred = new Rect(label.guiPosition.x - size.x * 0.5f, label.guiPosition.y - size.y * 0.5f, size.x, size.y);
                    Rect placed = FindLabelPosition(preferred, view);
                    PlacedLabelRects.Add(placed);
                    EditorGUI.DrawRect(placed, new Color(0.04f, 0.045f, 0.055f, 0.88f));
                    style.normal.textColor = label.color;
                    GUI.Label(new Rect(placed.x + 4f, placed.y + 1f, placed.width - 8f, placed.height - 2f), content, style);
                }
            }
            finally
            {
                style.normal.textColor = previousTextColor;
                Handles.EndGUI();
            }
        }

        private static GUIStyle GetSceneLabelStyle()
        {
            if (sceneLabelStyle == null)
            {
                sceneLabelStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    wordWrap = true,
                    clipping = TextClipping.Clip
                };
            }
            return sceneLabelStyle;
        }

        private static Rect FindLabelPosition(Rect preferred, Rect view)
        {
            float maxY = Mathf.Max(view.yMin, view.yMax - preferred.height);
            preferred.x = Mathf.Clamp(preferred.x, view.xMin, Mathf.Max(view.xMin, view.xMax - preferred.width));
            preferred.y = Mathf.Clamp(preferred.y, view.yMin, maxY);
            float step = preferred.height + 3f;
            for (int i = 0; i <= PlacedLabelRects.Count; i++)
            {
                Rect candidate = preferred;
                candidate.y = Mathf.Clamp(preferred.y + step * i, view.yMin, maxY);
                if (!OverlapsPlacedLabel(candidate)) return candidate;
            }
            for (int i = 1; i <= PlacedLabelRects.Count; i++)
            {
                Rect candidate = preferred;
                candidate.y = Mathf.Clamp(preferred.y - step * i, view.yMin, maxY);
                if (!OverlapsPlacedLabel(candidate)) return candidate;
            }
            return preferred;
        }

        private static bool OverlapsPlacedLabel(Rect candidate)
        {
            for (int i = 0; i < PlacedLabelRects.Count; i++)
            {
                Rect existing = PlacedLabelRects[i];
                existing.yMin -= 2f;
                existing.yMax += 2f;
                if (candidate.Overlaps(existing)) return true;
            }
            return false;
        }

        private static int CompareLabelY(SceneLabel left, SceneLabel right)
        {
            int byY = left.guiPosition.y.CompareTo(right.guiPosition.y);
            return byY != 0 ? byY : left.guiPosition.x.CompareTo(right.guiPosition.x);
        }
    }
}
