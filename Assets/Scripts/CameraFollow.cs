using UnityEngine;

public sealed class CameraFollow : MonoBehaviour
{
    private enum CameraMode
    {
        PlayerFollow,
        ImpactObserver,
        TargetFollow
    }

    [Header("Default Target")]
    [SerializeField] private Transform playerTarget;

    [Header("Player Camera")]
    [SerializeField] private Vector3 playerOffset = new Vector3(0f, 6f, -10f);
    [SerializeField] private Vector3 playerLookOffset = new Vector3(0f, 1f, 4f);

    [Header("Impact Observer Camera")]
    [Tooltip("Hit直後、JKを基準に置くカメラ位置。JKの向きに合わせて回転するローカル座標です。")]
    [SerializeField] private Vector3 impactObserverOffset = new Vector3(0f, 4.5f, -7f);

    [Tooltip("飛んでいくTargetのどこを見るか。ワールド座標系のオフセットです。")]
    [SerializeField] private Vector3 impactTargetLookOffset = new Vector3(0f, 1f, 0f);

    [Min(0f)]
    [SerializeField] private float impactObserverDuration = 1.5f;

    [Header("Launched Target Camera")]
    [SerializeField] private Vector3 targetOffset = new Vector3(0f, 5f, -8f);
    [SerializeField] private Vector3 targetLookOffset = new Vector3(0f, 1f, 0f);

    [Header("Follow")]
    [SerializeField] private float followSpeed = 8f;
    [SerializeField] private float rotateSpeed = 10f;

    private CameraMode mode;

    private Transform positionTarget;
    private Transform lookTarget;
    private Transform launchedTarget;

    private Vector3 currentPositionOffset;
    private Vector3 currentLookOffset;
    private bool useLocalPositionOffset;

    private float impactObserverTimer;

    private void Awake()
    {
        FollowPlayer(true);
    }

    private void LateUpdate()
    {
        UpdateImpactSequence();
        UpdateCameraTransform();
    }

    /// <summary>
    /// 通常のPlayer追従へ戻します。
    /// Reset時にもこのメソッドを呼びます。
    /// </summary>
    public void FollowPlayer(bool snap = false)
    {
        mode = CameraMode.PlayerFollow;
        impactObserverTimer = 0f;
        launchedTarget = null;

        SetView(
            playerTarget,
            playerTarget,
            playerOffset,
            playerLookOffset,
            useLocalOffset: false,
            snap
        );
    }

    /// <summary>
    /// Hit直後の観察カメラを開始します。
    /// カメラ位置はJKの後方、注視対象は飛んでいくTargetです。
    /// 一定時間後、自動的にTarget追従へ切り替わります。
    /// </summary>
    public void BeginImpactSequence(Transform newLaunchedTarget, bool snap = false)
    {
        if (newLaunchedTarget == null)
        {
            return;
        }

        launchedTarget = newLaunchedTarget;
        impactObserverTimer = impactObserverDuration;
        mode = CameraMode.ImpactObserver;

        SetView(
            playerTarget,
            launchedTarget,
            impactObserverOffset,
            impactTargetLookOffset,
            useLocalOffset: true,
            snap
        );

        if (impactObserverDuration <= 0f)
        {
            FollowLaunchedTarget(launchedTarget, snap);
        }
    }

    /// <summary>
    /// 飛翔中Targetの追従カメラへ直接切り替えます。
    /// </summary>
    public void FollowLaunchedTarget(Transform newLaunchedTarget, bool snap = false)
    {
        if (newLaunchedTarget == null)
        {
            return;
        }

        launchedTarget = newLaunchedTarget;
        impactObserverTimer = 0f;
        mode = CameraMode.TargetFollow;

        SetView(
            launchedTarget,
            launchedTarget,
            targetOffset,
            targetLookOffset,
            useLocalOffset: false,
            snap
        );
    }

    private void UpdateImpactSequence()
    {
        if (mode != CameraMode.ImpactObserver)
        {
            return;
        }

        impactObserverTimer -= Time.deltaTime;

        if (impactObserverTimer <= 0f && launchedTarget != null)
        {
            FollowLaunchedTarget(launchedTarget);
        }
    }

    private void UpdateCameraTransform()
    {
        if (positionTarget == null || lookTarget == null)
        {
            return;
        }

        Vector3 offset = useLocalPositionOffset
            ? positionTarget.TransformVector(currentPositionOffset)
            : currentPositionOffset;

        Vector3 desiredPosition = positionTarget.position + offset;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            followSpeed * Time.deltaTime
        );

        Vector3 lookPosition = lookTarget.position + currentLookOffset;
        Vector3 lookDirection = lookPosition - transform.position;

        if (lookDirection.sqrMagnitude <= 0.001f)
        {
            return;
        }

        Quaternion desiredRotation = Quaternion.LookRotation(
            lookDirection,
            Vector3.up
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            desiredRotation,
            rotateSpeed * Time.deltaTime
        );
    }

    private void SetView(
        Transform newPositionTarget,
        Transform newLookTarget,
        Vector3 newPositionOffset,
        Vector3 newLookOffset,
        bool useLocalOffset,
        bool snap)
    {
        positionTarget = newPositionTarget;
        lookTarget = newLookTarget;
        currentPositionOffset = newPositionOffset;
        currentLookOffset = newLookOffset;
        useLocalPositionOffset = useLocalOffset;

        if (!snap || positionTarget == null || lookTarget == null)
        {
            return;
        }

        Vector3 offset = useLocalPositionOffset
            ? positionTarget.TransformVector(currentPositionOffset)
            : currentPositionOffset;

        transform.position = positionTarget.position + offset;
        transform.LookAt(lookTarget.position + currentLookOffset);
    }
}
