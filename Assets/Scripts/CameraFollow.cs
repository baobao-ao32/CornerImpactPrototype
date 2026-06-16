using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;

    [Header("Camera Offset")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 6f, -10f);

    [Header("Follow")]
    [SerializeField] private float followSpeed = 8f;

    [Header("Look")]
    [SerializeField] private Vector3 lookOffset = new Vector3(0f, 1f, 4f);

    private void LateUpdate()
    {
        if(target == null) return;

        Vector3 desiredPosition = target.position + offset;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            followSpeed * Time.deltaTime
        );

        transform.LookAt(target.position + lookOffset);
    }
}
