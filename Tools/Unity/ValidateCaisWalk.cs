// Verificacoes controladas em Play Mode; complementam o percurso e foco reais.
using System;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEditor;
using Freyja.Prototype;
using Newtonsoft.Json;

if(!EditorApplication.isPlaying)throw new InvalidOperationException("Executar em Play Mode.");
var p=UnityEngine.Object.FindFirstObjectByType<CaisPlayerController>();
var c=UnityEngine.Object.FindFirstObjectByType<CaisCameraRig>();
var s=UnityEngine.Object.FindFirstObjectByType<CaisSessionController>();
bool initialized=s.IsInitialized;
var start=new Vector3(0,.945f,6.5f);
s.ReleaseControl();
c.ResetView(180);
float Travel(Vector2 input) {
 p.ResetTo(start,Quaternion.Euler(0,180,0));Physics.SyncTransforms();p.ControlEnabled=true;p.SetMoveInput(input);
 for(int i=0;i<10;i++)p.Tick(.02f);
 return Vector3.ProjectOnPlane(p.transform.position-start,Vector3.up).magnitude;
}
float axial=Travel(Vector2.up),diagonal=Travel(Vector2.one);
bool sameSpeed=Mathf.Abs(axial-diagonal)<.015f&&Mathf.Abs(axial-.6f)<.03f;

// Estado sintetico de tecla pressionada + callback de foco; janela real testada separadamente.
InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState(Key.W));InputSystem.Update();
s.SendMessage("OnApplicationFocus",false);
var stoppedAt=p.transform.position;
s.SendMessage("Update");p.Tick(.1f);
bool heldKeyStopped=!s.IsCaptured&&p.PlanarVelocity.sqrMagnitude<.00001f;
s.SendMessage("OnApplicationFocus",true);s.SendMessage("Update");
bool explicitResume=!s.IsCaptured;
InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState());InputSystem.Update();

int before=s.RecoveryCount;
p.ResetTo(new Vector3(30,1,0),Quaternion.identity);s.SendMessage("LateUpdate");
bool boundsReset=s.RecoveryCount==before+1&&Vector3.Distance(p.transform.position,new Vector3(0,.94f,7.5f))<.12f&&Mathf.Abs(p.VerticalVelocity)<.01f;

// Colocar o boom diante de solidos existentes e verificar volume da camera/near plane.
p.ResetTo(new Vector3(.8f,.945f,-2.5f),Quaternion.Euler(0,180,0));Physics.SyncTransforms();
c.ResetView(180);c.ControlEnabled=true;c.SetLookInput(new Vector2(0,1000));c.Tick(.5f);
int mask=LayerMask.GetMask("CaisSolid");
bool cameraClear=!Physics.CheckSphere(c.transform.position,.18f,mask,QueryTriggerInteraction.Ignore);
var view=c.ViewCamera;float near=view.nearClipPlane;float hh=near*Mathf.Tan(view.fieldOfView*.5f*Mathf.Deg2Rad);
bool nearClear=!Physics.CheckBox(c.transform.position+c.transform.forward*near,new Vector3(hh*view.aspect,hh,.004f),c.transform.rotation,mask,QueryTriggerInteraction.Ignore);
var blocker=GameObject.CreatePrimitive(PrimitiveType.Cube);blocker.name="TemporaryCameraProbe";blocker.layer=LayerMask.NameToLayer("CaisSolid");
blocker.transform.position=p.transform.position+Vector3.up*1.35f;blocker.transform.localScale=Vector3.one*.5f;Physics.SyncTransforms();
c.Tick(.02f);
bool blockedPivotClear=!Physics.CheckSphere(c.transform.position,.18f,mask,QueryTriggerInteraction.Ignore);
UnityEngine.Object.DestroyImmediate(blocker);Physics.SyncTransforms();

s.Recover();s.ReleaseControl();s.SendMessage("OnApplicationFocus",Application.isFocused);
var result=new { utc=DateTime.UtcNow.ToString("o"), initialized,axialDistance=axial,diagonalDistance=diagonal,sameSpeed,heldKeyStopped,explicitResume,boundsReset,cameraClear,nearClear,blockedPivotClear,
 note="Controlados via MCP; percurso, queda e troca real de janela registrados separadamente."};
var json=JsonConvert.SerializeObject(result,Formatting.Indented);
File.WriteAllText(Path.GetFullPath("../specs/002-caminhada-no-cais/evidence/controlled-validation.json"),json);
return json;
