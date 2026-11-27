using System;
using System.Collections.Generic;
using System.Linq;
using Freyja.Prototype;
using UnityEngine;

namespace Freyja.NpcScheduling.Unity
{
    [Serializable]
    public sealed class NpcLocationPoint
    {
        [SerializeField] private string pointId;
        [SerializeField] private string regionId;
        [SerializeField] private Transform point;
        public string PointId => pointId;
        public string RegionId => regionId;
        public Transform Point => point;
    }

    [DisallowMultipleComponent]
    public sealed class NpcLocationRegistry : MonoBehaviour
    {
        [SerializeField] private IslandWaypointNetwork waypointNetwork;
        [SerializeField] private List<NpcLocationPoint> points = new List<NpcLocationPoint>();
        private readonly Dictionary<string,Transform> byId = new Dictionary<string,Transform>(StringComparer.Ordinal);
        private readonly HashSet<string> duplicateIds = new HashSet<string>(StringComparer.Ordinal);
        private readonly Dictionary<string,List<NpcLocationPoint>> byRegion = new Dictionary<string,List<NpcLocationPoint>>(StringComparer.Ordinal);

        public bool UsesNetwork(IslandWaypointNetwork network)=>waypointNetwork!=null&&waypointNetwork==network;

        private void OnEnable() => Rebuild();
        private void OnDisable(){byId.Clear();duplicateIds.Clear();byRegion.Clear();}

        public bool TryGetPoint(string pointId,out Transform target)
        {
            target=null;
            if(String.IsNullOrWhiteSpace(pointId)||duplicateIds.Contains(pointId)||!byId.TryGetValue(pointId,out target))return false;
            if(target==null||!target.gameObject.activeInHierarchy){target=null;return false;}
            return true;
        }

        public IList<string> FindWaypointIds(string regionId)
        {
            if(String.IsNullOrWhiteSpace(regionId)||!byRegion.TryGetValue(regionId,out List<NpcLocationPoint> region))return new List<string>();
            return region.Where(x=>x!=null&&x.Point!=null&&x.Point.gameObject.activeInHierarchy&&!duplicateIds.Contains(x.PointId))
                .OrderBy(x=>x.PointId,StringComparer.Ordinal).Select(x=>x.PointId).ToList();
        }

        private void Rebuild()
        {
            byId.Clear();duplicateIds.Clear();byRegion.Clear();
            if(points==null)return;
            for(int i=0;i<points.Count;i++)
            {
                NpcLocationPoint item=points[i];
                if(item==null||String.IsNullOrWhiteSpace(item.PointId)||String.IsNullOrWhiteSpace(item.RegionId)||item.Point==null)
                { Debug.LogError("NPC location registry has an incomplete point at index "+i+".",this); continue; }
                if(waypointNetwork==null||!item.Point.IsChildOf(waypointNetwork.transform))
                { Debug.LogError("NPC location point '"+item.PointId+"' is not a child of the configured IslandWaypointNetwork.",this); continue; }
                if(byId.ContainsKey(item.PointId)||duplicateIds.Contains(item.PointId))
                {
                    byId.Remove(item.PointId); duplicateIds.Add(item.PointId);
                    Debug.LogError("Duplicate NPC location point ID '"+item.PointId+"'.",this); continue;
                }
                byId.Add(item.PointId,item.Point);
                if(!byRegion.TryGetValue(item.RegionId,out List<NpcLocationPoint> region))byRegion.Add(item.RegionId,region=new List<NpcLocationPoint>());
                region.Add(item);
            }
        }
    }
}
