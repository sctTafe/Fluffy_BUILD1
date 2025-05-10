using System;
using UnityEngine;

public class ScottsBackup_PlayerAction_Sprint : PlayerActionBase, IHudAbilityBinder
{
    private const bool ISDEBUGGING = false;

    public event Action<float> OnCooldownWithLengthTriggered;
    public event Action OnCooldownCanceled;

    [SerializeField] private ScottsBackup_ThirdPersonController _playerControler;
    [SerializeField] private ScottsBackup_ResourceMng _staminaSystem;
    [SerializeField] private float _sprintCostPerSec = 5f;

    private bool _inputRecived;



    void Start()
    {
        // Disable Self If Not Owner
        if (!IsOwner) 
        {
            this.enabled = false;
            return;
        }
        
        if (_playerControler == null)
            _playerControler = GetComponent<ScottsBackup_ThirdPersonController>();
    }

    void Update()
    {
        if (!IsOwner)
            return;

        if (_inputRecived)
        {
            TrySprint();
            _inputRecived = false;
        }
        else 
        {
            // Set to false if no sprint input recived
            if (ISDEBUGGING) Debug.Log("PlayerAction_Sprint: Is Not Sprinting");
            _playerControler.fn_SetIsSprintingInput(false);
        }
            
    }

 

    public override bool fn_ReceiveActivationInput(bool b)
    {
        if (ISDEBUGGING) Debug.Log("PlayerAction_Sprint: ActionInput Recived");
        _inputRecived = b;
        return false;
    }



    private void TrySprint()
    {
        float spritCost = _sprintCostPerSec * Time.deltaTime;
        bool canSprint = _staminaSystem.fn_TryReduceValue(spritCost);
        if (canSprint)
        {
            if (ISDEBUGGING) Debug.Log("PlayerAction_Sprint: Is Sprinting");
            _playerControler.fn_SetIsSprintingInput(true);
            OnCooldownWithLengthTriggered?.Invoke(0.5f);
        }
        else
        {
            if (ISDEBUGGING) Debug.Log("PlayerAction_Sprint: Is Not Sprinting");
            _playerControler.fn_SetIsSprintingInput(false);
        }

    }



}
