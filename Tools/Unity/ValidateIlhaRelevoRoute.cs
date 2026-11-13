// Execute this file with Unity MCP eval only after the island scene is ready and in Play Mode.
// It creates a runtime-only helper GameObject; it never saves or modifies the scene.
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Freyja.Prototype;

[Serializable]
public sealed class IlhaRelevoRouteConfig
{
    public string mode = "route"; // "route" or "fall"
    public float defaultArrivalRadius = 1.25f;
    public float defaultWaypointTimeoutSeconds = 180f;
    public float stuckWindowSeconds = 3f;
    public float stuckProgressMeters = 0.15f;
    public float sampleIntervalSeconds = 0.5f;
    public bool requireCapturedControl = true;
    public float[] expectedRecoveryPosition;
    public float recoveryPositionTolerance = 0.75f;
    public float fallObservationTimeoutSeconds = 180f;
    public IlhaRelevoWaypoint[] waypoints;
}

[Serializable]
public sealed class IlhaRelevoWaypoint
{
    public string id;
    public float[] position;
    public float arrivalRadius;
    public float timeoutSeconds;
}

[DefaultExecutionOrder(-50)]
public sealed class IlhaRelevoRouteDriver : MonoBehaviour
{
    private CaisSessionController session;
    private CaisPlayerController player;
    private CaisCameraRig cameraRig;
    private IlhaRelevoRouteConfig config;
    private string outputPath;
    private readonly List<Sample> samples = new List<Sample>();
    private readonly List<WaypointResult> waypointResults = new List<WaypointResult>();
    private DateTime startedUtc;
    private double startedRealtime;
    private double lastSampleRealtime;
    private double waypointStartedRealtime;
    private double lastProgressRealtime;
    private float bestWaypointDistance;
    private float lastPlanarSpeed;
    private Vector3 previousPosition;
    private Vector3 runStartPosition;
    private Vector3 lastPreRecoveryPosition;
    private float routeDistance3d;
    private float routeDistancePlanar;
    private int observedRecoveryCount;
    private int sideCollisionFrames;
    private int aboveCollisionFrames;
    private int belowCollisionFrames;
    private int waypointIndex;
    private bool started;
    private bool finished;
    private bool armed;
    private string result = "waiting-for-play-mode";
    private string detail = "";

    public void Begin(string jsonPath, string resultPath)
    {
        outputPath = Path.GetFullPath(resultPath);
        try
        {
            config = JsonConvert.DeserializeObject<IlhaRelevoRouteConfig>(File.ReadAllText(jsonPath));
            if (config == null) throw new InvalidDataException("JSON de rota vazio ou inválido.");
            ValidateConfig();
            if (!EditorApplication.isPlaying) throw new InvalidOperationException("Inicie Play Mode antes de executar o harness.");
            if (SceneManager.GetActiveScene().path != "Assets/_Project/Scenes/Ilha_Relevo_004.unity")
                throw new InvalidOperationException("A cena ativa precisa ser Assets/_Project/Scenes/Ilha_Relevo_004.unity.");
            session = FindFirstObjectByType<CaisSessionController>();
            player = FindFirstObjectByType<CaisPlayerController>();
            cameraRig = FindFirstObjectByType<CaisCameraRig>();
            if (session == null || player == null || cameraRig == null)
                throw new InvalidOperationException("CaisSessionController, CaisPlayerController ou CaisCameraRig ausente.");

            startedUtc = DateTime.UtcNow;
            observedRecoveryCount = session.RecoveryCount;
            previousPosition = lastPreRecoveryPosition = player.transform.position;
            runStartPosition = player.transform.position;
            armed = true;
            result = "armed-waiting-for-captured-control";
            detail = "Clique na Game View para captura real. Rota usa direção sintética guiada; modo queda aguarda movimento manual real.";
            Debug.Log("[IlhaRelevoValidation] " + detail);
            WriteResult();
        }
        catch (Exception exception)
        {
            Fail("setup-error", exception.Message);
        }
    }

    private void ValidateConfig()
    {
        if (config.mode != "route" && config.mode != "fall")
            throw new InvalidDataException("mode deve ser 'route' ou 'fall'.");
        if (config.mode == "route" && (config.waypoints == null || config.waypoints.Length == 0))
            throw new InvalidDataException("A rota precisa conter ao menos um waypoint.");
        if (config.mode == "route")
        {
            foreach (var point in config.waypoints)
                if (point == null || string.IsNullOrWhiteSpace(point.id) || point.position == null || point.position.Length != 3)
                    throw new InvalidDataException("Cada waypoint precisa de id e position [x,y,z].");
        }
        if (config.mode == "fall" && config.expectedRecoveryPosition != null && config.expectedRecoveryPosition.Length != 3)
            throw new InvalidDataException("expectedRecoveryPosition deve ser [x,y,z] ou null.");
    }

    private void Update()
    {
        if (!armed || finished || session == null || player == null) return;
        if (!EditorApplication.isPlaying)
        {
            Finish("aborted-play-mode-ended", "Play Mode terminou antes da validação.");
            return;
        }

        // SessionController runs at -100 and reads its Input Actions first. This driver runs at -50,
        // overwriting move input only in route mode; the player's normal Update/Tick runs afterward.
        if (!started)
        {
            if (!session.IsInitialized || (config.requireCapturedControl && !session.IsCaptured)) return;
            started = true;
            startedRealtime = Time.realtimeSinceStartupAsDouble;
            lastSampleRealtime = waypointStartedRealtime = lastProgressRealtime = startedRealtime;
            previousPosition = lastPreRecoveryPosition = player.transform.position;
            runStartPosition = player.transform.position;
            observedRecoveryCount = session.RecoveryCount;
            if (config.mode == "route")
            {
                player.ControlEnabled = true;
                player.SetMoveInput(Vector2.zero);
                bestWaypointDistance = DistanceToWaypoint(config.waypoints[0]);
                result = "running-guided-route";
                detail = "Entrada direcional guiada; deslocamento, gravidade e colisão passam pelo Tick normal/CharacterController.";
            }
            else
            {
                result = "running-manual-fall-observation";
                detail = "Não injeta movimento: percorra a borda e caia usando WASD real; o harness observa a recuperação.";
            }
            Debug.Log("[IlhaRelevoValidation] " + detail);
        }

        TrackMotionAndCollisions();
        if (config.mode == "route") TickRoute();
        else TickFallObservation();
        if (Time.realtimeSinceStartupAsDouble - lastSampleRealtime >= config.sampleIntervalSeconds)
            CaptureSample();
    }

    private void TickRoute()
    {
        if (waypointIndex >= config.waypoints.Length)
        {
            player.SetMoveInput(Vector2.zero);
            Finish("route-complete", "Todos os waypoints alcançados por movimento contínuo.");
            return;
        }

        var target = config.waypoints[waypointIndex];
        Vector3 destination = ToVector3(target.position);
        Vector3 offset = destination - player.transform.position;
        float distance = Vector3.ProjectOnPlane(offset, Vector3.up).magnitude;
        float radius = target.arrivalRadius > 0f ? target.arrivalRadius : config.defaultArrivalRadius;
        if (distance <= radius)
        {
            player.SetMoveInput(Vector2.zero);
            waypointResults.Add(new WaypointResult
            {
                id = target.id,
                reached = true,
                elapsedSeconds = (float)(Time.realtimeSinceStartupAsDouble - waypointStartedRealtime),
                distanceAtArrival = distance,
                reachedPosition = Vector(player.transform.position)
            });
            Debug.Log($"[IlhaRelevoValidation] waypoint {target.id} alcançado em {waypointResults[waypointResults.Count - 1].elapsedSeconds:F1}s; restante {distance:F2}m.");
            waypointIndex++;
            if (waypointIndex < config.waypoints.Length)
            {
                waypointStartedRealtime = lastProgressRealtime = Time.realtimeSinceStartupAsDouble;
                bestWaypointDistance = DistanceToWaypoint(config.waypoints[waypointIndex]);
            }
            return;
        }

        float timeout = target.timeoutSeconds > 0f ? target.timeoutSeconds : config.defaultWaypointTimeoutSeconds;
        double now = Time.realtimeSinceStartupAsDouble;
        if (now - waypointStartedRealtime > timeout)
        {
            Fail("waypoint-timeout", $"{target.id} não alcançado em {timeout:F1}s; distância restante {distance:F2}m.");
            return;
        }
        if (distance < bestWaypointDistance - config.stuckProgressMeters)
        {
            bestWaypointDistance = distance;
            lastProgressRealtime = now;
        }
        else if (now - lastProgressRealtime > config.stuckWindowSeconds)
        {
            Fail("stuck", $"Sem progresso de {config.stuckProgressMeters:F2}m por {config.stuckWindowSeconds:F1}s em {target.id}; restante {distance:F2}m.");
            return;
        }

        Vector3 forward = Vector3.ProjectOnPlane(player.CameraTransform.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(player.CameraTransform.right, Vector3.up).normalized;
        Vector3 desired = Vector3.ProjectOnPlane(offset, Vector3.up).normalized;
        Vector2 relativeInput = new Vector2(Vector3.Dot(desired, right), Vector3.Dot(desired, forward));
        player.SetMoveInput(Vector2.ClampMagnitude(relativeInput, 1f));
    }

    private void TickFallObservation()
    {
        if (session.RecoveryCount > observedRecoveryCount)
        {
            player.SetMoveInput(Vector2.zero);
            Vector3? expected = config.expectedRecoveryPosition == null ? (Vector3?)null : ToVector3(config.expectedRecoveryPosition);
            float? error = expected.HasValue ? Vector3.Distance(player.transform.position, expected.Value) : (float?)null;
            bool nearExpected = error.HasValue && error.Value <= config.recoveryPositionTolerance;
            string outcome = expected.HasValue
                ? (nearExpected ? "fall-recovered-at-expected-spawn" : "fall-recovery-position-mismatch")
                : "fall-recovery-observed-spawn-unverified";
            Finish(outcome, $"RecoveryCount {observedRecoveryCount} → {session.RecoveryCount}; última posição antes da recuperação {Format(lastPreRecoveryPosition)}, posição após {Format(player.transform.position)}.");
            return;
        }
        lastPreRecoveryPosition = player.transform.position;
        if (Time.realtimeSinceStartupAsDouble - startedRealtime > config.fallObservationTimeoutSeconds)
            Finish("fall-not-observed-timeout", $"Nenhuma recuperação manual observada em {config.fallObservationTimeoutSeconds:F1}s.");
    }

    private void TrackMotionAndCollisions()
    {
        Vector3 current = player.transform.position;
        Vector3 delta = current - previousPosition;
        routeDistance3d += delta.magnitude;
        routeDistancePlanar += Vector3.ProjectOnPlane(delta, Vector3.up).magnitude;
        previousPosition = current;
        lastPlanarSpeed = player.PlanarVelocity.magnitude;
        CollisionFlags flags = player.Body.collisionFlags;
        if ((flags & CollisionFlags.Sides) != 0) sideCollisionFrames++;
        if ((flags & CollisionFlags.Above) != 0) aboveCollisionFrames++;
        if ((flags & CollisionFlags.Below) != 0) belowCollisionFrames++;
    }

    private void CaptureSample()
    {
        if (config == null || player == null || session == null) return;
        lastSampleRealtime = Time.realtimeSinceStartupAsDouble;
        string waypoint = config.mode == "route" && waypointIndex < config.waypoints.Length ? config.waypoints[waypointIndex].id : null;
        samples.Add(new Sample
        {
            elapsedSeconds = Elapsed,
            position = Vector(player.transform.position),
            grounded = player.Body.isGrounded,
            planarSpeed = player.PlanarVelocity.magnitude,
            verticalVelocity = player.VerticalVelocity,
            collisionFlags = player.Body.collisionFlags.ToString(),
            remainingToWaypoint = config.mode == "route" && waypointIndex < config.waypoints.Length ? DistanceToWaypoint(config.waypoints[waypointIndex]) : (float?)null,
            waypoint = waypoint,
            recoveryCount = session.RecoveryCount
        });
        WriteResult();
    }

    private float DistanceToWaypoint(IlhaRelevoWaypoint point) =>
        Vector3.ProjectOnPlane(player.transform.position - ToVector3(point.position), Vector3.up).magnitude;

    private void Finish(string outcome, string message)
    {
        if (finished) return;
        finished = true;
        result = outcome;
        detail = message;
        if (player != null) player.SetMoveInput(Vector2.zero);
        if (config != null && player != null && session != null) CaptureSample();
        WriteResult();
        Debug.Log("[IlhaRelevoValidation] " + outcome + ": " + message);
        Destroy(gameObject);
    }

    private void Fail(string outcome, string message)
    {
        if (player != null) player.SetMoveInput(Vector2.zero);
        Finish(outcome, message);
    }

    private void WriteResult()
    {
        if (string.IsNullOrEmpty(outputPath)) return;
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
        var report = new
        {
            utcStarted = startedUtc.ToString("o"),
            scene = SceneManager.GetActiveScene().path,
            mode = config == null ? null : config.mode,
            outcome = result,
            detail,
            inputCoverage = config != null && config.mode == "route" ? "guided-synthetic-direction; mouse/click/Esc/WASD controls not validated" : "manual-real-WASD-required; harness observes only",
            timeScale = Time.timeScale,
            elapsedSeconds = Elapsed,
            startPosition = player == null ? null : Vector(runStartPosition),
            endPosition = player == null ? null : Vector(player.transform.position),
            lastPositionBeforeRecovery = Vector(lastPreRecoveryPosition),
            routeDistance3dMeters = routeDistance3d,
            routeDistancePlanarMeters = routeDistancePlanar,
            collisionFlagFrames = new { sides = sideCollisionFrames, above = aboveCollisionFrames, below = belowCollisionFrames },
            recoveryCount = session == null ? (int?)null : session.RecoveryCount,
            waypointIndex,
            waypoints = waypointResults,
            fallExpectedPosition = config == null ? null : config.expectedRecoveryPosition,
            fallRecoveryPositionTolerance = config == null ? (float?)null : config.recoveryPositionTolerance,
            fallRecoveryPositionError = config != null && config.mode == "fall" && config.expectedRecoveryPosition != null && player != null
                ? (float?)Vector3.Distance(player.transform.position, ToVector3(config.expectedRecoveryPosition)) : null,
            finalPlayerGrounded = player != null && player.Body != null && player.Body.isGrounded,
            finalPlanarSpeed = lastPlanarSpeed,
            samples
        };
        File.WriteAllText(outputPath, JsonConvert.SerializeObject(report, Formatting.Indented));
    }

    private float Elapsed => started ? (float)(Time.realtimeSinceStartupAsDouble - startedRealtime) : 0f;
    private static Vector3 ToVector3(float[] v) => new Vector3(v[0], v[1], v[2]);
    private static float[] Vector(Vector3 v) => new[] { v.x, v.y, v.z };
    private static string Format(Vector3 v) => $"[{v.x:F2}, {v.y:F2}, {v.z:F2}]";

    [Serializable]
    private sealed class Sample
    {
        public float elapsedSeconds;
        public float[] position;
        public bool grounded;
        public float planarSpeed;
        public float verticalVelocity;
        public string collisionFlags;
        public float? remainingToWaypoint;
        public string waypoint;
        public int recoveryCount;
    }

    [Serializable]
    private sealed class WaypointResult
    {
        public string id;
        public bool reached;
        public float elapsedSeconds;
        public float distanceAtArrival;
        public float[] reachedPosition;
    }
}

// Run this file via Unity MCP eval. The two JSON files are external to Assets/.
var repoRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "..", ".."));
var evidenceDir = Path.Combine(repoRoot, "specs", "004-relevo-ilha", "evidence");
var configPath = Path.Combine(evidenceDir, "validation-config.json");
var runConfig = JsonConvert.DeserializeObject<IlhaRelevoRouteConfig>(File.ReadAllText(configPath));
var modeArg = runConfig == null ? "unknown" : runConfig.mode;
var outputPath = Path.Combine(evidenceDir, "percurso-" + modeArg + "-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + ".json");
var driverObject = new GameObject("Temporary Ilha Relevo Validation Driver");
UnityEngine.Object.DontDestroyOnLoad(driverObject);
var driver = driverObject.AddComponent<IlhaRelevoRouteDriver>();
driver.Begin(configPath, outputPath);
return outputPath;
