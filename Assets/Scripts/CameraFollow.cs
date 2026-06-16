using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Default Target")]
    [SerializeField] private Transform playerTarget;

    [Header("Player Offset")]
    [SerializeField] private Vector3 playerOffset = new Vector3(0f, 6f, -10f);
    [SerializeField] private Vector3 playerLookOffset = new Vector3(0f, 1f, 4f);

    [Header("Launched Target Camera")]
    [SerializeField] private Vector3 targetOffset = new Vector3(0f, 5f, -8f);
    [SerializeField] private Vector3 targetLookOffset = new Vector3(0f, 1f, 0f);

    [Header("Follow")]
    [SerializeField] private float followSpeed = 8f;
    [SerializeField] private float rotateSpeed = 10f;

    private Transform currentTarget;
    private Vector3 currentOffset;
    private Vector3 currentLookOffset;

    private void Awake()
    {
        FollowPlayer(true);
    }

    private void LateUpdate()
    {
        if (currentTarget == null) return;

        Vector3 desiredPosition = currentTarget.position + currentOffset;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            followSpeed * Time.deltaTime
        );

        Vector3 lookPosition = currentTarget.position + currentLookOffset;
        Vector3 lookDirection = lookPosition - transform.position;

        if (lookDirection.sqrMagnitude > 0.001f) {
            Quaternion desiredRotation = Quaternion.LookRotation(lookDirection, Vector3.up);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                desiredRotation,
                rotateSpeed * Time.deltaTime
            );
        }
    }

    public void FollowPlayer(bool snap = false)
    {
        SetTarget(playerTarget, playerOffset, playerLookOffset, snap);
    }

    public void FollowLaunchedTarget(Transform launchedTarget, bool snap = false)
    {
        SetTarget(launchedTarget, targetOffset, targetLookOffset, snap);
    }

    private void SetTarget(Transform newTarget, Vector3 newOffset, Vector3 newLookOffset, bool snap)
    {
        currentTarget = newTarget;
        currentOffset = newOffset;
        currentLookOffset = newLookOffset;

        if (snap && currentTarget != null)
        {
            transform.position = currentTarget.position + currentOffset;
            transform.LookAt(currentTarget.position + currentLookOffset);
        }
    }
}
