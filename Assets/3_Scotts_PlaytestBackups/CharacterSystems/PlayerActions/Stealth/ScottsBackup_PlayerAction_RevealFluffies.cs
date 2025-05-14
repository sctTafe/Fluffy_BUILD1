using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Unity.Netcode;

public class ScottsBackup_PlayerAction_RevealFluffies : PlayerActionBase, IHudAbilityBinder
{
    private const bool ISDEBUGGING = true;

    // IHudAbilityBinder
    public event Action<float> OnCooldownWithLengthTriggered;
    public event Action OnCooldownCanceled;

    // On Action Performed - Link to mutant feedback
    public Action OnActivationSuccess;
    public UnityEvent OnActivationSuccess_UE;

    public Action OnActivationFail_NotEnoughEnergy;
    public Action OnActivationFail_OnCooldown;

    [Header("Target Zone Collider")]
    [SerializeField] private float _activationRadius;
    [SerializeField] private LayerMask _targetLayer;
    [SerializeField] private float _effectDelay = 0.5f;

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
        //if (!IsOwner)
        //{
        //    this.enabled = false;
        //    return;
        //}

        // Need to be enable to play the effect across the netwrok
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(this.transform.position, _activationRadius);
        
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
            SendOnActionPerformedRpc();
            StartCoroutine(StartCooldown(_abilityCooldownLength));
            _staminaSystem.fn_TryReduceValue(_enegryCost);
            OnCooldownWithLengthTriggered?.Invoke(_abilityCooldownLength);
        }
    }

    /// <summary>
    /// Trys to trigger DestructibleObject_Reciver
    /// </summary>
    private bool TryDoAction()
    {      
        var hitColliders = Physics.OverlapSphere(this.transform.position, _activationRadius, _targetLayer);
        foreach (Collider col in hitColliders)
        {          
            if(col.gameObject.TryGetComponent<ScottsBackup_Receiver_RevealFluffies>(out ScottsBackup_Receiver_RevealFluffies dr))
            {
                if (ISDEBUGGING) Debug.Log($"ScottsBackup_PlayerAction_RevealFluffies: Hit {col.name}");
                dr.fn_Trigger();
            }
        }

        //always return true for this ability
        return true;
    }




    IEnumerator StartCooldown(float delay)
    {
        _isOnCooldown = true;
        yield return new WaitForSeconds(delay);

        _isOnCooldown = false;

        if (ISDEBUGGING) Debug.Log("ScottsBackup_PlayerAction_RevealFluffies: Cooldown Ended");
    }



    // --- _onDestructibleGettingAttacked ---
    // ServerRpc - called from client, runs on server
    [Rpc(SendTo.Server)]
    private void SendOnActionPerformedRpc()
    {
        SendOnActionPerformedClientRpc();
    }

    // ClientRpc - called from server, runs on all clients
    [Rpc(SendTo.ClientsAndHost)]
    private void SendOnActionPerformedClientRpc()
    {

        OnActivationSuccess?.Invoke();
        OnActivationSuccess_UE?.Invoke();

        if (ISDEBUGGING) Debug.Log("ScottsBackup_PlayerAction_RevealFluffies: ClientRPC SendOnActionPerformedClientRpc Called!");
    }
}
