using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TargetWalker : MonoBehaviour
{
    private enum WalkMode
    {
        OneWay,
        PingPong
    }

    [Header("Walk")]
    [SerializeField] private Vector3 walkDirection = Vector3.left;
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float walkDistance = 110f;
    [SerializeField] private WalkMode walkMode = WalkMode.PingPong;
    [SerializeField] private bool walkOnStart = true;

    [Header("Rotation")]
    [SerializeField] private bool faceWalkDirection = true;
    [SerializeField] private float rotationSpeed = 12f;

    [Header("Timing")]
    [SerializeField] private float startDelay = 1.5f;

    [Header("Physics")]
    [SerializeField] private bool freezeTiltWhileWalking = true;

    private Rigidbody rb;
    private Vector3 normalizedDirection;
    private RigidbodyConstraints originalConstraints;
    private Quaternion targetRotation;

    private bool isWaiting;
    private bool isWalking;

    private int directionSign = 1;
    private float traveledDistance;
    private float waitTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        originalConstraints = rb.constraints;
        normalizedDirection = GetNormalizedWalkDirection();

        ResetWalkState();
    }

    private void FixedUpdate()
    {
        if (isWaiting) {
            UpdateWaiting();
            return;
        }

        if (!isWalking) return;

        MoveTarget();
    }

    private void UpdateWaiting()
    {
        waitTimer -= Time.fixedDeltaTime;

        if (waitTimer <= 0f) {
            isWaiting = false;
            isWalking = true;
        }
    }

    private void MoveTarget()
    {
        float moveAmount = Mathf.Abs(walkSpeed) * Time.fixedDeltaTime;
        float remainingDistance = walkDistance - traveledDistance;
        float actualMoveAmount = Mathf.Min(moveAmount, remainingDistance);

        Vector3 moveDirection = normalizedDirection * directionSign;
        Vector3 delta = moveDirection * actualMoveAmount;

        if (faceWalkDirection) {
            RotateToward(moveDirection);
        }

        rb.MovePosition(rb.position + delta);

        traveledDistance += actualMoveAmount;

        if (traveledDistance >= walkDistance) {
            HandleReachEnd();
        }
    }

    private void HandleReachEnd()
    {
        if (walkMode == WalkMode.PingPong) {
            directionSign *= -1;
            traveledDistance = 0f;

            if (faceWalkDirection) {
                UpdateTargetRotation();
            }
        }
        else {
            StopWalking();
        }
    }

    public void StopWalking()
    {
        isWalking = false;

        if (freezeTiltWhileWalking) {
            rb.constraints = originalConstraints;
        }
    }

    public void ResetWalkState()
    {
        normalizedDirection = GetNormalizedWalkDirection();

        directionSign = 1;
        traveledDistance = 0f;

        if (freezeTiltWhileWalking) {
            rb.constraints =
                originalConstraints |
                RigidbodyConstraints.FreezeRotationX |
                RigidbodyConstraints.FreezeRotationZ;
        }

        if (faceWalkDirection) {
            UpdateTargetRotation();
            rb.rotation = targetRotation;
        }

        if (walkOnStart) {
            waitTimer = Mathf.Max(0f, startDelay);
            isWaiting = waitTimer > 0f;
            isWalking = waitTimer <= 0f;
        }
        else {
            isWaiting = false;
            isWalking = false;
        }
    }

    private Vector3 GetNormalizedWalkDirection()
    {
        if (walkDirection.sqrMagnitude < 0.001f) {
            return Vector3.left;
        }

        return walkDirection.normalized;
    }

    private void RotateToward(Vector3 direction)
    {
        Vector3 horizontalDirection = new Vector3(direction.x, 0f, direction.z);

        if (horizontalDirection.sqrMagnitude < 0.001f) {
            return;
        }

        targetRotation = Quaternion.LookRotation(horizontalDirection.normalized, Vector3.up);

        Quaternion nextRotation = Quaternion.Slerp(
            rb.rotation,
            targetRotation,
            rotationSpeed * Time.fixedDeltaTime
        );

        rb.MoveRotation(nextRotation);
    }

    private void UpdateTargetRotation()
    {
        Vector3 direction = normalizedDirection * directionSign;
        Vector3 horizontalDirection = new Vector3(direction.x, 0f, direction.z);

        if (horizontalDirection.sqrMagnitude < 0.001f) {
            horizontalDirection = Vector3.left;
        }

        targetRotation = Quaternion.LookRotation(horizontalDirection.normalized, Vector3.up);
    }
}
