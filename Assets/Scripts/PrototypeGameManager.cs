using UnityEngine;

public class PrototypeGameManager : MonoBehaviour
{
    private enum RoundState
    {
        Playing,
        Hit,
        Miss
    }

    [SerializeField] private Transform player;
    [SerializeField] private Transform target;

    [Header("Scripts")]
    [SerializeField] private CameraFollow cameraFollow;
    [SerializeField] private PlayerRunner playerRunner;
    [SerializeField] private PlayerImpactLauncher playerImpactLauncher;
    [SerializeField] private TargetFlightTracker targetFlightTracker;
    [SerializeField] private TargetWalker targetWalker;
    [SerializeField] private TargetFlightPoseController targetFlightPoseController;

    [Header("GUI")]
    [SerializeField] private Color guiTextColor = Color.black;
    [SerializeField] private int guiFontSize = 100;

    [Header("Miss")]
    [SerializeField] private float passMissDistance = 5f;

    private RoundState roundState = RoundState.Playing;

    private GUIStyle labelStyle;

    private Rigidbody playerRb;
    private Rigidbody targetRb;

    private Vector3 playerStartPosition;
    private Quaternion playerStartRotation;

    private Vector3 targetStartPosition;
    private Quaternion targetStartRotation;

    private void Awake()
    {
        playerRb = player.GetComponent<Rigidbody>();
        targetRb = target.GetComponent<Rigidbody>();

        if (playerRunner == null) {
            playerRunner = player.GetComponent<PlayerRunner>();
        }

        if (playerImpactLauncher == null) {
            playerImpactLauncher = player.GetComponent<PlayerImpactLauncher>();
        }

        if (targetWalker == null) {
            targetWalker = target.GetComponent<TargetWalker>();
        }

        if (targetFlightPoseController == null) {
            targetFlightPoseController = target.GetComponent<TargetFlightPoseController>();
        }

        playerStartPosition = player.position;
        playerStartRotation = player.rotation;

        targetStartPosition = target.position;
        targetStartRotation = target.rotation;

        roundState = RoundState.Playing;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.R)) {
            ResetScene();
        }

        UpdateRoundState();
    }

    private void UpdateRoundState()
    {
        if (roundState != RoundState.Playing) {
            return;
        }

        if (playerImpactLauncher != null && playerImpactLauncher.HasLaunched) {
            roundState = RoundState.Hit;
            return;
        }

        if (HasTargetPassedPlayer()) {
            SetMiss();
            return;
        }

        if (targetWalker != null && targetWalker.HasFinishedWalk) {
            SetMiss();
        }
    }

    private bool HasTargetPassedPlayer()
    {
        if (targetWalker == null) {
            return false;
        }

        if (!targetWalker.IsWalking) {
            return false;
        }

        Vector3 moveDirection = targetWalker.CurrentMoveDirection;
        moveDirection.y = 0f;

        if (moveDirection.sqrMagnitude < 0.001f) {
            return false;
        }

        moveDirection.Normalize();

        Vector3 fromPlayerToTarget = target.position - player.position;
        fromPlayerToTarget.y = 0f;

        float signedDistanceAlongMoveDirection = Vector3.Dot(
            fromPlayerToTarget,
            moveDirection
        );

        return signedDistanceAlongMoveDirection > passMissDistance;
    }

    private void SetMiss()
    {
        roundState = RoundState.Miss;

        if (cameraFollow != null) {
            cameraFollow.FollowPlayer(true);
        }

        Debug.Log("Miss");
    }

    private void OnGUI()
    {
        if (labelStyle == null) {
            labelStyle = new GUIStyle(GUI.skin.label);
        }
        labelStyle.normal.textColor = guiTextColor;
        labelStyle.fontSize = guiFontSize;

        if (targetFlightTracker == null) {
            GUI.Label(new Rect(10, 10, 500, 30), "TargetFlightTracker is not assigned.");
            return;
        }

        if (roundState == RoundState.Playing) {
            GUI.Label(new Rect(10, 10, 500, 30), "Playing", labelStyle);
            GUI.Label(new Rect(10, 35, 800, 30), "W/S/A/D: Move, Space: Jump, R: Reset", labelStyle);
            return;
        }

        if (roundState == RoundState.Miss) {
            GUI.Label(new Rect(10, 10, 700, 30), "MISS!!", labelStyle);
            GUI.Label(new Rect(10, 35, 900, 30), "R: Reset", labelStyle);
            return;
        }

        float displayDistance = targetFlightTracker.IsLanded
            ? targetFlightTracker.FinalDistance
            : targetFlightTracker.CurrentDistance;

        GUI.Label(new Rect(10, 10, 700, 30), "HIT!!", labelStyle);
        GUI.Label(new Rect(10, 35, 700, 30), $"Distance: {displayDistance:F2} m", labelStyle);
        GUI.Label(new Rect(10, 60, 700, 30), $"Max Height: {targetFlightTracker.MaxHeight:F2} m", labelStyle);
        GUI.Label(new Rect(10, 85, 700, 30), $"Flight Time: {targetFlightTracker.FlightTime:F2} s", labelStyle);
        GUI.Label(new Rect(10, 110, 700, 30), $"Rotations: {targetFlightTracker.RotationCount:F1}", labelStyle);
        GUI.Label(new Rect(10, 135, 900, 30), "R: Reset", labelStyle);
    }

    private void ResetScene()
    {
        ResetBody(player, playerRb, playerStartPosition, playerStartRotation);
        ResetBody(target, targetRb, targetStartPosition, targetStartRotation);

        if (playerRunner != null) {
            playerRunner.ResetMoveState();
        }

        if (playerImpactLauncher != null) {
            playerImpactLauncher.ResetImpactState();
        }

        if (cameraFollow != null) {
            cameraFollow.FollowPlayer(true);
        }

        if (targetFlightTracker != null) {
            targetFlightTracker.ResetStats();
        }

        if (targetFlightPoseController != null) {
            targetFlightPoseController.ResetPose();
        }

        if (targetWalker != null) {
            targetWalker.ResetWalkState();
        }

        roundState = RoundState.Playing;
    }

    private void ResetBody(Transform bodyTransform, Rigidbody bodyRb, Vector3 position, Quaternion rotation)
    {
        bodyRb.linearVelocity = Vector3.zero;
        bodyRb.angularVelocity = Vector3.zero;

        bodyRb.position = position;
        bodyRb.rotation = rotation;

        bodyTransform.SetPositionAndRotation(position, rotation);
    }

    private float GetHorizontalDistance(Vector3 from, Vector3 to)
    {
        Vector2 a = new Vector2(from.x, from.z);
        Vector2 b = new Vector2(to.x, to.z);

        return Vector2.Distance(a, b);
    }
}
