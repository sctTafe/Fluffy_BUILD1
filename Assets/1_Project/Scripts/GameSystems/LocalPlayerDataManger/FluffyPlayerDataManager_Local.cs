using StarterAssets;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class FluffyPlayerDataManager_Local : NetworkBehaviour
{
    [Header("References")]
    //private ThirdPersonController_Netcode _playerControler;
    private AnimalCharacter _playerControler;
    private InputManager_Singleton _input;
    private PlayerInput _playerInput;
    private InputActionV2 _inputActions;

    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    public float staminaDrainPerSecond = 20f;
    public float staminaRechargePerSecond = 10f;
    public float exhaustedCooldown = 2f;

    [SerializeField] private float currentStamina;
    [SerializeField] private bool isSprinting;
    [SerializeField] private bool isExhausted;
    [SerializeField] private float exhaustionTimer;
    [SerializeField] private bool isPressingSprint = false;

    void Start()
    {
        // This only runs on the Player Owned Network Prefab
        if(!IsOwner)
            return;

        //Bind To UI Manager
        var mng = LocalPlayerUI_Fluffy.Instance;
        if(mng != null )
            mng.fn_BindLocalPlayerData(this); 


        currentStamina = maxStamina;
        
        _playerControler = GetComponent<AnimalCharacter>();
        //TODO: clean up this mess of a player input system
        _input = InputManager_Singleton.Instance;
        _playerInput = _input._playerInput;

        this._inputActions = _input._inputActions;


        BindButton();
    }

    void BindButton()
    {
        _inputActions.Player.Sprint.performed += ctx => isPressingSprint = true;
        _inputActions.Player.Sprint.canceled += ctx => isPressingSprint = false;
    }



    void Update()
    {
        // This only runs on the Player Owned Network Prefab
        if (!IsOwner)
            return;
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
        isSprinting = isPressingSprint && !isExhausted;
        //_input.sprint
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
