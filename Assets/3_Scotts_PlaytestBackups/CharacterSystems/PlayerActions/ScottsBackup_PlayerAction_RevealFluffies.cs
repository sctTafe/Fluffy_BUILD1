using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class ScottsBackup_PlayerAction_RevealFluffies : PlayerActionBase, IHudAbilityBinder
{
    private const bool ISDEBUGGING = true;

    public event Action<float> OnCooldownWithLengthTriggered;
    public event Action OnCooldownCanceled;

    public Action OnActivationSuccess;
    public UnityEvent OnActivationSuccess_UE;

    public Action OnActivationFail_NotEnoughEnergy;
    public Action OnActivationFail_OnCooldown;

    [Header("Target Zone Collider")]
    [SerializeField] private float _activationRadius;

    [Header("Resoruce System")]
    [SerializeField] private ScottsBackup_ResourceMng _staminaSystem;
    [SerializeField] private float _enegryCost = 10f;

    //Cooldown
    [Header("Ability Cooldown")]
    [SerializeField] private float _abilityCooldownLength = 10f;



    private bool _isOnCooldown;
    private bool _inputRecived;

    private void Start()
    {
        // Disable Self If Not Owner
        if (!IsOwner)
        {
            this.enabled = false;
            return;
        }
    }


    public override bool fn_ReceiveActivationInput(bool b)
    {
        if (ISDEBUGGING) Debug.Log("ScottsBackup_PlayerAction_RevealFluffies: ActionInput Recived");
        _inputRecived = b;

        Handle_InputRecived();

        return false;
    }

    private void Handle_InputRecived()
    {
        if (_isOnCooldown)
        {
            if (ISDEBUGGING) Debug.Log("ScottsBackup_PlayerAction_RevealFluffies: OnActivationFail OnCooldown!");
            OnActivationFail_OnCooldown?.Invoke();
            return;
        }

        //check if theres sufficent Stamina 
        if (_staminaSystem.fn_GetCurrentValue() < _enegryCost)
        {
            if (ISDEBUGGING) Debug.Log("ScottsBackup_PlayerAction_RevealFluffies: OnActivationFail NotEnoughEnergy!");
            OnActivationFail_NotEnoughEnergy?.Invoke();
            return;
        }

        // If sucessfull
        if (TryDoAction())
        {
            StartCoroutine(StartCooldown(_abilityCooldownLength));
            _staminaSystem.fn_TryReduceValue(_enegryCost);
            OnActivationSuccess?.Invoke();
            OnActivationSuccess_UE?.Invoke();
            OnCooldownWithLengthTriggered?.Invoke(_abilityCooldownLength);
        }
    }

    /// <summary>
    /// Trys to trigger DestructibleObject_Reciver
    /// </summary>
    private bool TryDoAction()
    {
        return true;
    }

    IEnumerator StartCooldown(float delay)
    {
        _isOnCooldown = true;
        yield return new WaitForSeconds(delay);

        _isOnCooldown = false;

        if (ISDEBUGGING) Debug.Log("ScottsBackup_PlayerAction_RevealFluffies: Cooldown Ended");
    }
}
