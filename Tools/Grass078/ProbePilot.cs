using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Freyja.Environment;

var grass = UnityEngine.Object.FindFirstObjectByType<CoastalGrass>();
var wind = GlobalWindController.Active;
var camera = Camera.main;
if (!Application.isPlaying || !camera || camera.gameObject.scene.name != "Grama_Piloto_078" || !wind)
    throw new InvalidOperationException("Enter Play Mode in Grama_Piloto_078.");

var folder = Path.GetFullPath(Path.Combine(Application.dataPath, "../../specs/078-grama-vento-global/evidence/pilot"));
Directory.CreateDirectory(folder);
var sun = GameObject.Find("Pilot Sun").GetComponent<Light>();
var lamp = GameObject.Find("Pilot Lamp").GetComponent<Light>();
var target = camera.targetTexture;
var image = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
var names = new[] { "01-sol", "02-sem-sombra", "03-luz-local-desligada", "04-luz-local-ligada", "05-calmaria", "06-calmaria", "07-vento-forte", "08-vento-forte", "09-mudanca-de-direcao" };
var records = new System.Collections.Generic.List<object>();
int step = 0;
int lastFrame = Time.frameCount;
double due = Time.realtimeSinceStartupAsDouble + 2;
EditorApplication.CallbackFunction capture = null;
capture = () =>
{
    if (!Application.isPlaying)
    {
        EditorApplication.update -= capture;
        return;
    }
    if (lastFrame == Time.frameCount || Time.realtimeSinceStartupAsDouble < due)
        return;
    lastFrame = Time.frameCount;
    var previous = RenderTexture.active;
    try
    {
        RenderTexture.active = target;
        image.ReadPixels(new Rect(0, 0, image.width, image.height), 0, 0);
        image.Apply();
        File.WriteAllBytes(Path.Combine(folder, names[step] + ".png"), image.EncodeToPNG());
        records.Add(new { sample = names[step], frame = lastFrame, grass.SubmittedInstances, grass.SubmittedBatches,
            grass.SubmittedTriangles, wind = Shader.GetGlobalVector("_FreyjaWind").ToString("F6") });

        switch (++step)
        {
            case 1: sun.shadows = LightShadows.None; break;
            case 2:
                sun.intensity = 0;
                RenderSettings.ambientSkyColor = new Color(0.21f, 0.28f, 0.42f);
                RenderSettings.ambientEquatorColor = new Color(0.18f, 0.22f, 0.32f);
                RenderSettings.ambientGroundColor = new Color(0.12f, 0.16f, 0.24f);
                break;
            case 3: lamp.enabled = true; break;
            case 4: lamp.enabled = false; sun.intensity = 1.1f; wind.CurrentPreset = GlobalWindController.Preset.Calm; break;
            case 6: wind.CurrentPreset = GlobalWindController.Preset.Strong; break;
            case 8: wind.Direction = new Vector2(-0.6f, 0.8f); break;
            case 9:
                EditorApplication.update -= capture;
                UnityEngine.Object.Destroy(image);
                File.WriteAllText(Path.Combine(folder, "observacoes.json"), Newtonsoft.Json.JsonConvert.SerializeObject(records, Newtonsoft.Json.Formatting.Indented));
                return;
        }
        due = Time.realtimeSinceStartupAsDouble + ((step == 4 || step == 6) ? 2.5 : 0.38);
    }
    catch (Exception exception)
    {
        EditorApplication.update -= capture;
        UnityEngine.Object.Destroy(image);
        Debug.LogException(exception);
    }
    finally { RenderTexture.active = previous; }
};
EditorApplication.update += capture;
return folder;
