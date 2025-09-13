using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Unity.Netcode;

public class PlayerAction_MutantBreath : PlayerActionBase, IHudAbilityBinder
{
    private const bool ISDEBUGGING = false;

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

    public string _targetTag;

    private bool _isOnCooldown;
    private bool _inputRecived;

    // Animator for playing local fart/reveal animations
    [SerializeField] private Animator _animator;

    private void Start()
    {
        // find animator on this object or children if not assigned in inspector
        if (_animator == null)
            _animator = GetComponentInChildren<Animator>(true);

        if (_animator == null && ISDEBUGGING)
        {
            Debug.LogWarning($"PlayerAction_MutantBreath: Animator not found on '{name}' or its children.");
        }

        // Need to be enable to play the effect across the netwrok
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(this.transform.position, _activationRadius);
    }

    public override bool fn_ReceiveActivationInput(bool b)
    {
        if (ISDEBUGGING) Debug.Log("PlayerAction_MutantBreath: ActionInput Recived");
        _inputRecived = b;
        OnCooldownWithLengthTriggered?.Invoke(0.1f);
        Handle_InputRecived();

        return false;
    }

    private void Handle_InputRecived()
    {
        if (_isOnCooldown)
        {
            if (ISDEBUGGING) Debug.Log("PlayerAction_MutantBreath: OnActivationFail OnCooldown!");
            OnActivationFail_OnCooldown?.Invoke();
            return;
        }

        //check if theres sufficent Stamina 
        if (_staminaSystem.fn_GetCurrentValue() < _enegryCost)
        {
            if (ISDEBUGGING) Debug.Log("PlayerAction_MutantBreath: OnActivationFail NotEnoughEnergy!");
            OnActivationFail_NotEnoughEnergy?.Invoke();
            return;
        }

        // If sucessfull
        if (TryDoAction())
        {
            // play mutant fart animation/effect locally and request network broadcast
            PlayMutantFartLocal();
            RequestPlayMutantFartServerRpc();
            SendOnActionPerformedRpc();
            StartCoroutine(StartCooldown(_abilityCooldownLength));
            _staminaSystem.fn_TryReduceValue(_enegryCost);
            OnCooldownWithLengthTriggered?.Invoke(_abilityCooldownLength);
        }
        else
        {
            if (ISDEBUGGING) Debug.Log($"PlayerAction_MutantBreath: ");
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
            if(col.CompareTag(_targetTag))
            {
                if (ISDEBUGGING) Debug.Log($"PlayerAction_MutantBreath: Found Player");

                col.gameObject.TryGetComponent<ScottsBackup_Receiver_RevealFluffies>(out ScottsBackup_Receiver_RevealFluffies dr);
                if (ISDEBUGGING) Debug.Log($"PlayerAction_MutantBreath: Hit {col.name}");

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

        if (ISDEBUGGING) Debug.Log("PlayerAction_MutantBreath: Cooldown Ended");
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

        if (ISDEBUGGING) Debug.Log("PlayerAction_MutantBreath: ClientRPC SendOnActionPerformedClientRpc Called!");
    }

    // helper: check if animator has a parameter
    private bool AnimatorHasParameter(string paramName)
    {
        if (_animator == null) return false;
        foreach (var p in _animator.parameters)
        {
            if (p.name == paramName) return true;
        }
        return false;
    }

    // Play local fart animation/effect
    private void PlayMutantFartLocal()
    {
        if (_animator == null)
        {
            Debug.LogWarning("PlayMutantFartLocal: animator is null");
            return;
        }

        // rebind to ensure parameters are available (safe call)
        try { _animator.Rebind(); } catch { }

        _animator.SetTrigger("MutantFart");
    }

    // ServerRpc to request network broadcast
    [ServerRpc]
    private void RequestPlayMutantFartServerRpc()
    {
        PlayMutantFartClientRpc();
    }

    // ClientRpc to play on all clients
    [ClientRpc]
    private void PlayMutantFartClientRpc()
    {
        if (!_animator.enabled)
            _animator.enabled = true;

        try { _animator.Rebind(); } catch { }

        _animator.SetTrigger("MutantFart");
    }

}
