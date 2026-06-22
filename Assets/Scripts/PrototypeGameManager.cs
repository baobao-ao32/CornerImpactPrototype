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
    [SerializeField] private ResultPresenter resultPresenter;

    [Header("Prototype Score")]
    [Tooltip("飛距離1mあたりの得点")]
    [SerializeField] private float distanceScorePerMeter = 100f;

    [Tooltip("最高到達点1mあたりのボーナス")]
    [SerializeField] private float heightScorePerMeter = 50f;

    [Tooltip("滞空時間1秒あたりのボーナス")]
    [SerializeField] private float flightTimeScorePerSecond = 100f;

    [Tooltip("1回転あたりのボーナス")]
    [SerializeField] private float rotationScorePerTurn = 500f;

    [Header("Miss")]
    [SerializeField] private float passMissDistance = 5f;

    private RoundState roundState = RoundState.Playing;

    private Rigidbody playerRb;
    private Rigidbody targetRb;

    private Vector3 playerStartPosition;
    private Quaternion playerStartRotation;

    private Vector3 targetStartPosition;
    private Quaternion targetStartRotation;

    private int penaltyPoints;
    private bool resultShown;

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
        penaltyPoints = 0;
        resultShown = false;

        resultPresenter?.HideImmediate();
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.R)) {
            ResetScene();
        }

        UpdateRoundState();
        UpdateResultUI();
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

    private void UpdateResultUI()
    {
        if (resultShown || resultPresenter == null) {
            return;
        }

        if (roundState != RoundState.Hit) {
            return;
        }

        if (targetFlightTracker == null || !targetFlightTracker.IsLanded) {
            return;
        }

        ResultData result = new ResultData(
            targetFlightTracker.FinalDistance,
            targetFlightTracker.MaxHeight,
            targetFlightTracker.FlightTime,
            targetFlightTracker.RotationCount,
            penaltyPoints,
            CalculateFinalScore()
        );

        resultPresenter.ShowResult(result);
        resultShown = true;
    }

    private bool HasTargetPassedPlayer()
    {
        if (targetWalker == null || !targetWalker.IsWalking) {
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

        cameraFollow?.FollowPlayer(true);

        if (resultPresenter != null) {
            resultPresenter.ShowMiss();
            resultShown = true;
        }

        Debug.Log("Miss");
    }

    private int CalculateFinalScore()
    {
        if (targetFlightTracker == null) {
            return 0;
        }

        float rawScore =
            targetFlightTracker.FinalDistance * distanceScorePerMeter +
            targetFlightTracker.MaxHeight * heightScorePerMeter +
            targetFlightTracker.FlightTime * flightTimeScorePerSecond +
            targetFlightTracker.RotationCount * rotationScorePerTurn;

        int roundedScore = Mathf.RoundToInt(rawScore);
        return Mathf.Max(0, roundedScore - penaltyPoints);
    }

    /// <summary>
    /// 将来、通行人や障害物への接触から呼ぶ。
    /// </summary>
    public void AddPenalty(int points)
    {
        penaltyPoints += Mathf.Max(0, points);
    }

    private void ResetScene()
    {
        ResetBody(player, playerRb, playerStartPosition, playerStartRotation);
        ResetBody(target, targetRb, targetStartPosition, targetStartRotation);

        playerRunner?.ResetMoveState();
        playerImpactLauncher?.ResetImpactState();
        cameraFollow?.FollowPlayer(true);
        targetFlightTracker?.ResetStats();
        targetFlightPoseController?.ResetPose();
        targetWalker?.ResetWalkState();
        resultPresenter?.HideImmediate();

        penaltyPoints = 0;
        resultShown = false;
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
}
