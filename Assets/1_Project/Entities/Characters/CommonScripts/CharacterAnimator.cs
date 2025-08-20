using Unity.Netcode;
using UnityEngine;

public class CharacterAnimator : NetworkBehaviour
{
    private Animator animator;

    private float forwardSpeed;
    private float sidewaysSpeed;

    [SerializeField] private string forwardParam = "ForwardsSpeed";
    [SerializeField] private string sidewaysParam = "SidewaysSpeed";
    [SerializeField] private string jumpStateParam = "JumpState";

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

    /// <summary>
    /// Updates the jump state parameter in the animator.
    /// 0 = Normal locomotion, 1 = Jump start, 2 = Float (loops), 3 = Land
    /// </summary>
    public void UpdateJumpState(int jumpState)
    {
        if (!animator) return;
        animator.SetInteger(jumpStateParam, jumpState);
    }
}
