using UnityEngine;

namespace Unity.Cinemachine.Samples
{
    public class RagDollAnimator : MonoBehaviour
    {
        public float NormalWalkSpeed = 1.7f;
        public float NormalSprintSpeed = 5;
        public float MaxSprintScale = 1.4f;
        public float JumpAnimationScale = 0.65f;

        private Rigidbody rb;
        private Vector3 previousPosition;
        private Animator animator;

        protected struct AnimationParams
        {
            public bool IsWalking;
            public bool IsRunning;
            public bool IsJumping;
            public bool LandTriggered;
            public bool JumpTriggered;
            public Vector3 Direction;
            public float MotionScale;
            public float JumpScale;
        }

        private AnimationParams animationParams;
        private static readonly float k_IdleThreshold = 0.2f;

        private void Start()
        {
            rb = GetComponentInParent<Rigidbody>();
            animator = GetComponent<Animator>();
            if (rb == null || animator == null)
            {
                Debug.LogError("RagDollAnimator requires a Rigidbody and an Animator component.");
                enabled = false;
                return;
            }
            previousPosition = transform.position;
        }

        private void LateUpdate()
        {
            UpdateAnimationState();
        }

        private void UpdateAnimationState()
        {
            Vector3 velocity = rb.linearVelocity;
            velocity.y = 0;
            float speed = velocity.magnitude;

            bool isRunning = speed > NormalWalkSpeed * 2;
            bool isWalking = !isRunning && speed > k_IdleThreshold;
            animationParams.IsWalking = isWalking;
            animationParams.IsRunning = isRunning;
            animationParams.Direction = speed > k_IdleThreshold ? velocity.normalized : Vector3.zero;
            animationParams.MotionScale = isWalking ? speed / NormalWalkSpeed : 1;
            animationParams.JumpScale = JumpAnimationScale;

            if (isRunning)
            {
                animationParams.MotionScale = (speed < NormalSprintSpeed)
                    ? speed / NormalSprintSpeed
                    : Mathf.Min(MaxSprintScale, 1 + (speed - NormalSprintSpeed) / (3 * NormalSprintSpeed));
            }

            UpdateAnimation(animationParams);
        }

        private void UpdateAnimation(AnimationParams animationParams)
        {
            animator.SetFloat("SidewaysSpeed", animationParams.Direction.x);
            animator.SetFloat("ForwardsSpeed", animationParams.Direction.z);
            animator.SetFloat("MotionScale", animationParams.MotionScale);
            animator.SetBool("Walking", animationParams.IsWalking);
            animator.SetBool("Running", animationParams.IsRunning);
            animator.SetFloat("JumpScale", animationParams.JumpScale);

            if (animationParams.JumpTriggered)
                animator.SetTrigger("Jump");
            if (animationParams.LandTriggered)
                animator.SetTrigger("Land");
        }
    }
}
