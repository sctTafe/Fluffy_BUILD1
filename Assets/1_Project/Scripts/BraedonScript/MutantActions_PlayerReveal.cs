using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Unity.Netcode;

public class MutantActions_PlayerReveal : PlayerActionBase, IHudAbilityBinder
{
    public event Action<float> OnCooldownWithLengthTriggered;
    public event Action OnCooldownCanceled;



    [Header("Resoruce System")]
    [SerializeField] private ScottsBackup_ResourceMng _staminaSystem;
    [SerializeField] private float _enegryCost = 50f;


    //Cooldown
    [Header("Ability Cooldown")]
    [SerializeField] private float _abilityCooldownLength = 30f;

    public string _targetTag;

    private bool _isOnCooldown;
    public override bool fn_ReceiveActivationInput(bool b)
    {
        Debug.Log("MutantActions_Player: ActionInput Recived");
        //_inputRecived = b;
        //OnCooldownWithLengthTriggered?.Invoke(0.1f);
        //do action
        Handle_InputRecived();

        return false;
        
    }


    //need the feedback for lacking cooldown or stamina 
    //and need to call a function to disable the reveal after a bit
    private void Handle_InputRecived()
    {
        //check if on cooldown
        if (_isOnCooldown)
        {
            Debug.Log("MutantActions_Player: Action on cooldown");
            return;
        }

        //check if theres sufficent Stamina 
        if (_staminaSystem.fn_GetCurrentValue() < _enegryCost)
        {
            Debug.Log("MutantActions_Player: not enough stamina");
            return;
        }
        //check if action is done succesfully 
        if (TryRevealPlayers())
        {
            //should include the length of the reveal.
            StartCoroutine(StartCooldown(_abilityCooldownLength));
            _staminaSystem.fn_TryReduceValue(_enegryCost);
            //wait a bit before starting since the reveal will last a bit.
            OnCooldownWithLengthTriggered?.Invoke(_abilityCooldownLength);
        } 
        else
        {
            Debug.Log("MutantActions_Player: error");
        }


    }

    /// <summary>
    /// finds all players that aren;t the client and triggers their reveal trigger on this local client
    /// in there it activates a local shader and infrom the owner of the player that they are revealed (bool)
    /// need a way to disable after time.
    /// shader not in
    /// </summary>
    /// <returns>true</returns>
    private bool TryRevealPlayers()
    {
        foreach(ulong player in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if(player != gameObject.GetComponent<NetworkObject>().OwnerClientId)
            {
                //trigger shader
                NetworkManager.Singleton.ConnectedClients[player].PlayerObject.gameObject.TryGetComponent<Receiver_FluffiesReveal>(out Receiver_FluffiesReveal receiver);
                Debug.Log($"MutantActions_Player: found player {player}");
                receiver.fn_Trigger();
            }
        }
        //NetworkManager.Singleton.ConnectedClientsIds;
        return true;
    }




    IEnumerator StartCooldown(float delay)
    {
        _isOnCooldown = true;
        yield return new WaitForSeconds(delay);

        _isOnCooldown = false;

        //if (ISDEBUGGING) Debug.Log("ScottsBackup_PlayerAction_RevealFluffies: Cooldown Ended");
    }
}
