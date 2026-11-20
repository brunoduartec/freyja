// Executado pelo MCP Unity. As mudanças são temporárias e restauradas no finally.
var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
if (scene.path != "Assets/_Project/Scenes/Ilha_Tripo_Caminhada_011.unity" || scene.isDirty || UnityEditor.EditorApplication.isPlaying)
    throw new System.Exception("Cena incorreta, alterada ou em Play Mode; experimento cancelado.");
var tempAsset = "Assets/_FreyjaExperimentoRampa.shader";
if (System.IO.File.Exists(tempAsset)) throw new System.Exception("Arquivo temporário já existe; não sobrescrever.");
var player = UnityEngine.GameObject.Find("PlayerPrototype").transform;
var camera = UnityEngine.Camera.main;
var sun = UnityEngine.GameObject.Find("Coastal Sun").GetComponent<UnityEngine.Light>();
var galpao = UnityEngine.GameObject.Find("Galpão do cais e píer");
var renderers = player.GetComponentsInChildren<UnityEngine.Renderer>().Concat(galpao.GetComponentsInChildren<UnityEngine.Renderer>()).ToArray();
var originals = renderers.Select(r => r.sharedMaterials).ToArray();
var copies = new System.Collections.Generic.Dictionary<UnityEngine.Material, UnityEngine.Material>();
var pp = player.position; var pr = player.rotation; var cp = camera.transform.position; var cr = camera.transform.rotation;
var fov = camera.fieldOfView; var target = camera.targetTexture; var active = UnityEngine.RenderTexture.active;
var sunColor = sun.color; var sunIntensity = sun.intensity;
var sky = UnityEngine.RenderSettings.ambientSkyColor; var equator = UnityEngine.RenderSettings.ambientEquatorColor; var ground = UnityEngine.RenderSettings.ambientGroundColor;
var outdir = "../specs/014-renderer-pixelado/";
try {
    System.IO.File.Copy(outdir + "ExperimentoRampa.shader", tempAsset);
    UnityEditor.AssetDatabase.ImportAsset(tempAsset, UnityEditor.ImportAssetOptions.ForceSynchronousImport);
    var shader = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Shader>(tempAsset);
    if (shader == null || !shader.isSupported || UnityEditor.ShaderUtil.ShaderHasError(shader)) throw new System.Exception("Falha de compilação do shader experimental.");
    player.position = new UnityEngine.Vector3(-3.35f, 1.84f, 22.65f);
    player.rotation = UnityEngine.Quaternion.Euler(0,110,0);
    camera.transform.position = new UnityEngine.Vector3(-0.9f,3.35f,24.7f);
    camera.transform.LookAt(new UnityEngine.Vector3(-4.05f,2.85f,21.7f));
    camera.fieldOfView = 42f;
    for (int variant = 0; variant < 3; variant++) {
        if (variant == 1) {
            sun.color = new UnityEngine.Color(1f,0.90f,0.72f); sun.intensity = 1.45f;
            UnityEngine.RenderSettings.ambientSkyColor = new UnityEngine.Color(0.78f,0.76f,0.83f);
            UnityEngine.RenderSettings.ambientEquatorColor = new UnityEngine.Color(0.68f,0.62f,0.68f);
            UnityEngine.RenderSettings.ambientGroundColor = new UnityEngine.Color(0.52f,0.47f,0.54f);
            UnityEngine.DynamicGI.UpdateEnvironment();
        }
        if (variant == 2) {
            foreach (var r in renderers) {
                var a = r.sharedMaterials;
                for (int i=0;i<a.Length;i++) {
                    var m=a[i]; if (m==null) continue;
                    if (!copies.ContainsKey(m)) {
                        var copy = new UnityEngine.Material(shader);
                        copy.mainTexture = m.mainTexture;
                        copy.mainTextureScale = m.mainTextureScale; copy.mainTextureOffset = m.mainTextureOffset;
                        if (m.HasProperty("_Color")) copy.color = m.color;
                        copies[m]=copy;
                    }
                    a[i]=copies[m];
                }
                r.sharedMaterials=a;
            }
        }
        foreach (int width in new int[]{320,480,640}) {
            int height=width*9/16;
            var rt = new UnityEngine.RenderTexture(width,height,24); rt.antiAliasing=1; rt.filterMode=UnityEngine.FilterMode.Point;
            var tex = new UnityEngine.Texture2D(width,height,UnityEngine.TextureFormat.RGB24,false);
            try { camera.targetTexture=rt; camera.Render(); UnityEngine.RenderTexture.active=rt; tex.ReadPixels(new UnityEngine.Rect(0,0,width,height),0,0); tex.Apply(); System.IO.File.WriteAllBytes(outdir + "calibracao-clara-" + "ABC"[variant] + "-" + width + ".png",tex.EncodeToPNG()); }
            finally { camera.targetTexture=target; UnityEngine.RenderTexture.active=active; rt.Release(); UnityEngine.Object.DestroyImmediate(rt); UnityEngine.Object.DestroyImmediate(tex); }
        }
    }
} finally {
    for (int i=0;i<renderers.Length;i++) renderers[i].sharedMaterials=originals[i];
    foreach (var m in copies.Values) UnityEngine.Object.DestroyImmediate(m);
    player.position=pp; player.rotation=pr; camera.transform.position=cp; camera.transform.rotation=cr; camera.fieldOfView=fov; camera.targetTexture=target; UnityEngine.RenderTexture.active=active;
    sun.color=sunColor; sun.intensity=sunIntensity; UnityEngine.RenderSettings.ambientSkyColor=sky; UnityEngine.RenderSettings.ambientEquatorColor=equator; UnityEngine.RenderSettings.ambientGroundColor=ground; UnityEngine.DynamicGI.UpdateEnvironment();
    UnityEditor.AssetDatabase.DeleteAsset(tempAsset);
}
return new {scene=scene.path,dirty=scene.isDirty,variants="A Standard original; B Standard luz quente e ambiente violeta; C B com rampa no protagonista e galpão", resolutions="320x180, 480x270, 640x360"};
