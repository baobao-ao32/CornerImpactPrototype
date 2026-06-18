using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerAnimationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerRunner playerRunner;

    [Header("Animator Parameters")]
    [SerializeField] private string moveSpeedParameter = "MoveSpeed";
    [SerializeField] private string isGroundedParameter = "IsGrounded";
    [SerializeField] private string verticalSpeedParameter = "VerticalSpeed";

    [Header("Tuning")]
    [SerializeField] private float referenceRunSpeed = 12f;
    [SerializeField] private float dampingTime = 0.1f;

    private void Awake()
    {
        if (playerRunner == null) {
            playerRunner = GetComponent<PlayerRunner>();
        }

        if (animator == null) {
            animator = GetComponentInChildren<Animator>();
        }
    }

    private void Update()
    {
        if (animator == null || playerRunner == null) {
            return;
        }

        float normalizedSpeed = playerRunner.HorizontalSpeed / Mathf.Max(referenceRunSpeed, 0.001f);

        animator.SetFloat(
            moveSpeedParameter,
            normalizedSpeed,
            dampingTime,
            Time.deltaTime
        );

        animator.SetBool(
            isGroundedParameter,
            playerRunner.IsGroundedNow
        );

        animator.SetFloat(
            verticalSpeedParameter,
            playerRunner.VerticalSpeed,
            dampingTime,
            Time.deltaTime
        );
    }
}
