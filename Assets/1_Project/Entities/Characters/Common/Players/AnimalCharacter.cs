using System;
using Unity.Cinemachine;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.Events;
public class AnimalCharacter : CharacterBase
{

    private InputManager_Singleton _input;

    [Header("Movement Settings")]
    [SerializeField] private float Speed = 1f;
    [SerializeField] private float SprintSpeed = 4f;
    [SerializeField] private float JumpSpeed = 4f;
    [SerializeField] private float SprintJumpSpeed = 6f;
    [SerializeField] private float Acceleration = 10f;
    [SerializeField] private float RotationSpeed = 10f;
    [SerializeField] private bool strafing = false;

    private Rigidbody rb;
    private Vector3 m_LastInput;
    private bool m_IsSprinting;
    private bool m_IsJumping;

    [Header("Ground Settings")]
    [SerializeField] private LayerMask GroundMask;
    [SerializeField] private float GroundCheckRadius = 0.3f;
    [SerializeField] private float GroundCheckDistance = 0.2f;
    [SerializeField] private Vector3 groundCheckStartOffset = Vector3.up;
    [SerializeField] private float GroundAlignmentForce = 25f;
    [SerializeField] private float GroundSpringDamping = 2f;
    [SerializeField] private bool DebugRaycasts = true;
    //[SerializeField] private float StepCorrectionRange = 0.25f;         // Max distance you want to correct for, maybe later for better foot placement prediction
    [SerializeField] private float MaxGroundAlignmentForce = 50f;       // Cap the lift force

    [Header("Slope Settings")]
    [SerializeField] private float maxStableSlopeAngle = 40f; // Maximum walkable angle
    [SerializeField] private float slideAcceleration = 10f;   // How fast the character slides

    [Header("Events")]
    //public UnityEvent Landed = new UnityEvent();
    public Action PreUpdate;
    public Action<Vector3, float> PostUpdate;
    public Action StartJump;
    public Action EndJump;

    [Header("Animations")]
    // --- Network Animation Variables ---
    [SerializeField] private CharacterAnimator characterAnimator;
    [HideInInspector] public NetworkVariable<float> _sidewaysSpeed_NWV = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    [HideInInspector] public NetworkVariable<float> _forwardsSpeed_NWV = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [Header("Camera Vars")]
    bool m_InTopHemisphere = true;
    float m_TimeInHemisphere = 100;
    Vector3 m_LastRawInput;
    Quaternion m_Upsidedown = Quaternion.AngleAxis(180, Vector3.left);

    public Camera CameraOverride; // cam override was left in for a zoom in cam change, todo
    public Camera Camera => CameraOverride == null ? Camera.main : CameraOverride;

    private Vector3 rawInput;
    private bool hasInput;
    private bool shouldRotate;

    //breaedon code
    public bool isGrabbed = false;
    public void fn_SetIsSprintingInput(bool isSprinting) => m_IsSprinting = isSprinting;

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
            // Link our local camera to this character
            CameraManager.Instance.SetThirdPersonCamera(GetComponentInChildren<PlayerCameraAimController>().transform);
            _input = InputManager_Singleton.Instance;
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        Vector2 movementInput = _input.movementInput;

        // Prepare input
        rawInput = new Vector3(movementInput.x, 0, movementInput.y);
        shouldRotate = rawInput.sqrMagnitude > 0.001f;
        hasInput = shouldRotate;

        var inputFrame = GetInputFrame(Vector3.Dot(rawInput, m_LastRawInput) < 0.8f);
        m_LastRawInput = rawInput;
        m_LastInput = inputFrame * rawInput;

        if (m_LastInput.sqrMagnitude > 1)
            m_LastInput.Normalize();

        PreUpdate?.Invoke(); // Still fire PreUpdate here

        ApplyGroundAlignment();
        UpdateAnimationParameters();
    }


    private void FixedUpdate()
    {
        if (!IsOwner) return;
        if (isGrabbed) return;

        if (hasInput)
        {
            ApplyMotion();

            if (!strafing)
            {
                var qA = transform.rotation;
                Quaternion targetRotation = Quaternion.LookRotation(m_LastInput, Vector3.up);
                transform.rotation = Quaternion.Slerp(qA, targetRotation, RotationSpeed * Time.fixedDeltaTime);
            }
        }

        // Get local-space velocity after applying motion
        var vel = Quaternion.Inverse(rb.rotation) * rb.linearVelocity;
        vel.y = rb.linearVelocity.y;

        PostUpdate?.Invoke(vel, m_IsSprinting ? JumpSpeed / SprintJumpSpeed : 1);
    }

    // HandleMovement is used to receive the raw horizontal input. // it has been removed for now, but may be updated again in the future
    public override void HandleMovement(Vector2 input) // Required to be here from the base class
    {
        
    }

    Quaternion GetInputFrame(bool inputDirectionChanged)
    {
        // This code is trying to prevent gimbal lock

        var frame = Camera.transform.rotation;

        var playerUp = transform.up;
        var up = frame * Vector3.up;

        const float BlendTime = 2f;
        m_TimeInHemisphere += Time.deltaTime;
        bool inTopHemisphere = Vector3.Dot(up, playerUp) >= 0;
        if (inTopHemisphere != m_InTopHemisphere)
        {
            m_InTopHemisphere = inTopHemisphere;
            m_TimeInHemisphere = Mathf.Max(0, BlendTime - m_TimeInHemisphere);
        }

        var axis = Vector3.Cross(up, playerUp);
        if (axis.sqrMagnitude < 0.001f && inTopHemisphere)
            return frame;

        var angle = UnityVectorExtensions.SignedAngle(up, playerUp, axis);
        var frameA = Quaternion.AngleAxis(angle, axis) * frame;

        Quaternion frameB = frameA;
        if (!inTopHemisphere || m_TimeInHemisphere < BlendTime)
        {
            frameB = frame * m_Upsidedown;
            var axisB = Vector3.Cross(frameB * Vector3.up, playerUp);
            if (axisB.sqrMagnitude > 0.001f)
                frameB = Quaternion.AngleAxis(180f - angle, axisB) * frameB;
        }

        if (inputDirectionChanged)
            m_TimeInHemisphere = BlendTime;

        if (m_TimeInHemisphere >= BlendTime)
            return inTopHemisphere ? frameA : frameB;

        if (inTopHemisphere)
            return Quaternion.Slerp(frameB, frameA, m_TimeInHemisphere / BlendTime);
        return Quaternion.Slerp(frameA, frameB, m_TimeInHemisphere / BlendTime);
    }

    private void ApplyMotion()
    {
        Vector3 desiredHorizontal = m_LastInput * (m_IsSprinting ? SprintSpeed : Speed);
        Vector3 currentHorizontal = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        Vector3 force = desiredHorizontal * rb.mass * Acceleration;
        rb.AddForce(force, ForceMode.Force);

        // Jumping logic
        if (IsGrounded() && !m_IsJumping && _input.jumpInput)
        {
            Debug.Log("Jumped ");
            float jumpImpulse = m_IsSprinting ? SprintJumpSpeed : JumpSpeed;
            rb.AddForce(Vector3.up * jumpImpulse * rb.mass, ForceMode.Impulse);
            m_IsJumping = true;
            StartJump?.Invoke();
        }
        else if (IsGrounded())
        {
            m_IsJumping = false;
        }

    }

    // --- Animation Synchronization ---
    // UpdateAnimationParameters syncs movement speeds.
    private void UpdateAnimationParameters()
    {
        if (!characterAnimator) return;

        if (IsOwner)
        {
            Vector3 localVel = Quaternion.Inverse(transform.rotation) * rb.linearVelocity;
            _sidewaysSpeed_NWV.Value = localVel.x;
            _forwardsSpeed_NWV.Value = localVel.z;
        }
        // Pass both the current velocity and networked speeds.
        characterAnimator.UpdateAnimatorLocomotion(rb.linearVelocity, transform, IsOwner, _sidewaysSpeed_NWV.Value, _forwardsSpeed_NWV.Value);
    }

    // Raycast downward to maintain a desired distance from the ground.
    private void ApplyGroundAlignment()
    {
        Vector3 origin = transform.position + groundCheckStartOffset;
        Vector3 direction = Vector3.down;
        float rayLength = GroundCheckDistance;

        if (Physics.SphereCast(origin, GroundCheckRadius, direction, out RaycastHit hit, rayLength, GroundMask))
        {
            if (DebugRaycasts)
                Debug.DrawLine(origin, hit.point, Color.green);

            // Get the slope angle from the hit normal.
            float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);

            // Compute the penetration error (how far under our desired ground distance the hit is).
            float distanceToGround = hit.distance;
            float penetration = GroundCheckDistance - distanceToGround;

            // Always try to correct vertically.
            Vector3 alignmentForce = Vector3.zero;
            if (penetration > 0f)
            {
                float normalizedPenetration = penetration / GroundCheckDistance;
                float upwardForce = normalizedPenetration * GroundAlignmentForce;
                float verticalVelocity = Vector3.Dot(rb.linearVelocity, Vector3.up);
                float damping = verticalVelocity * GroundSpringDamping; // might adjust this term if needed.
                alignmentForce = Vector3.up * (upwardForce - damping);
            }

            // If the slope is too steep, calculate a slide force.
            Vector3 slideForce = Vector3.zero;
            if (slopeAngle > maxStableSlopeAngle)
            {
                float steepness = (slopeAngle - maxStableSlopeAngle) / (90f - maxStableSlopeAngle);
                Vector3 slideDirection = Vector3.ProjectOnPlane(Vector3.down, hit.normal).normalized;
                slideForce = slideDirection * slideAcceleration * steepness;
            }

            // Combine the upward alignment and sliding forces.
            rb.AddForce(alignmentForce + slideForce, ForceMode.Acceleration);
        }
        else if (DebugRaycasts)
        {
            Debug.DrawRay(origin, direction * rayLength, Color.red);
        }
    }

    public virtual void SetstrafingMode(bool b) { }
    public virtual bool IsMoving { get; } // Implement as needed.


    private void OnDrawGizmosSelected()
    {
        if (!DebugRaycasts) return;

        Vector3 origin = transform.position + groundCheckStartOffset;
        Vector3 direction = Vector3.down;
        float rayLength = GroundCheckDistance;

        Gizmos.color = Color.yellow;

        int steps = 2;
        float stepLength = rayLength / steps;

        for (int i = 0; i <= steps; i++)
        {
            Vector3 pos = origin + direction * (stepLength * i);
            Gizmos.DrawWireSphere(pos, GroundCheckRadius);
        }
    }


    private bool IsGrounded()
    {
        return Physics.SphereCast(transform.position+Vector3.up, GroundCheckRadius, Vector3.down, out RaycastHit hit, GroundCheckDistance*2.0f, GroundMask);
    }

    private Quaternion GetInputFrame()
    {
        return Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0);
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

    public void fn_IsMovementInputDisabled(bool isDisabled)
    {
        isGrabbed = isDisabled;
    }
}