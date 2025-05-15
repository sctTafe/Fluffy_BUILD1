using UnityEngine;

public class ActiveRagdoll : MonoBehaviour
{
    public enum RagdollState { Animated, Ragdoll, PoseMatching }
    public RagdollState currentState = RagdollState.Animated;

    public Animator animator;
    public Rigidbody[] ragdollBodies;  // Ragdoll skeleton (physics-driven)
    public Transform[] animatedBones;  // Animator-controlled skeleton (animation-driven)

    [Header("Physics Settings")]
    public float poseMatchStrength = 500f;
    public float balanceThreshold = 15f;
    public float balanceRecoveryTorque = 300f;
    public float impactForceThreshold = 5f;
    public float staggerForce = 250f;
    public float animationBlendSpeed = 5f;
    private bool isStaggered = false;

    private void Start()
    {
        SetState(currentState);
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            SetState(currentState);
        }
    }

    private void FixedUpdate()
    {
        if (currentState == RagdollState.PoseMatching)
        {
            ApplyPoseMatching();
            // MaintainBalance();
        }
    }

    public void SetState(RagdollState newState)
    {
        currentState = newState;
        switch (newState)
        {
            case RagdollState.Animated:
                EnableRagdoll(false);
                animator.enabled = true;
                break;
            case RagdollState.Ragdoll:
                EnableRagdoll(true);
                animator.enabled = false;
                break;
            case RagdollState.PoseMatching:
                EnableRagdoll(true);
                animator.enabled = true;  // Animator remains active
                break;
        }
    }

    private void EnableRagdoll(bool enabled)
    {
        foreach (var rb in ragdollBodies)
        {
            rb.isKinematic = !enabled;
        }
    }

    private void ApplyPoseMatching()
    {
        for (int i = 0; i < ragdollBodies.Length; i++)
        {
            if (ragdollBodies[i] == null || animatedBones[i] == null) continue;

            // Get the target rotation from the animated bones (Animator-controlled)
            Quaternion targetRotation = animatedBones[i].rotation;

            // Get the current ragdoll body rotation
            Quaternion currentRotation = ragdollBodies[i].transform.rotation;
            Quaternion deltaRotation = targetRotation * Quaternion.Inverse(currentRotation);

            // Convert the delta rotation to torque (to apply to ragdoll body)
            deltaRotation.ToAngleAxis(out float angle, out Vector3 axis);

            if (angle > 180f) angle -= 360f; // Keep angle in range -180 to 180

            // Apply torque with strength scaling
            Vector3 torque = axis * Mathf.Clamp(angle * poseMatchStrength, -500f, 500f);
            ragdollBodies[i].AddTorque(torque, ForceMode.Acceleration);
        }
    }

    private void MaintainBalance()
    {
        Vector3 up = transform.up;
        Vector3 worldUp = Vector3.up;
        float angle = Vector3.Angle(up, worldUp);

        if (angle > balanceThreshold)
        {
            Vector3 correctionTorque = Vector3.Cross(up, worldUp) * balanceRecoveryTorque;
            ragdollBodies[0].AddTorque(correctionTorque, ForceMode.VelocityChange);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > impactForceThreshold)
        {
            ReactToImpact(collision);
        }
    }

    private void ReactToImpact(Collision collision)
    {
        isStaggered = true;
        Vector3 impactDirection = collision.impulse.normalized;
        ragdollBodies[0].AddForce(impactDirection * staggerForce, ForceMode.Impulse);
        Invoke(nameof(RecoverFromStagger), 1f);
    }

    private void RecoverFromStagger()
    {
        isStaggered = false;
    }

    private void Update()
    {
        float targetWeight = isStaggered ? 0f : 1f;
        animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1), targetWeight, Time.deltaTime * animationBlendSpeed));
    }
}
