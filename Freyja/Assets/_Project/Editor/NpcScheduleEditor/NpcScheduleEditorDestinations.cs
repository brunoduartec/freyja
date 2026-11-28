using System;
using System.Collections.Generic;
using System.Linq;
using Freyja.NpcScheduling.Unity;
using Freyja.SmartObjects;
using UnityEditor;
using UnityEngine;

namespace Freyja.NpcScheduling.Editor
{
    internal sealed class NpcScheduleDestinationOption
    {
        public string Id;
        public string Label;
        public string Status;
        public string Evidence;
    }

    internal sealed class NpcScheduleDestinationSnapshot
    {
        private readonly Dictionary<string, NpcScheduleDestinationOption[]> options = new Dictionary<string, NpcScheduleDestinationOption[]>(StringComparer.Ordinal);
        private readonly Dictionary<string, string> statusByKindAndId = new Dictionary<string, string>(StringComparer.Ordinal);

        public string CapturedScene { get; private set; }
        public DateTime CapturedAtUtc { get; private set; }

        public static NpcScheduleDestinationSnapshot Capture()
        {
            var snapshot = new NpcScheduleDestinationSnapshot
            {
                CapturedScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path,
                CapturedAtUtc = DateTime.UtcNow
            };
            snapshot.CaptureLocations();
            snapshot.CaptureSmartObjects();
            return snapshot;
        }

        public NpcScheduleDestinationOption[] Choices(string activityId, TargetKind targetKind)
        {
            if (targetKind == TargetKind.None) return Array.Empty<NpcScheduleDestinationOption>();
            string key = Key(activityId, targetKind);
            if (!options.ContainsKey(key) && targetKind == TargetKind.Location) key = Key("location", targetKind);
            return options.TryGetValue(key, out NpcScheduleDestinationOption[] values) ? values : Array.Empty<NpcScheduleDestinationOption>();
        }

        public string StatusFor(string activityId, TargetKind targetKind, string id)
        {
            if (targetKind == TargetKind.None) return string.IsNullOrWhiteSpace(id) ? "resolved" : "invalid_target";
            if (string.IsNullOrWhiteSpace(id)) return "invalid_target";
            string key = Key(activityId, targetKind);
            if (!options.ContainsKey(key) && targetKind == TargetKind.Location) key = Key("location", targetKind);
            return statusByKindAndId.TryGetValue(key + "|" + id, out string status) ? status : "unverified";
        }

        private void CaptureLocations()
        {
            var knownRegions = new HashSet<string>(StringComparer.Ordinal);
            var activeRegions = new HashSet<string>(StringComparer.Ordinal);
            foreach (NpcLocationRegistry registry in Resources.FindObjectsOfTypeAll<NpcLocationRegistry>())
            {
                if (registry == null || EditorUtility.IsPersistent(registry) || !IsInActiveScene(registry)) continue;
                var serialized = new SerializedObject(registry);
                SerializedProperty points = serialized.FindProperty("points");
                Component networkComponent = serialized.FindProperty("waypointNetwork")?.objectReferenceValue as Component;
                Transform network = networkComponent != null ? networkComponent.transform : null;
                for (int i = 0; points != null && i < points.arraySize; i++)
                {
                    SerializedProperty row = points.GetArrayElementAtIndex(i);
                    string region = row.FindPropertyRelative("regionId")?.stringValue;
                    Transform point = row.FindPropertyRelative("point")?.objectReferenceValue as Transform;
                    if (string.IsNullOrWhiteSpace(region) || point == null) continue;
                    knownRegions.Add(region);
                    if (registry.isActiveAndEnabled && network != null && point.IsChildOf(network) && point.gameObject.activeInHierarchy)
                        activeRegions.Add(region);
                }
            }
            AddLocationOptions("wander", knownRegions.OrderBy(x => x, StringComparer.Ordinal).Select(id => new NpcScheduleDestinationOption
            {
                Id = id, Label = id + (activeRegions.Contains(id) ? "" : " · indisponível"),
                Status = activeRegions.Contains(id) ? "resolved" : "unavailable",
                Evidence = activeRegions.Contains(id)
                    ? "Região com waypoint ativo no NpcLocationRegistry da cena capturada."
                    : "Região declarada, mas sem registry/waypoint ativo compatível na cena capturada."
            }).ToArray());
            AddLocationOptions("location", knownRegions.OrderBy(x => x, StringComparer.Ordinal).Select(id => new NpcScheduleDestinationOption
            { Id = id, Label = id, Status = "unverified", Evidence = "Região conhecida; compatibilidade específica desta atividade não está exposta pelo runtime." }).ToArray());
        }

        private void CaptureSmartObjects()
        {
            SmartObject[] allSceneObjects = Resources.FindObjectsOfTypeAll<SmartObject>()
                .Where(item => item != null && !EditorUtility.IsPersistent(item) && IsInActiveScene(item)).ToArray();
            SmartObject[] loaded = allSceneObjects.Where(item => item.IsAvailable).ToArray();
            var compatibleRegions = new HashSet<string>(StringComparer.Ordinal);
            foreach (SmartObject item in loaded)
                if (item.Slots.Any(slot => slot != null && slot.IsEnabled && slot.SupportsActivity("sit")) && !string.IsNullOrWhiteSpace(item.RegionId))
                    compatibleRegions.Add(item.RegionId);

            var allSitRegions = allSceneObjects.Where(item => item.Slots.Any(slot => slot != null && slot.SupportsActivity("sit")) && !string.IsNullOrWhiteSpace(item.RegionId))
                .Select(item => item.RegionId).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray();
            AddLocationOptions("sit", allSitRegions.Select(id => new NpcScheduleDestinationOption
            {
                Id = id, Label = id + (compatibleRegions.Contains(id) ? "" : " · indisponível"),
                Status = compatibleRegions.Contains(id) ? "resolved" : "unavailable",
                Evidence = compatibleRegions.Contains(id)
                    ? "Smart Object ativo com slot sit compatível; disponibilidade do slot é dinâmica."
                    : "Smart Object sit conhecido, mas sem objeto/slot ativo na cena capturada."
            }).ToArray());

            var eligible = loaded.Where(item => item.Slots.Any(slot => slot != null && slot.IsEnabled &&
                slot.ActivityIds != null && slot.ActivityIds.Any(activity => !string.IsNullOrWhiteSpace(activity)))).ToArray();
            AddObjectsForActivities(eligible, allSceneObjects, loaded);
        }

        private void AddObjectsForActivities(SmartObject[] eligible, SmartObject[] allSceneObjects, SmartObject[] loaded)
        {
            var activityIds = allSceneObjects.SelectMany(item => item.Slots.Where(slot => slot != null)
                    .SelectMany(slot => slot.ActivityIds ?? Array.Empty<string>())).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.Ordinal).ToArray();
            foreach (string activityId in activityIds)
            {
                SmartObject[] compatible = eligible.Where(item => item.Slots.Any(slot => slot != null && slot.IsEnabled && slot.SupportsActivity(activityId))).ToArray();
                var counts = allSceneObjects.GroupBy(x => x.ObjectId ?? "", StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal);
                NpcScheduleDestinationOption[] values = compatible.Where(x => !string.IsNullOrWhiteSpace(x.ObjectId)).Select(item =>
                {
                    bool duplicate = counts[item.ObjectId] > 1;
                    string state = duplicate ? "ambiguous" : "resolved";
                    return new NpcScheduleDestinationOption
                    {
                        Id = item.ObjectId,
                        Label = item.ObjectId + " — " + HierarchyPath(item.transform) + (duplicate ? " · ID duplicado (" + counts[item.ObjectId] + ")" : ""),
                        Status = state,
                        Evidence = duplicate ? "IDs de Smart Object repetidos na cena; runtime não pode desambiguar" : "Smart Object com slot compatível na cena capturada"
                    };
                }).OrderBy(x => x.Id, StringComparer.Ordinal).ThenBy(x => x.Label, StringComparer.Ordinal).ToArray();
                options[Key(activityId, TargetKind.Object)] = values;
                AddStatus(activityId, TargetKind.Object, values);

                foreach (IGrouping<string, SmartObject> group in allSceneObjects.Where(x => !string.IsNullOrWhiteSpace(x.ObjectId)).GroupBy(x => x.ObjectId, StringComparer.Ordinal))
                {
                    if (statusByKindAndId.ContainsKey(Key(activityId, TargetKind.Object) + "|" + group.Key)) continue;
                    bool supportsActivity = group.Any(x => x.Slots.Any(slot => slot != null && slot.SupportsActivity(activityId)));
                    bool enabled = loaded.Any(x => x.ObjectId == group.Key && x.Slots.Any(slot => slot != null && slot.IsEnabled && slot.SupportsActivity(activityId)));
                    statusByKindAndId[Key(activityId, TargetKind.Object) + "|" + group.Key] = group.Count() > 1 ? "ambiguous" :
                        !supportsActivity ? "invalid_target" : !enabled ? "unavailable" : "unverified";
                }
            }
        }

        private void AddLocationOptions(string activityId, NpcScheduleDestinationOption[] values)
        {
            options[Key(activityId, TargetKind.Location)] = values;
            AddStatus(activityId, TargetKind.Location, values);
        }

        private void AddStatus(string activityId, TargetKind kind, NpcScheduleDestinationOption[] values)
        {
            foreach (IGrouping<string, NpcScheduleDestinationOption> group in values.GroupBy(x => x.Id, StringComparer.Ordinal))
                statusByKindAndId[Key(activityId, kind) + "|" + group.Key] = group.Select(x => x.Status).OrderByDescending(StatusRank).First();
        }

        private static int StatusRank(string status)
        {
            switch (status)
            {
                case "ambiguous": return 5;
                case "unavailable": return 4;
                case "invalid_target": return 3;
                case "resolved": return 2;
                default: return 1;
            }
        }

        private bool IsInActiveScene(Component item) => item.gameObject.scene.IsValid() && item.gameObject.scene.path == CapturedScene;
        private static string Key(string activityId, TargetKind targetKind) => targetKind + "|" + activityId;
        private static string HierarchyPath(Transform transform)
        {
            string path = transform.name;
            while (transform.parent != null) { transform = transform.parent; path = transform.name + "/" + path; }
            return path;
        }
    }
}
