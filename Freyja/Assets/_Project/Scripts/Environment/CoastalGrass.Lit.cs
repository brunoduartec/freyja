using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace Freyja.Environment
{
    public sealed partial class CoastalGrass
    {
        public enum Quality { Low, Medium, High }

        [Serializable]
        private struct Profile
        {
            public Color root;
            public Color tip;
            [Range(0, 0.8f)] public float normalUp;
            [Range(0.5f, 1.5f)] public float width;
            [Range(0.5f, 1.5f)] public float height;
            [Range(0, 0.12f)] public float bend;
            [Range(0, 1)] public float translucency;

            public Profile(Color root, Color tip, float width, float height, float bend, float translucency, float normalUp)
            {
                this.root = root;
                this.tip = tip;
                this.width = width;
                this.height = height;
                this.bend = bend;
                this.translucency = translucency;
                this.normalUp = normalUp;
            }
        }

        private sealed class Region
        {
            public Bounds Bounds;
            public CoastalGrassLayout.Cell Source;
            public bool AuthoredCoastal;
            public GrassDensityLayout.Tuft[] ExtraTufts;
            public CoastalGrassLayout.ManualEdit[] ManualTufts;
            public Matrix4x4[][] Batches;
        }

        private const int Profiles = 3;
        private const int MaxLights = 4;
        private const int DetailVariants = 3;
        private const int Stages = DetailVariants + 2;

        private static readonly int ColorRootId = Shader.PropertyToID("_RootColor");
        private static readonly int ColorTipId = Shader.PropertyToID("_TipColor");
        private static readonly int GeometryId = Shader.PropertyToID("_GrassGeometry");
        private static readonly int LodId = Shader.PropertyToID("_GrassLod");
        private static readonly int NormalUpId = Shader.PropertyToID("_NormalUp");
        private static readonly int BendId = Shader.PropertyToID("_WindBend");
        private static readonly int TranslucencyId = Shader.PropertyToID("_Translucency");
        private static readonly int LightCountId = Shader.PropertyToID("_FreyjaGrassLightCount");
        private static readonly int LightPositionsId = Shader.PropertyToID("_FreyjaGrassLightPositionRange");
        private static readonly int AmbientSkyId = Shader.PropertyToID("_FreyjaGrassAmbientSky");
        private static readonly int AmbientEquatorId = Shader.PropertyToID("_FreyjaGrassAmbientEquator");
        private static readonly int AmbientGroundId = Shader.PropertyToID("_FreyjaGrassAmbientGround");
        private static readonly int LightColorsId = Shader.PropertyToID("_FreyjaGrassLightColor");

        [Header("Lit grass")]
        [SerializeField] private Material litMaterial;
        [SerializeField] private CoastalGrassLayout authoredLayout;
        [SerializeField] private GrassSurfaceMask surfaceMask;
        [SerializeField] private GrassDensityLayout densityLayout;
        [SerializeField] private Quality quality = Quality.High;
        [SerializeField] private int variationSeed = 78;
        [SerializeField] private bool castGrassShadows;
        [SerializeField] private bool showRegions;
        [SerializeField] private Light[] localLights = Array.Empty<Light>();
        [SerializeField] private Profile shortGrass = new(
            new Color(0.28f, 0.33f, 0.17f), new Color(0.46f, 0.52f, 0.27f), 0.95f, 0.76f, 0.07f, 0.23f, 0.42f);
        [SerializeField] private Profile wildGrass = new(
            new Color(0.23f, 0.31f, 0.14f), new Color(0.44f, 0.50f, 0.22f), 1.13f, 1.27f, 0.11f, 0.31f, 0.38f);
        [SerializeField] private Profile coastalGrass = new(
            new Color(0.32f, 0.34f, 0.16f), new Color(0.55f, 0.53f, 0.30f), 0.85f, 1.04f, 0.10f, 0.30f, 0.34f);
        [SerializeField, Range(0.5f, 2f)] private float lodRangeScale = 1f;
        [SerializeField, Min(0)] private float coastalElevation = 3.7f;
        [SerializeField, Min(0)] private float wildElevation = 7.5f;

        private Region[] regions;
        private HashSet<Vector2Int> blockedRoots;
        private bool hasDensity;
        private Submission[] litSubmissions;
        private bool placementDirty;
        private MaterialPropertyBlock unlitBlock;
        private MaterialPropertyBlock localBlock;
        private readonly List<GrassExclusionVolume> exclusions = new();
        private readonly Vector4[] lightPositions = new Vector4[MaxLights];
        private readonly Vector4[] lightColors = new Vector4[MaxLights];
        private readonly float[] lightDistances = new float[MaxLights];
        private readonly HashSet<CoastalGrassLayout.RootKey> suppressedRoots = new();

        public int SubmittedTriangles { get; private set; }
        public int RebuiltRegions { get; private set; }
        public int BakedInstances { get; private set; }
        public long CachedMatrixBytes { get; private set; }
        public RenderMode Mode { get => mode; set => mode = value; }
        public bool CastGrassShadows { get => castGrassShadows; set => castGrassShadows = value; }
        public Quality CurrentQuality
        {
            get => quality;
            set
            {
                if (quality == value)
                    return;
                quality = value;
                RebuildPlacement();
            }
        }

        private int[] triangles;

        private void InitializeLitRendering()
        {
            if (!litMaterial || !layout || tuftMeshes?.Length != DetailVariants)
                return;

            blockedRoots = surfaceMask ? surfaceMask.BuildLookup() : null;
            litSubmissions = new Submission[Profiles * Stages];
            for (int i = 0; i < litSubmissions.Length; i++)
                litSubmissions[i] = new Submission();
            unlitBlock = new MaterialPropertyBlock();
            localBlock = new MaterialPropertyBlock();
            triangles = new int[DetailVariants];
            for (int i = 0; i < triangles.Length; i++)
                triangles[i] = checked((int)tuftMeshes[i].GetIndexCount(0) / 3);

            CoastalGrassLayout.Cell[] extraCells = authoredLayout ? authoredLayout.Cells : Array.Empty<CoastalGrassLayout.Cell>();
            hasDensity = densityLayout && densityLayout.Matches(layout);
            var manualByCell = new Dictionary<Vector2Int, List<CoastalGrassLayout.ManualEdit>>();
            suppressedRoots.Clear();
            var sourceEdits = new Dictionary<CoastalGrassLayout.RootKey, CoastalGrassLayout.ManualEdit>();
            foreach (CoastalGrassLayout.ManualEdit edit in layout.ManualEdits)
                if (edit.Kind == CoastalGrassLayout.ManualKind.Add)
                    AddManualToCell(manualByCell, edit);
                else sourceEdits[edit.Source] = edit;
            foreach (var pair in sourceEdits)
            {
                suppressedRoots.Add(pair.Key);
                if (pair.Value.Kind == CoastalGrassLayout.ManualKind.Replace)
                    AddManualToCell(manualByCell, pair.Value);
            }

            regions = new Region[layout.Cells.Length + extraCells.Length + manualByCell.Count];
            for (int i = 0; i < regions.Length; i++)
            {
                bool authored = i >= layout.Cells.Length;
                CoastalGrassLayout.Cell cell = authored ? extraCells[i - layout.Cells.Length] : layout.Cells[i];
                Bounds bounds = cell.Bounds;
                GrassDensityLayout.Tuft[] extraTufts = hasDensity && !authored
                    ? densityLayout.GetTufts(i) : Array.Empty<GrassDensityLayout.Tuft>();
                foreach (GrassDensityLayout.Tuft tuft in extraTufts)
                    bounds.Encapsulate(tuft.Root);
                bounds.Expand(1.6f);
                regions[i] = new Region
                {
                    Bounds = bounds,
                    Source = cell,
                    AuthoredCoastal = authored,
                    ExtraTufts = extraTufts
                };
            }
            int regionIndex = layout.Cells.Length + extraCells.Length;
            foreach (var pair in manualByCell)
            {
                var bounds = new Bounds(pair.Value[0].Root, Vector3.one * 0.1f);
                foreach (CoastalGrassLayout.ManualEdit edit in pair.Value)
                    bounds.Encapsulate(edit.Root);
                bounds.Expand(1.6f);
                regions[regionIndex++] = new Region { Bounds = bounds, ExtraTufts = Array.Empty<GrassDensityLayout.Tuft>(), ManualTufts = pair.Value.ToArray() };
            }
            RebuildPlacement();
        }

        private static void AddManualToCell(Dictionary<Vector2Int, List<CoastalGrassLayout.ManualEdit>> cells,
            CoastalGrassLayout.ManualEdit edit)
        {
            var key = new Vector2Int(Mathf.FloorToInt(edit.Root.x / 8f), Mathf.FloorToInt(edit.Root.z / 8f));
            if (!cells.TryGetValue(key, out List<CoastalGrassLayout.ManualEdit> list))
                cells.Add(key, list = new List<CoastalGrassLayout.ManualEdit>());
            list.Add(edit);
        }

        private void OnValidate() => placementDirty = true;

        [ContextMenu("Rebuild grass placement")]
        public void RebuildPlacement()
        {
            placementDirty = false;
            if (regions == null)
                return;

            RebuiltRegions = 0;
            for (int i = 0; i < regions.Length; i++)
                RebuildRegion(i);
        }

        private void RebuildRegion(int index)
        {
            Region region = regions[index];
            var batches = new List<Matrix4x4>[Profiles * Stages];
            float coverage = quality switch { Quality.Low => 0.4f, Quality.Medium => 0.7f, _ => 0.98f };
            if (region.Source != null)
            foreach (CoastalGrassLayout.Batch source in region.Source.Batches)
            {
                foreach (Matrix4x4 matrix in source.Instances)
                {
                    Vector3 root = matrix.GetColumn(3);
                    var rootKey = new CoastalGrassLayout.RootKey(region.AuthoredCoastal
                        ? CoastalGrassLayout.ManualSource.Authored : CoastalGrassLayout.ManualSource.Layout, source.MeshIndex, root);
                    if (suppressedRoots.Contains(rootKey))
                        continue;
                    if (!region.AuthoredCoastal && blockedRoots != null && blockedRoots.Contains(GrassSurfaceMask.ToKey(root)))
                        continue;
                    float score = PlacementScore(root, variationSeed);
                    int profile = region.AuthoredCoastal ? 2 : SelectProfile(root.y, score);
                    float density = region.AuthoredCoastal ? 1 : coverage * (profile == 2 ? 0.81f : 1f);
                    if (hasDensity && !region.AuthoredCoastal)
                        density *= Mathf.Lerp(0.78f, 1f, densityLayout.SampleCoverage(root));
                    if (score >= density || IsExcluded(root))
                        continue;

                    int section = profile * Stages;
                    (batches[section + source.MeshIndex] ??= new()).Add(matrix);
                    if (score < coverage * 0.66f)
                        (batches[section + DetailVariants] ??= new()).Add(matrix);
                    if (score < coverage * 0.29f)
                        (batches[section + DetailVariants + 1] ??= new()).Add(matrix);
                }
            }

            float extraCoverage = quality switch { Quality.Low => 0, Quality.Medium => 0.25f, _ => 0.96f };
            if (extraCoverage > 0)
            {
                foreach (GrassDensityLayout.Tuft tuft in region.ExtraTufts)
                {
                    var rootKey = new CoastalGrassLayout.RootKey(CoastalGrassLayout.ManualSource.Density, tuft.MeshIndex, tuft.Root);
                    if (suppressedRoots.Contains(rootKey))
                        continue;
                    float score = PlacementScore(tuft.Root, variationSeed);
                    if (score >= extraCoverage || IsExcluded(tuft.Root))
                        continue;

                    int profile = SelectProfile(tuft.Root.y, score);
                    int section = profile * Stages;
                    Matrix4x4 matrix = tuft.Matrix;
                    (batches[section + tuft.MeshIndex] ??= new()).Add(matrix);
                    if (score < extraCoverage * 0.30f)
                        (batches[section + DetailVariants] ??= new()).Add(matrix);
                }
            }

            if (region.ManualTufts != null)
            foreach (CoastalGrassLayout.ManualEdit tuft in region.ManualTufts)
            {
                if (tuft.MeshIndex < 0 || tuft.MeshIndex >= DetailVariants)
                    continue;
                if (IsExcluded(tuft.Root))
                    continue;
                int profile = SelectProfile(tuft.Root.y, PlacementScore(tuft.Root, variationSeed));
                int section = profile * Stages;
                (batches[section + tuft.MeshIndex] ??= new()).Add(tuft.Matrix);
                (batches[section + DetailVariants] ??= new()).Add(tuft.Matrix);
                (batches[section + DetailVariants + 1] ??= new()).Add(tuft.Matrix);
            }

            region.Batches ??= new Matrix4x4[batches.Length][];
            for (int i = 0; i < batches.Length; i++)
            {
                if (i % Stages < DetailVariants)
                    BakedInstances += (batches[i]?.Count ?? 0) - (region.Batches[i]?.Length ?? 0);
                CachedMatrixBytes += ((batches[i]?.Count ?? 0) - (region.Batches[i]?.Length ?? 0)) * 64L;
                region.Batches[i] = batches[i]?.ToArray() ?? Array.Empty<Matrix4x4>();
            }
            RebuiltRegions++;
        }

        private int SelectProfile(float altitude, float variation)
        {
            float offset = (variation - 0.5f) * 1.6f;
            if (altitude + offset < coastalElevation)
                return 2;
            return altitude + offset >= wildElevation ? 1 : 0;
        }

        public static float PlacementScore(Vector3 point, int seed)
        {
            unchecked
            {
                uint hash = (uint)(Mathf.RoundToInt(point.x * 1024) * 73856093)
                    ^ (uint)(Mathf.RoundToInt(point.z * 1024) * 19349663) ^ (uint)seed;
                hash ^= hash >> 16;
                hash *= 0x7feb352d;
                hash ^= hash >> 15;
                hash *= 0x846ca68b;
                return (hash & 0xffffff) / 16777216f;
            }
        }

        private bool IsExcluded(Vector3 position)
        {
            foreach (GrassExclusionVolume volume in exclusions)
                if (volume.Contains(position))
                    return true;
            return false;
        }

        internal void AddExclusion(GrassExclusionVolume volume)
        {
            if (exclusions.Contains(volume))
                return;
            exclusions.Add(volume);
            MoveExclusion(volume.Coverage, volume.Coverage);
        }

        internal void RemoveExclusion(GrassExclusionVolume volume)
        {
            if (exclusions.Remove(volume))
                MoveExclusion(volume.Coverage, volume.Coverage);
        }

        internal void MoveExclusion(Bounds previous, Bounds current)
        {
            RebuiltRegions = 0;
            if (regions == null)
                return;
            for (int i = 0; i < regions.Length; i++)
                if (regions[i].Bounds.Intersects(previous) || regions[i].Bounds.Intersects(current))
                    RebuildRegion(i);
        }

        private void SubmitLit()
        {
            if (regions == null)
                return;

            if (placementDirty)
                RebuildPlacement();

            bool all = mode == RenderMode.AllInstances;
            float lensScale = gameplayCamera.orthographic
                ? Mathf.Clamp(8 / Mathf.Max(1, gameplayCamera.orthographicSize), 0.7f, 1.65f)
                : Mathf.Clamp(58 / Mathf.Max(1, gameplayCamera.fieldOfView), 0.7f, 1.65f);
            float scale = lodRangeScale * lensScale;
            Vector3 eye = gameplayCamera.transform.position;
            GeometryUtility.CalculateFrustumPlanes(gameplayCamera, planes);
            var render = new RenderParams(litMaterial)
            {
                camera = gameplayCamera,
                layer = gameObject.layer,
                receiveShadows = true,
                lightProbeUsage = LightProbeUsage.Off,
                reflectionProbeUsage = ReflectionProbeUsage.Off,
                motionVectorMode = MotionVectorGenerationMode.ForceNoMotion
            };

            if (RenderSettings.ambientMode == AmbientMode.Trilight)
            {
                Color sky = LightingColor(RenderSettings.ambientSkyColor);
                Color equator = LightingColor(RenderSettings.ambientEquatorColor);
                Color ground = LightingColor(RenderSettings.ambientGroundColor);
                ApplyAmbient(unlitBlock, sky, equator, ground);
                ApplyAmbient(localBlock, sky, equator, ground);
            }
            else
            {
                unlitBlock.SetVector(AmbientSkyId, Vector4.zero);
                localBlock.SetVector(AmbientSkyId, Vector4.zero);
            }

            foreach (Region region in regions)
            {
                float near = region.Bounds.SqrDistance(eye);
                if (!all && (near >= Square(42 * scale) || !GeometryUtility.TestPlanesAABB(planes, region.Bounds)))
                    continue;

                int lights = CollectLights(region.Bounds);
                MaterialPropertyBlock block = lights > 0 ? localBlock : unlitBlock;
                block.SetInt(LightCountId, lights);
                if (lights > 0)
                {
                    block.SetVectorArray(LightPositionsId, lightPositions);
                    block.SetVectorArray(LightColorsId, lightColors);
                }

                bool submitted = false;
                for (int i = 0; i < region.Batches.Length; i++)
                {
                    int stage = i % Stages;
                    Matrix4x4[] matrices = region.Batches[i];
                    if (matrices.Length == 0 || !ShouldSubmit(stage, near, region.Bounds, eye, scale, all))
                        continue;
                    submitted = true;
                    QueueLit(ref render, i, matrices, region.Bounds, block, scale, all, lights > 0);
                }

                if (submitted)
                    SubmittedCells++;
            }

            for (int i = 0; i < litSubmissions.Length; i++)
                if (litSubmissions[i].Count != 0)
                    DrawLit(ref render, i, unlitBlock, scale, all);
        }

        private static Color LightingColor(Color color) =>
            QualitySettings.activeColorSpace == ColorSpace.Linear ? color.linear : color;

        private static void ApplyAmbient(MaterialPropertyBlock block, Color sky, Color equator, Color ground)
        {
            block.SetVector(AmbientSkyId, new Vector4(sky.r, sky.g, sky.b, 1));
            block.SetVector(AmbientEquatorId, equator);
            block.SetVector(AmbientGroundId, ground);
        }

        private static float Square(float value) => value * value;

        private static bool ShouldSubmit(int stage, float near, Bounds bounds, Vector3 eye, float scale, bool all)
        {
            if (all)
                return stage < DetailVariants;
            if (stage < DetailVariants)
                return near < Square(20 * scale);

            float far = (Vector3.Max(bounds.max - eye, eye - bounds.min)).sqrMagnitude;
            if (stage == DetailVariants)
                return near < Square(32 * scale) && far > Square(12 * scale);
            return far > Square(25 * scale);
        }

        private void QueueLit(ref RenderParams render, int index, Matrix4x4[] matrices, Bounds bounds,
            MaterialPropertyBlock block, float scale, bool all, bool localized)
        {
            Submission submission = litSubmissions[index];
            if (localized && submission.Count != 0)
                DrawLit(ref render, index, unlitBlock, scale, all);

            for (int offset = 0; offset < matrices.Length;)
            {
                int count = Mathf.Min(matrices.Length - offset, MaxSubmissionSize - submission.Count);
                if (submission.Count == 0)
                    submission.Bounds = bounds;
                else
                    submission.Bounds.Encapsulate(bounds);
                Array.Copy(matrices, offset, submission.Matrices, submission.Count, count);
                offset += count;
                submission.Count += count;
                if (submission.Count == MaxSubmissionSize)
                    DrawLit(ref render, index, block, scale, all);
            }

            if (localized && submission.Count != 0)
                DrawLit(ref render, index, block, scale, all);
        }

        private void DrawLit(ref RenderParams render, int index, MaterialPropertyBlock block, float scale, bool all)
        {
            int stage = index % Stages;
            bool detail = stage < DetailVariants;
            bool middle = stage == DetailVariants;
            Vector4 fade = detail || all ? new Vector4(100000, 100002, 0, 0) :
                middle ? new Vector4(25, 32, 0, 0) * scale : new Vector4(34, 42, 0, 0) * scale;
            Vector4 transition = all ? Vector4.zero : detail ? new Vector4(12 * scale, 20 * scale, -1, 0) :
                middle ? new Vector4(12 * scale, 20 * scale, 1, 0) : new Vector4(25 * scale, 32 * scale, 1, 0);
            Profile profile = (index / Stages) switch
            {
                0 => shortGrass,
                1 => wildGrass,
                _ => coastalGrass
            };
            block.SetColor(ColorRootId, profile.root);
            block.SetColor(ColorTipId, profile.tip);
            block.SetVector(GeometryId, new Vector4(profile.width, profile.height, 0, 0));
            block.SetFloat(BendId, profile.bend);
            block.SetFloat(NormalUpId, profile.normalUp);
            block.SetFloat(TranslucencyId, profile.translucency);
            block.SetVector(FadeDistanceId, fade);
            block.SetVector(LodId, transition);

            Submission submission = litSubmissions[index];
            int meshIndex = detail ? stage : DetailVariants - 1;
            render.worldBounds = submission.Bounds;
            render.matProps = block;
            render.shadowCastingMode = castGrassShadows && detail ? ShadowCastingMode.On : ShadowCastingMode.Off;
            Graphics.RenderMeshInstanced(render, tuftMeshes[meshIndex], 0, submission.Matrices, submission.Count);
            SubmittedTriangles += submission.Count * triangles[meshIndex];
            SubmittedInstances += submission.Count;
            SubmittedBatches++;
            submission.Count = 0;
        }

        private int CollectLights(Bounds bounds)
        {
            int count = 0;
            foreach (Light light in localLights)
            {
                if (!light || !light.isActiveAndEnabled || light.type != LightType.Point || light.intensity <= 0 ||
                    (light.cullingMask & (1 << gameObject.layer)) == 0)
                    continue;

                Vector3 position = light.transform.position;
                float distance = bounds.SqrDistance(position);
                if (distance >= Square(light.range))
                    continue;

                float centerDistance = (bounds.center - position).sqrMagnitude;
                int slot = Mathf.Min(count, MaxLights - 1);
                if (count == MaxLights && centerDistance >= lightDistances[slot])
                    continue;

                while (slot > 0 && centerDistance < lightDistances[slot - 1])
                {
                    lightDistances[slot] = lightDistances[slot - 1];
                    lightPositions[slot] = lightPositions[slot - 1];
                    lightColors[slot] = lightColors[slot - 1];
                    slot--;
                }

                Color color = LightingColor(light.color);
                lightDistances[slot] = centerDistance;
                lightPositions[slot] = new Vector4(position.x, position.y, position.z, light.range);
                lightColors[slot] = color * light.intensity;
                count = Mathf.Min(count + 1, MaxLights);
            }
            return count;
        }

        private void OnDrawGizmosSelected()
        {
            if (!showRegions || !layout)
                return;
            Gizmos.color = new Color(0.35f, 0.7f, 0.2f, 0.8f);
            foreach (CoastalGrassLayout.Cell cell in layout.Cells)
                Gizmos.DrawWireCube(cell.Bounds.center, cell.Bounds.size);
        }
    }
}
