using System;
using Unity.Cinemachine;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class AnimalCharacter : NetworkBehaviour
{
    #region Private Components
    private InputManager_Singleton _input;
    private CharacterController _controller;
    #endregion

    #region Movement Settings
    [Header("Movement Settings")]
    [SerializeField] private float Speed = 1f;
    [SerializeField] private float SprintSpeed = 4f;
    [SerializeField] private float JumpSpeed = 4f;
    [SerializeField] private float SprintJumpSpeed = 6f;
    [SerializeField] private float Acceleration = 10f;
    [SerializeField] private float RotationSpeed = 10f;
    [SerializeField] private bool strafing = false;

    [Header("Physics Settings")]
    [SerializeField] private float Gravity = -15f;
    [SerializeField] private float TerminalVelocity = 53f;
    #endregion

    #region Jump Animation Settings
    [Header("Jump Animation Settings")]
    [SerializeField] private float jumpStartDuration = 0.15f;
    [SerializeField] private float landDuration = 0.3f;
    [SerializeField] private float floatTransitionDelay = 0.1f;
    #endregion

    #region Movement State
    private Vector3 m_LastInput;
    private Vector3 m_Velocity;
    private bool m_IsSprinting;
    private bool m_IsJumping;

    // Jump state management
    private int currentJumpState = 0; // 0=normal, 1=jump start, 2=float, 3=land
    private float jumpStateTimer = 0f;
    private bool wasGroundedLastFrame = true;
    private bool isGrounded;
    #endregion

    #region Ground Detection (Optional)
    [Header("Ground Settings (Optional)")]
    [SerializeField] private LayerMask GroundMask = -1;
    [SerializeField] private float GroundCheckRadius = 0.28f;
    [SerializeField] private float GroundedOffset = -0.14f;
    [SerializeField] private bool DebugRaycasts = false;
    #endregion

    #region Slope Physics (Optional)
    [Header("Smooth Slope Settings")]
    [SerializeField] private bool enableSmoothSlopes = true;
    [SerializeField] private float maxWalkableAngle = 45f; // Angle where character can walk normally
    [SerializeField] private float maxClimbableAngle = 60f; // Angle where character can climb temporarily but slides back
    [SerializeField] private float slideForce = 15f; // How strong the slide back is
    [SerializeField] private float slopeSpeedMultiplier = 0.5f; // Speed reduction on steep slopes
    [SerializeField] private float momentumDecay = 0.95f; // How quickly momentum decays on steep slopes

    [Header("Steep Slope Settings (Alternative)")]
    [SerializeField] private bool enableSteepSlopeSliding = false;
    [SerializeField] private float slideAcceleration = 10f;

    // Slope tracking
    private Vector3 slopeSlideVelocity = Vector3.zero;
    private float currentSlopeAngle = 0f;
    private Vector3 currentSlopeNormal = Vector3.up;
    private bool onSteepSlope = false;
    #endregion

    #region Events
    [Header("Events")]
    public Action PreUpdate;
    public Action<Vector3, float> PostUpdate;
    public Action StartJump;
    public Action EndJump;
    #endregion

    #region Animation & Network
    [Header("Animations")]
    [SerializeField] private CharacterAnimator characterAnimator;
    
    [HideInInspector] public NetworkVariable<float> _sidewaysSpeed_NWV = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    [HideInInspector] public NetworkVariable<float> _forwardsSpeed_NWV = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    [HideInInspector] public NetworkVariable<int> _jumpState_NWV = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    #endregion

    #region Camera Input Frame
    [Header("Camera Input Frame")]
    private bool m_InTopHemisphere = true;
    private float m_TimeInHemisphere = 100;
    private Vector3 m_LastRawInput;
    private readonly Quaternion m_Upsidedown = Quaternion.AngleAxis(180, Vector3.left);

    public Camera CameraOverride;
    public Camera Camera => CameraOverride == null ? Camera.main : CameraOverride;
    #endregion

    #region Input Processing
    private Vector3 rawInput;
    private bool hasInput;
    private bool shouldRotate;
    #endregion

    #region Movement Control
    public bool isGrabbed = false;
    public void fn_SetIsSprintingInput(bool isSprinting) => m_IsSprinting = isSprinting;
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        _controller = GetComponent<CharacterController>();
        if (_controller == null)
        {
            Debug.LogError("CharacterController component missing on " + gameObject.name);
        }
    }

    private void Start()
    {
        if (IsOwner)
        {
            // Link camera and input
            CameraManager.Instance.SetThirdPersonCamera(GetComponentInChildren<PlayerCameraAimController>().transform);
            _input = InputManager_Singleton.Instance;
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        ProcessInput();
        PreUpdate?.Invoke();
        UpdateAnimationParameters();
    }

    private void FixedUpdate()
    {
        if (!IsOwner || isGrabbed) return;

        // Core updates
        UpdateGroundedState();
        UpdateSlopeInfo();
        UpdateJumpState();

        // Apply physics
        ApplyMovement();
        ApplyGravity();
        ApplySlopePhysics();

        // Apply rotation
        ApplyRotation();

        // Move character
        _controller.Move(m_Velocity * Time.fixedDeltaTime);

        // Update events
        PostUpdate?.Invoke(GetLocalVelocity(), m_IsSprinting ? JumpSpeed / SprintJumpSpeed : 1);

        // Update frame state
        wasGroundedLastFrame = isGrounded;
    }
    #endregion

    #region Input Processing
    private void ProcessInput()
    {
        Vector2 movementInput = _input.movementInput;
        rawInput = new Vector3(movementInput.x, 0, movementInput.y);
        shouldRotate = rawInput.sqrMagnitude > 0.001f;
        hasInput = shouldRotate;

        // Process input frame with roll prevention
        var inputFrame = GetInputFrame(Vector3.Dot(rawInput, m_LastRawInput) < 0.8f);
        m_LastRawInput = rawInput;
        m_LastInput = inputFrame * rawInput;

        if (m_LastInput.sqrMagnitude > 1)
            m_LastInput.Normalize();
        
        // **ADDITIONAL SAFETY: Ensure movement input stays horizontal**
        m_LastInput.y = 0f;
        if (m_LastInput.sqrMagnitude > 0.001f)
            m_LastInput.Normalize();
    }
    #endregion

    #region Ground & Slope Detection
    private void UpdateGroundedState()
    {
        isGrounded = _controller.isGrounded;
        
        // Optional enhanced detection
        if (!isGrounded && DebugRaycasts)
        {
            Vector3 spherePosition = transform.position + Vector3.up * GroundedOffset;
            isGrounded = Physics.CheckSphere(spherePosition, GroundCheckRadius, GroundMask, QueryTriggerInteraction.Ignore);
        }
    }

    private void UpdateSlopeInfo()
    {
        if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out RaycastHit hit, 1f, GroundMask))
        {
            currentSlopeAngle = Vector3.Angle(hit.normal, Vector3.up);
            currentSlopeNormal = hit.normal;
            onSteepSlope = currentSlopeAngle > maxWalkableAngle;
        }
        else
        {
            currentSlopeAngle = 0f;
            currentSlopeNormal = Vector3.up;
            onSteepSlope = false;
        }
    }
    #endregion

    #region Jump State Management
    private void UpdateJumpState()
    {
        int newJumpState = currentJumpState;
        jumpStateTimer += Time.fixedDeltaTime;

        switch (currentJumpState)
        {
            case 0: // Normal locomotion
                if (!isGrounded && wasGroundedLastFrame && m_Velocity.y > 0)
                {
                    // Just left ground with upward velocity - start jump
                    newJumpState = 1;
                    jumpStateTimer = 0f;
                }
                else if (!isGrounded && !wasGroundedLastFrame)
                {
                    // Falling off ledge - go straight to float
                    newJumpState = 2;
                    jumpStateTimer = 0f;
                }
                break;

            case 1: // Jump start
                if (jumpStateTimer >= jumpStartDuration || jumpStateTimer >= floatTransitionDelay)
                {
                    // Time to enter float state
                    newJumpState = 2;
                    jumpStateTimer = 0f;
                }
                else if (isGrounded)
                {
                    // Landed before reaching float (very short jump)
                    newJumpState = 3;
                    jumpStateTimer = 0f;
                }
                break;

            case 2: // Float (loops until grounded)
                if (isGrounded)
                {
                    // Landed - enter land state
                    newJumpState = 3;
                    jumpStateTimer = 0f;
                }
                break;

            case 3: // Land
                if (jumpStateTimer >= landDuration)
                {
                    // Landing animation complete - return to normal
                    newJumpState = 0;
                    jumpStateTimer = 0f;
                    m_IsJumping = false;
                    EndJump?.Invoke();
                }
                break;
        }

        // Update jump state if changed
        if (newJumpState != currentJumpState)
        {
            currentJumpState = newJumpState;
            
            if (IsOwner)
            {
                _jumpState_NWV.Value = currentJumpState;
            }
        }
    }
    #endregion

    #region Movement Physics
    private void ApplyMovement()
    {
        Vector3 desiredHorizontal = m_LastInput * (m_IsSprinting ? SprintSpeed : Speed);
        Vector3 currentHorizontal = new Vector3(m_Velocity.x, 0, m_Velocity.z);
        
        // Apply slope speed reduction
        if (onSteepSlope && enableSmoothSlopes)
        {
            float steepnessRatio = (currentSlopeAngle - maxWalkableAngle) / (maxClimbableAngle - maxWalkableAngle);
            float slopeSpeedMod = Mathf.Lerp(1f, slopeSpeedMultiplier, steepnessRatio);
            desiredHorizontal *= slopeSpeedMod;
        }
        
        // Apply acceleration
        Vector3 horizontalAcceleration = (desiredHorizontal - currentHorizontal) * Acceleration;
        m_Velocity.x += horizontalAcceleration.x * Time.fixedDeltaTime;
        m_Velocity.z += horizontalAcceleration.z * Time.fixedDeltaTime;

        // Jumping logic
        if (isGrounded && !m_IsJumping && _input.jumpInput)
        {
            float jumpVelocity = m_IsSprinting ? SprintJumpSpeed : JumpSpeed;
            m_Velocity.y = jumpVelocity;
            m_IsJumping = true;
            StartJump?.Invoke();
        }
        else if (isGrounded && currentJumpState == 0 && m_Velocity.y < 0)
        {
            m_Velocity.y = -2f; // Small downward force to stay grounded
        }
    }

    private void ApplyGravity()
    {
        if (!isGrounded && m_Velocity.y > -TerminalVelocity)
        {
            m_Velocity.y += Gravity * Time.fixedDeltaTime;
        }
    }

    private void ApplyRotation()
    {
        if (hasInput && !strafing)
        {
            Quaternion targetRotation = Quaternion.LookRotation(m_LastInput, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotationSpeed * Time.fixedDeltaTime);
        }
    }

    private void ApplySlopePhysics()
    {
        if (enableSmoothSlopes)
            ApplySmoothSlopePhysics();
        else if (enableSteepSlopeSliding)
            ApplySteepSlopeSliding();
    }
    #endregion

    #region Slope Physics Implementation
    private void ApplySmoothSlopePhysics()
    {
        if (!isGrounded) return;

        if (currentSlopeAngle > maxWalkableAngle)
        {
            Vector3 slideDirection = Vector3.ProjectOnPlane(Vector3.down, currentSlopeNormal).normalized;
            
            if (currentSlopeAngle <= maxClimbableAngle)
            {
                // Temporarily climbable but should slide back
                float steepnessRatio = (currentSlopeAngle - maxWalkableAngle) / (maxClimbableAngle - maxWalkableAngle);
                float currentSlideForce = slideForce * steepnessRatio;
                Vector3 slideVelocity = slideDirection * currentSlideForce;
                
                // Smooth slide velocity blending
                slopeSlideVelocity = Vector3.Lerp(slopeSlideVelocity, slideVelocity, Time.fixedDeltaTime * 5f);
                
                // Apply slide velocity
                m_Velocity.x += slopeSlideVelocity.x * Time.fixedDeltaTime;
                m_Velocity.z += slopeSlideVelocity.z * Time.fixedDeltaTime;
                
                // Reduce player control on steep slopes
                if (hasInput)
                {
                    Vector3 horizontalVel = new Vector3(m_Velocity.x, 0, m_Velocity.z) * momentumDecay;
                    m_Velocity.x = horizontalVel.x;
                    m_Velocity.z = horizontalVel.z;
                }
            }
            else
            {
                // Too steep - strong sliding
                Vector3 strongSlide = slideDirection * slideForce * 2f;
                m_Velocity.x += strongSlide.x * Time.fixedDeltaTime;
                m_Velocity.z += strongSlide.z * Time.fixedDeltaTime;
            }
        }
        else
        {
            // On walkable slope - reduce slide velocity
            slopeSlideVelocity = Vector3.Lerp(slopeSlideVelocity, Vector3.zero, Time.fixedDeltaTime * 10f);
        }
    }

    private void ApplySteepSlopeSliding()
    {
        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out RaycastHit hit, 2f, GroundMask))
        {
            float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
            
            if (slopeAngle > _controller.slopeLimit)
            {
                Vector3 slideDirection = Vector3.ProjectOnPlane(Vector3.down, hit.normal).normalized;
                Vector3 slideForceVec = slideDirection * slideAcceleration;
                
                m_Velocity.x += slideForceVec.x * Time.fixedDeltaTime;
                m_Velocity.z += slideForceVec.z * Time.fixedDeltaTime;
            }
        }
    }
    #endregion

    #region Animation & Network Updates
    private void UpdateAnimationParameters()
    {
        if (!characterAnimator) return;

        if (IsOwner)
        {
            Vector3 localVel = GetLocalVelocity();
            _sidewaysSpeed_NWV.Value = localVel.x;
            _forwardsSpeed_NWV.Value = localVel.z;
        }
        
        // Update locomotion and jump state
        characterAnimator.UpdateAnimatorLocomotion(m_Velocity, transform, IsOwner, _sidewaysSpeed_NWV.Value, _forwardsSpeed_NWV.Value);
        characterAnimator.UpdateJumpState(IsOwner ? currentJumpState : _jumpState_NWV.Value);
    }

    private Vector3 GetLocalVelocity()
    {
        Vector3 localVel = Quaternion.Inverse(transform.rotation) * new Vector3(m_Velocity.x, 0, m_Velocity.z);
        localVel.y = m_Velocity.y;
        return localVel;
    }
    #endregion

    #region Camera Input Frame (Gimbal Lock Prevention)
    private Quaternion GetInputFrame(bool inputDirectionChanged)
    {
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

        // **FIX: Remove roll component from final result**
        Quaternion result = m_TimeInHemisphere >= BlendTime ? 
            (inTopHemisphere ? frameA : frameB) :
            (inTopHemisphere ? 
                Quaternion.Slerp(frameB, frameA, m_TimeInHemisphere / BlendTime) :
                Quaternion.Slerp(frameA, frameB, m_TimeInHemisphere / BlendTime));

        // **CRITICAL FIX: Remove any roll rotation to prevent camera tilt**
        Vector3 euler = result.eulerAngles;
        return Quaternion.Euler(0, euler.y, 0); // Keep only Y rotation (horizontal)
    }
    #endregion

    #region Public Interface
    public virtual void SetstrafingMode(bool b) { strafing = b; }
    public virtual bool IsMoving => m_Velocity.magnitude > 0.1f;

    public void MoveTo(Vector3 pos)
    {
        _controller.enabled = false;
        transform.position = pos;
        _controller.enabled = true;
        
        // Reset all movement state
        m_Velocity = Vector3.zero;
        slopeSlideVelocity = Vector3.zero;
        currentJumpState = 0;
        jumpStateTimer = 0f;
        
        if (IsOwner)
        {
            _jumpState_NWV.Value = 0;
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
    #endregion

    #region Debug Visualization
    private void OnDrawGizmosSelected()
    {
        if (!DebugRaycasts) return;

        // Ground check visualization
        Vector3 spherePosition = transform.position + Vector3.up * GroundedOffset;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(spherePosition, GroundCheckRadius);

        // Slope debug visualization
        if (enableSmoothSlopes && onSteepSlope)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, currentSlopeNormal * 2f);
            
            Vector3 slideDirection = Vector3.ProjectOnPlane(Vector3.down, currentSlopeNormal).normalized;
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, slideDirection * 2f);
        }
    }
    #endregion

    #region Deprecated/Unused Methods
    // These methods exist for compatibility but are unused
    private bool IsGrounded() => isGrounded;
    private Quaternion GetInputFrame() => Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0);
    #endregion
}