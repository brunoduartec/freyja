using System;
using System.Collections.Generic;
using System.Linq;
using Freyja.Environment;
using UnityEditor;
using UnityEngine;

namespace Freyja.Editor
{
    public static partial class CoastalGrassAuthoring
    {
        private const string IslandArtPath = "Assets/_Project/Art/Grass032/";
        private const int IslandSamplesPerSide = 48;
        private const float PatchEdgeScores = 0.06f;
        private const float ZoneBlendWidth = 1.8f;
        private const float ZoneWarp = 4.2f;
        private const float ZoneInfluence = ZoneBlendWidth + ZoneWarp * 0.5f;

        [Serializable]
        private sealed class IslandGrassProfile
        {
            public int seed;
            public IslandPolygon[] exclusions = Array.Empty<IslandPolygon>();
            public IslandZone[] zones = Array.Empty<IslandZone>();
        }

        [Serializable]
        private sealed class IslandZone
        {
            public int id;
            public string name;
            public float coverage;
            public float edgeDensity;
            public float coreDensity;
            public float scaleMin;
            public float scaleMax;
            public float anchorRadius;
            public Vector2[] anchors = Array.Empty<Vector2>();
            public IslandPolygon[] shapes = Array.Empty<IslandPolygon>();

            public Vector4 Style => new(edgeDensity, coreDensity, scaleMin, scaleMax);

            public float Score(Vector2 point)
            {
                float anchorStrength = 0;
                foreach (Vector2 anchor in anchors)
                    anchorStrength = Mathf.Max(anchorStrength, 1 - Vector2.Distance(point, anchor) / anchorRadius);

                float broad = Mathf.PerlinNoise(point.x * 0.11f + id * 1.7f, point.y * 0.11f + 36);
                float edge = Mathf.PerlinNoise(point.x * 0.29f + 24, point.y * 0.29f + id * 1.3f);
                return 0.75f * broad + 0.25f * edge + 0.42f * anchorStrength;
            }
        }

        [Serializable]
        private sealed class IslandPolygon
        {
            public Vector2[] points = Array.Empty<Vector2>();
            private Vector2 minimum;
            private Vector2 maximum;

            public void Prepare()
            {
                if (points.Length < 3)
                    throw new InvalidOperationException("An island grass shape requires at least three points.");
                minimum = maximum = points[0];
                foreach (Vector2 point in points)
                {
                    minimum = Vector2.Min(minimum, point);
                    maximum = Vector2.Max(maximum, point);
                }
            }

            public bool Overlaps(Vector2 min, Vector2 max) =>
                maximum.x >= min.x && minimum.x <= max.x && maximum.y >= min.y && minimum.y <= max.y;

            public bool Contains(Vector2 point)
            {
                if (!Overlaps(point, point))
                    return false;

                bool inside = false;
                int previous = points.Length - 1;
                for (int index = 0; index < points.Length; index++)
                {
                    Vector2 a = points[previous];
                    Vector2 b = points[index];
                    if ((a.y > point.y) != (b.y > point.y) &&
                        point.x < a.x + (point.y - a.y) * (b.x - a.x) / (b.y - a.y))
                        inside = !inside;
                    previous = index;
                }

                return inside;
            }

            public float ZoneWeight(Vector2 point)
            {
                Vector2 margin = Vector2.one * ZoneBlendWidth;
                if (!Overlaps(point - margin, point + margin))
                    return 0;

                float nearest = ZoneBlendWidth * ZoneBlendWidth;
                Vector2 a = points[points.Length - 1];
                foreach (Vector2 b in points)
                {
                    Vector2 edge = b - a;
                    if (edge.sqrMagnitude > 0)
                    {
                        float t = Mathf.Clamp01(Vector2.Dot(point - a, edge) / edge.sqrMagnitude);
                        nearest = Mathf.Min(nearest, (point - (a + t * edge)).sqrMagnitude);
                    }
                    a = b;
                }

                float signedDistance = Mathf.Sqrt(nearest);
                if (!Contains(point))
                    signedDistance = -signedDistance;
                return Mathf.SmoothStep(0, 1, 0.5f + signedDistance / (2 * ZoneBlendWidth));
            }
        }

        private readonly struct IslandSite
        {
            public IslandSite(Vector3 position, int zoneIndex, float score, float surfaceWeight)
            {
                Position = position;
                ZoneIndex = zoneIndex;
                Score = score;
                SurfaceWeight = surfaceWeight;
            }

            public Vector3 Position { get; }
            public int ZoneIndex { get; }
            public float Score { get; }
            public float SurfaceWeight { get; }
        }

        private sealed class IslandCellSites
        {
            public IslandCellSites(int seed, int[] zones, List<IslandSite> sites)
            {
                Seed = seed;
                Zones = zones;
                Sites = sites;
            }

            public int Seed { get; }
            public int[] Zones { get; }
            public List<IslandSite> Sites { get; }
        }

        private static IslandGrassProfile LoadIslandProfile(float maximumDensity)
        {
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(IslandArtPath + "IslandGrassZones.json");
            if (!asset)
                throw new InvalidOperationException("IslandGrassZones.json was not imported.");

            var profile = JsonUtility.FromJson<IslandGrassProfile>(asset.text);
            if (profile == null || profile.zones.Length == 0 ||
                profile.zones.Select(zone => zone.id).Distinct().Count() != profile.zones.Length)
                throw new InvalidOperationException("The island grass profile needs distinct zone IDs.");

            foreach (IslandZone zone in profile.zones)
            {
                if (zone.coverage <= 0 || zone.coverage > 1 || zone.edgeDensity <= 0 ||
                    zone.coreDensity < zone.edgeDensity || zone.coreDensity > maximumDensity ||
                    zone.scaleMin <= 0 || zone.scaleMax < zone.scaleMin || zone.anchorRadius <= 0 ||
                    zone.shapes.Length == 0)
                    throw new InvalidOperationException($"Invalid island grass zone: {zone.name}.");

                foreach (IslandPolygon shape in zone.shapes)
                    shape.Prepare();
            }

            foreach (IslandPolygon exclusion in profile.exclusions)
                exclusion.Prepare();

            return profile;
        }

        private static CoastalGrassLayout.Cell[] BuildIslandCells(Terrain terrain, Mesh[] meshes,
            PathTriangle[] paths, List<Vector2> accessPoints)
        {
            const float step = CellSize / IslandSamplesPerSide;
            const float sampleArea = step * step;
            IslandGrassProfile profile = LoadIslandProfile(1 / sampleArea);
            List<float>[] scores = profile.zones.Select(_ => new List<float>()).ToArray();
            List<IslandCellSites> cells = SampleIsland(terrain, profile, paths, accessPoints, scores);
            float[] thresholds = new float[scores.Length];
            for (int index = 0; index < scores.Length; index++)
            {
                List<float> zoneScores = scores[index];
                if (zoneScores.Count == 0)
                    continue;

                zoneScores.Sort();
                int cutoff = Mathf.Clamp(Mathf.FloorToInt(zoneScores.Count * (1 - profile.zones[index].coverage)),
                    0, zoneScores.Count - 1);
                thresholds[index] = zoneScores[cutoff];
            }

            int[] planted = new int[scores.Length];
            int[] tufts = new int[scores.Length];
            var output = new List<CoastalGrassLayout.Cell>();
            foreach (IslandCellSites cell in cells)
            {
                var random = new System.Random(cell.Seed ^ 19853);
                var variants = meshes.Select(_ => new List<Matrix4x4>()).ToArray();
                var bounds = new Bounds();
                bool hasBounds = false;
                foreach (IslandSite site in cell.Sites)
                {
                    float acceptance = Next(random);
                    float choice = Next(random);
                    float sizeVariation = Next(random);
                    float yaw = Next(random) * 360;
                    float aboveEdge = site.Score - thresholds[site.ZoneIndex];
                    if (aboveEdge < 0)
                        continue;

                    Vector2 point = Xz(site.Position);
                    Vector4 style = BlendIslandStyle(profile, cell.Zones, site.ZoneIndex, point);
                    float density = Mathf.SmoothStep(style.x, style.y, aboveEdge / PatchEdgeScores);
                    float clusters = ClusterNoise(point);
                    float densityWeight = Mathf.SmoothStep(0.20f, 0.97f, Mathf.InverseLerp(0.30f, 0.70f, clusters));
                    planted[site.ZoneIndex]++;
                    if (acceptance >= density * densityWeight * site.SurfaceWeight * sampleArea)
                        continue;

                    tufts[site.ZoneIndex]++;
                    int variant = choice < 0.29f ? 0 : choice < 0.54f ? 1 : 2;
                    float scale = Mathf.Lerp(style.z, style.w, Mathf.Lerp(sizeVariation, clusters, 0.30f));
                    var matrix = Matrix4x4.TRS(site.Position, Quaternion.Euler(0, yaw, 0), Vector3.one * scale);
                    variants[variant].Add(matrix);
                    IncludeTuftBounds(ref bounds, ref hasBounds, meshes[variant], site.Position, scale);
                }

                if (hasBounds)
                    output.Add(PackCell(bounds, variants));
            }

            for (int index = 0; index < scores.Length; index++)
                Debug.Log($"Island grass {profile.zones[index].id}: {tufts[index]} tufts, " +
                    $"{planted[index] * sampleArea:F1} / {scores[index].Count * sampleArea:F1} eligible square meters.");

            return output.ToArray();
        }

        private static List<IslandCellSites> SampleIsland(Terrain terrain, IslandGrassProfile profile,
            PathTriangle[] paths, List<Vector2> accessPoints, List<float>[] scores)
        {
            TerrainData data = terrain.terrainData;
            Vector3 origin = terrain.GetPosition();
            int grassLayer = Array.FindIndex(data.terrainLayers, layer => layer && layer.name == "Grass");
            int sandLayer = Array.FindIndex(data.terrainLayers, layer => layer && layer.name == "BeachSand");
            if (grassLayer < 0 || sandLayer < 0)
                throw new InvalidOperationException("Island grass requires Grass and BeachSand terrain layers.");

            int width = data.alphamapWidth, height = data.alphamapHeight;
            float[,,] coverage = data.GetAlphamaps(0, 0, width, height);
            int firstX = Mathf.FloorToInt(origin.x / CellSize);
            int firstZ = Mathf.FloorToInt(origin.z / CellSize);
            int endX = Mathf.CeilToInt((origin.x + data.size.x) / CellSize);
            int endZ = Mathf.CeilToInt((origin.z + data.size.z) / CellSize);
            const float step = CellSize / IslandSamplesPerSide;

            var output = new List<IslandCellSites>();
            for (int cellZ = firstZ; cellZ < endZ; cellZ++)
            for (int cellX = firstX; cellX < endX; cellX++)
            {
                Vector2 minimum = new(cellX * CellSize, cellZ * CellSize);
                Vector2 maximum = minimum + Vector2.one * CellSize;
                Vector2 influence = Vector2.one * ZoneInfluence;
                int[] localZones = Enumerable.Range(0, profile.zones.Length).Where(index =>
                    profile.zones[index].shapes.Any(shape => shape.Overlaps(minimum - influence, maximum + influence))).ToArray();
                if (localZones.Length == 0)
                    continue;

                IslandPolygon[] exclusions = profile.exclusions.Where(shape => shape.Overlaps(minimum, maximum)).ToArray();
                PathTriangle[] localPaths = paths.Where(path => path.Overlaps(minimum, maximum)).ToArray();
                int seed = unchecked(profile.seed ^ cellX * 73856093 ^ cellZ * 19349663);
                var sites = new List<IslandSite>();
                for (int row = 0; row < IslandSamplesPerSide; row++)
                for (int column = 0; column < IslandSamplesPerSide; column++)
                {
                    int gridX = cellX * IslandSamplesPerSide + column;
                    int gridZ = cellZ * IslandSamplesPerSide + row;
                    Vector2 jitter = new(IslandJitter(gridX, gridZ, profile.seed), IslandJitter(gridX, gridZ, profile.seed ^ 2137));
                    Vector2 point = (new Vector2(gridX, gridZ) + jitter) * step;
                    float u = (point.x - origin.x) / data.size.x;
                    float v = (point.y - origin.z) / data.size.z;
                    if (u < 0 || u >= 1 || v < 0 || v >= 1)
                        continue;

                    int tx = Mathf.RoundToInt(u * (width - 1)), tz = Mathf.RoundToInt(v * (height - 1));
                    float grassWeight = coverage[tz, tx, grassLayer];
                    float sandWeight = coverage[tz, tx, sandLayer];
                    if (grassWeight < 0.60f || sandWeight >= 0.24f)
                        continue;

                    float maximumSlope = grassWeight >= 0.80f ? 52 : 40;
                    float slope = data.GetSteepness(u, v);
                    int zone = FindZone(profile, localZones, point);
                    if (zone < 0 || InAnyPolygon(exclusions, point) ||
                        OnPath(point, localPaths) || NearAccess(point, accessPoints) ||
                        slope > maximumSlope)
                        continue;

                    float ground = origin.y + data.GetInterpolatedHeight(u, v);
                    if (ground < 1.4f || !Physics.Raycast(new Vector3(point.x, origin.y + data.size.y + 10, point.y),
                            Vector3.down, out RaycastHit hit, data.size.y + 20, Physics.DefaultRaycastLayers,
                            QueryTriggerInteraction.Ignore) || hit.collider.gameObject != terrain.gameObject)
                        continue;

                    float score = profile.zones[zone].Score(point);
                    float edge = Mathf.Min(Mathf.InverseLerp(0.60f, 0.76f, grassWeight),
                        Mathf.Min(Mathf.InverseLerp(0, 9, maximumSlope - slope),
                            Mathf.InverseLerp(0, 0.12f, 0.24f - sandWeight)));
                    float surfaceWeight = Mathf.SmoothStep(0.62f, 1, edge);
                    scores[zone].Add(score);
                    sites.Add(new IslandSite(new Vector3(point.x, ground + 0.003f, point.y), zone, score, surfaceWeight));
                }

                if (sites.Count > 0)
                    output.Add(new IslandCellSites(seed, localZones, sites));
            }

            return output;
        }

        // Authoring boundaries blend in world space, independently of the render cells.
        private static Vector4 BlendIslandStyle(IslandGrassProfile profile, int[] zones, int owner, Vector2 point)
        {
            Vector2 warp = new(Mathf.PerlinNoise(point.x * 0.24f + 31, point.y * 0.24f + 79) - 0.5f,
                Mathf.PerlinNoise(point.x * 0.24f + 71, point.y * 0.24f + 13) - 0.5f);
            Vector2 sample = point + warp * ZoneWarp;
            float remaining = 1;
            Vector4 style = Vector4.zero;
            foreach (int index in zones)
            {
                IslandZone zone = profile.zones[index];
                float weight = 0;
                foreach (IslandPolygon shape in zone.shapes)
                    weight = Mathf.Max(weight, shape.ZoneWeight(sample));

                float share = weight * remaining;
                style += zone.Style * share;
                remaining -= share;
                if (remaining <= 0.0001f)
                    break;
            }

            return style + profile.zones[owner].Style * remaining;
        }

        private static float ClusterNoise(Vector2 point)
        {
            float group = Mathf.PerlinNoise(point.x * 0.23f + 95, point.y * 0.23f + 47);
            float detail = Mathf.PerlinNoise(point.x * 0.83f + 13, point.y * 0.83f + 82);
            return 0.82f * group + 0.18f * detail;
        }

        private static float IslandJitter(int x, int z, int seed)
        {
            unchecked
            {
                uint hash = (uint)x * 0x9e3779b1u ^ (uint)z * 0x85ebca77u ^ (uint)seed;
                hash ^= hash >> 16;
                hash *= 0x7feb352du;
                hash ^= hash >> 15;
                hash *= 0x846ca68bu;
                hash ^= hash >> 16;
                return (hash >> 8) * (1f / 16777216f);
            }
        }

        private static int FindZone(IslandGrassProfile profile, int[] localZones, Vector2 point)
        {
            foreach (int index in localZones)
                if (InAnyPolygon(profile.zones[index].shapes, point))
                    return index;
            return -1;
        }

        private static bool InAnyPolygon(IslandPolygon[] polygons, Vector2 point)
        {
            foreach (IslandPolygon polygon in polygons)
                if (polygon.Contains(point))
                    return true;
            return false;
        }
    }
}
