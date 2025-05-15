using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

namespace Unity.Cinemachine.Samples
{
    public abstract class DynamicBodyPlayerControllerBase : MonoBehaviour, Unity.Cinemachine.IInputAxisOwner
    {
        [Tooltip("Ground speed when walking")]
        public float Speed = 1f;
        [Tooltip("Ground speed when sprinting")]
        public float SprintSpeed = 4;
        [Tooltip("Initial vertical speed when jumping")]
        public float JumpSpeed = 4;
        [Tooltip("Initial vertical speed when sprint-jumping")]
        public float SprintJumpSpeed = 6;

        public Action PreUpdate;
        public Action<Vector3, float> PostUpdate;
        public Action StartJump;
        public Action EndJump;

        [Header("Input Axes")]
        [Tooltip("X Axis movement.  Value is -1..1.  Controls the sideways movement")]
        public InputAxis MoveX = InputAxis.DefaultMomentary;

        [Tooltip("Z Axis movement.  Value is -1..1. Controls the forward movement")]
        public InputAxis MoveZ = InputAxis.DefaultMomentary;

        [Tooltip("Jump movement.  Value is 0 or 1. Controls the vertical movement")]
        public InputAxis Jump = InputAxis.DefaultMomentary;

        [Tooltip("Sprint movement.  Value is 0 or 1. If 1, then is sprinting")]
        public InputAxis Sprint = InputAxis.DefaultMomentary;

        [Header("Events")]
        [Tooltip("This event is sent when the player lands after a jump.")]
        public UnityEvent Landed = new();

        void IInputAxisOwner.GetInputAxes(List<IInputAxisOwner.AxisDescriptor> axes)
        {
            axes.Add(new() { DrivenAxis = () => ref MoveX, Name = "Move X", Hint = IInputAxisOwner.AxisDescriptor.Hints.X });
            axes.Add(new() { DrivenAxis = () => ref MoveZ, Name = "Move Z", Hint = IInputAxisOwner.AxisDescriptor.Hints.Y });
            axes.Add(new() { DrivenAxis = () => ref Jump, Name = "Jump" });
            axes.Add(new() { DrivenAxis = () => ref Sprint, Name = "Sprint" });
        }

        public virtual void SetStrafeMode(bool b) { }
        public abstract bool IsMoving { get; }
    }

    [RequireComponent(typeof(Rigidbody))]
    public class DynamicBodyPlayerController : DynamicBodyPlayerControllerBase
    {
        [Tooltip("Acceleration factor for horizontal movement.")]
        public float Acceleration = 10f;
        [Tooltip("Rotation speed when turning.")]
        public float RotationSpeed = 10f;
        [Tooltip("Makes the player strafe when moving sideways, otherwise it turns to face the direction of motion.")]
        public bool Strafe = false;

        public enum ForwardModes { Camera, Player, World };
        public enum UpModes { Player, World };

        public ForwardModes InputForward = ForwardModes.Camera;
        public UpModes UpMode = UpModes.World;

        [Tooltip("Layer mask for ground detection.")]
        public LayerMask GroundMask;
        [Tooltip("Radius for ground check.")]
        public float GroundCheckRadius = 0.3f;
        [Tooltip("Distance for ground check.")]
        public float GroundCheckDistance = 0.2f;

        private Rigidbody rb;
        private Vector3 m_LastInput;
        private bool m_IsSprinting;
        private bool m_IsJumping;

        public override void SetStrafeMode(bool b) => Strafe = b;
        public override bool IsMoving => m_LastInput.sqrMagnitude > 0.01f;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.freezeRotation = true;
        }

        private void Update()
        {
            PreUpdate?.Invoke();
            ProcessInput();
        }

        private void FixedUpdate()
        {
            ApplyMotion();
        }

        private void ProcessInput()
        {
            var rawInput = new Vector3(MoveX.Value, 0, MoveZ.Value);
            var inputFrame = GetInputFrame();
            m_LastInput = inputFrame * rawInput;
            if (m_LastInput.sqrMagnitude > 1)
                m_LastInput.Normalize();

            m_IsSprinting = Sprint.Value > 0.5f;
        }

        private void ApplyMotion()
        {
            Vector3 desiredHorizontal = m_LastInput * (m_IsSprinting ? SprintSpeed : Speed);
            Vector3 currentHorizontal = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            Vector3 velocityDelta = desiredHorizontal - currentHorizontal;
            Vector3 force = desiredHorizontal * rb.mass * Acceleration;
            rb.AddForce(force, ForceMode.Force);

            if (IsGrounded() && Jump.Value > 0.5f && !m_IsJumping)
            {
                float jumpImpulse = m_IsSprinting ? SprintJumpSpeed : JumpSpeed;
                rb.AddForce(Vector3.up * jumpImpulse, ForceMode.Impulse);
                m_IsJumping = true;
                StartJump?.Invoke();
            }
            else if (IsGrounded())
            {
                m_IsJumping = false;
            }

            if (!Strafe && m_LastInput.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(m_LastInput, Vector3.up);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, RotationSpeed * Time.fixedDeltaTime));
            }
        }

        private bool IsGrounded()
        {
            // Use a SphereCast to provide a more robust ground check.
            return Physics.SphereCast(transform.position, GroundCheckRadius, Vector3.down, out RaycastHit hit, GroundCheckDistance, GroundMask);
        }

        private Quaternion GetInputFrame()
        {
            switch (InputForward)
            {
                case ForwardModes.Camera:
                    return Camera.main ? Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0) : Quaternion.identity;
                case ForwardModes.Player:
                    return transform.rotation;
                default:
                    return Quaternion.identity;
            }
        }
    }
}
