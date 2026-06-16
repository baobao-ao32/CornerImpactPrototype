using UnityEngine;

public class PrototypeGameManager : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform target;

    [Header("Scripts")]
    [SerializeField] private CameraFollow cameraFollow;
    [SerializeField] private PlayerRunner playerRunner;
    [SerializeField] private PlayerImpactLauncher playerImpactLauncher;
    [SerializeField] private TargetFlightTracker targetFlightTracker;

    [Header("GUI")]
    [SerializeField] private Color guiTextColor = Color.black;
    [SerializeField] private int guiFontSize = 30;

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

        playerStartPosition = player.position;
        playerStartRotation = player.rotation;

        targetStartPosition = target.position;
        targetStartRotation = target.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.R)) {
            ResetScene();
        }
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

        float displayDistance = targetFlightTracker.IsLanded
            ? targetFlightTracker.FinalDistance
            : targetFlightTracker.CurrentDistance;

        GUI.Label(new Rect(10, 10, 500, 30), $"Distance: {displayDistance:F2} m", labelStyle);
        GUI.Label(new Rect(10, 35, 500, 30), $"Max Height: {targetFlightTracker.MaxHeight:F2} m", labelStyle);
        GUI.Label(new Rect(10, 60, 500, 30), $"Flight Time: {targetFlightTracker.FlightTime:F2} s", labelStyle);
        GUI.Label(new Rect(10, 85, 800, 30), "W: Forward, S: Back, A/D: Move, Space: Jump, R: Reset", labelStyle);
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
