using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerRunner : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float maxForwardSpeed = 12f;
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
        bool accel = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ||
                     Input.GetKey(KeyCode.Space);
        float horizontal = 0f;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) {
            horizontal -= 1f;
        }

        /*if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.LeftArrow)) {
            horizontal = 1f;
        }*/

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.LeftArrow)) {
            horizontal += 1f;
        }
        
        if (accel) {
            forwardSpeed += acceleration * Time.fixedDeltaTime;
        }
        else {
            forwardSpeed -= acceleration * Time.fixedDeltaTime;
        }

        forwardSpeed = Mathf.Clamp(forwardSpeed, 0f, maxForwardSpeed);

        Vector3 velocity = new Vector3(
            horizontal * sideSpeed,
            rb.linearVelocity.y,
            forwardSpeed
        );

        rb.linearVelocity = velocity;
    }
}
