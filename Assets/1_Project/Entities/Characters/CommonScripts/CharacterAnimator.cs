using Unity.Netcode;
using UnityEngine;

public class CharacterAnimator : NetworkBehaviour
{
    private Animator animator;

    private float forwardSpeed;
    private float sidewaysSpeed;

    [SerializeField] private string forwardParam = "ForwardsSpeed";
    [SerializeField] private string sidewaysParam = "SidewaysSpeed";

    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float footRaycastDistance = 0.5f;
    [SerializeField] private float footOffsetY = 0.05f;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Updates animator parameters using movement data.
    /// For the owner, calculate local velocity; for remote clients, use networked values.
    /// </summary>
    public void UpdateAnimatorLocomotion(Vector3 velocity, Transform characterTransform, bool isOwner, float networkSideways, float networkForward)
    {
        if (!animator) return;

        if (isOwner)
        {
            Vector3 localVelocity = characterTransform.InverseTransformDirection(velocity);
            forwardSpeed = localVelocity.z;
            sidewaysSpeed = localVelocity.x;
        }
        else
        {
            forwardSpeed = networkForward;
            sidewaysSpeed = networkSideways;
        }

        animator.SetFloat(forwardParam, forwardSpeed);
        animator.SetFloat(sidewaysParam, sidewaysSpeed);
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (animator == null) return;

        ApplyFootIK(AvatarIKGoal.LeftFoot);
        ApplyFootIK(AvatarIKGoal.RightFoot);
    }

    private void ApplyFootIK(AvatarIKGoal foot)
    {
        Vector3 footPosition = animator.GetIKPosition(foot);

        float sphereRadius = 0.05f;
        Vector3 origin = footPosition + Vector3.up * footRaycastDistance;
        Vector3 rayDirection = Vector3.down;
        float castDistance = footRaycastDistance * 2f;

        Debug.DrawLine(origin, origin + rayDirection * castDistance, Color.cyan);

        if (Physics.SphereCast(origin, sphereRadius, rayDirection, out RaycastHit hit, castDistance, groundMask))
        {
            Vector3 hitPoint = hit.point;
            Vector3 horizontalDelta = hitPoint - footPosition;
            horizontalDelta.y = 0;

            float maxOffset = 0.2f;
            if (horizontalDelta.magnitude > maxOffset)
                horizontalDelta = horizontalDelta.normalized * maxOffset;

            Vector3 adjustedPos = new Vector3(
                footPosition.x + horizontalDelta.x,
                hitPoint.y,
                footPosition.z + horizontalDelta.z
            ) + Vector3.up * footOffsetY;

            float weight = (foot == AvatarIKGoal.LeftFoot)
                ? animator.GetFloat("IKLeftFootWeight")
                : animator.GetFloat("IKRightFootWeight");

            animator.SetIKPositionWeight(foot, weight);
            animator.SetIKRotationWeight(foot, weight);

            Quaternion adjustedRot = Quaternion.LookRotation(
                Vector3.ProjectOnPlane(transform.forward, hit.normal), hit.normal);

            animator.SetIKPosition(foot, adjustedPos);
            animator.SetIKRotation(foot, adjustedRot);
        }
        else
        {
            animator.SetIKPositionWeight(foot, 0f);
            animator.SetIKRotationWeight(foot, 0f);
        }
    }
}
