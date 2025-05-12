
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Central input manager singleton for local player input using the new Input System.
/// </summary>
[DefaultExecutionOrder(-100)]
[RequireComponent(typeof(PlayerInput))]
public class InputManager_Singleton : MonoBehaviour
{
    public static InputManager_Singleton Instance { get; private set; }

    public PlayerInputs _playerInput;

    [Header("Character Input Values")]
    public Vector2 movementInput { get; private set; }
    public Vector2 lookInput { get; private set; }
    public bool jumpInput { get; private set; }
    public bool sprintInput { get; private set; }

    [Header("Settings")]
    public bool analogMovement;
    public bool cursorLocked = true;
    public bool cursorInputForLook = true;

    [Header("Sensitivity Settings")]
    [Tooltip("Multiplier for aim (look) input. Adjust to change how fast the camera rotates.")]
    public float AimSensitivity = 0.1f;

    private void Awake()
    {
        SetupSingleton();
        SetupInput();
    }

    private void OnEnable()
    {
        _playerInput?.Player.Enable();
    }

    private void OnDisable()
    {
        _playerInput?.Player.Disable();
    }

    private void SetupSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        //DontDestroyOnLoad(gameObject);
    }

    private void SetupInput()
    {
        _playerInput = new PlayerInputs();

        _playerInput.Player.Move.performed += ctx => movementInput = ctx.ReadValue<Vector2>();
        _playerInput.Player.Move.canceled += ctx => movementInput = Vector2.zero;

        // Multiply the raw look delta by AimSensitivity so that fractional values are preserved.
        _playerInput.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>() * AimSensitivity;
        _playerInput.Player.Look.canceled += ctx => lookInput = Vector2.zero;

        _playerInput.Player.Jump.performed += ctx => jumpInput = true;
        _playerInput.Player.Jump.canceled += ctx => jumpInput = false;

        _playerInput.Player.Sprint.performed += ctx => sprintInput = true;
        _playerInput.Player.Sprint.canceled += ctx => sprintInput = false;
    }

    // --- Cursor Management ---
    private void OnApplicationFocus(bool hasFocus)
    {
        SetCursorState(cursorLocked);
    }

    private void SetCursorState(bool newState)
    {
        Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !newState;
    }
}
