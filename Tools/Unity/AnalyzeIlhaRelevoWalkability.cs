// Execute the whole file with Unity MCP eval while Ilha_Relevo_004 is loaded.
// This is an analysis harness only; it does not modify or save the scene.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public sealed class IlhaRelevoAnalysisConfig
{
    public string expectedScenePath = "Assets/_Project/Scenes/Ilha_Relevo_004.unity";
    // Exact Transform paths relative to the scene root; all descendant MeshColliders are analyzed.
    public string[] terrainRootPaths;
    public float maxWalkableSlopeDegrees = 35f;
    public float gridSpacingMeters = 0.5f;
    public float rayStartAboveBoundsMeters = 3f;
    public float rayDistanceMeters = 100f;
    public float edgeSampleRadiusMeters = 0.29f;
    public int edgeSampleCount = 12;
    public float edgeHeightToleranceMeters = 0.25f;
    public float supportRayNormalMaxDegrees = 35f;
    public float capsuleRadiusMeters = 0.3f;
    public float capsuleHeightMeters = 1.8f;
    public float stepOffsetMeters = 0.25f;
    public float capsuleSkinClearanceMeters = 0.015f;
    public int maxGridCells = 300000;
    public float regionSearchRadiusMeters = 2f;
    public string[] regions;
    public string outputPath;
}

public static class IlhaRelevoWalkabilityAnalyzer
{
    private sealed class Cell
    {
        public int x, z;
        public Vector3 point, normal;
        public string status;
        public int component = -1;
        public string detail;
    }

    private sealed class TriangleIssue
    {
        public string colliderPath;
        public int triangleIndex;
        public float slopeDegrees;
        public float areaSquareMeters;
        public float[] centroid;
        public string reason;
    }

    private sealed class CellIssue
    {
        public int gridX, gridZ;
        public float[] position;
        public string reason;
        public string detail;
    }

    private static readonly Vector3[] AxialAndDiagonal = {
        new Vector3(1,0,0), new Vector3(-1,0,0), new Vector3(0,0,1), new Vector3(0,0,-1),
        new Vector3(1,0,1), new Vector3(1,0,-1), new Vector3(-1,0,1), new Vector3(-1,0,-1)
    };

    public static string Run(string configPath, string resultPath)
    {
        var scene = SceneManager.GetActiveScene();
        var started = DateTime.UtcNow;
        var cfg = JsonConvert.DeserializeObject<IlhaRelevoAnalysisConfig>(File.ReadAllText(configPath));
        if (cfg == null) throw new InvalidDataException("Configuração vazia ou inválida.");
        ValidateConfig(cfg);
        if (scene.path != cfg.expectedScenePath)
            throw new InvalidOperationException($"Cena ativa '{scene.path}', esperada '{cfg.expectedScenePath}'.");

        var allRoots = scene.GetRootGameObjects();
        var targetRoots = new List<Transform>();
        var unresolvedRoots = new List<string>();
        foreach (string path in cfg.terrainRootPaths)
        {
            Transform found = null;
            foreach (var root in allRoots)
            {
                found = FindByPath(root.transform, path);
                if (found != null) break;
            }
            if (found == null) unresolvedRoots.Add(path); else targetRoots.Add(found);
        }
        if (unresolvedRoots.Count > 0)
            throw new InvalidOperationException("terrainRootPaths não resolvidos: " + string.Join(", ", unresolvedRoots));

        var targetColliders = new HashSet<MeshCollider>();
        foreach (var root in targetRoots)
            foreach (var mc in root.GetComponentsInChildren<MeshCollider>(false))
                if (mc.enabled && mc.gameObject.activeInHierarchy) targetColliders.Add(mc);
        if (targetColliders.Count == 0)
            throw new InvalidOperationException("Nenhum MeshCollider habilitado encontrado nos terrainRootPaths.");

        Physics.SyncTransforms();
        var triangleIssues = new List<TriangleIssue>();
        var colliderReports = new List<object>();
        var bounds = new Bounds();
        bool hasBounds = false;
        int triangleCount = 0, degenerateCount = 0, invalidTransformCount = 0;
        float minSlope = float.PositiveInfinity, maxSlope = 0f;
        int slopeLe30 = 0, slope30To35 = 0, slope35To60 = 0, slopeOver60 = 0;

        foreach (var collider in targetColliders.OrderBy(c => GetPath(c.transform)))
        {
            var mesh = collider.sharedMesh;
            if (mesh == null) throw new InvalidOperationException("MeshCollider sem sharedMesh: " + GetPath(collider.transform));
            if (collider.convex)
                throw new InvalidOperationException("MeshCollider convexo não expõe aqui os triângulos cozidos usados pela colisão; exclua-o do conjunto de terreno e analise-o como obstáculo: " + GetPath(collider.transform));
            Matrix4x4 matrix = collider.transform.localToWorldMatrix;
            bool transformValid = MatrixFinite(matrix) && Mathf.Abs(matrix.determinant) > 1e-8f &&
                                  Finite(collider.transform.lossyScale.x) && Finite(collider.transform.lossyScale.y) && Finite(collider.transform.lossyScale.z);
            if (!transformValid) invalidTransformCount++;
            if (!transformValid) throw new InvalidOperationException("Transformação não finita ou singular: " + GetPath(collider.transform));

            var vertices = mesh.vertices;
            var indices = mesh.triangles;
            var world = new Vector3[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                world[i] = matrix.MultiplyPoint3x4(vertices[i]);
                if (!Finite(world[i].x) || !Finite(world[i].y) || !Finite(world[i].z))
                    throw new InvalidOperationException($"Vértice não finito em {GetPath(collider.transform)} (índice {i}).");
                if (!hasBounds) { bounds = new Bounds(world[i], Vector3.zero); hasBounds = true; }
                else bounds.Encapsulate(world[i]);
            }

            int localDegenerate = 0;
            for (int i = 0; i + 2 < indices.Length; i += 3)
            {
                triangleCount++;
                int tri = i / 3;
                Vector3 a = world[indices[i]], b = world[indices[i + 1]], c = world[indices[i + 2]];
                Vector3 cross = Vector3.Cross(b - a, c - a);
                float area = cross.magnitude * 0.5f;
                if (!Finite(area) || area <= 1e-8f)
                {
                    degenerateCount++; localDegenerate++;
                    triangleIssues.Add(new TriangleIssue { colliderPath = GetPath(collider.transform), triangleIndex = tri,
                        slopeDegrees = 90f, areaSquareMeters = area, centroid = Vector((a + b + c) / 3f), reason = "degenerate-or-nonfinite" });
                    continue;
                }
                Vector3 normal = cross.normalized;
                // Plane slope is independent of triangle winding; keep winding separately in the report.
                float slope = Mathf.Acos(Mathf.Clamp(Mathf.Abs(Vector3.Dot(normal, Vector3.up)), 0f, 1f)) * Mathf.Rad2Deg;
                minSlope = Mathf.Min(minSlope, slope); maxSlope = Mathf.Max(maxSlope, slope);
                if (slope <= 30f) slopeLe30++;
                else if (slope <= cfg.maxWalkableSlopeDegrees) slope30To35++;
                else if (slope <= 60f) slope35To60++;
                else slopeOver60++;
                if (slope > cfg.maxWalkableSlopeDegrees)
                    triangleIssues.Add(new TriangleIssue { colliderPath = GetPath(collider.transform), triangleIndex = tri,
                        slopeDegrees = slope, areaSquareMeters = area, centroid = Vector((a + b + c) / 3f), reason = "slope-over-limit" });
            }
            colliderReports.Add(new { path = GetPath(collider.transform), layer = LayerMask.LayerToName(collider.gameObject.layer),
                mesh = mesh.name, convex = collider.convex, vertexCount = vertices.Length, triangleCount = indices.Length / 3,
                degenerateTriangles = localDegenerate, lossyScale = Vector(collider.transform.lossyScale), determinant = matrix.determinant });
        }
        if (!hasBounds) throw new InvalidOperationException("Os colliders selecionados não têm vértices.");

        var grid = BuildGrid(bounds, cfg);
        var cellIssues = new List<CellIssue>();
        int supported = 0, invalidEdge = 0, blockedCapsule = 0, unsupported = 0, steepSupport = 0;
        for (int z = 0; z < grid.GetLength(1); z++)
        for (int x = 0; x < grid.GetLength(0); x++)
        {
            var cell = grid[x, z];
            Vector3 origin = new Vector3(cell.point.x, bounds.max.y + cfg.rayStartAboveBoundsMeters, cell.point.z);
            if (!TryTerrainRay(origin, cfg.rayDistanceMeters, targetColliders, out var hit))
            {
                cell.status = "no-terrain-support"; unsupported++;
                AddCellIssue(cellIssues, cell, "no-terrain-support", "Nenhum collider-alvo atingido pelo raio vertical."); continue;
            }
            cell.point = hit.point; cell.normal = hit.normal.normalized;
            if (Vector3.Angle(cell.normal, Vector3.up) > cfg.supportRayNormalMaxDegrees)
            {
                cell.status = "support-too-steep"; steepSupport++;
                AddCellIssue(cellIssues, cell, "support-too-steep", "Inclinação local acima do limite de suporte da grade."); continue;
            }
            if (!HasEdgeSupport(cell, cfg, bounds, targetColliders, out string edgeDetail))
            {
                cell.status = "edge-or-step-discontinuity"; invalidEdge++;
                AddCellIssue(cellIssues, cell, "edge-or-step-discontinuity", edgeDetail); continue;
            }
            if (IsCapsuleBlocked(cell.point, cfg, targetColliders, out string blockerDetail))
            {
                cell.status = "capsule-obstructed"; blockedCapsule++;
                AddCellIssue(cellIssues, cell, "capsule-obstructed", blockerDetail); continue;
            }
            cell.status = "valid"; supported++;
        }

        int componentCount = LabelComponents(grid, cfg, targetColliders);
        var componentReports = SummarizeComponents(grid, cfg.gridSpacingMeters);
        var regionReports = new List<object>();
        if (cfg.regions != null)
        foreach (var region in cfg.regions)
        {
            var p = ParseRegion(region);
            var cell = NearestCell(grid, p[0], p[2], cfg.gridSpacingMeters);
            var nearestValid = NearestValidCell(grid, p[0], p[2], cfg.gridSpacingMeters, cfg.regionSearchRadiusMeters);
            regionReports.Add(new { id = region.Split(',')[0].Trim(), requested = p,
                nearestGridStatus = cell == null ? "outside-grid" : cell.status,
                nearestGridHorizontalDistanceMeters = cell == null ? (float?)null : HorizontalDistance(cell.point, p[0], p[2]),
                supportHeightDeltaFromRequestedMeters = cell == null ? (float?)null : cell.point.y - p[1],
                nearestGridPosition = cell == null ? null : Vector(cell.point), component = cell == null ? -1 : cell.component,
                nearestValidPositionWithinSearchRadius = nearestValid == null ? null : Vector(nearestValid.point),
                distanceToNearestValidMeters = nearestValid == null ? (float?)null : HorizontalDistance(nearestValid.point, p[0], p[2]),
                nearestValidComponent = nearestValid == null ? -1 : nearestValid.component });
        }

        var report = new
        {
            utcStarted = started.ToString("o"), scene = scene.path, analysis = "mesh-triangle-full-scan-plus-finite-grid-approximation",
            parameters = new { cfg.maxWalkableSlopeDegrees, cfg.gridSpacingMeters, cfg.edgeSampleRadiusMeters, cfg.edgeSampleCount,
                cfg.edgeHeightToleranceMeters, cfg.capsuleRadiusMeters, cfg.capsuleHeightMeters, cfg.stepOffsetMeters,
                cfg.capsuleSkinClearanceMeters, cfg.maxGridCells, supportRayNormalMaxDegrees = cfg.supportRayNormalMaxDegrees,
                regionSearchRadiusMeters = cfg.regionSearchRadiusMeters,
                capsuleTest = "Physics.OverlapCapsule with selected terrain MeshColliders excluded to prevent self-ground false positives; not CharacterController.Move" },
            terrainRoots = cfg.terrainRootPaths, colliders = colliderReports,
            bounds = new { min = Vector(bounds.min), max = Vector(bounds.max), size = Vector(bounds.size) },
            meshScan = new { triangleCount, degenerateCount, invalidTransformCount, minSlopeDegrees = triangleCount == degenerateCount ? (float?)null : minSlope,
                maxSlopeDegrees = triangleCount == degenerateCount ? (float?)null : maxSlope, slopeLe30, slope30To35, slope35To60, slopeOver60,
                violationCount = triangleIssues.Count, violations = triangleIssues },
            grid = new { width = grid.GetLength(0), depth = grid.GetLength(1), totalCells = grid.Length, supported,
                unsupported, steepSupport, invalidEdgeOrStep = invalidEdge, blockedCapsule, componentCount,
                estimatedWorstCaseQueries = new { raycasts = (long)grid.Length * (1 + cfg.edgeSampleCount), capsuleChecks = grid.Length,
                    neighborCapsuleCasts = (long)grid.Length * 8 },
                disconnectedComponents = componentReports, regions = regionReports, issues = cellIssues,
                limitation = "Finite configurable grid and perimeter samples approximate accessibility; features narrower than grid spacing may be missed. This scans assigned MeshCollider.sharedMesh triangles, not Unity's internal cooked representation. This is not a mathematical proof or a Play Mode walk test." }
        };
        string json = JsonConvert.SerializeObject(report, Formatting.Indented);
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(resultPath)));
        File.WriteAllText(Path.GetFullPath(resultPath), json);
        Debug.Log($"[IlhaRelevoAnalysis] {triangleCount} triângulos; {triangleIssues.Count} violações de inclinação/degeneração; {supported}/{grid.Length} células válidas; {componentCount} componentes. JSON: {Path.GetFullPath(resultPath)}");
        return Path.GetFullPath(resultPath);
    }

    private static void ValidateConfig(IlhaRelevoAnalysisConfig c)
    {
        if (c.terrainRootPaths == null || c.terrainRootPaths.Length == 0) throw new InvalidDataException("Informe terrainRootPaths explicitamente.");
        if (c.gridSpacingMeters <= 0 || c.edgeSampleCount < 4 || c.edgeSampleRadiusMeters <= 0 || c.edgeHeightToleranceMeters < 0)
            throw new InvalidDataException("Parâmetros de grade/amostragem inválidos.");
        if (c.maxGridCells <= 0) throw new InvalidDataException("maxGridCells deve ser positivo.");
        if (c.capsuleRadiusMeters <= 0 || c.capsuleHeightMeters < 2 * c.capsuleRadiusMeters || c.stepOffsetMeters < 0)
            throw new InvalidDataException("Dimensões do personagem inválidas.");
    }

    private static Transform FindByPath(Transform root, string path)
    {
        if (GetPath(root) == path) return root;
        foreach (var t in root.GetComponentsInChildren<Transform>(true)) if (GetPath(t) == path) return t;
        return null;
    }

    private static string GetPath(Transform t)
    {
        var parts = new Stack<string>();
        while (t != null) { parts.Push(t.name); t = t.parent; }
        return string.Join("/", parts.ToArray());
    }

    private static bool MatrixFinite(Matrix4x4 m)
    {
        for (int r = 0; r < 4; r++) for (int c = 0; c < 4; c++) if (!Finite(m[r, c])) return false;
        return true;
    }
    private static bool Finite(float f) => !float.IsNaN(f) && !float.IsInfinity(f);
    private static float[] Vector(Vector3 v) => new[] { v.x, v.y, v.z };

    private static Cell[,] BuildGrid(Bounds b, IlhaRelevoAnalysisConfig cfg)
    {
        int nx = Mathf.CeilToInt(b.size.x / cfg.gridSpacingMeters) + 1;
        int nz = Mathf.CeilToInt(b.size.z / cfg.gridSpacingMeters) + 1;
        if ((long)nx * nz > cfg.maxGridCells) throw new InvalidOperationException($"Grade de {nx} x {nz} células excede maxGridCells={cfg.maxGridCells}; aumente gridSpacingMeters ou ajuste explicitamente o limite.");
        var grid = new Cell[nx, nz];
        for (int z = 0; z < nz; z++) for (int x = 0; x < nx; x++)
            grid[x, z] = new Cell { x = x, z = z, point = new Vector3(b.min.x + x * cfg.gridSpacingMeters, 0, b.min.z + z * cfg.gridSpacingMeters), status = "pending" };
        return grid;
    }

    private static bool TryTerrainRay(Vector3 origin, float distance, HashSet<MeshCollider> targets, out RaycastHit best)
    {
        best = default;
        var hits = Physics.RaycastAll(origin, Vector3.down, distance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        float closest = float.PositiveInfinity;
        bool found = false;
        foreach (var hit in hits)
        {
            var mc = hit.collider as MeshCollider;
            if (mc != null && targets.Contains(mc) && hit.distance < closest) { closest = hit.distance; best = hit; found = true; }
        }
        return found;
    }

    private static bool HasEdgeSupport(Cell cell, IlhaRelevoAnalysisConfig cfg, Bounds bounds, HashSet<MeshCollider> targets, out string detail)
    {
        var failures = new List<string>();
        Vector3 n = cell.normal;
        for (int i = 0; i < cfg.edgeSampleCount; i++)
        {
            float a = Mathf.PI * 2f * i / cfg.edgeSampleCount;
            float dx = Mathf.Cos(a) * cfg.edgeSampleRadiusMeters, dz = Mathf.Sin(a) * cfg.edgeSampleRadiusMeters;
            Vector3 sample = cell.point + new Vector3(dx, 0, dz);
            Vector3 origin = new Vector3(sample.x, bounds.max.y + cfg.rayStartAboveBoundsMeters, sample.z);
            if (!TryTerrainRay(origin, cfg.rayDistanceMeters, targets, out var hit)) { failures.Add("missing-support"); continue; }
            if (Vector3.Angle(hit.normal, Vector3.up) > cfg.supportRayNormalMaxDegrees) { failures.Add("steep-support"); continue; }
            float predictedY = cell.point.y - (n.x * dx + n.z * dz) / Mathf.Max(0.001f, n.y);
            float residual = Mathf.Abs(hit.point.y - predictedY);
            if (residual > cfg.edgeHeightToleranceMeters) failures.Add("step-residual=" + residual.ToString("F3"));
        }
        detail = failures.Count == 0 ? "ok" : string.Join(";", failures);
        return failures.Count == 0;
    }

    private static bool IsCapsuleBlocked(Vector3 floor, IlhaRelevoAnalysisConfig cfg, HashSet<MeshCollider> terrain, out string detail)
    {
        float radius = Mathf.Max(0.01f, cfg.capsuleRadiusMeters - cfg.capsuleSkinClearanceMeters);
        Vector3 bottom = floor + Vector3.up * (radius + cfg.capsuleSkinClearanceMeters);
        Vector3 top = floor + Vector3.up * (cfg.capsuleHeightMeters - radius);
        // Do not call support terrain a head/side obstruction. Terrain clearance is handled
        // by the support ring, slope scan and step graph; this overlap check targets blockers.
        var blockers = new SortedSet<string>();
        foreach (var hit in Physics.OverlapCapsule(bottom, top, radius, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            // The player's own CharacterController may be moving while analysis runs in Play Mode.
            if (hit is CharacterController) continue;
            var meshCollider = hit as MeshCollider;
            if (meshCollider == null || !terrain.Contains(meshCollider)) blockers.Add(GetPath(hit.transform) + " [" + hit.GetType().Name + "]");
        }
        detail = blockers.Count == 0 ? "ok" : string.Join("; ", blockers.Take(4).ToArray());
        return blockers.Count > 0;
    }

    private static int LabelComponents(Cell[,] grid, IlhaRelevoAnalysisConfig cfg, HashSet<MeshCollider> targets)
    {
        int id = 0;
        var q = new Queue<Cell>();
        for (int z = 0; z < grid.GetLength(1); z++) for (int x = 0; x < grid.GetLength(0); x++)
        {
            var start = grid[x, z];
            if (start.status != "valid" || start.component >= 0) continue;
            start.component = id; q.Enqueue(start);
            while (q.Count > 0)
            {
                var a = q.Dequeue();
                foreach (var d in AxialAndDiagonal)
                {
                    int nx = a.x + (int)d.x, nz = a.z + (int)d.z;
                    if (nx < 0 || nz < 0 || nx >= grid.GetLength(0) || nz >= grid.GetLength(1)) continue;
                    var b = grid[nx, nz];
                    if (b.status != "valid" || b.component >= 0 || !StepConnected(a, b, cfg, targets)) continue;
                    b.component = id; q.Enqueue(b);
                }
            }
            id++;
        }
        return id;
    }

    private static bool StepConnected(Cell a, Cell b, IlhaRelevoAnalysisConfig cfg, HashSet<MeshCollider> targets)
    {
        Vector3 horizontal = b.point - a.point; horizontal.y = 0;
        Vector3 avg = (a.normal + b.normal).normalized;
        float expectedDy = -Vector3.Dot(new Vector3(avg.x, 0, avg.z), horizontal) / Mathf.Max(0.001f, avg.y);
        float residual = Mathf.Abs((b.point.y - a.point.y) - expectedDy);
        if (residual > cfg.stepOffsetMeters) return false;
        float radius = Mathf.Max(0.01f, cfg.capsuleRadiusMeters - cfg.capsuleSkinClearanceMeters);
        Vector3 p0 = a.point + Vector3.up * (radius + cfg.capsuleSkinClearanceMeters);
        Vector3 p1 = b.point + Vector3.up * (radius + cfg.capsuleSkinClearanceMeters);
        Vector3 topOffset = Vector3.up * (cfg.capsuleHeightMeters - 2f * radius - cfg.capsuleSkinClearanceMeters);
        // Ignore the selected terrain mesh here: its support surface can overlap the swept
        // vertical capsule on slopes. Terrain traversability is approximated by support,
        // ring samples and residual height; other colliders remain blocking.
        var hits = Physics.CapsuleCastAll(p0, p0 + topOffset, radius, (p1 - p0).normalized,
            (p1 - p0).magnitude, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        foreach (var hit in hits)
        {
            if (hit.collider is CharacterController) continue;
            var meshCollider = hit.collider as MeshCollider;
            if (meshCollider == null || !targets.Contains(meshCollider)) return false;
        }
        return true;
    }

    private static List<object> SummarizeComponents(Cell[,] grid, float spacing)
    {
        var groups = new Dictionary<int, List<Cell>>();
        foreach (var c in grid) if (c.component >= 0)
        {
            if (!groups.ContainsKey(c.component)) groups[c.component] = new List<Cell>();
            groups[c.component].Add(c);
        }
        return groups.OrderByDescending(kv => kv.Value.Count).Select(kv =>
        {
            var cells = kv.Value;
            return (object)new { id = kv.Key, cellCount = cells.Count, approximateAreaSquareMeters = cells.Count * spacing * spacing,
                bounds = new { min = new[] { cells.Min(c => c.point.x), cells.Min(c => c.point.z) },
                    max = new[] { cells.Max(c => c.point.x), cells.Max(c => c.point.z) } }, representative = Vector(cells[cells.Count / 2].point),
                cells = cells.Count <= 100 ? cells.Select(c => Vector(c.point)).ToArray() : null };
        }).ToList();
    }

    private static Cell NearestCell(Cell[,] grid, float x, float z, float spacing)
    {
        int ix = Mathf.RoundToInt((x - grid[0, 0].point.x) / spacing), iz = Mathf.RoundToInt((z - grid[0, 0].point.z) / spacing);
        return ix < 0 || iz < 0 || ix >= grid.GetLength(0) || iz >= grid.GetLength(1) ? null : grid[ix, iz];
    }

    private static Cell NearestValidCell(Cell[,] grid, float x, float z, float spacing, float searchRadius)
    {
        int cx = Mathf.RoundToInt((x - grid[0, 0].point.x) / spacing);
        int cz = Mathf.RoundToInt((z - grid[0, 0].point.z) / spacing);
        int r = Mathf.CeilToInt(searchRadius / spacing);
        Cell best = null; float bestDistance = searchRadius;
        for (int zIndex = Mathf.Max(0, cz - r); zIndex <= Mathf.Min(grid.GetLength(1) - 1, cz + r); zIndex++)
        for (int xIndex = Mathf.Max(0, cx - r); xIndex <= Mathf.Min(grid.GetLength(0) - 1, cx + r); xIndex++)
        {
            var c = grid[xIndex, zIndex];
            if (c.status != "valid") continue;
            float distance = HorizontalDistance(c.point, x, z);
            if (distance <= bestDistance) { best = c; bestDistance = distance; }
        }
        return best;
    }

    private static float HorizontalDistance(Vector3 p, float x, float z)
    {
        float dx = p.x - x, dz = p.z - z;
        return Mathf.Sqrt(dx * dx + dz * dz);
    }

    private static void AddCellIssue(List<CellIssue> issues, Cell c, string reason, string detail) =>
        issues.Add(new CellIssue { gridX = c.x, gridZ = c.z, position = Vector(c.point), reason = reason, detail = detail });

    private static float[] ParseRegion(string s)
    {
        var parts = s.Split(',');
        if (parts.Length != 4 || string.IsNullOrWhiteSpace(parts[0]))
            throw new InvalidDataException("regions deve usar 'id,x,y,z'. Valor: " + s);
        return new[] {
            float.Parse(parts[1].Trim(), System.Globalization.CultureInfo.InvariantCulture),
            float.Parse(parts[2].Trim(), System.Globalization.CultureInfo.InvariantCulture),
            float.Parse(parts[3].Trim(), System.Globalization.CultureInfo.InvariantCulture)
        };
    }
}

// Example execution; create/edit the config only after confirming exact scene hierarchy paths.
var repoRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "..", ".."));
var configPath = Path.Combine(repoRoot, "specs", "004-relevo-ilha", "evidence", "walkability-config.json");
var outputPath = Path.Combine(repoRoot, "specs", "004-relevo-ilha", "evidence", "walkability-analysis-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + ".json");
var analysisResultPath = IlhaRelevoWalkabilityAnalyzer.Run(configPath, outputPath);
return analysisResultPath;
