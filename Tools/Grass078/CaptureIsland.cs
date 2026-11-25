using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Freyja.Environment;
using Freyja.Prototype;

var session = UnityEngine.Object.FindFirstObjectByType<CaisSessionController>();
if (!Application.isPlaying || !session || !session.IsInitialized)
    throw new InvalidOperationException("Enter Play Mode in Ilha_Principal.");
var camera = session.CameraRig.ViewCamera;
var player = UnityEngine.Object.FindFirstObjectByType<CaisPlayerController>();
var grass = UnityEngine.Object.FindFirstObjectByType<CoastalGrass>();
var clock = UnityEngine.Object.FindFirstObjectByType<CoastalDayNight>();
var lamp = UnityEngine.Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None)
    .Single(light => light.transform.parent && light.transform.parent.name == "B1 — Luz da casa");
var wind = GlobalWindController.Active;
var target = camera.targetTexture;
var terrain = Terrain.activeTerrain;
var folder = Path.GetFullPath(Path.Combine(Application.dataPath, "../../specs/078-grama-vento-global/evidence/island"));
Directory.CreateDirectory(folder);
var image = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
var records = new List<object>();
var shots = new List<(string name, Action setup, double delay)>();
bool originalClock = clock.AdvancesAutomatically;
float originalHour = clock.Hour;
float originalFov = camera.fieldOfView;
Vector3 originalPosition = player.transform.position;
float originalYaw = session.CameraRig.Yaw;
float lampIntensity = 0;
bool lampEnabled = false;
Vector2 previousWindDirection = wind.Direction;
var originalWind = wind.CurrentPreset;
var originalMode = grass.Mode;
session.ReleaseControl();
clock.AdvancesAutomatically = false;

void Pose(float x, float z, float yaw, float fov = 58)
{
    float y = terrain.SampleHeight(new Vector3(x, 0, z)) + terrain.transform.position.y + 0.035f;
    player.ResetTo(new Vector3(x, y, z), Quaternion.Euler(0, yaw, 0));
    session.CameraRig.ResetView(yaw);
    camera.fieldOfView = fov;
}

void Add(string name, Action setup = null, double delay = 0.45) => shots.Add((name, setup, delay));
Add("01-06h", () => { Pose(-38, -31, 20); clock.SetHour(6); });
Add("02-12h", () => clock.SetHour(12));
Add("03-17h", () => clock.SetHour(17));
Add("04-22h", () => clock.SetHour(22));
Add("05-luminaria-ligada", () => Pose(-27, -28, 60));
Add("06-luminaria-desligada", () => { lampIntensity = lamp.intensity; lampEnabled = lamp.enabled; lamp.enabled = false; });
Add("07-beira-do-caminho", () => { lamp.intensity = lampIntensity; lamp.enabled = lampEnabled; clock.SetHour(12); Pose(-35, -28, 335); });
Add("08-praia", () => Pose(-24, -46, 35));
Add("09-casas", () => Pose(-26, -14, 60));
Add("10-encosta", () => Pose(-36, 16, 55));
Add("11-encosta-zoom-proximo", () => Pose(-36, 16, 55, 38));
Add("12-encosta-zoom-aberto", () => Pose(-36, 16, 55, 78));
Add("13-grama-anterior", () => { Pose(-36, 16, 55); grass.Mode = CoastalGrass.RenderMode.Legacy; });
Add("14-nova-grama", () => grass.Mode = CoastalGrass.RenderMode.Optimized);
Add("15-encosta-desligada", () => grass.Mode = CoastalGrass.RenderMode.Off);
Add("16-calmaria", () => { grass.Mode = CoastalGrass.RenderMode.Optimized; wind.CurrentPreset = GlobalWindController.Preset.Calm; }, 3);
Add("17-brisa", () => wind.CurrentPreset = GlobalWindController.Preset.Breeze, 1.8);
for (int i = 0; i < 15; i++)
{
    int index = i;
    Add($"vento-{index:D2}", index == 0 ? () => wind.CurrentPreset = GlobalWindController.Preset.Strong :
        index == 5 ? () => wind.Direction = new Vector2(-0.6f, -0.8f) : null, index == 0 ? 2.5 : 0.082);
}
Add("18-percurso-inicio", () =>
{
    Pose(-38, -31, 20);
    player.ControlEnabled = true;
    player.SetMoveInput(Vector2.zero);
});
for (int i = 0; i < 7; i++)
{
    int index = i;
    Add($"ida-{i:D2}", index == 0 ? () => player.SetMoveInput(new Vector2(0, 0.82f)) : null, 0.40);
}
for (int i = 0; i < 7; i++)
{
    int index = i;
    Add($"volta-{i:D2}", index == 0 ? () => player.SetMoveInput(new Vector2(0, -0.82f)) : null, 0.40);
}
Add("19-percurso-fim", () => player.SetMoveInput(Vector2.zero));
Add("20-caminho-desligada", () =>
{
    Pose(-38, -31, 20);
    player.ControlEnabled = false;
    grass.Mode = CoastalGrass.RenderMode.Off;
});
Add("21-caminho-anterior", () => grass.Mode = CoastalGrass.RenderMode.Legacy);
Add("22-caminho-rajada", () =>
{
    grass.Mode = CoastalGrass.RenderMode.Optimized;
    wind.CurrentPreset = GlobalWindController.Preset.Strong;
}, 2.5);

int step = 0;
int lastFrame = Time.frameCount;
double due = Time.realtimeSinceStartupAsDouble + shots[0].delay;
shots[0].setup?.Invoke();
EditorApplication.CallbackFunction capture = null;
void Restore()
{
    EditorApplication.update -= capture;
    if (!Application.isPlaying)
        return;

    grass.Mode = originalMode;
    wind.Direction = previousWindDirection;
    wind.CurrentPreset = originalWind;
    clock.SetHour(originalHour);
    clock.AdvancesAutomatically = originalClock;
    player.SetMoveInput(Vector2.zero);
    player.ResetTo(originalPosition, Quaternion.Euler(0, originalYaw, 0));
    session.CameraRig.ResetView(originalYaw);
    session.ReleaseControl();
    camera.fieldOfView = originalFov;
    UnityEngine.Object.Destroy(image);
}
capture = () =>
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
        RenderTexture previous = RenderTexture.active;
        try
        {
            RenderTexture.active = target;
            image.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
            image.Apply();
        }
        finally { RenderTexture.active = previous; }

        string name = shots[step].name;
        File.WriteAllBytes(Path.Combine(folder, name + ".png"), image.EncodeToPNG());
        records.Add(new
        {
            name, frame = lastFrame, hour = clock.Hour, fov = camera.fieldOfView,
            camera = camera.transform.position.ToString("F5"), player = player.transform.position.ToString("F5"),
            lamp = lamp.isActiveAndEnabled, recoveries = session.RecoveryCount,
            grass.SubmittedInstances, grass.SubmittedBatches, grass.SubmittedTriangles,
            wind = Shader.GetGlobalVector("_FreyjaWind").ToString("F5")
        });

        if (++step == shots.Count)
        {
            File.WriteAllText(Path.Combine(folder, "observacoes.json"), Newtonsoft.Json.JsonConvert.SerializeObject(records, Newtonsoft.Json.Formatting.Indented));
            Restore();
            return;
        }

        shots[step].setup?.Invoke();
        due = Time.realtimeSinceStartupAsDouble + shots[step].delay;
    }
    catch (Exception exception)
    {
        Restore();
        Debug.LogException(exception);
    }
};
EditorApplication.update += capture;
return folder;
