using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerRunner : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float maxForwardSpeed = 12f;
    [SerializeField] private float maxBackwardSpeed = 4f;
    [SerializeField] private float acceleration = 20f;
    [SerializeField] private float sideSpeed = 5f;

    [Header("Rotation")]
    [SerializeField] private bool faceMoveDirection = true;
    [SerializeField] private float rotationSpeed = 12f;
    [SerializeField] private float minSpeedToRotate = 0.1f;
    [SerializeField] private bool freezeTilt = true;

    [Header("Facing Lock")]
    [Tooltip("指定キーを押している間は身体の向きを固定し、現在の向きを基準に移動します。")]
    [SerializeField] private KeyCode facingLockKey = KeyCode.LeftShift;
    [SerializeField] private bool allowRightShift = true;

    [Header("Jump")]
    [Tooltip("ジャンプ直後の上向き速度（m/s）")]
    [SerializeField] private float jumpSpeed = 5f;
    [SerializeField] private float groundCheckDistance = 0.6f;
    [SerializeField] private LayerMask groundLayerMask = ~0;

    private Rigidbody rb;
    private float forwardSpeed;
    private bool jumpRequested;

    public bool IsGroundedNow { get; private set; }
    public bool IsFacingLocked { get; private set; }
    public float VerticalSpeed => rb != null ? rb.linearVelocity.y : 0f;
    public float ForwardSpeed => forwardSpeed;

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
        IsFacingLocked = IsFacingLockPressed();

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

        // 通常時のSは移動方向へ振り向いて走るため、前進と同じ最高速度を使う。
        // Shift中は、W単独だけmaxForwardSpeedを許可し、S/A/Dを含む移動は
        // maxBackwardSpeedへ制限する。
        bool hasHorizontalInput = !Mathf.Approximately(horizontal, 0f);
        bool isLockedForwardOnly = IsFacingLocked &&
                                   forwardInput &&
                                   !backwardInput &&
                                   !hasHorizontalInput;

        float lockedMoveSpeedLimit = isLockedForwardOnly
            ? maxForwardSpeed
            : maxBackwardSpeed;

        float positiveSpeedLimit = IsFacingLocked
            ? lockedMoveSpeedLimit
            : maxForwardSpeed;

        float negativeSpeedLimit = IsFacingLocked
            ? maxBackwardSpeed
            : maxForwardSpeed;

        forwardSpeed = Mathf.Clamp(
            forwardSpeed,
            -negativeSpeedLimit,
            positiveSpeedLimit
        );

        Vector3 horizontalVelocity;

        if (IsFacingLocked) {
            // Shift中はPlayerの現在の向きを基準に移動する。
            // Sで後ずさり、A/Dですり足になる。
            Vector3 localVelocity = new Vector3(
                horizontal * maxBackwardSpeed,
                0f,
                forwardSpeed
            );

            // 斜め入力で速度が合成され、上限を超えないようにする。
            localVelocity = Vector3.ClampMagnitude(
                localVelocity,
                lockedMoveSpeedLimit
            );

            horizontalVelocity = rb.rotation * localVelocity;
        }
        else {
            // 通常時はワールド座標基準のストレイフ移動。
            horizontalVelocity = new Vector3(
                horizontal * sideSpeed,
                0f,
                forwardSpeed
            );
        }

        rb.linearVelocity = new Vector3(
            horizontalVelocity.x,
            rb.linearVelocity.y,
            horizontalVelocity.z
        );
    }

    private void HandleJump()
    {
        if (!jumpRequested) return;

        jumpRequested = false;

        if (!IsGroundedNow) return;

        Vector3 velocity = rb.linearVelocity;
        velocity.y = jumpSpeed;
        rb.linearVelocity = velocity;

        IsGroundedNow = false;
    }

    private void HandleRotation()
    {
        if (!faceMoveDirection || IsFacingLocked) return;

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

    private bool IsFacingLockPressed()
    {
        if (Input.GetKey(facingLockKey)) {
            return true;
        }

        return allowRightShift && Input.GetKey(KeyCode.RightShift);
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
