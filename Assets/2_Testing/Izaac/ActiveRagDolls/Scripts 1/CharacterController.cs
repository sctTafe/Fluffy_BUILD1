using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ThirdPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    public float turnSpeed = 10f;

    [Header("Physics Settings")]
    public float groundCheckDistance = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody rb;
    [SerializeField] private Animator animator;
    private Vector3 moveDirection;
    private bool isGrounded;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        HandleMovementInput();
        HandleJumpInput();
        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        MoveCharacter();
        RotateCharacter();
        CheckGroundStatus();
    }

    private void HandleMovementInput()
    {
        float horizontal = Input.GetAxis("Horizontal"); // Left / Right
        float vertical = Input.GetAxis("Vertical"); // Forward / Backward

        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        moveDirection = (camForward * vertical + camRight * horizontal).normalized;
    }

    private void MoveCharacter()
    {
        if (moveDirection.magnitude > 0)
        {
            Vector3 targetVelocity = moveDirection * moveSpeed;
            rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);
        }
    }

    private void RotateCharacter()
    {
        if (moveDirection.sqrMagnitude > 0.01f) // Only rotate when moving
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            Quaternion currentRotation = rb.rotation;

            // Compute the shortest rotation difference
            Quaternion rotationDelta = Quaternion.FromToRotation(currentRotation * Vector3.forward, moveDirection);

            // Convert to angular velocity
            Vector3 torque = new Vector3(0, rotationDelta.eulerAngles.y, 0);

            // Apply torque for smooth rotation
            rb.AddTorque(torque * turnSpeed, ForceMode.Force);
        }
    }

    private void HandleJumpInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void CheckGroundStatus()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);
    }

    private void UpdateAnimator()
    {
        if (animator)
        {
            // Convert input direction to local space
            Vector3 localInput = transform.InverseTransformDirection(moveDirection);

            // Assign values based on input (not velocity)
            animator.SetFloat("ForwardsSpeed", localInput.x);
            animator.SetFloat("SidewaysSpeed", -localInput.z);
        }
    }
}
