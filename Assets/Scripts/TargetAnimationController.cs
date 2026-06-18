using UnityEngine;

public class TargetAnimationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TargetWalker targetWalker;
    [SerializeField] private Animator animator;

    [Header("Animator Parameters")]
    [SerializeField] private string moveSpeedParameter = "MoveSpeed";

    [Header("Tuning")]
    [SerializeField] private float referenceWalkSpeed = 3f;
    [SerializeField] private float dampingTime = 0.1f;

    private void Awake()
    {
        if (targetWalker == null) {
            targetWalker = GetComponent<TargetWalker>();
        }

        if (animator == null) {
            animator = GetComponentInChildren<Animator>();
        }
    }

    private void Update()
    {
        if (animator == null || targetWalker == null) {
            return;
        }

        float normalizedSpeed = 0f;

        if (targetWalker.IsWalking) {
            normalizedSpeed = targetWalker.CurrentMoveSpeed / Mathf.Max(referenceWalkSpeed, 0.001f);
        }

        animator.SetFloat(
            moveSpeedParameter,
            normalizedSpeed,
            dampingTime,
            Time.deltaTime
        );
    }
}
