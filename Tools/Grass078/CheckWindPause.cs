using System.IO;
using UnityEditor;
using UnityEngine;
using Freyja.Environment;

if (!Application.isPlaying || !GlobalWindController.Active)
    throw new System.InvalidOperationException("Start Ilha_Principal with global wind active.");

int property = Shader.PropertyToID("_FreyjaWind");
float previousScale = Time.timeScale;
Time.timeScale = 0;
int initialFrame = Time.frameCount;
Vector4 initialState = Shader.GetGlobalVector(property);
EditorApplication.CallbackFunction check = null;
check = () =>
{
    if (!Application.isPlaying)
    {
        EditorApplication.update -= check;
        Time.timeScale = previousScale;
        return;
    }
    if (Time.frameCount < initialFrame + 8)
        return;

    EditorApplication.update -= check;
    Vector4 pausedState = Shader.GetGlobalVector(property);
    int frames = Time.frameCount - initialFrame;
    Time.timeScale = previousScale;
    bool passed = initialState == pausedState;
    var path = Path.GetFullPath(Path.Combine(Application.dataPath, "../../specs/078-grama-vento-global/evidence/wind-pause.json"));
    File.WriteAllText(path, Newtonsoft.Json.JsonConvert.SerializeObject(new { passed, frames, state = initialState.ToString("F5") }));
    if (!passed)
        Debug.LogError("Wind changed during a paused simulation.");
};
EditorApplication.update += check;
return "Wind pause test queued.";
