using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Freyja.Environment;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using Object = UnityEngine.Object;

public sealed class SeaStudyEvidence : MonoBehaviour
{
    private const int CaptureRate = 24;
    private const int ClipFrames = 10 * CaptureRate;
    private const int BenchmarkFrames = 160;
    private const int WarmupFrames = 40;
    private const int VariantCount = 3;
    private static readonly (string Name, Vector3 Position, Vector3 Target, Vector3 Move, Vector3 Aim)[] Views =
    {
        ("pier", new(-47, 3.2f, -72), new(-35, 1.2f, -60), new(2.2f, 0, 0.8f), new(1.2f, 0, 0.5f)),
        ("encosta", new(69, 18, 15), new(58, 2, -30), new(3, 0, 2), new(-4, 0, 4)),
        ("praia", new(20, 5, -61), new(8, 0.5f, -54), new(-2, 0, -1), new(-2, 0, 0))
    };
    private static int CombinationCount => Views.Length * VariantCount;

    private readonly float[] samples = new float[BenchmarkFrames];
    private Camera cameraToCapture;
    private SeaStudyComparison comparison;
    private SeaStudyComparison.Mode previousMode;
    private Behaviour cameraRig;
    private Vector3 previousPosition;
    private Quaternion previousRotation;
    private Texture2D image;
    private string output;
    private string action;
    private int shot;
    private int frame;
    private int previousVSync;
    private int previousTargetRate;
    private int previousCaptureRate;
    private int lastCapturedFrame = -1;
    private bool initialized;
    private bool keyboardTest;
    private bool rigWasEnabled;

    public static string Run(string mode, string directory)
    {
        if (!Application.isPlaying || UnityEngine.SceneManagement.SceneManager.GetActiveScene().name !=
            "Ilha_Tripo_Caminhada_011")
            throw new InvalidOperationException("Start scene 011 in Play Mode.");

        if (mode is not ("preview" or "capture" or "measure" or "keys"))
            throw new ArgumentException("Use preview, capture, measure or keys.", nameof(mode));

        if (GameObject.Find("Sea study evidence"))
            throw new InvalidOperationException("The study is already running.");

        var harness = new GameObject("Sea study evidence").AddComponent<SeaStudyEvidence>();
        harness.Initialize(mode, directory);
        return $"Started {mode} in {directory}";
    }

    private void Initialize(string mode, string directory)
    {
        action = mode;
        output = directory;
        Directory.CreateDirectory(output);
        comparison = Object.FindFirstObjectByType<SeaStudyComparison>();
        previousMode = comparison.CurrentMode;
        cameraToCapture = GameObject.Find("Cais Gameplay Camera").GetComponent<Camera>();
        cameraRig = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .Single(component => component.GetType().Name == "CaisCameraRig");
        previousPosition = cameraToCapture.transform.position;
        previousRotation = cameraToCapture.transform.rotation;
        previousVSync = QualitySettings.vSyncCount;
        previousTargetRate = Application.targetFrameRate;
        previousCaptureRate = Time.captureFramerate;
        rigWasEnabled = cameraRig.enabled;
        initialized = true;
        if (action == "keys")
        {
            keyboardTest = true;
            return;
        }

        cameraRig.enabled = false;
        if (action == "measure")
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = -1;
            Time.captureFramerate = 0;
            File.WriteAllText(Path.Combine(output, "times.csv"), "pass,view,mode,frame,ms\n");
            return;
        }

        Time.captureFramerate = CaptureRate;
        var target = cameraToCapture.targetTexture;
        image = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
        Camera.onPostRender += SaveFrame;
    }

    private void LateUpdate()
    {
        if (!initialized)
            return;

        if (keyboardTest)
        {
            TestKeys();
            return;
        }

        int combination = shot % CombinationCount;
        int view = combination / VariantCount;
        int pass = shot / CombinationCount;
        int mode = (combination + pass) % VariantCount;
        if (frame == 0)
            comparison.Select((SeaStudyComparison.Mode)mode);

        float motion = action == "measure"
            ? Mathf.Clamp01((frame - WarmupFrames) / (float)BenchmarkFrames)
            : Mathf.Clamp01((frame - ClipFrames * 0.35f) / (ClipFrames * 0.65f));
        SetPose(view, motion);
        if (action != "measure")
            return;

        if (frame >= WarmupFrames)
            samples[frame - WarmupFrames] = Time.unscaledDeltaTime * 1000;
        if (++frame < WarmupFrames + BenchmarkFrames)
            return;

        string prefix = $"{pass + 1},{Views[view].Name},{comparison.CurrentMode},";
        var lines = samples.Select((value, index) =>
            prefix + index + "," + value.ToString("F4", CultureInfo.InvariantCulture));
        File.AppendAllLines(Path.Combine(output, "times.csv"), lines);
        NextShot(CombinationCount * VariantCount);
    }

    private void TestKeys()
    {
        var key = Keyboard.current ?? InputSystem.AddDevice<Keyboard>();
        int state = frame % 5;
        int mode = frame / 5;
        if (state == 0)
            InputSystem.QueueStateEvent(key, new KeyboardState((Key)((int)Key.Digit1 + mode)));
        if (state == 2 && comparison.CurrentMode != (SeaStudyComparison.Mode)mode)
        {
            Finish($"ERROR keyboard mode {mode} remained {comparison.CurrentMode}");
            return;
        }
        if (state == 3)
            InputSystem.QueueStateEvent(key, new KeyboardState());
        if (++frame == 15)
            Finish("Keyboard 1/2/3 selected Reference, Surface and Geometry.");
    }

    private void SetPose(int view, float motion)
    {
        var shot = Views[view];
        var position = shot.Position + shot.Move * motion;
        var target = shot.Target + shot.Aim * motion;
        cameraToCapture.transform.SetPositionAndRotation(position, Quaternion.LookRotation(target - position));
    }

    private void SaveFrame(Camera renderedCamera)
    {
        if (renderedCamera != cameraToCapture || Time.frameCount == lastCapturedFrame || !initialized)
            return;

        lastCapturedFrame = Time.frameCount;
        int view = shot / VariantCount;
        string path = Path.Combine(output, $"{Views[view].Name}-{comparison.CurrentMode}");
        var previous = RenderTexture.active;
        try
        {
            RenderTexture.active = cameraToCapture.targetTexture;
            image.ReadPixels(new Rect(0, 0, image.width, image.height), 0, 0, false);
            if (action == "preview")
            {
                File.WriteAllBytes(path + ".png", image.EncodeToPNG());
                NextShot(CombinationCount);
                return;
            }

            if (frame == 0)
                Directory.CreateDirectory(path);
            File.WriteAllBytes(Path.Combine(path, $"{frame:D4}.jpg"), image.EncodeToJPG(96));
            if (++frame == ClipFrames)
                NextShot(CombinationCount);
        }
        finally
        {
            RenderTexture.active = previous;
        }
    }

    private void NextShot(int count)
    {
        frame = 0;
        if (++shot == count)
            Finish($"Completed {action} ({count} shots).");
    }

    private void Finish(string message)
    {
        File.WriteAllText(Path.Combine(output, action + ".done"), message);
        initialized = false;
        Restore();
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (initialized)
            Restore();
    }

    private void Restore()
    {
        Camera.onPostRender -= SaveFrame;
        QualitySettings.vSyncCount = previousVSync;
        Application.targetFrameRate = previousTargetRate;
        Time.captureFramerate = previousCaptureRate;
        if (comparison)
            comparison.Select(previousMode);
        if (cameraRig)
            cameraRig.enabled = rigWasEnabled;
        if (cameraToCapture)
            cameraToCapture.transform.SetPositionAndRotation(previousPosition, previousRotation);
        if (image)
            Destroy(image);
    }
}
