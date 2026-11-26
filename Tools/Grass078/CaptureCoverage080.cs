using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Freyja.Environment;
using Freyja.Prototype;

var session = UnityEngine.Object.FindFirstObjectByType<CaisSessionController>();
var player = UnityEngine.Object.FindFirstObjectByType<CaisPlayerController>();
var grass = UnityEngine.Object.FindFirstObjectByType<CoastalGrass>();
var clock = UnityEngine.Object.FindFirstObjectByType<CoastalDayNight>();
var wind = GlobalWindController.Active;
var terrain = Terrain.activeTerrain;
if (!Application.isPlaying || !session || !session.IsInitialized || !player || !grass || !clock || !wind || !terrain)
    throw new InvalidOperationException("Run this capture in Play Mode in Ilha_Principal after session initialization.");

var camera = session.CameraRig.ViewCamera;
var target = camera.targetTexture;
if (!target || target.width != 480 || target.height != 270)
    throw new InvalidOperationException("Coverage comparison requires the production 480 x 270 camera RenderTexture.");

bool oldAutoClock = clock.AdvancesAutomatically;
float oldHour = clock.Hour;
float oldFov = camera.fieldOfView;
Vector3 oldPlayerPosition = player.transform.position;
float oldYaw = session.CameraRig.Yaw;
Transform oldCameraTarget = session.CameraRig.Target;
int oldRecoveries = session.RecoveryCount;
Vector2 oldWindDirection = wind.Direction;
GlobalWindController.Preset oldWind = wind.CurrentPreset;
CoastalGrass.RenderMode oldMode = grass.Mode;
CoastalGrass.Quality oldQuality = grass.CurrentQuality;
grass.CurrentQuality = CoastalGrass.Quality.High;
string root = Path.GetFullPath(Path.Combine(Application.dataPath, "../../specs/080-grama-tripla-cobertura/evidence/coverage"));
string stage = grass.BakedInstances >= 300000 ? "candidate" : "baseline";
string folder = Path.Combine(root, stage);
Directory.CreateDirectory(folder);
var image = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
var cases = new (string name, float x, float z, float yaw)[]
{
    ("campo-oeste", -35, -28, 335),
    ("campo-leste", -26, -14, 60),
    ("encosta-oeste", -24, 32, 55),
    ("encosta-leste", 8, 40, 235)
};
var observations = new List<object>();
int aggregateChangedPixels = 0;
int aggregateRoiPixels = 0;
session.ReleaseControl();
clock.AdvancesAutomatically = false;
clock.SetHour(12);
wind.CurrentPreset = GlobalWindController.Preset.Calm;
grass.CurrentQuality = CoastalGrass.Quality.High;
camera.fieldOfView = 58;
int caseIndex = 0;
int phase = 0;
int lastFrame = Time.frameCount;
double due = Time.realtimeSinceStartupAsDouble + 0.45;

void Pose((string name, float x, float z, float yaw) item)
{
    var ground = new Vector3(item.x, 0, item.z);
    ground.y = terrain.SampleHeight(ground) + terrain.transform.position.y + 0.035f;
    player.ResetTo(ground, Quaternion.Euler(0, item.yaw, 0));
    session.CameraRig.Target = player.transform;
    session.CameraRig.ResetView(item.yaw);
}

void Restore()
{
    EditorApplication.update -= Capture;
    if (!Application.isPlaying)
        return;
    grass.Mode = oldMode;
    grass.CurrentQuality = oldQuality;
    wind.Direction = oldWindDirection;
    wind.CurrentPreset = oldWind;
    clock.SetHour(oldHour);
    clock.AdvancesAutomatically = oldAutoClock;
    player.ResetTo(oldPlayerPosition, Quaternion.Euler(0, oldYaw, 0));
    session.CameraRig.Target = oldCameraTarget;
    session.CameraRig.ResetView(oldYaw);
    session.ReleaseControl();
    camera.fieldOfView = oldFov;
    UnityEngine.Object.Destroy(image);
}

void Capture()
{
    if (!Application.isPlaying)
    {
        Restore();
        return;
    }
    if (Time.frameCount == lastFrame || Time.realtimeSinceStartupAsDouble < due)
        return;

    try
    {
        lastFrame = Time.frameCount;
        float cameraFollowDistance = Vector3.Distance(camera.transform.position, player.transform.position);
        if (cameraFollowDistance > 12f || session.RecoveryCount != oldRecoveries)
            throw new InvalidOperationException($"Pose {cases[caseIndex].name} did not settle on the player: distance={cameraFollowDistance:F2}, recoveries={session.RecoveryCount}.");
        RenderTexture previous = RenderTexture.active;
        try
        {
            RenderTexture.active = target;
            image.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
            image.Apply();
        }
        finally { RenderTexture.active = previous; }

        var item = cases[caseIndex];
        Color32[] pixels = image.GetPixels32();
        string suffix = phase == 0 ? "on" : "off";
        File.WriteAllBytes(Path.Combine(folder, $"{item.name}-{suffix}.png"), image.EncodeToPNG());
        if (phase == 0)
        {
            observations.Add(new { item.name, camera = camera.transform.position.ToString("F5"), player = player.transform.position.ToString("F5"), cameraFollowDistance, recoveries = session.RecoveryCount, item.yaw, fov = camera.fieldOfView, hour = clock.Hour, wind = wind.CurrentPreset.ToString(), bakedRoots = grass.BakedInstances });
            grass.Mode = CoastalGrass.RenderMode.Off;
            phase = 1;
            due = Time.realtimeSinceStartupAsDouble + 0.30;
            return;
        }

        var previousOn = File.ReadAllBytes(Path.Combine(folder, $"{item.name}-on.png"));
        var on = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
        on.LoadImage(previousOn, false);
        Color32[] onPixels = on.GetPixels32();
        UnityEngine.Object.Destroy(on);
        int roiStartY = target.height / 4;
        int changed = 0;
        int roiPixels = 0;
        for (int y = roiStartY; y < target.height; y++)
        for (int x = 0; x < target.width; x++)
        {
            int index = y * target.width + x;
            int dr = onPixels[index].r - pixels[index].r;
            int dg = onPixels[index].g - pixels[index].g;
            int db = onPixels[index].b - pixels[index].b;
            if (dr * dr + dg * dg + db * db > 400)
                changed++;
            roiPixels++;
        }
        aggregateChangedPixels += changed;
        aggregateRoiPixels += roiPixels;
        observations.Add(new { item.name, changedPixels = changed, roiPixels, coverageRatio = changed / (double)roiPixels, roi = "bottom 75%", rgbEuclideanThreshold8Bit = 20 });

        if (++caseIndex >= cases.Length)
        {
            double aggregateCoverageRatio = aggregateChangedPixels / (double)aggregateRoiPixels;
            double? baselineCoverageRatio = null;
            double? coverageIncreaseRatio = null;
            bool? passesCoverageTarget = null;
            if (stage == "candidate")
            {
                string baselinePath = Path.Combine(root, "baseline", "coverage.json");
                if (!File.Exists(baselinePath))
                    throw new InvalidOperationException("Capture the 078 baseline before the 080 candidate.");
                var baselineJson = Newtonsoft.Json.Linq.JObject.Parse(File.ReadAllText(baselinePath));
                baselineCoverageRatio = (double)baselineJson["aggregateCoverageRatio"];
                if (baselineCoverageRatio <= 0)
                    throw new InvalidOperationException("The 078 baseline coverage must be greater than zero.");
                coverageIncreaseRatio = aggregateCoverageRatio / baselineCoverageRatio.Value - 1;
                passesCoverageTarget = coverageIncreaseRatio >= 0.25;
            }
            File.WriteAllText(Path.Combine(folder, "coverage.json"), Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                engine = Application.unityVersion, camera = camera.name, renderTexture = new[] { target.width, target.height }, stage,
                fov = camera.fieldOfView, hour = clock.Hour, wind = wind.CurrentPreset.ToString(), observations,
                roi = "bottom 75% (y >= height / 4)", aggregateChangedPixels, aggregateRoiPixels, aggregateCoverageRatio,
                baselineCoverageRatio, coverageIncreaseRatio, passesCoverageTarget
            }, Newtonsoft.Json.Formatting.Indented));
            Restore();
            Debug.Log($"080 coverage captures and pixel counts saved to {folder}");
            return;
        }

        Pose(cases[caseIndex]);
        grass.Mode = CoastalGrass.RenderMode.Optimized;
        phase = 0;
        due = Time.realtimeSinceStartupAsDouble + 0.45;
    }
    catch (Exception exception)
    {
        Restore();
        Debug.LogException(exception);
    }
}

Pose(cases[0]);
grass.Mode = CoastalGrass.RenderMode.Optimized;
EditorApplication.update += Capture;
return new { stage, folder, poses = cases.Length, renderTexture = new[] { target.width, target.height } };
