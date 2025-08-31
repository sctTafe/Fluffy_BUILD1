using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]

public class ScottsBackup_ThirdPersonController : NetworkBehaviour
{
    public Action OnLandingEvent;   // Landing Sounds
    public Action OnFootStepEvent;  // Footstep Sounds

    [Header("Player")]
    [Tooltip("Move speed of the character in m/s")]
    public float MoveSpeed = 2.0f;

    [Tooltip("Sprint speed of the character in m/s")]
    public float SprintSpeed = 5.335f;

    [Tooltip("How fast the character turns to face movement direction")]
    [Range(0.0f, 0.3f)]
    public float RotationSmoothTime = 0.12f;

    [Tooltip("Acceleration and deceleration")]
    public float SpeedChangeRate = 10.0f;

    public AudioClip LandingAudioClip;
    public AudioClip[] FootstepAudioClips;
    [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

    [Space(10)]
    [Tooltip("The height the player can jump")]
    public float JumpHeight = 1.2f;

    [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
    public float Gravity = -15.0f;

    [Space(10)]
    [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
    public float JumpTimeout = 0.50f;

    [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
    public float FallTimeout = 0.15f;

    [Header("Player Grounded")]
    [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
    public bool Grounded = true;

    [Tooltip("Useful for rough ground")]
    public float GroundedOffset = -0.14f;

    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    public float GroundedRadius = 0.28f;

    [Tooltip("What layers the character uses as ground")]
    public LayerMask GroundLayers;

    [Header("Cinemachine")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    public GameObject CinemachineCameraTarget;

    [Tooltip("How far in degrees can you move the camera up")]
    public float TopClamp = 70.0f;

    [Tooltip("How far in degrees can you move the camera down")]
    public float BottomClamp = -30.0f;

    [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
    public float CameraAngleOverride = 0.0f;

    [Tooltip("For locking the camera position on all axis")]
    public bool LockCameraPosition = false;

    // cinemachine
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;

    // player
    private float _speed;
    private float _animationBlend;
    private float _targetRotation = 0.0f;
    private float _rotationVelocity;
    private float _verticalVelocity;
    private float _terminalVelocity = 53.0f;

    // timeout deltatime
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;

    // animation IDs (kept if you still want raw animator access)
    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDFreeFall;
    private int _animIDMotionSpeed;

#if ENABLE_INPUT_SYSTEM
    private PlayerInput _playerInput;
#endif
    private Animator _animator;
    private CharacterController _controller;
    private ScottsBackupInputSystem _input;
    private GameObject _mainCamera;

    private const float _threshold = 0.01f;

    private bool _hasAnimator;

    private bool IsCurrentDeviceMouse
    {
        get
        {
#if ENABLE_INPUT_SYSTEM
            return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
        }
    }

    #region Network Animations (compact)
    // compact NetworkVariables for the new Animator controller
    // Owner writes, everyone reads
    [HideInInspector] public NetworkVariable<float> _forwardsSpeed_NWV =
        new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [HideInInspector] public NetworkVariable<float> _sidewaysSpeed_NWV =
        new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [HideInInspector] public NetworkVariable<int> _jumpState_NWV =
        new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    // Local anim flags (used to compute jumpState)
    bool _animVarLocal_Jump;
    bool _animVarLocal_Freefall;

    // local change detect var (reintroduced)
    float _animVarLocalSpeed_Prev;
    float _animVarLocalMotionSpeed_Prev;

    // Jump state machine (local) to avoid stuck land state
    private int _localJumpState = 0; // 0=normal,1=jump start,2=float,3=land
    private float _localJumpStateTimer = 0f;
    private bool _wasGroundedLastFrame = true;
    [SerializeField] private float _jumpStartDuration = 0.15f;
    [SerializeField] private float _landDuration = 0.3f;
    [SerializeField] private float _floatTransitionDelay = 0.1f;
    #endregion

    #region Control Overrides
    // External Movement Inputs (public fn Driven)
    private bool _isSprinting_Input;
    private bool _isJumping_Input;
    bool _isMovementDistabled = false;

    float _inputMagnitude; // Used for Animations / Blend Tree
    #endregion

    // bridge to the CharacterAnimator that lives on a child
    private CharacterAnimator _characterAnimator;

    private void Awake()
    {
        // get a reference to our main camera
        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }
    }

    private void Start()
    {
        // find Animator/CharacterAnimator on this object or any child (including inactive)
        _hasAnimator = TryGetComponentInChildren(out _animator);
        _characterAnimator = GetComponentInChildren<CharacterAnimator>(true);

        AssignAnimationIDs();
        _controller = GetComponent<CharacterController>();

        // subscribe to NetworkVariable changes to apply on clients immediately
        if (_characterAnimator != null)
        {
            _forwardsSpeed_NWV.OnValueChanged += OnNetworkAnimChangedFloat;
            _sidewaysSpeed_NWV.OnValueChanged += OnNetworkAnimChangedFloat;
            _jumpState_NWV.OnValueChanged += OnNetworkAnimChangedInt;
        }
        else
        {
            Debug.LogWarning($"CharacterAnimator not found on '{name}' or its children.");
        }

        if (IsOwner)
        {
            var InputRefs = ScottsBackup_InputRefSingleton.Instance;
            _input = InputRefs._inputs;
            _playerInput = InputRefs._playerInput;

            ScottsBackup_3RDPersonCamMng.Instance.fn_BindChracterToCam(CinemachineCameraTarget.transform);
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }
    }

    private void OnDisable()
    {
        // unsubscribe to avoid leaks
        if (_forwardsSpeed_NWV != null) _forwardsSpeed_NWV.OnValueChanged -= OnNetworkAnimChangedFloat;
        if (_sidewaysSpeed_NWV != null) _sidewaysSpeed_NWV.OnValueChanged -= OnNetworkAnimChangedFloat;
        if (_jumpState_NWV != null) _jumpState_NWV.OnValueChanged -= OnNetworkAnimChangedInt;

        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
    }

    #region NEW
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        Debug.Log($" This object; {this.name}, is in Scene name: {gameObject.scene.name}, DestroyWithScene: {this.GetComponent<NetworkObject>().DestroyWithScene}");
    }
    public override void OnDestroy()
    {
        Debug.Log($"PlayerObject {gameObject.name}  is being destroyed.");
        base.OnDestroy();
    }

    private void OnEnable()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private void OnDisableNetworkCallback()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (IsOwner || IsServer)
        {
            Debug.Log($"Client {clientId} disconnected. Cleaning up player object.");

            // Find and despawn the player's object
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
            {
                if (client.PlayerObject != null && client.PlayerObject.IsSpawned)
                {
                    client.PlayerObject.Despawn();
                }
            }
        }
    }
    #endregion

    private void Update()
    {
        // keep animator reference up-to-date (search children)
        _hasAnimator = TryGetComponentInChildren(out _animator);

        if (IsOwner)
        {
            Update_HandleMovementAndPlayerInput();
            Update_NetworkAnimationVaraibles(); // write compact values into NWVs

            // update local jump state machine based on grounded/vertical velocity
            UpdateLocalJumpState();

            // drive local animator via CharacterAnimator for immediate feedback
            UpdateOwnerAnimatorLocal();
        }
        else
        {
            if (IsClient)
            {
                // apply networked values to child CharacterAnimator
                ApplyNetworkAnimValues();
            }
        }
    }

    private void LateUpdate()
    {
        if (!IsOwner)
            return;

        CameraRotation();
    }

    #region Public Functions

    public void fn_SetIsSprintingInput(bool isSprinting) => _isSprinting_Input = isSprinting;

    /// <summary>
    /// If able to jump, it activates the jump & returns true 
    /// </summary>
    public bool fn_TryJump()
    {
        if (Grounded && _jumpTimeoutDelta <= 0.0f)
        {
            //Can Jump
            _isJumping_Input = true;
            return true;
        }
        else return false;
    }

    public void fn_Despawn() => NetworkObject.Despawn();

    public void fn_IsMovementInputDisabled(bool isDisabled) => _isMovementDistabled = isDisabled;

    #endregion

    private void Update_HandleMovementAndPlayerInput()
    {
        // If movement is disabled return.
        if (_isMovementDistabled)
            return;

        JumpAndGravity();
        GroundedCheck();
        Move();
    }

    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    }

    private void GroundedCheck()
    {
        // set sphere position, with offset
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
            transform.position.z);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
            QueryTriggerInteraction.Ignore);

        // still update legacy animator if present (kept for compatibility)
        if (_hasAnimator)
        {
            _animator.SetBool(_animIDGrounded, Grounded);
        }
    }

    private void CameraRotation()
    {
        // if there is an input and camera position is not fixed
        if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
        {
            //Don't multiply mouse input by Time.deltaTime;
            float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

            _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
            _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
        }

        // clamp our rotations so our values are limited 360 degrees
        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

        // Cinemachine will follow this target
        CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
            _cinemachineTargetYaw, 0.0f);
    }

    private void Move()
    {
        // set target speed based on move speed, sprint speed and if sprint is pressed
        float targetSpeed = _isSprinting_Input ? SprintSpeed : MoveSpeed;

        // if there is no input, set the target speed to 0
        if (_input.move == Vector2.zero) targetSpeed = 0.0f;

        // a reference to the players current horizontal velocity
        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

        float speedOffset = 0.1f;
        _inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

        // accelerate or decelerate to target speed
        if (currentHorizontalSpeed < targetSpeed - speedOffset ||
            currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * _inputMagnitude,
                Time.deltaTime * SpeedChangeRate);

            // round speed to 3 decimal places
            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
        {
            _speed = targetSpeed;
        }

        _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
        if (_animationBlend < 0.01f) _animationBlend = 0f;

        // normalise input direction
        Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

        if (_input.move != Vector2.zero)
        {
            _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                              _mainCamera.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                RotationSmoothTime);

            // rotate to face input direction relative to camera position
            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }

        Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

        // move the player
        _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                         new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
    }

    private void JumpAndGravity()
    {
        if (Grounded)
        {
            // reset the fall timeout timer
            _fallTimeoutDelta = FallTimeout;

            _animVarLocal_Jump = false;
            _animVarLocal_Freefall = false;

            // stop our velocity dropping infinitely when grounded
            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2f;
            }

            // Jump
            if (_isJumping_Input && _jumpTimeoutDelta <= 0.0f)
            {
                _isJumping_Input = false;

                // the square root of H * -2 * G = how much velocity needed to reach desired height
                _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                _animVarLocal_Jump = true;
            }

            // jump timeout
            if (_jumpTimeoutDelta >= 0.0f)
            {
                _jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else
        {
            // reset the jump timeout timer
            _jumpTimeoutDelta = JumpTimeout;

            // fall timeout
            if (_fallTimeoutDelta >= 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                _animVarLocal_Freefall = true;
            }
        }

        // apply gravity over time if under terminal
        if (_verticalVelocity < _terminalVelocity)
        {
            _verticalVelocity += Gravity * Time.deltaTime;
        }
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    private void OnDrawGizmosSelected()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        if (Grounded) Gizmos.color = transparentGreen;
        else Gizmos.color = transparentRed;

        // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
        Gizmos.DrawSphere(
            new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
            GroundedRadius);
    }

    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            OnFootStepEvent?.Invoke();
        }
    }

    private void OnLand(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            OnLandingEvent?.Invoke();
        }
    }

    #region Animation -> Network bridge (compact)

    // Called on clients when network values change (float overload)
    private void OnNetworkAnimChangedFloat(float oldValue, float newValue)
    {
        ApplyNetworkAnimValues();
    }

    // Called on clients when network values change (int overload)
    private void OnNetworkAnimChangedInt(int oldValue, int newValue)
    {
        ApplyNetworkAnimValues();
    }

    private void ApplyNetworkAnimValues()
    {
        if (_characterAnimator == null) return;

        // CharacterAnimator expects (velocity, transform, isOwner, networkSideways, networkForward)
        _characterAnimator.UpdateAnimatorLocomotion(Vector3.zero, transform, false, _sidewaysSpeed_NWV.Value, _forwardsSpeed_NWV.Value);
        _characterAnimator.UpdateJumpState(_jumpState_NWV.Value);
    }

    // Owner-side: update local animator immediately and write compact values to NetworkVariables (owner-writable)
    private void UpdateOwnerAnimatorLocal()
    {
        if (_characterAnimator == null) return;

        // owner uses local velocity for accurate blends
        _characterAnimator.UpdateAnimatorLocomotion(_controller.velocity, transform, true, 0f, 0f);

        // use local jump state machine
        int jumpState = _localJumpState;

        // compute local velocity in character space for network sync
        Vector3 localVel = transform.InverseTransformDirection(_controller.velocity);
        float localForward = localVel.z;
        float localSideways = localVel.x;

        // apply small deadzone to avoid jitter at very low speeds
        const float deadzone = 0.05f;
        if (Mathf.Abs(localForward) < deadzone) localForward = 0f;
        if (Mathf.Abs(localSideways) < deadzone) localSideways = 0f;

        // write to NetworkVariables only when changed (simple change detection)
        bool forwardChanged = Mathf.Abs(_forwardsSpeed_NWV.Value - localForward) > 0.01f;
        bool sidewaysChanged = Mathf.Abs(_sidewaysSpeed_NWV.Value - localSideways) > 0.01f;
        bool jumpChanged = _jumpState_NWV.Value != jumpState;

        if (forwardChanged) _forwardsSpeed_NWV.Value = localForward;
        if (sidewaysChanged) _sidewaysSpeed_NWV.Value = localSideways;
        if (jumpChanged) _jumpState_NWV.Value = jumpState;

        // update previous markers
        _animVarLocalSpeed_Prev = localForward;
        _animVarLocalMotionSpeed_Prev = localSideways;
    }

    private void Update_NetworkAnimationVaraibles()
    {
        // This function kept for backward-compatible change detection of movement -> network variables.
        // The actual writing is done in UpdateOwnerAnimatorLocal() (owner writes directly to NWVs).
        // Keep the function to preserve existing call sites and future logic if needed.
    }

    // New: local jump state machine copied/adapted from AnimalCharacter
    private void UpdateLocalJumpState()
    {
        int newState = _localJumpState;
        _localJumpStateTimer += Time.deltaTime;

        switch (_localJumpState)
        {
            case 0: // normal
                if (!Grounded && _wasGroundedLastFrame && _verticalVelocity > 0)
                {
                    newState = 1; // jump start
                    _localJumpStateTimer = 0f;
                }
                else if (!Grounded && !_wasGroundedLastFrame)
                {
                    newState = 2; // fall/float
                    _localJumpStateTimer = 0f;
                }
                break;

            case 1: // jump start
                if (_localJumpStateTimer >= _jumpStartDuration || _localJumpStateTimer >= _floatTransitionDelay)
                {
                    newState = 2; // enter float
                    _localJumpStateTimer = 0f;
                }
                else if (Grounded)
                {
                    newState = 3; // landed quickly
                    _localJumpStateTimer = 0f;
                }
                break;

            case 2: // float/fall
                if (Grounded)
                {
                    newState = 3; // land
                    _localJumpStateTimer = 0f;
                }
                break;

            case 3: // land
                if (_localJumpStateTimer >= _landDuration)
                {
                    newState = 0; // back to normal
                    _localJumpStateTimer = 0f;
                }
                break;
        }

        // if changed, write to network var so others see the change
        if (newState != _localJumpState)
        {
            _localJumpState = newState;
            if (IsOwner)
            {
                _jumpState_NWV.Value = _localJumpState;
            }
        }

        _wasGroundedLastFrame = Grounded;
    }

    #endregion

    // Helper: try-get component in children (including inactive)
    private bool TryGetComponentInChildren<T>(out T component) where T : Component
    {
        component = GetComponentInChildren<T>(true);
        return component != null;
    }
}

































