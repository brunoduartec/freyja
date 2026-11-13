// Execute with Unity MCP eval while Ilha_Bruta_003 is in Play Mode.
// All keyboard, click, and mouse-look events are synthetic Input System events.
// This helper is runtime-only and does not save or edit the scene.
using System;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using Freyja.Prototype;

[DefaultExecutionOrder(-50)]
public sealed class CaisSyntheticInputProbe : MonoBehaviour
{
        private enum Phase
        {
            WaitForSession, InitialClickDown, InitialClickUp, MoveForward, MoveRelease,
            EscapeDown, EscapeUp, MoveWhileReleased, RecaptureClickDown, RecaptureClickUp,
            MouseLook, MouseRelease, Finish
        }

        private CaisSessionController session;
        private CaisPlayerController player;
        private CaisCameraRig cameraRig;
        private Phase phase;
        private double phaseStarted;
        private double startedAt;
        private string outputPath;
        private bool initiallyCaptured;
        private bool initiallyFocused;
        private bool syntheticFocusCallback;
        private bool initialClickCaptured;
        private bool recaptureClickCaptured;
        private bool directCaptureFallback;
        private bool escapeReleasedBySyntheticKey;
        private bool releaseStoppedPlayer;
        private bool moveWasObserved;
        private bool lookWasObserved;
        private string outcome = "waiting";
        private string error;
        private Vector3 moveStart;
        private Vector3 moveEnd;
        private Vector3 releasedMoveStart;
        private Vector3 releasedMoveEnd;
        private Vector3 positionAfterUncapturedW;
        private float startYaw;
        private float endYaw;
        private float startPitch;
        private float endPitch;
        private double moveDurationSeconds = 0.8;
        private double releasedMoveDurationSeconds = 0.35;
        private double lookDurationSeconds = 0.6;
        private int recoveryCountAtStart;

        public void Begin(string resultPath)
        {
            outputPath = Path.GetFullPath(resultPath);
            try
            {
                if (!EditorApplication.isPlaying) throw new InvalidOperationException("Inicie Play Mode antes do eval.");
                if (SceneManager.GetActiveScene().path != "Assets/_Project/Scenes/Ilha_Bruta_003.unity")
                    throw new InvalidOperationException("A cena ativa precisa ser Assets/_Project/Scenes/Ilha_Bruta_003.unity.");
                session = UnityEngine.Object.FindFirstObjectByType<CaisSessionController>();
                player = UnityEngine.Object.FindFirstObjectByType<CaisPlayerController>();
                cameraRig = UnityEngine.Object.FindFirstObjectByType<CaisCameraRig>();
                if (session == null || player == null || cameraRig == null)
                    throw new InvalidOperationException("Session, player ou camera rig não encontrado.");
                if (Keyboard.current == null || Mouse.current == null)
                    throw new InvalidOperationException("Keyboard ou Mouse indisponível no Input System.");

                initiallyCaptured = session.IsCaptured;
                initiallyFocused = session.HasFocus;
                recoveryCountAtStart = session.RecoveryCount;
                startedAt = phaseStarted = Time.realtimeSinceStartupAsDouble;
                phase = Phase.WaitForSession;
                outcome = "waiting-for-session-initialization";
                WriteResult();
            }
            catch (Exception exception)
            {
                error = exception.ToString();
                outcome = "setup-error";
                WriteResult();
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            if (phase == Phase.Finish) return;
            if (!EditorApplication.isPlaying)
            {
                Complete("aborted-play-mode-ended");
                return;
            }
            if (session == null || player == null || cameraRig == null) return;

            // SessionController (-100) reads Input Actions before this helper (-50).
            // Queued events are processed by Unity's normal Input System update next frame.
            switch (phase)
            {
                case Phase.WaitForSession:
                    if (!session.IsInitialized) return;
                    if (!session.HasFocus)
                    {
                        session.SendMessage("OnApplicationFocus", true, SendMessageOptions.DontRequireReceiver);
                        syntheticFocusCallback = true;
                    }
                    SetPhase(Phase.InitialClickDown);
                    break;
                case Phase.InitialClickDown:
                    QueueMouseButton(true);
                    if (PhaseElapsed >= 0.12) SetPhase(Phase.InitialClickUp);
                    break;
                case Phase.InitialClickUp:
                    QueueMouseButton(false);
                    initialClickCaptured |= session.IsCaptured;
                    if (PhaseElapsed >= 0.12)
                    {
                        if (!session.IsCaptured)
                        {
                            session.CaptureControl();
                            directCaptureFallback = session.IsCaptured;
                        }
                        if (!session.IsCaptured)
                        {
                            Complete("capture-unavailable");
                            return;
                        }
                        moveStart = player.transform.position;
                        recoveryCountAtStart = session.RecoveryCount;
                        SetPhase(Phase.MoveForward);
                    }
                    break;
                case Phase.MoveForward:
                    QueueKeyboard(Key.W);
                    if (PhaseElapsed >= moveDurationSeconds) SetPhase(Phase.MoveRelease);
                    break;
                case Phase.MoveRelease:
                    QueueKeyboard();
                    if (PhaseElapsed >= 0.15)
                    {
                        moveEnd = player.transform.position;
                        moveWasObserved = Vector3.Distance(moveStart, moveEnd) > 0.05f;
                        QueueKeyboard(Key.Escape);
                        SetPhase(Phase.EscapeDown);
                    }
                    break;
                case Phase.EscapeDown:
                    QueueKeyboard(Key.Escape);
                    if (PhaseElapsed >= 0.12) SetPhase(Phase.EscapeUp);
                    break;
                case Phase.EscapeUp:
                    QueueKeyboard();
                    if (PhaseElapsed >= 0.12)
                    {
                        escapeReleasedBySyntheticKey = !session.IsCaptured;
                        if (session.IsCaptured)
                        {
                            // Fallback keeps the remaining probes safe; result records that Esc failed.
                            session.ReleaseControl();
                        }
                        releasedMoveStart = player.transform.position;
                        SetPhase(Phase.MoveWhileReleased);
                    }
                    break;
                case Phase.MoveWhileReleased:
                    QueueKeyboard(Key.W);
                    if (PhaseElapsed >= releasedMoveDurationSeconds)
                    {
                        QueueKeyboard();
                        SetPhase(Phase.RecaptureClickDown);
                    }
                    break;
                case Phase.RecaptureClickDown:
                    QueueKeyboard();
                    QueueMouseButton(true);
                    if (PhaseElapsed >= 0.12) SetPhase(Phase.RecaptureClickUp);
                    break;
                case Phase.RecaptureClickUp:
                    QueueMouseButton(false);
                    recaptureClickCaptured |= session.IsCaptured;
                    if (PhaseElapsed >= 0.12)
                    {
                        releasedMoveEnd = player.transform.position;
                        positionAfterUncapturedW = releasedMoveEnd;
                        releaseStoppedPlayer = escapeReleasedBySyntheticKey && Vector3.Distance(releasedMoveStart, releasedMoveEnd) < 0.05f;
                        if (!session.IsCaptured)
                        {
                            session.CaptureControl();
                            directCaptureFallback |= session.IsCaptured;
                        }
                        if (!session.IsCaptured)
                        {
                            Complete("recapture-unavailable");
                            return;
                        }
                        startYaw = cameraRig.Yaw;
                        startPitch = cameraRig.Pitch;
                        SetPhase(Phase.MouseLook);
                    }
                    break;
                case Phase.MouseLook:
                    QueueMouseDelta(new Vector2(8f, -2f));
                    if (PhaseElapsed >= lookDurationSeconds) SetPhase(Phase.MouseRelease);
                    break;
                case Phase.MouseRelease:
                    QueueMouseDelta(Vector2.zero);
                    if (PhaseElapsed >= 0.15)
                    {
                        endYaw = cameraRig.Yaw;
                        endPitch = cameraRig.Pitch;
                        lookWasObserved = Mathf.Abs(Mathf.DeltaAngle(startYaw, endYaw)) > 0.1f || Mathf.Abs(endPitch - startPitch) > 0.1f;
                        Complete("completed");
                    }
                    break;
            }
        }

        private void SetPhase(Phase next)
        {
            phase = next;
            phaseStarted = Time.realtimeSinceStartupAsDouble;
        }

        private double PhaseElapsed => Time.realtimeSinceStartupAsDouble - phaseStarted;

        private static void QueueKeyboard(params Key[] keys)
        {
            if (Keyboard.current == null) return;
            InputSystem.QueueStateEvent(Keyboard.current, keys.Length == 0 ? new KeyboardState() : new KeyboardState(keys));
        }

        private static void QueueMouseButton(bool pressed)
        {
            if (Mouse.current == null) return;
            var state = new MouseState();
            if (pressed) state = state.WithButton(MouseButton.Left);
            InputSystem.QueueStateEvent(Mouse.current, state);
        }

        private static void QueueMouseDelta(Vector2 delta)
        {
            if (Mouse.current != null) InputSystem.QueueDeltaStateEvent(Mouse.current.delta, delta);
        }

        private void Complete(string finalOutcome)
        {
            if (phase == Phase.Finish) return;
            phase = Phase.Finish;
            outcome = finalOutcome;
            QueueKeyboard();
            QueueMouseButton(false);
            if (session != null && !initiallyCaptured && session.IsCaptured) session.ReleaseControl();
            else if (session != null && initiallyCaptured && !session.IsCaptured) session.CaptureControl();
            if (session != null && session.HasFocus != initiallyFocused)
                session.SendMessage("OnApplicationFocus", initiallyFocused, SendMessageOptions.DontRequireReceiver);
            if (player != null) player.SetMoveInput(Vector2.zero);
            WriteResult();
            Destroy(gameObject);
        }

        private void WriteResult()
        {
            if (string.IsNullOrEmpty(outputPath)) return;
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            var report = new
            {
                utc = DateTime.UtcNow.ToString("o"),
                scene = SceneManager.GetActiveScene().path,
                outcome,
                error,
                inputNature = "SYNTHETIC: KeyboardState/MouseState/QueueDeltaStateEvent through InputSystem; not physical keyboard, mouse, click, or OS focus.",
                syntheticFocusCallback,
                initialClickCaptured,
                directCaptureFallback,
                recaptureClickCaptured,
                escapeReleasedBySyntheticKey,
                releaseStoppedPlayer,
                moveWasObserved,
                lookWasObserved,
                initialCaptured = initiallyCaptured,
                initialFocus = initiallyFocused,
                capturedAtEnd = session != null && session.IsCaptured,
                focusAtEnd = session != null && session.HasFocus,
                recoveryCountAtStart,
                recoveryCountAtEnd = session == null ? (int?)null : session.RecoveryCount,
                moveDurationSeconds,
                movedPlanarMeters = Vector3.ProjectOnPlane(moveEnd - moveStart, Vector3.up).magnitude,
                positionAfterUncapturedW = new[] { positionAfterUncapturedW.x, positionAfterUncapturedW.y, positionAfterUncapturedW.z },
                releasedPhasePlanarDeltaMeters = Vector3.ProjectOnPlane(releasedMoveEnd - releasedMoveStart, Vector3.up).magnitude,
                yawBefore = startYaw,
                yawAfter = endYaw,
                pitchBefore = startPitch,
                pitchAfter = endPitch,
                elapsedSeconds = Time.realtimeSinceStartupAsDouble - startedAt,
                timeScale = Time.timeScale
            };
            File.WriteAllText(outputPath, JsonConvert.SerializeObject(report, Formatting.Indented));
        }
}

if (!EditorApplication.isPlaying) throw new InvalidOperationException("Inicie Play Mode antes do eval.");
var outputRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "specs", "003-ilha-bruta", "evidence"));
var output = Path.Combine(outputRoot, "synthetic-input-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + ".json");
var probeObject = new GameObject("Temporary Cais Synthetic Input Probe");
UnityEngine.Object.DontDestroyOnLoad(probeObject);
var probe = probeObject.AddComponent<CaisSyntheticInputProbe>();
probe.Begin(output);
return output;
