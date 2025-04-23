using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;

public class MutantPlayerDataManager_Local : PlayerDataManger_Local
{
    [Header("References")]
    private ThirdPersonController_Netcode _playerControler;
    private InputManager_Singleton _input;
    private PlayerInput _playerInput;

    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    public float staminaDrainPerSecond = 20f;
    public float staminaRechargePerSecond = 10f;
    public float exhaustedCooldown = 2f;

    private float currentStamina;
    private bool isSprinting;
    private bool isExhausted;
    private float exhaustionTimer;

    void Start()
    {
        currentStamina = maxStamina;

        //TODO: clean up this mess of a player input system
        _input = InputManager_Singleton.Instance;
        _playerInput = _input._playerInput;
    }

    void Update()
    {
        HandleInput();
        HandleStamina();
    }

    public float fn_GetStaminaPercent()
    {
        return currentStamina / maxStamina;
    }

    private void HandleInput()
    {
        // Example input: hold Left Shift to sprint
        // isSprinting = Input.GetKey(KeyCode.LeftShift) && !isExhausted;
        isSprinting = _input.sprint && !isExhausted;
    }

    private void HandleStamina()
    {
        if (isSprinting)
        {
            // IS Sprinting 
            _playerControler.fn_SetIsSprintingInput(true);

            // Deduct stamina 
            currentStamina -= staminaDrainPerSecond * Time.deltaTime;

            // Check for / handle Exhaustion
            if (currentStamina <= 0f)
            {
                _playerControler.fn_SetIsSprintingInput(false);

                currentStamina = 0f;
                isExhausted = true;
                exhaustionTimer = exhaustedCooldown;
            }
        }
        else
        {
            // Is Not Sprinting 
            _playerControler.fn_SetIsSprintingInput(false);

            // Regen stamina
            currentStamina += staminaRechargePerSecond * Time.deltaTime;
            currentStamina = Mathf.Min(currentStamina, maxStamina);
        }

        if (isExhausted)
        {
            exhaustionTimer -= Time.deltaTime;
            if (exhaustionTimer <= 0f)
            {
                isExhausted = false;
            }
        }
    }
}