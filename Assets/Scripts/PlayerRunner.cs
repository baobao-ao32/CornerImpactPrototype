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

    private Rigidbody rb;
    private float forwardSpeed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        bool forwardInput = Input.GetKey(KeyCode.W) ||
                            Input.GetKey(KeyCode.UpArrow) ||
                            Input.GetKey(KeyCode.Space);
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

    public void ResetMoveState()
    {
        forwardSpeed = 0f;
    }
}
