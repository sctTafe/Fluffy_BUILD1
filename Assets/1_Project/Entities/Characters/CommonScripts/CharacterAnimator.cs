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

    [Header("Smoothing")]
    [Tooltip("Higher values snap faster; lower values smooth more")] [SerializeField]
    private float smoothing = 10f;

    // smoothed values used to drive animator to avoid jitter at low speeds
    private float _smoothedForward;
    private float _smoothedSideways;

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

        // smooth small fluctuations to avoid animator stutter at low speeds
        _smoothedForward = Mathf.Lerp(_smoothedForward, forwardSpeed, Time.deltaTime * smoothing);
        _smoothedSideways = Mathf.Lerp(_smoothedSideways, sidewaysSpeed, Time.deltaTime * smoothing);

        animator.SetFloat(forwardParam, _smoothedForward);
        animator.SetFloat(sidewaysParam, _smoothedSideways);
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
