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

    [Header("Physics")]
    [SerializeField] private bool freezeTiltWhileWalking = true;

    private Rigidbody rb;
    private Vector3 normalizedDirection;
    private RigidbodyConstraints originalConstraints;

    private bool isWalking;
    private int directionSign = 1;
    private float traveledDistance;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        originalConstraints = rb.constraints;
        normalizedDirection = GetNormalizedWalkDirection();

        ResetWalkState();
    }

    private void FixedUpdate()
    {
        if (!isWalking) return;

        float moveAmount = Mathf.Abs(walkSpeed) * Time.fixedDeltaTime;
        float remainingDistance = walkDistance - traveledDistance;
        float actualMoveAmount = Mathf.Min(moveAmount, remainingDistance);

        Vector3 delta = normalizedDirection * directionSign * actualMoveAmount;

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

        isWalking = walkOnStart;
        directionSign = 1;
        traveledDistance = 0f;

        if (freezeTiltWhileWalking) {
            rb.constraints =
                originalConstraints |
                RigidbodyConstraints.FreezeRotationX |
                RigidbodyConstraints.FreezeRotationZ;
        }
    }

    private Vector3 GetNormalizedWalkDirection()
    {
        if (walkDirection.sqrMagnitude < 0.001f) {
            return Vector3.left;
        }

        return walkDirection.normalized;
    }
}
