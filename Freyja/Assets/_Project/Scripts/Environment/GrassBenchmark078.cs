#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Freyja.Prototype;
using Unity.Profiling;
using UnityEngine;

namespace Freyja.Environment
{
    public sealed class GrassBenchmark078 : MonoBehaviour
    {
        private const string Command = "-freyja-grass-benchmark";
        private const int SampleCount = 300;
        private string outputPath;

        private readonly struct Case
        {
            public readonly string Name;
            public readonly Vector2 Position;
            public readonly float Yaw;
            public readonly float Fov;
            public readonly float Hour;
            public readonly CoastalGrass.RenderMode Mode;
            public readonly CoastalGrass.Quality Quality;
            public readonly bool Shadows;

            public Case(string name, Vector2 position, float yaw, float fov, float hour,
                CoastalGrass.RenderMode mode = CoastalGrass.RenderMode.Optimized,
                CoastalGrass.Quality quality = CoastalGrass.Quality.High, bool shadows = false)
            {
                Name = name;
                Position = position;
                Yaw = yaw;
                Fov = fov;
                Hour = hour;
                Mode = mode;
                Quality = quality;
                Shadows = shadows;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Launch()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, Command);
            if (index >= 0 && index + 1 < args.Length)
                Begin(args[index + 1]);
        }

        public static void Begin(string path)
        {
            var runner = new GameObject("Grass Benchmark 078").AddComponent<GrassBenchmark078>();
            runner.outputPath = path;
        }

        private IEnumerator Start()
        {
            var session = FindFirstObjectByType<CaisSessionController>();
            var player = FindFirstObjectByType<CaisPlayerController>();
            var clock = FindFirstObjectByType<CoastalDayNight>();
            var grass = FindFirstObjectByType<CoastalGrass>();
            if (!session || !player || !clock || !grass)
            {
                Debug.LogError("The grass benchmark requires Ilha_Principal.", this);
                yield break;
            }

            while (!session.IsInitialized)
                yield return null;
            session.ReleaseControl();
            Camera camera = session.CameraRig.ViewCamera;
            Terrain terrain = Terrain.activeTerrain;

            Vector2 field = new(-36, 16);
            Vector2 coast = new(-38, -31);
            Vector2 village = new(-26, -14);
            Case[] cases =
            {
                new("campo-desligada", field, 55, 58, 12, CoastalGrass.RenderMode.Off),
                new("campo-anterior", field, 55, 58, 12, CoastalGrass.RenderMode.Legacy),
                new("campo-baixa", field, 55, 58, 12, quality: CoastalGrass.Quality.Low),
                new("campo-media", field, 55, 58, 12, quality: CoastalGrass.Quality.Medium),
                new("campo-alta", field, 55, 58, 12),
                new("campo-alta-sombra", field, 55, 58, 12, shadows: true),
                new("campo-zoom-proximo-desligada", field, 55, 38, 12, CoastalGrass.RenderMode.Off),
                new("campo-zoom-proximo", field, 55, 38, 12),
                new("campo-zoom-aberto-desligada", field, 55, 78, 12, CoastalGrass.RenderMode.Off),
                new("campo-zoom-aberto", field, 55, 78, 12),
                new("vila-desligada", village, 60, 58, 12, CoastalGrass.RenderMode.Off),
                new("vila-alta", village, 60, 58, 12),
                new("costa-noite-desligada", coast, 20, 58, 22, CoastalGrass.RenderMode.Off),
                new("costa-noite-alta", coast, 20, 58, 22),
                new("campo-desligada-volta", field, 55, 58, 12, CoastalGrass.RenderMode.Off),
                new("campo-alta-volta", field, 55, 58, 12)
            };

            var result = new List<object>(cases.Length);
            var timing = new FrameTiming[1];
            var frames = new double[SampleCount];
            var main = new double[SampleCount];
            var render = new double[SampleCount];
            var gpu = new double[SampleCount];
            int oldVSync = QualitySettings.vSyncCount;
            int oldCap = Application.targetFrameRate;
            float oldFov = camera.fieldOfView;
            var oldMode = grass.Mode;
            var oldQuality = grass.CurrentQuality;
            var oldShadows = grass.CastGrassShadows;
            bool clockWasRunning = clock.AdvancesAutomatically;
            float oldHour = clock.Hour;
            Vector3 oldPlayer = player.transform.position;
            float oldYaw = session.CameraRig.Yaw;

            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = -1;
            clock.AdvancesAutomatically = false;
            grass.MeasureRenderAllocations = true;
            using var drawCalls = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count", 1);
            using var batches = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Batches Count", 1);
            using var memory = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Used Memory", 1);
            using var gcFrame = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame", 1);
            using var grassTime = ProfilerRecorder.StartNew(ProfilerCategory.Scripts, "Freyja.CoastalGrass.Submit", 1);

            foreach (Case item in cases)
            {
                clock.SetHour(item.Hour);
                grass.Mode = item.Mode;
                grass.CurrentQuality = item.Quality;
                grass.CastGrassShadows = item.Shadows;
                var location = new Vector3(item.Position.x, 0, item.Position.y);
                location.y = terrain.SampleHeight(location) + terrain.transform.position.y + 0.035f;
                player.ResetTo(location, Quaternion.Euler(0, item.Yaw, 0));
                session.CameraRig.ResetView(item.Yaw);
                camera.fieldOfView = item.Fov;
                double waitUntil = Time.realtimeSinceStartupAsDouble + 5;
                while (Time.realtimeSinceStartupAsDouble < waitUntil)
                    yield return null;

                long totalDraws = 0;
                long totalBatches = 0;
                long totalGc = 0;
                long totalGrassGc = 0;
                long maximumGrassGc = 0;
                long totalGrassNs = 0;
                long totalGrassDraws = 0;
                long totalInstances = 0;
                long totalTriangles = 0;
                double elapsed = 0;
                for (int frame = 0; frame < SampleCount; frame++)
                {
                    FrameTimingManager.CaptureFrameTimings();
                    yield return null;
                    frames[frame] = Time.unscaledDeltaTime * 1000.0;
                    elapsed += frames[frame];
                    bool hasTiming = FrameTimingManager.GetLatestTimings(1, timing) == 1;
                    main[frame] = hasTiming ? timing[0].cpuMainThreadFrameTime : 0;
                    render[frame] = hasTiming ? timing[0].cpuRenderThreadFrameTime : 0;
                    gpu[frame] = hasTiming ? timing[0].gpuFrameTime : 0;
                    if (drawCalls.Valid) totalDraws += drawCalls.LastValue;
                    if (batches.Valid) totalBatches += batches.LastValue;
                    if (gcFrame.Valid) totalGc += gcFrame.LastValue;
                    if (grassTime.Valid && item.Mode != CoastalGrass.RenderMode.Off) totalGrassNs += grassTime.LastValue;
                    totalGrassGc += grass.RenderAllocatedBytes;
                    maximumGrassGc = Math.Max(maximumGrassGc, grass.RenderAllocatedBytes);
                    totalGrassDraws += grass.SubmittedBatches;
                    totalInstances += grass.SubmittedInstances;
                    totalTriangles += grass.SubmittedTriangles;
                }

                result.Add(new
                {
                    item.Name, item.Fov, item.Hour, item.Shadows,
                    camera = camera.transform.position.ToString("F5"), rotation = camera.transform.eulerAngles.ToString("F5"),
                    frameCount = SampleCount, fps = 1000 * SampleCount / elapsed,
                    p50Ms = Percentile(frames, 0.50), p95Ms = Percentile(frames, 0.95),
                    mainP50Ms = Percentile(main, 0.50), mainP95Ms = Percentile(main, 0.95),
                    renderP50Ms = Percentile(render, 0.50), renderP95Ms = Percentile(render, 0.95),
                    gpuP50Ms = Percentile(gpu, 0.50), gpuP95Ms = Percentile(gpu, 0.95),
                    draws = Average(drawCalls, totalDraws), renderBatches = Average(batches, totalBatches),
                    frameGcBytes = Average(gcFrame, totalGc), grassGcBytes = (double)totalGrassGc / SampleCount,
                    maximumGrassGc, grassSubmitMs = Average(grassTime, totalGrassNs) / 1e6,
                    managedBytes = memory.Valid ? (long?)memory.LastValue : null,
                    grassDraws = (double)totalGrassDraws / SampleCount,
                    instances = (double)totalInstances / SampleCount,
                    triangles = item.Mode == CoastalGrass.RenderMode.Legacy ? null : (double?)totalTriangles / SampleCount,
                    grass.BakedInstances, grass.CachedMatrixBytes, recoveries = session.RecoveryCount
                });
            }

            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outputPath)));
            var report = new
            {
                engine = Application.unityVersion, editor = Application.isEditor, gpu = SystemInfo.graphicsDeviceName,
                graphicsApi = SystemInfo.graphicsDeviceType.ToString(), quality = QualitySettings.names[QualitySettings.GetQualityLevel()],
                screen = new[] { Screen.width, Screen.height }, renderTexture = new[] { camera.targetTexture.width, camera.targetTexture.height },
                vSync = QualitySettings.vSyncCount, cap = Application.targetFrameRate,
                wind = GlobalWindController.Active.CurrentPreset.ToString(), result
            };
            File.WriteAllText(outputPath, JsonConvert.SerializeObject(report, Formatting.Indented));

            QualitySettings.vSyncCount = oldVSync;
            Application.targetFrameRate = oldCap;
            grass.Mode = oldMode;
            grass.CurrentQuality = oldQuality;
            grass.CastGrassShadows = oldShadows;
            grass.MeasureRenderAllocations = false;
            clock.SetHour(oldHour);
            clock.AdvancesAutomatically = clockWasRunning;
            player.ResetTo(oldPlayer, Quaternion.Euler(0, oldYaw, 0));
            session.CameraRig.ResetView(oldYaw);
            camera.fieldOfView = oldFov;

            if (!Application.isEditor)
                Application.Quit();
            Destroy(gameObject);
        }

        private static double? Average(ProfilerRecorder recorder, long total) => recorder.Valid ? (double)total / SampleCount : null;

        private static double? Percentile(double[] values, double percentile)
        {
            Array.Sort(values);
            int start = Array.FindIndex(values, value => value > 0);
            if (start < 0)
                return null;
            return values[start + (int)Math.Ceiling((values.Length - start) * percentile) - 1];
        }
    }
}
#endif
