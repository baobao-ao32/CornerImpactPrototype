using UnityEngine;

public class PrototypeGameManager : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform target;

    private Rigidbody playerRb;
    private Rigidbody targetRb;

    private Vector3 playerStartPosition;
    private Quaternion playerStartRotation;

    private Vector3 targetStartPosition;
    private Quaternion targetStartRotation;

    private void Awake()
    {
        playerRb = player.GetComponent<Rigidbody>();
        targetRb = target.GetComponent<Rigidbody>();

        playerStartPosition = player.position;
        playerStartRotation = player.rotation;

        targetStartPosition = target.position;
        targetStartRotation = target.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.R)) {
            ResetScene();
        }
    }

    private void OnGUI()
    {
        float distance = GetHorizontalDistance(targetStartPosition, target.position);

        GUI.Label(new Rect(10, 10, 400, 30), $"Target DIstance: {distance:F2} m");
        GUI.Label(new Rect(10, 35, 400, 30), $"W / Space: Accelerate, A/D: Move, R: Reset");
    }

    private void ResetScene()
    {
        ResetBody(player, playerRb, playerStartPosition, playerStartRotation);
        ResetBody(target, targetRb, targetStartPosition, targetStartRotation);
    }

    private void ResetBody(Transform bodyTransform, Rigidbody bodyRb, Vector3 position, Quaternion rotation)
    {
        bodyRb.linearVelocity = Vector3.zero;
        bodyRb.angularVelocity = Vector3.zero;

        bodyTransform.SetPositionAndRotation(position, rotation);
    }

    private float GetHorizontalDistance(Vector3 from, Vector3 to)
    {
        Vector2 a = new Vector2(from.x, from.z);
        Vector2 b = new Vector2(to.x, to.z);

        return Vector2.Distance(a, b);
    }
}
