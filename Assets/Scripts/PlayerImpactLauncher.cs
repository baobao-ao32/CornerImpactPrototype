using Unity.VisualScripting;
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

    private Rigidbody playerRb;

    private void Awake()
    {
        playerRb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(!collision.gameObject.CompareTag(targetTag)) {
            return;
        }

        Rigidbody targetRb = collision.rigidbody;
        if(targetRb == null) return;

        Vector3 playerVelocity = playerRb.linearVelocity;
        Vector3 horizontalVelocity = new Vector3(playerVelocity.x, 0f, playerVelocity.z);
        float impactSpeed = horizontalVelocity.magnitude;

        if(impactSpeed < minImpactSpeed) return;

        Vector3 forwardDir = horizontalVelocity.normalized;

        // 前方向 + 上方向。現実世界での物理ではなく、このゲーム向けのネタ物理で、
        // 男の子の発射方向を決める
        Vector3 launchDir = (forwardDir * (1f - upwardRatio) + Vector3.up * upwardRatio).normalized;

        float impulse = Mathf.Clamp(
            impactSpeed * impulseMultiplier,
            minImpulse, maxImpulse
        );

        ContactPoint contact = collision.GetContact(0);

        targetRb.AddForceAtPosition(
            launchDir * impulse,
            contact.point,
            ForceMode.Impulse
        );

        // 宙に浮いている間は雑に回転させる。
        Vector3 torqueAxis = Vector3.Cross(launchDir, Vector3.up).normalized;
        targetRb.AddTorque(torqueAxis * impactSpeed * torqueMultiplier, ForceMode.Impulse);

        Debug.Log($"Impact!! speed={impactSpeed:F2}, impulse={impulse:F2}");
    }
}
