using System;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering;

namespace Freyja.Environment
{
    [DefaultExecutionOrder(500)]
    [DisallowMultipleComponent]
    public sealed class CoastalGrass : MonoBehaviour
    {
        public enum RenderMode { Optimized, Off, AllInstances }

        [SerializeField] private Camera gameplayCamera;
        [SerializeField] private CoastalGrassLayout layout;
        [SerializeField] private Material grassMaterial;
        [SerializeField] private Mesh[] tuftMeshes;
        [SerializeField] private RenderMode mode;

        private static readonly ProfilerMarker RenderMarker = new("Freyja.CoastalGrass.Submit");
        private static readonly int FadeDistanceId = Shader.PropertyToID("_FadeDistance");

        private const int MaxSubmissionSize = 511;
        private const float DetailFadeStart = 12;
        private const float DetailFadeEnd = 28;
        private const float BaseFadeStart = 28;
        private const float BaseFadeEnd = 35;

        private readonly Plane[] planes = new Plane[6];
        private sealed class Submission
        {
            public readonly Matrix4x4[] Matrices = new Matrix4x4[MaxSubmissionSize];
            public Bounds Bounds;
            public int Count;
        }

        private Submission[] submissions;
        private MaterialPropertyBlock detailed;
        private MaterialPropertyBlock distant;
        private MaterialPropertyBlock unlimited;

        public int SubmittedCells { get; private set; }
        public int SubmittedInstances { get; private set; }
        public int SubmittedBatches { get; private set; }

        private void Awake()
        {
            detailed = CreateFade(DetailFadeStart, DetailFadeEnd);
            distant = CreateFade(BaseFadeStart, BaseFadeEnd);
            unlimited = CreateFade(100000, 100002);
            submissions = new Submission[tuftMeshes?.Length ?? 0];
            for (int index = 0; index < submissions.Length; index++)
                submissions[index] = new Submission();
        }

        private static MaterialPropertyBlock CreateFade(float start, float end)
        {
            var block = new MaterialPropertyBlock();
            block.SetVector(FadeDistanceId, new Vector4(start, end, 0, 0));
            return block;
        }

        private void LateUpdate()
        {
            SubmittedCells = 0;
            SubmittedInstances = 0;
            SubmittedBatches = 0;

            if (mode == RenderMode.Off || !gameplayCamera || !gameplayCamera.isActiveAndEnabled ||
                !layout || !grassMaterial || !grassMaterial.enableInstancing ||
                submissions.Length == 0 || !SystemInfo.supportsInstancing)
                return;

            using (RenderMarker.Auto())
                Submit();
        }

        private void Submit()
        {
            bool optimized = mode == RenderMode.Optimized;
            Vector3 cameraPosition = gameplayCamera.transform.position;
            if (optimized)
                GeometryUtility.CalculateFrustumPlanes(gameplayCamera, planes);

            var render = new RenderParams(grassMaterial)
            {
                camera = gameplayCamera,
                layer = gameObject.layer,
                shadowCastingMode = ShadowCastingMode.Off,
                receiveShadows = false,
                lightProbeUsage = LightProbeUsage.Off,
                reflectionProbeUsage = ReflectionProbeUsage.Off,
                motionVectorMode = MotionVectorGenerationMode.ForceNoMotion
            };

            foreach (CoastalGrassLayout.Cell cell in layout.Cells)
            {
                float distanceSquared = cell.Bounds.SqrDistance(cameraPosition);
                if (optimized && (distanceSquared >= BaseFadeEnd * BaseFadeEnd ||
                    !GeometryUtility.TestPlanesAABB(planes, cell.Bounds)))
                    continue;

                bool submitted = false;
                foreach (CoastalGrassLayout.Batch batch in cell.Batches)
                {
                    bool simple = batch.MeshIndex == tuftMeshes.Length - 1;
                    if (optimized && !simple && distanceSquared >= DetailFadeEnd * DetailFadeEnd)
                        continue;

                    Queue(ref render, batch, cell.Bounds, optimized);
                    submitted = true;
                }

                if (submitted)
                    SubmittedCells++;
            }

            for (int meshIndex = 0; meshIndex < submissions.Length; meshIndex++)
                if (submissions[meshIndex].Count > 0)
                    Draw(ref render, meshIndex, optimized);
        }

        // Only cells that passed visibility tests contribute to the draw buffers.
        private void Queue(ref RenderParams render, CoastalGrassLayout.Batch batch, Bounds bounds, bool optimized)
        {
            Submission submission = submissions[batch.MeshIndex];
            int offset = 0;
            while (offset < batch.Instances.Length)
            {
                int count = Mathf.Min(batch.Instances.Length - offset, submission.Matrices.Length - submission.Count);
                if (submission.Count == 0)
                    submission.Bounds = bounds;
                else
                    submission.Bounds.Encapsulate(bounds);

                Array.Copy(batch.Instances, offset, submission.Matrices, submission.Count, count);
                offset += count;
                submission.Count += count;
                if (submission.Count == submission.Matrices.Length)
                    Draw(ref render, batch.MeshIndex, optimized);
            }
        }

        private void Draw(ref RenderParams render, int meshIndex, bool optimized)
        {
            Submission submission = submissions[meshIndex];
            render.worldBounds = submission.Bounds;
            MaterialPropertyBlock fade = meshIndex == tuftMeshes.Length - 1 ? distant : detailed;
            render.matProps = optimized ? fade : unlimited;
            Graphics.RenderMeshInstanced(render, tuftMeshes[meshIndex], 0, submission.Matrices, submission.Count);
            SubmittedBatches++;
            SubmittedInstances += submission.Count;
            submission.Count = 0;
        }

#if UNITY_EDITOR
        public void Configure(Camera camera, CoastalGrassLayout bakedLayout, Material material, Mesh[] meshes)
        {
            gameplayCamera = camera;
            layout = bakedLayout;
            grassMaterial = material;
            tuftMeshes = meshes;
        }
#endif
    }
}
