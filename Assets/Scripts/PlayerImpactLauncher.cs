using UnityEngine;

public class PlayerImpactLauncher : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private string targetTag = "Target";

    [Header("Launch")]
    [SerializeField] private float minImpactSpeed = 3f;
    [SerializeField] private float impulseMultiplier = 2.8f;
    [SerializeField] private float minImpulse = 8f;
    [SerializeField] private float maxImpulse = 35f;
    [SerializeField, Range(0f, 1f)] private float upwardRatio = 0.28f;

    [Header("Rotation")]
    [SerializeField] private float torqueMultiplier = 8f;

    [Header("Jump Impact")]
    [SerializeField] private float maxVerticalSpeedForBonus = 6f;
    [SerializeField] private float jumpImpulseBonus = 0.35f;
    [SerializeField] private float jumpUpwardRatioBonus = 0.18f;
    [SerializeField, Range(0f, 1f)] private float maxUpwardRatio = 0.55f;

    [Header("Camera")]
    [SerializeField] private CameraFollow cameraFollow;
    [SerializeField] private bool switchCameraOnImpact = true;

    [Header("Flight Stats")]
    [SerializeField] private TargetFlightTracker targetFlightTracker;

    private Rigidbody playerRb;
    private bool hasLaunched;

    private void Awake()
    {
        playerRb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasLaunched) return;

        if(!collision.gameObject.CompareTag(targetTag)) {
            return;
        }

        Rigidbody targetRb = collision.rigidbody;
        if(targetRb == null) return;

        Vector3 playerVelocity = playerRb.linearVelocity;
        Vector3 horizontalVelocity = new Vector3(playerVelocity.x, 0f, playerVelocity.z);
        float horizontalSpeed = horizontalVelocity.magnitude;
        float upwardSpeed = Mathf.Max(0f, playerVelocity.y);

        if (horizontalSpeed < minImpactSpeed) {
            return;
        }

        Vector3 forwardDir = horizontalVelocity.normalized;

        float jumpBonus = Mathf.Clamp01(upwardSpeed / maxVerticalSpeedForBonus);
        float effectiveUpwardRatio = Mathf.Clamp(
            upwardRatio + jumpUpwardRatioBonus * jumpBonus,
            upwardRatio,
            maxUpwardRatio
        );

        // 前方向 + 上方向。現実世界での物理ではなく、このゲーム向けのネタ物理で、
        // 男の子の発射方向を決める
        Vector3 launchDir = (forwardDir * (1f - effectiveUpwardRatio) + Vector3.up * effectiveUpwardRatio).normalized;

        float baseImpulse = horizontalSpeed * impulseMultiplier;
        float jumpImpulseMultiplier = 1f + jumpImpulseBonus * jumpBonus;

        float impulse = Mathf.Clamp(
            baseImpulse * jumpImpulseMultiplier,
            minImpulse, maxImpulse
        );

        TargetWalker targetWalker = collision.gameObject.GetComponent<TargetWalker>();
        if (targetWalker != null) {
            targetWalker.StopWalking();
        }

        ContactPoint contact = collision.GetContact(0);

        targetRb.AddForceAtPosition(
            launchDir * impulse,
            contact.point,
            ForceMode.Impulse
        );

        // 宙に浮いている間は雑に回転させる
        Vector3 torqueAxis = Vector3.Cross(launchDir, Vector3.up).normalized;
        if (torqueAxis.sqrMagnitude > 0.001f) {
            targetRb.AddTorque(torqueAxis * horizontalSpeed * torqueMultiplier, ForceMode.Impulse);
        }

        if (targetFlightTracker != null) {
            targetFlightTracker.BeginFlight(targetRb.position);
        }

        hasLaunched = true;

        if (switchCameraOnImpact && cameraFollow != null) {
            cameraFollow.FollowLaunchedTarget(targetRb.transform);
        }

        Debug.Log(
            $"Impact!! horizontal={horizontalSpeed:F2}, upward={upwardSpeed:F2}, " +
            $"jumpBonus={jumpBonus:F2}, upwardRatio={effectiveUpwardRatio:F2}, impulse={impulse:F2}"
        );
    }

    public void ResetImpactState()
    {
        hasLaunched = false;
    }
}
