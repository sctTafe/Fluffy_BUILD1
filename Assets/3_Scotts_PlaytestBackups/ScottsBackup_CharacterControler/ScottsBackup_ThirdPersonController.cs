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
    [Tooltip("Move speed of the character in m/s")] public float MoveSpeed = 2.0f;
    [Tooltip("Sprint speed of the character in m/s")] public float SprintSpeed = 5.335f;
    [Tooltip("How fast the character turns to face movement direction")][Range(0.0f, 0.3f)] public float RotationSmoothTime = 0.12f;
    [Tooltip("Acceleration and deceleration")] public float SpeedChangeRate = 10.0f;

    public AudioClip LandingAudioClip;
    public AudioClip[] FootstepAudioClips;
    [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

    [Space(10)]
    [Tooltip("The height the player can jump")] public float JumpHeight = 1.2f;
    [Tooltip("The character uses its own gravity value. The engine default is -9.81f")] public float Gravity = -15.0f;

    [Space(10)]
    [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")] public float JumpTimeout = 0.50f;
    [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")] public float FallTimeout = 0.15f;

    [Header("Player Grounded")]
    [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")] public bool Grounded = true;
    [Tooltip("Useful for rough ground")] public float GroundedOffset = -0.14f;
    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")] public float GroundedRadius = 0.28f;
    [Tooltip("What layers the character uses as ground")] public LayerMask GroundLayers;

    [Header("Cinemachine")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")] public GameObject CinemachineCameraTarget;
    [Tooltip("How far in degrees can you move the camera up")] public float TopClamp = 70.0f;
    [Tooltip("How far in degrees can you move the camera down")] public float BottomClamp = -30.0f;
    [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")] public float CameraAngleOverride = 0.0f;
    [Tooltip("For locking the camera position on all axis")] public bool LockCameraPosition = false;

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

    // animation IDs (legacy direct animator support if needed)
    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDFreeFall;
    private int _animIDMotionSpeed;

#if ENABLE_INPUT_SYSTEM
    private PlayerInput _playerInput;
#endif
    private Animator _animator; // legacy animator (can be removed once everything uses CharacterAnimator)
    private CharacterController _controller;
    private ScottsBackupInputSystem _input;
    private GameObject _mainCamera;

    private const float _threshold = 0.01f;
    private bool _hasAnimator;

    private const bool ISDEBUGGING = false; // disable verbose logs

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

    #region Jump State (Local Only)
    // Local jump state machine (no network variables; CharacterAnimator handles replication via MovementState NV)
    private int _localJumpState = 0; // 0=normal,1=jump start,2=float,3=land
    private float _localJumpStateTimer = 0f;
    private bool _wasGroundedLastFrame = true;
    [SerializeField] private float _jumpStartDuration = 0.15f;
    [SerializeField] private float _landDuration = 0.3f;
    [SerializeField] private float _floatTransitionDelay = 0.1f;
    #endregion

    #region Control Overrides
    private bool _isSprinting_Input;
    private bool _isJumping_Input;
    bool _isMovementDistabled = false;
    float _inputMagnitude; // Used for Animations / Blend Tree
    #endregion

    private CharacterAnimator _characterAnimator; // new state-based network animator

    private void Awake()
    {
        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }
    }

    private void Start()
    {
        _hasAnimator = TryGetComponentInChildren(out _animator);
        _characterAnimator = GetComponentInChildren<CharacterAnimator>(true);

        AssignAnimationIDs();
        _controller = GetComponent<CharacterController>();

        if (IsOwner)
        {
            var InputRefs = ScottsBackup_InputRefSingleton.Instance;
            _input = InputRefs._inputs;
            _playerInput = InputRefs._playerInput;

            ScottsBackup_3RDPersonCamMng.Instance.fn_BindChracterToCam(CinemachineCameraTarget.transform);
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }
    }

    private void OnDisable()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
    }

    private void OnEnable()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (IsOwner || IsServer)
        {
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
            {
                if (client.PlayerObject != null && client.PlayerObject.IsSpawned)
                {
                    client.PlayerObject.Despawn();
                }
            }
        }
    }

    private void Update()
    {
        _hasAnimator = TryGetComponentInChildren(out _animator);

        if (!IsOwner) return; // Non-owners rely entirely on CharacterAnimator's network state callbacks

        Update_HandleMovementAndPlayerInput();
        UpdateLocalJumpState();
        UpdateAnimatorOwner();
    }

    private void LateUpdate()
    {
        if (!IsOwner) return;
        CameraRotation();
    }

    #region Public API
    public void fn_SetIsSprintingInput(bool isSprinting) => _isSprinting_Input = isSprinting;

    public bool fn_TryJump()
    {
        if (Grounded && _jumpTimeoutDelta <= 0.0f)
        {
            _isJumping_Input = true;
            return true;
        }
        return false;
    }

    public void fn_Despawn() => NetworkObject.Despawn();
    public void fn_IsMovementInputDisabled(bool isDisabled) => _isMovementDistabled = isDisabled;
    #endregion

    private void Update_HandleMovementAndPlayerInput()
    {
        if (_isMovementDistabled) return;
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
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
        if (_hasAnimator)
        {
            _animator.SetBool(_animIDGrounded, Grounded); // optional legacy animator support
        }
    }

    private void CameraRotation()
    {
        if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
        {
            float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;
            _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
            _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
        }
        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);
        CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride, _cinemachineTargetYaw, 0.0f);
    }

    private void Move()
    {
        float targetSpeed = _isSprinting_Input ? SprintSpeed : MoveSpeed;
        if (_input.move == Vector2.zero) targetSpeed = 0.0f;

        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;
        float speedOffset = 0.1f;
        _inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

        if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * _inputMagnitude, Time.deltaTime * SpeedChangeRate);
            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
        {
            _speed = targetSpeed;
        }

        _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
        if (_animationBlend < 0.01f) _animationBlend = 0f;

        Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;
        if (_input.move != Vector2.zero)
        {
            _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);
            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }

        Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;
        _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
    }

    private void JumpAndGravity()
    {
        if (Grounded)
        {
            _fallTimeoutDelta = FallTimeout;
            if (_verticalVelocity < 0.0f) _verticalVelocity = -2f;
            if (_isJumping_Input && _jumpTimeoutDelta <= 0.0f)
            {
                _isJumping_Input = false;
                _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
            }
            if (_jumpTimeoutDelta >= 0.0f) _jumpTimeoutDelta -= Time.deltaTime;
        }
        else
        {
            _jumpTimeoutDelta = JumpTimeout;
            if (_fallTimeoutDelta >= 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }
        }
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
        Gizmos.color = Grounded ? transparentGreen : transparentRed;
        Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
    }

    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f) OnFootStepEvent?.Invoke();
    }

    private void OnLand(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f) OnLandingEvent?.Invoke();
    }

    private void UpdateAnimatorOwner()
    {
        if (_characterAnimator == null) return;
        _characterAnimator.UpdateAnimatorState(_controller.velocity, transform, true, _isSprinting_Input);
    }

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

        if (newState != _localJumpState)
        {
            _localJumpState = newState;
            if (_characterAnimator != null)
            {
                switch (_localJumpState)
                {
                    case 1: _characterAnimator.TriggerJumpingState(); break;
                    case 2: _characterAnimator.TriggerFloatingState(); break;
                    case 3: _characterAnimator.TriggerFallingState(); break;
                }
            }
        }

        _wasGroundedLastFrame = Grounded;
    }

    private bool TryGetComponentInChildren<T>(out T component) where T : Component
    {
        component = GetComponentInChildren<T>(true);
        return component != null;
    }
}
