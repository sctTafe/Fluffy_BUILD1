using System;
using UnityEngine;
using UnityEngine.Events;

public class ScottsBackup_PlayerAction_Jump : PlayerActionBase, IHudAbilityBinder
{
    private const bool ISDEBUGGING = false;

    public event Action<float> OnCooldownWithLengthTriggered;
    public event Action OnCooldownCanceled;

    public UnityEvent _OnAvilityActivation;

    [SerializeField] private ScottsBackup_ThirdPersonController _playerControler;
    [SerializeField] private ScottsBackup_ResourceMng _staminaSystem;
    [SerializeField] private float _abilityUseCost = 2f;
    [SerializeField] private float _abilityCoolDown = 0.5f;

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
            TryAction();
            _inputRecived = false;
        }

    }

    public override bool fn_ReceiveActivationInput(bool b)
    {
        if (ISDEBUGGING) Debug.Log("ScottsBackup_PlayerAction_Jump: ActionInput Recived");
        _inputRecived = b;
        OnCooldownWithLengthTriggered?.Invoke(0.1f);
        return false;
    }

    private void TryAction()
    {
        float spritCost = _abilityUseCost * Time.deltaTime;
        bool canAffordAbilityUsage = _staminaSystem.fn_GetCurrentValue() > _abilityUseCost? true: false;

        if (canAffordAbilityUsage)
        {
            if (_playerControler.fn_TryJump())
            {
                if (ISDEBUGGING) Debug.Log("ScottsBackup_PlayerAction_Jump: Player Jump Activation Tirggered");
                //Is Jumping
                _staminaSystem.fn_TryReduceValue(_abilityUseCost); //Deduct Stamina 
                OnCooldownWithLengthTriggered?.Invoke(_abilityCoolDown);
                _OnAvilityActivation?.Invoke();
            }
            else
            {
                if (ISDEBUGGING) Debug.Log("ScottsBackup_PlayerAction_Jump: Character Controler Not Able To Jump Currently");
            }
        }
        else
        {
            if (ISDEBUGGING) Debug.Log("ScottsBackup_PlayerAction_Jump: Cannot Afford");
        }
    }
}
