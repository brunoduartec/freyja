using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Freyja.Prototype;

public static class VerificarBlocos019
{
    static EditorApplication.CallbackFunction tick;
    public static string Main()
    {
        var p = GameObject.Find("PlayerPrototype");
        var player = p.GetComponent<CaisPlayerController>();
        var session = GameObject.Find("Cais Session").GetComponent<CaisSessionController>();
        var rig = GameObject.Find("Cais Gameplay Camera").GetComponent<CaisCameraRig>();
        var camera = rig.GetComponent<Camera>();
        var target = camera.targetTexture;
        string path = "/Users/bruno.duartec/Documents/Projects/Freyja/specs/019-blocos-ilha-nova/evidence";
        Directory.CreateDirectory(Path.Combine(path, "quadros-teste"));
        var points = new[] {new Vector2(50,5),new Vector2(46,5),new Vector2(46,2),new Vector2(46,-4),new Vector2(46,-9),new Vector2(45,-9),new Vector2(45,-11),new Vector2(44,-11),new Vector2(44,-12),new Vector2(40,-12),new Vector2(35,-12),new Vector2(35,-11),new Vector2(33,-11),new Vector2(33,-10),new Vector2(32,-10),new Vector2(32,-9),new Vector2(30,-9),new Vector2(30,-8),new Vector2(29,-8),new Vector2(29,-7),new Vector2(27,-7),new Vector2(27,-6),new Vector2(25,-6),new Vector2(25,-5),new Vector2(23,-5),new Vector2(23,-4),new Vector2(20,-4),new Vector2(20,-3),new Vector2(15,-3),new Vector2(10,-3),new Vector2(5,-3),new Vector2(1,-3),new Vector2(1,-1)};
        for (int i=0; i<points.Length; i++) points[i] *= 0.625f;
        session.enabled = false;
        player.ControlEnabled = true;
        int index = 0, frame = 0;
        float start = Time.realtimeSinceStartup, next = start;
        var tex = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
        tick = () => {
            try {
                if (!EditorApplication.isPlaying) { EditorApplication.update -= tick; return; }
                float elapsed = Time.realtimeSinceStartup - start;
                var pos = p.transform.position;
                var delta = new Vector3(points[index].x-pos.x, 0, points[index].y-pos.z);
                if (delta.magnitude < .30f && index < points.Length-1) { index++; delta = new Vector3(points[index].x-pos.x,0,points[index].y-pos.z); }
                bool done = index == points.Length-1 && delta.magnitude < 2f;
                var dir = delta.normalized;
                var f = Vector3.ProjectOnPlane(camera.transform.forward, Vector3.up).normalized;
                var r = Vector3.ProjectOnPlane(camera.transform.right, Vector3.up).normalized;
                player.SetMoveInput(done ? Vector2.zero : new Vector2(Vector3.Dot(dir,r),Vector3.Dot(dir,f)));
                if (Time.realtimeSinceStartup >= next) {
                    var previous = RenderTexture.active; RenderTexture.active = target;
                    tex.ReadPixels(new Rect(0,0,target.width,target.height),0,0); tex.Apply(); RenderTexture.active = previous;
                    File.WriteAllBytes(Path.Combine(path,"quadros-teste",frame.ToString("D5")+".jpg"),tex.EncodeToJPG(85));
                    frame++; next = Time.realtimeSinceStartup + .1f;
                }
                if (done || elapsed > 35f) {
                    player.SetMoveInput(Vector2.zero); EditorApplication.update -= tick;
                    File.WriteAllText(Path.Combine(path,"teste-rota-status.txt"),$"chegou={done}\nsegundos={elapsed:F2}\nquadros={frame}\nposicao={pos}\nponto={index}\n");
                }
            } catch (Exception e) { EditorApplication.update -= tick; File.WriteAllText(Path.Combine(path,"captura-erro.txt"),e.ToString()); }
        };
        EditorApplication.update += tick;
        return "Gravação de Play Mode iniciada";
    }
}
