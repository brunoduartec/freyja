using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

var camera = Camera.main;
if (camera == null) throw new InvalidOperationException("Camera principal ausente.");
var renderers = UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);
var missing = renderers.Count(r => r.sharedMaterials.Any(m => m == null || m.shader == null || m.shader.name == "Hidden/InternalErrorShader"));
Physics.SyncTransforms();
bool Probe(Vector3 origin) => Physics.Raycast(origin, Vector3.down, out var hit, 20f) && hit.collider != null;
var dockHit = Probe(new Vector3(0f, 5f, 7f));
var pathHit = Probe(new Vector3(0f, 5f, -6f));
var rt = new RenderTexture(1280, 960, 24, RenderTextureFormat.ARGB32);
var image = new Texture2D(1280, 960, TextureFormat.RGB24, false);
var previous = RenderTexture.active;
var output = Path.GetFullPath("../Source/Unity/Cais_Iteracao02.png");
try
{
    camera.targetTexture = rt;
    camera.Render();
    RenderTexture.active = rt;
    image.ReadPixels(new Rect(0, 0, 1280, 960), 0, 0);
    image.Apply();
    File.WriteAllBytes(output, image.EncodeToPNG());
}
finally
{
    camera.targetTexture = null;
    RenderTexture.active = previous;
    UnityEngine.Object.DestroyImmediate(rt);
    UnityEngine.Object.DestroyImmediate(image);
}
return "scene=" + SceneManager.GetActiveScene().path + "; playMode=" + EditorApplication.isPlaying
    + "; renderers=" + renderers.Length + "; missingMaterials=" + missing
    + "; dockRaycast=" + dockHit + "; pathRaycast=" + pathHit + "; preview=" + output;
