using System.Collections;
using System.IO;
using UnityEngine;

namespace Freyja.SmartObjects.Demo
{
    public sealed class ChairDemoFrameCapture : MonoBehaviour
    {
        [SerializeField] private Camera captureCamera;
        [SerializeField] private string frameDirectory;
        [SerializeField, Min(1)] private int framesPerSecond = 24;
        [SerializeField, Min(1)] private int frameCount = 192;
        [SerializeField, Min(16)] private int width = 1280;
        [SerializeField, Min(16)] private int height = 720;
        [SerializeField] private bool recordOnStart;

        private void Start()
        {
            if (recordOnStart) StartCoroutine(CaptureSequence());
        }

        private IEnumerator CaptureSequence()
        {
            if (captureCamera == null || string.IsNullOrWhiteSpace(frameDirectory))
            {
                Debug.LogError("[ChairVideo] Missing capture camera or frame directory.", this);
                yield break;
            }

            Directory.CreateDirectory(frameDirectory);
            var renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            var frame = new Texture2D(width, height, TextureFormat.RGB24, false);
            RenderTexture previousTarget = captureCamera.targetTexture;
            RenderTexture previousActive = RenderTexture.active;
            int previousCaptureFramerate = Time.captureFramerate;
            captureCamera.targetTexture = renderTexture;
            Time.captureFramerate = framesPerSecond;

            for (int i = 0; i < frameCount; i++)
            {
                yield return new WaitForEndOfFrame();
                captureCamera.Render();
                RenderTexture.active = renderTexture;
                frame.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);
                frame.Apply(false, false);
                File.WriteAllBytes(Path.Combine(frameDirectory, $"frame_{i:D5}.png"), frame.EncodeToPNG());
            }

            captureCamera.targetTexture = previousTarget;
            RenderTexture.active = previousActive;
            Time.captureFramerate = previousCaptureFramerate;
            Destroy(renderTexture);
            Destroy(frame);
            recordOnStart = false;
            Debug.Log($"[ChairVideo] Captured {frameCount} frames at {framesPerSecond} fps in {frameDirectory}.", this);
        }
    }
}
