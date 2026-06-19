using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TargetFlightTracker : MonoBehaviour
{
    [Header("Ground Detection")]
    [SerializeField] private string groundTag = "Ground";
    [SerializeField] private float minFlightTime = 0.15f;
    [SerializeField] private float leaveHeightThreshold = 0.15f;

    private Rigidbody rb;

    private bool isTracking;
    private bool hasLeftGround;
    private bool isLanded;

    private Vector3 launchPosition;
    private Vector3 landingPosition;

    private float launchTime;
    private float landingTime;

    private float maxY;

    private Quaternion previousRotation;
    private float accumulatedRotationDegrees;

    public bool IsTracking => isTracking;
    public bool IsLanded => isLanded;

    public float CurrentDistance { get; private set; }
    public float FinalDistance { get; private set; }
    public float MaxHeight { get; private set; }
    public float FlightTime { get; private set; }
    public float RotationCount => accumulatedRotationDegrees / 360f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (!isTracking || isLanded) return;

        Vector3 currentPosition = rb.position;

        CurrentDistance = GetHorizontalDistance(launchPosition, currentPosition);

        if (currentPosition.y > maxY) {
            maxY = currentPosition.y;
            MaxHeight = maxY - launchPosition.y;
        }

        FlightTime = Time.fixedTime - launchTime;

        if (!hasLeftGround) {
            if (currentPosition.y > launchPosition.y + leaveHeightThreshold) {
                hasLeftGround = true;
            }
        }
    }

    public void BeginFlight(Vector3 startPosition)
    {
        isTracking = true;
        hasLeftGround = false;
        isLanded = false;

        launchPosition = startPosition;
        landingPosition = startPosition;

        launchTime = Time.fixedTime;
        landingTime = launchTime;

        maxY = startPosition.y;

        CurrentDistance = 0f;
        FinalDistance = 0f;
        MaxHeight = 0f;
        FlightTime = 0f;
    }

    public void ResetStats()
    {
        isTracking = false;
        hasLeftGround = false;
        isLanded = false;

        CurrentDistance = 0f;
        FinalDistance = 0f;
        MaxHeight = 0f;
        FlightTime = 0f;

        launchPosition = rb != null ? rb.position : transform.position;
        landingPosition = launchPosition;

        launchTime = 0f;
        landingTime = 0f;
        maxY = launchPosition.y;

        previousRotation = rb != null ? rb.rotation : transform.rotation;
        accumulatedRotationDegrees = 0f;
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryLand(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        TryLand(collision);
    }

    private void TryLand(Collision collision)
    {
        if (!isTracking || isLanded) return;

        if (!collision.collider.CompareTag(groundTag)) {
            return;
        }

        if (!hasLeftGround){
            return;
        }

        if (Time.fixedTime - launchTime < minFlightTime) {
            return;
        }

        EndFlight();
    }

    private void EndFlight()
    {
        // FixedUpdateの後に発生した最後の物理ステップ分も取りこぼさないようにする
        AccumulateRotation();

        isLanded = true;
        isTracking = false;

        landingPosition = rb.position;
        landingTime = Time.fixedTime;

        FinalDistance = GetHorizontalDistance(launchPosition, landingPosition);
        FlightTime = landingTime - launchTime;
    }

    private void AccumulateRotation()
    {
        Quaternion currentRotation = rb.rotation;
        float deltaAngle = Quaternion.Angle(previousRotation, currentRotation);

        accumulatedRotationDegrees += deltaAngle;
        previousRotation = currentRotation;
    }

    private float GetHorizontalDistance(Vector3 from, Vector3 to)
    {
        Vector2 a = new Vector2(from.x, from.z);
        Vector2 b = new Vector2(to.x, to.z);

        return Vector2.Distance(a, b);
    }
}
