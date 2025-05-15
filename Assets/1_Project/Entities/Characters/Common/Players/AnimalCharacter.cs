using System;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.Events;

public class AnimalCharacter : CharacterBase // (Assume CharacterBase inherits from NetworkBehaviour)
{
    [Header("Movement Settings")]
    public float Speed = 1f;
    public float SprintSpeed = 4f;
    public float JumpSpeed = 4f;
    public float SprintJumpSpeed = 6f;
    public float Acceleration = 10f;
    public float RotationSpeed = 10f;
    public bool Strafe = false;

    public enum ForwardModes { Camera, Player, World };
    public ForwardModes InputForward = ForwardModes.Camera;

    [Header("Ground Check")]
    public LayerMask GroundMask;
    public float GroundCheckRadius = 0.3f;
    public float GroundCheckDistance = 0.2f;

    [Header("Events")]
    public UnityEvent Landed = new UnityEvent();
    public Action PreUpdate;
    public Action<Vector3, float> PostUpdate;
    public Action StartJump;
    public Action EndJump;

    // Animator references
    public Animator animator;
    public Animator animator2; // for testing active ragdolls

    private Rigidbody rb;
    private Vector3 m_LastInput;
    private bool m_IsSprinting;
    private bool m_IsJumping;

    // --- Network Animation Variables ---
    [SerializeField] private NetworkVariable<float> _sidewaysSpeed_NWV = new NetworkVariable<float>(0f);
    [SerializeField] private NetworkVariable<float> _forwardsSpeed_NWV = new NetworkVariable<float>(0f);

    // Local values (for detecting changes)
    private float _localSidewaysSpeed;
    private float _localForwardsSpeed;
    private float _prevLocalSidewaysSpeed;
    private float _prevLocalForwardsSpeed;
    private const float kAnimThreshold = 0.05f;



    //breaedon code
    public bool isGrabbed = false;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }
    private void Start()
    {
        if (IsOwner)
        {
            PlayerAimCoreLinker_Singleton.Instance.AssignAimCoreTarget(this);
            CameraManager.Instance.SetThirdPersonCamera(transform);
        }
    }
    // HandleMovement is used to receive the raw horizontal input.
    public override void HandleMovement(Vector2 input)
    {
        Quaternion inputFrame = GetInputFrame();
        Vector3 rawInput = new Vector3(input.x, 0, input.y);
        m_LastInput = inputFrame * rawInput;
        if (m_LastInput.sqrMagnitude > 1)
            m_LastInput.Normalize();
    }


    private void Update()
    {
        PreUpdate?.Invoke();
    }

    private void FixedUpdate()
    {
        if (!isGrabbed)
        {
            ApplyMotion();
        }
        UpdateAnimationParameters();
    }

    public virtual void SetStrafeMode(bool b) { }
    public virtual bool IsMoving { get; } // Implement as needed.

    private void ApplyMotion()
    {
        Vector3 desiredHorizontal = m_LastInput * (m_IsSprinting ? SprintSpeed : Speed);
        Vector3 currentHorizontal = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        Vector3 force = desiredHorizontal * rb.mass * Acceleration;
        rb.AddForce(force, ForceMode.Force);

        // Jumping logic
        if (IsGrounded())//&& !m_IsJumping)
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

    // --- Animation Synchronization ---
    private void UpdateAnimationParameters()
    {
        if (animator == null)
            return;

        // Convert world velocity to local space.
        Vector3 localVelocity = transform.InverseTransformDirection(rb.linearVelocity);
        _localForwardsSpeed = localVelocity.z;
        _localSidewaysSpeed = localVelocity.x;

        if (IsOwner)
        {
            // If our locally computed values change by more than the threshold, send them to the server.
            if (Mathf.Abs(_localSidewaysSpeed - _prevLocalSidewaysSpeed) > kAnimThreshold)
            {
                _prevLocalSidewaysSpeed = _localSidewaysSpeed;
                UpdateSidewaysSpeedServerRpc(_localSidewaysSpeed);
            }
            if (Mathf.Abs(_localForwardsSpeed - _prevLocalForwardsSpeed) > kAnimThreshold)
            {
                _prevLocalForwardsSpeed = _localForwardsSpeed;
                UpdateForwardsSpeedServerRpc(_localForwardsSpeed);
            }
            // Also update our local animator immediately.
            animator.SetFloat("SidewaysSpeed", _localSidewaysSpeed);
            animator.SetFloat("ForwardsSpeed", _localForwardsSpeed);
            if (animator2)
            {
                animator2.SetFloat("SidewaysSpeed", _localSidewaysSpeed);
                animator2.SetFloat("ForwardsSpeed", _localForwardsSpeed);
            }
        }
        else
        {
            // Non-owner clients read from the NetworkVariables.
            animator.SetFloat("SidewaysSpeed", _sidewaysSpeed_NWV.Value);
            animator.SetFloat("ForwardsSpeed", _forwardsSpeed_NWV.Value);
            if (animator2)
            {
                animator2.SetFloat("SidewaysSpeed", _sidewaysSpeed_NWV.Value);
                animator2.SetFloat("ForwardsSpeed", _forwardsSpeed_NWV.Value);
            }
        }
    }

    [ServerRpc]
    private void UpdateSidewaysSpeedServerRpc(float newSpeed)
    {
        _sidewaysSpeed_NWV.Value = newSpeed;
    }

    [ServerRpc]
    private void UpdateForwardsSpeedServerRpc(float newSpeed)
    {
        _forwardsSpeed_NWV.Value = newSpeed;
    }

    private bool IsGrounded()
    {
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

    public void MoveTo(Vector3 pos)
    {
        transform.position = pos;
        if (IsOwner)
        {
            Debug.Log("im grabed");
        }
    }


    [Rpc(SendTo.Everyone)]
    public void DisableMovementRpc()
    {
        gameObject.GetComponent<NetworkTransform>().enabled = false;
        isGrabbed = true;
    }
}
