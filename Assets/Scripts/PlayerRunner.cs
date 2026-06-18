using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerRunner : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float maxForwardSpeed = 12f;
    [SerializeField] private float maxBackwardSpeed = 4f;
    [SerializeField] private float acceleration = 20f;
    // [SerializeField] private float deceleration = 12f;
    [SerializeField] private float sideSpeed = 5f;

    [Header("Rotation")]
    [SerializeField] private bool faceMoveDirection = true;
    [SerializeField] private float rotationSpeed = 12f;
    [SerializeField] private float minSpeedToRotate = 0.1f;
    [SerializeField] private bool freezeTilt = true;

    [Header("Jump")]
    [SerializeField] private float jumpImpulse = 6f;
    [SerializeField] private float groundCheckDistance = 0.6f;
    [SerializeField] private LayerMask groundLayerMask = ~0;

    private Rigidbody rb;
    private float forwardSpeed;
    private bool jumpRequested;

    public bool IsGroundedNow { get; private set; }
    public float VerticalSpeed => rb != null ? rb.linearVelocity.y : 0f;

    public float HorizontalSpeed
    {
        get
        {
            if (rb == null) return 0f;

            Vector3 horizontalVelocity = new Vector3(
                rb.linearVelocity.x,
                0f,
                rb.linearVelocity.z
            );

            return horizontalVelocity.magnitude;
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (freezeTilt) {
            rb.constraints |= RigidbodyConstraints.FreezeRotationX |
                            RigidbodyConstraints.FreezeRotationZ;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) {
            jumpRequested = true;
        }
    }

    private void FixedUpdate()
    {
        IsGroundedNow = CheckGrounded();

        HandleMove();
        HandleRotation();
        HandleJump();
    }

    private void HandleMove()
    {
        bool forwardInput = Input.GetKey(KeyCode.W) ||
                            Input.GetKey(KeyCode.UpArrow);
        bool backwardInput = Input.GetKey(KeyCode.S) ||
                             Input.GetKey(KeyCode.DownArrow);

        float horizontal = 0f;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) {
            horizontal -= 1f;
        }

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) {
            horizontal += 1f;
        }

        if (forwardInput && !backwardInput) {
            forwardSpeed += acceleration * Time.fixedDeltaTime;
        }
        else if (backwardInput && !forwardInput) {
            forwardSpeed -= acceleration * Time.fixedDeltaTime;
        }
        else {
            forwardSpeed = Mathf.MoveTowards(
                forwardSpeed,
                0f,
                acceleration * Time.fixedDeltaTime
            );
        }

        forwardSpeed = Mathf.Clamp(forwardSpeed, -maxBackwardSpeed, maxForwardSpeed);

        Vector3 velocity = new Vector3(
            horizontal * sideSpeed,
            rb.linearVelocity.y,
            forwardSpeed
        );

        rb.linearVelocity = velocity;
    }

    private void HandleJump()
    {
        if (!jumpRequested) return;

        jumpRequested = false;

        if (!IsGroundedNow) return;

        Vector3 velocity = rb.linearVelocity;
        velocity.y = 0f;
        rb.linearVelocity = velocity;

        rb.AddForce(Vector3.up * jumpImpulse, ForceMode.Impulse);

        IsGroundedNow = false;
    }

    private void HandleRotation()
    {
        if (!faceMoveDirection) return;

        Vector3 horizontalVelocity = new Vector3(
            rb.linearVelocity.x,
            0f,
            rb.linearVelocity.z
        );

        if (horizontalVelocity.sqrMagnitude < minSpeedToRotate * minSpeedToRotate) {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(
            horizontalVelocity.normalized,
            Vector3.up
        );

        Quaternion nextRotation = Quaternion.Slerp(
            rb.rotation,
            targetRotation,
            rotationSpeed * Time.fixedDeltaTime
        );

        rb.MoveRotation(nextRotation);
    }

    private bool CheckGrounded()
    {
        return Physics.Raycast(
            rb.position,
            Vector3.down,
            groundCheckDistance,
            groundLayerMask,
            QueryTriggerInteraction.Ignore
        );
    }
    public void ResetMoveState()
    {
        forwardSpeed = 0f;
        jumpRequested = false;
    }
}
