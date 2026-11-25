using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

if (Application.isPlaying || EditorApplication.isCompiling)
    throw new System.InvalidOperationException("Stop Play Mode and wait for compilation.");
string folder = Path.Combine(Path.GetTempPath(), "FreyjaGrass078Benchmark");
Directory.CreateDirectory(folder);
string output = Path.Combine(folder, "FreyjaGrass078.app");
string status = Path.Combine(folder, "build.json");
if (File.Exists(status))
    File.Delete(status);

EditorApplication.CallbackFunction build = null;
build = () =>
{
    EditorApplication.update -= build;
    File.WriteAllText(status, "{\"result\":\"Running\"}");
    const string settingsPath = "ProjectSettings/ProjectSettings.asset";
    byte[] originalSettings = File.ReadAllBytes(settingsPath);
    Object[] originalPreloadedAssets = PlayerSettings.GetPreloadedAssets();
    bool originalTiming = PlayerSettings.enableFrameTimingStats;
    int originalArchitecture = PlayerSettings.GetArchitecture(BuildTargetGroup.Standalone);
    try
    {
        PlayerSettings.enableFrameTimingStats = true;
        PlayerSettings.SetArchitecture(BuildTargetGroup.Standalone, 1);
        BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { "Assets/_Project/Scenes/Ilha_Principal.unity" },
            locationPathName = output, target = BuildTarget.StandaloneOSX,
            options = BuildOptions.Development
        });
        BuildSummary summary = report.summary;
        File.WriteAllText(status, Newtonsoft.Json.JsonConvert.SerializeObject(new
        {
            result = summary.result.ToString(), summary.totalErrors, summary.totalWarnings,
            bytes = summary.totalSize, seconds = summary.totalTime.TotalSeconds, summary.outputPath
        }, Newtonsoft.Json.Formatting.Indented));
    }
    catch (System.Exception exception)
    {
        File.WriteAllText(status, Newtonsoft.Json.JsonConvert.SerializeObject(new { error = exception.ToString() }));
        Debug.LogException(exception);
    }
    finally
    {
        PlayerSettings.SetArchitecture(BuildTargetGroup.Standalone, originalArchitecture);
        PlayerSettings.enableFrameTimingStats = originalTiming;
        PlayerSettings.SetPreloadedAssets(originalPreloadedAssets);
        // The build and its package preprocessors serialize temporary settings.
        File.WriteAllBytes(settingsPath, originalSettings);
    }
};
EditorApplication.update += build;
return new { output, status };
