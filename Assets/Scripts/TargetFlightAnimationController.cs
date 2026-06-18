using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetFlightPoseController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;

    [Header("Timing")]
    [SerializeField] private float impactPoseDuration = 0.12f;

    [Header("Pose")]
    [SerializeField] private bool applyOnStartForDebug = false;

    private readonly Dictionary<HumanBodyBones, Quaternion> initialLocalRotations = new();
    private Coroutine hitRoutine;

    private void Awake()
    {
        if (animator == null) {
            animator = GetComponentInChildren<Animator>();
        }

        CacheInitialPose();
    }

    private void Start()
    {
        if (applyOnStartForDebug) {
            ApplyFlightPose();
        }
    }

    private void CacheInitialPose()
    {
        initialLocalRotations.Clear();

        CacheBone(HumanBodyBones.Hips);
        CacheBone(HumanBodyBones.Spine);
        CacheBone(HumanBodyBones.Chest);
        CacheBone(HumanBodyBones.Neck);
        CacheBone(HumanBodyBones.Head);

        CacheBone(HumanBodyBones.LeftUpperArm);
        CacheBone(HumanBodyBones.LeftLowerArm);
        CacheBone(HumanBodyBones.LeftHand);

        CacheBone(HumanBodyBones.RightUpperArm);
        CacheBone(HumanBodyBones.RightLowerArm);
        CacheBone(HumanBodyBones.RightHand);

        CacheBone(HumanBodyBones.LeftUpperLeg);
        CacheBone(HumanBodyBones.LeftLowerLeg);
        CacheBone(HumanBodyBones.LeftFoot);

        CacheBone(HumanBodyBones.RightUpperLeg);
        CacheBone(HumanBodyBones.RightLowerLeg);
        CacheBone(HumanBodyBones.RightFoot);
    }

    private void CacheBone(HumanBodyBones bone)
    {
        if (animator == null) return;

        Transform t = animator.GetBoneTransform(bone);
        if (t == null) return;

        initialLocalRotations[bone] = t.localRotation;
    }

    public void PlayHitReaction()
    {
        if (hitRoutine != null) {
            StopCoroutine(hitRoutine);
        }

        hitRoutine = StartCoroutine(HitReactionRoutine());
    }

    private IEnumerator HitReactionRoutine()
    {
        if (animator == null) {
            yield break;
        }

        animator.enabled = false;

        ApplyImpactPose();

        yield return new WaitForSeconds(impactPoseDuration);

        ApplyFlightPose();

        hitRoutine = null;
    }

    private void ApplyImpactPose()
    {
        ResetBonesOnly();

        // 衝突直後の「うわっ」ポーズ
        RotateBone(HumanBodyBones.Spine, new Vector3(15f, 0f, 0f));
        RotateBone(HumanBodyBones.Chest, new Vector3(20f, 0f, 0f));
        RotateBone(HumanBodyBones.Head, new Vector3(-10f, 0f, 0f));

        // 腕を前に出す感じ
        RotateBone(HumanBodyBones.LeftUpperArm, new Vector3(40f, 0f, 45f));
        RotateBone(HumanBodyBones.LeftLowerArm, new Vector3(0f, 0f, 50f));

        RotateBone(HumanBodyBones.RightUpperArm, new Vector3(40f, 0f, -45f));
        RotateBone(HumanBodyBones.RightLowerArm, new Vector3(0f, 0f, -50f));

        // 膝を少し曲げる
        RotateBone(HumanBodyBones.LeftUpperLeg, new Vector3(15f, 0f, -10f));
        RotateBone(HumanBodyBones.LeftLowerLeg, new Vector3(-30f, 0f, 0f));

        RotateBone(HumanBodyBones.RightUpperLeg, new Vector3(15f, 0f, 10f));
        RotateBone(HumanBodyBones.RightLowerLeg, new Vector3(-30f, 0f, 0f));
    }

    public void ApplyFlightPose()
    {
        ResetBonesOnly();

        // 胴体を少し反らせる・ひねる
        RotateBone(HumanBodyBones.Spine, new Vector3(-10f, 0f, 15f));
        RotateBone(HumanBodyBones.Chest, new Vector3(-15f, 10f, -20f));
        RotateBone(HumanBodyBones.Head, new Vector3(10f, -20f, 0f));

        // 左腕: 上に曲げる
        RotateBone(HumanBodyBones.LeftUpperArm, new Vector3(-40f, 0f, 80f));
        RotateBone(HumanBodyBones.LeftLowerArm, new Vector3(0f, 0f, 80f));
        RotateBone(HumanBodyBones.LeftHand, new Vector3(0f, 20f, 20f));

        // 右腕: 横〜下方向へ伸ばす
        RotateBone(HumanBodyBones.RightUpperArm, new Vector3(30f, 0f, -90f));
        RotateBone(HumanBodyBones.RightLowerArm, new Vector3(0f, 0f, -25f));
        RotateBone(HumanBodyBones.RightHand, new Vector3(0f, -20f, -20f));

        // 左足: 膝を大きく曲げる
        RotateBone(HumanBodyBones.LeftUpperLeg, new Vector3(70f, 20f, -35f));
        RotateBone(HumanBodyBones.LeftLowerLeg, new Vector3(-100f, 0f, 0f));
        RotateBone(HumanBodyBones.LeftFoot, new Vector3(30f, 0f, 0f));

        // 右足: 斜めに伸ばす
        RotateBone(HumanBodyBones.RightUpperLeg, new Vector3(-30f, -20f, 35f));
        RotateBone(HumanBodyBones.RightLowerLeg, new Vector3(15f, 0f, 0f));
        RotateBone(HumanBodyBones.RightFoot, new Vector3(-20f, 0f, 0f));
    }

    public void ResetPose()
    {
        if (hitRoutine != null) {
            StopCoroutine(hitRoutine);
            hitRoutine = null;
        }

        ResetBonesOnly();

        if (animator != null) {
            animator.enabled = true;
            animator.Rebind();
            animator.Update(0f);
        }
    }

    private void ResetBonesOnly()
    {
        foreach (var pair in initialLocalRotations) {
            if (animator == null) continue;

            Transform t = animator.GetBoneTransform(pair.Key);
            if (t == null) continue;

            t.localRotation = pair.Value;
        }
    }

    private void RotateBone(HumanBodyBones bone, Vector3 eulerOffset)
    {
        if (animator == null) return;

        Transform t = animator.GetBoneTransform(bone);
        if (t == null) return;

        Quaternion initialRotation = initialLocalRotations.TryGetValue(
            bone,
            out Quaternion cached
        )
            ? cached
            : t.localRotation;

        t.localRotation = initialRotation * Quaternion.Euler(eulerOffset);
    }
}