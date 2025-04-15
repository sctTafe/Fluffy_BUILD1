using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using Unity.Cinemachine;

public class MutantCharacter : CharacterBase // (Assume CharacterBase inherits from NetworkBehaviour)
{
    [Header("Movement Settings")]
    public float Speed = 1f;
    public float SprintSpeed = 4f;
    public float JumpSpeed = 4f;
    public float SprintJumpSpeed = 6f;
    public float Acceleration = 10f;

    [Header("Ground Settings")]
    public Vector3 groundCheckStartOffset = Vector3.zero;
    public float GroundCheckDistance = 0.5f;
    public float GroundCheckRadius = 0.4f;
    public float GroundAlignmentForce = 20f;
    public LayerMask GroundMask;
    public bool DebugRaycasts = true;

    [Header("Mesh Rotation Settings")]
    public Transform cinemachineCamera;
    public Transform meshTransform;

    [Header("Events")]
    public UnityEvent Landed = new UnityEvent();
    public Action PreUpdate;
    public Action<Vector3, float> PostUpdate;
    public Action StartJump;
    public Action EndJump;

    [Header("Camera")]
    public Transform cameraTransform;

    public Animator animator;

    private Rigidbody rb;
    private Vector3 m_LastInput;
    private bool m_IsSprinting;
    private bool m_JumpInput;


    [SerializeField] private NetworkVariable<float> _sidewaysSpeed_NWV = new NetworkVariable<float>(0f);
    [SerializeField] private NetworkVariable<float> _forwardsSpeed_NWV = new NetworkVariable<float>(0f);

    private float _localSidewaysSpeed;
    private float _localForwardsSpeed;
    private float _prevLocalSidewaysSpeed;
    private float _prevLocalForwardsSpeed;
    private const float kAnimThreshold = 0.05f;

    //bradeon stuff 
    public bool isPouncing = false;
    public float startPounceTime;
    public float forwardForce = 1.25f;
    public float upwardForce = 0.5f;

    public GameObject grabBox;
    public GameObject grabPoint;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

    }

    public override void HandleMovement(Vector2 input)
    {
        Quaternion inputFrame = Quaternion.Euler(0, cinemachineCamera ? cinemachineCamera.eulerAngles.y : 0, 0);
        Vector3 rawInput = new Vector3(input.x, 0, input.y);
        m_LastInput = inputFrame * rawInput;
        if (m_LastInput.sqrMagnitude > 1)
            m_LastInput.Normalize();
    }

    //public override void HandleJumpInput()
    //{
    //    m_JumpInput = true;
    //}
    //
    //public override void HandleAim(Vector2 aimInput)
    //{
    //    // Implement aiming if needed.
    //}

    private void Update()
    {
        PreUpdate?.Invoke();
    }

    private void FixedUpdate()
    {
        if (!isPouncing)
            ApplyMotion();
        else
            Pounce();
        ApplyGroundAlignment();
        UpdateAnimationParameters();
        m_JumpInput = false;
    }
    private void Start()
    {
        if (IsOwner)
        {
            CameraManager.Instance.SetFirstPersonCamera(this, cameraTransform);
        }
    }

    // Directly align the mesh's yaw with the camera's yaw.
    private void LateUpdate()
    {
        if (cinemachineCamera != null && meshTransform != null)
        {
            float targetYaw = cinemachineCamera.eulerAngles.y;
            meshTransform.rotation = Quaternion.Euler(0, targetYaw, 0);
        }
    }

    private void ApplyMotion()
    {
        float currentSpeed = m_IsSprinting ? SprintSpeed : Speed;
        Vector3 desiredHorizontal = m_LastInput * currentSpeed;
        Vector3 currentHorizontal = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        Vector3 force = (desiredHorizontal - currentHorizontal) * rb.mass * Acceleration;
        rb.AddForce(force, ForceMode.Force);

        if (m_JumpInput && IsGrounded())
        {
            float jumpImpulse = m_IsSprinting ? SprintJumpSpeed : JumpSpeed;
            rb.AddForce(Vector3.up * jumpImpulse, ForceMode.Impulse);
            StartJump?.Invoke();
        }
    }

    // Raycast downward to maintain a desired distance from the ground.
    private void ApplyGroundAlignment()
    {
        RaycastHit hit;
        float rayLength = GroundCheckDistance + 1f;
        if (Physics.SphereCast(transform.position + groundCheckStartOffset, GroundCheckRadius, Vector3.down, out hit, rayLength, GroundMask))
        {
            if (DebugRaycasts)
                Debug.DrawLine(transform.position + groundCheckStartOffset, hit.point, Color.green);

            float error = GroundCheckDistance - hit.distance;
            if (error > 0)
            {
                rb.AddForce(Vector3.up * error * GroundAlignmentForce, ForceMode.Acceleration);
            }
        }
        else if (DebugRaycasts)
        {
            Debug.DrawRay(transform.position + groundCheckStartOffset, Vector3.down * rayLength, Color.red);
        }
    }

    private void UpdateAnimationParameters()
    {
        if (animator == null)
            return;

        Vector3 localVelocity = transform.InverseTransformDirection(rb.linearVelocity);
        _localForwardsSpeed = localVelocity.z;
        _localSidewaysSpeed = localVelocity.x;

        if (IsOwner)
        {
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
            animator.SetFloat("SidewaysSpeed", _localSidewaysSpeed);
            animator.SetFloat("ForwardsSpeed", _localForwardsSpeed);
        }
        else
        {
            animator.SetFloat("SidewaysSpeed", _sidewaysSpeed_NWV.Value);
            animator.SetFloat("ForwardsSpeed", _forwardsSpeed_NWV.Value);
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

    // Uses a raycast to check if the character is grounded.
    private bool IsGrounded()
    {
        float rayLength = GroundCheckDistance + 0.1f;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, rayLength, GroundMask))
        {
            if (DebugRaycasts)
                Debug.DrawLine(transform.position, hit.point, Color.blue);
            return true;
        }
        else if (DebugRaycasts)
        {
            Debug.DrawRay(transform.position, Vector3.down * rayLength, Color.red);
        }
        return false;
    }

    private Quaternion GetInputFrame()
    {
        return Camera.main ? Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0) : Quaternion.identity;
    }


    public override void Attack()
    {
        
        if (isPouncing) return;

        isPouncing = true;
        startPounceTime = Time.time;


    }


    private void Pounce()
    {
        if (Time.time <= startPounceTime + 1f)
        {
            //cinemachineCamera
            //pounceForce.z
            Vector3 f = new Vector3(meshTransform.forward.x * forwardForce, upwardForce, meshTransform.forward.z * forwardForce);
            Vector3 force = f * rb.mass * Acceleration;
            rb.AddForce(force, ForceMode.Force);
        }
        else
        {

            if (IsGrounded())
            {
                isPouncing = false;
                //grabBox.SetActive(false);
            }
            else
            {
                Vector3 force = (new Vector3(0, 0, forwardForce)) * rb.mass * Acceleration;
                rb.AddForce(force, ForceMode.Force);
            }
            
        }

    }
}
