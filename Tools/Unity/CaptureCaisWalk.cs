using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using Freyja.Prototype;
using Newtonsoft.Json;

if(!EditorApplication.isPlaying)throw new InvalidOperationException("Capturar somente durante Play Mode.");
var session=UnityEngine.Object.FindFirstObjectByType<CaisSessionController>();
var player=UnityEngine.Object.FindFirstObjectByType<CaisPlayerController>();
var camera=UnityEngine.Object.FindFirstObjectByType<CaisCameraRig>();
string directory=Path.GetFullPath("../specs/002-caminhada-no-cais/evidence");
Directory.CreateDirectory(directory);
string label="walk-"+Time.frameCount;
string output=Path.Combine(directory,label+".png");
ScreenCapture.CaptureScreenshot(output);
var data=new { utc=DateTime.UtcNow.ToString("o"), frame=Time.frameCount, scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene().path,
 initialized=session.IsInitialized, focused=session.HasFocus, captured=session.IsCaptured, recoveries=session.RecoveryCount,
 player=new[]{player.transform.position.x,player.transform.position.y,player.transform.position.z},
 camera=new[]{camera.transform.position.x,camera.transform.position.y,camera.transform.position.z}, yaw=camera.Yaw,pitch=camera.Pitch,
 grounded=player.Body.isGrounded,positionVelocity=player.PlanarVelocity.magnitude,verticalVelocity=player.VerticalVelocity };
File.WriteAllText(Path.Combine(directory,label+".json"),JsonConvert.SerializeObject(data,Formatting.Indented));
return output;
