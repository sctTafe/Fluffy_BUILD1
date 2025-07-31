using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Unity.Netcode;
/// <summary>
/// the mutants full map player reveal ability (for proper use fluffies must have the Receiver_FluffiesReveal script)
/// -------needs span prevention (only really need for after its used )
/// ------- looks at all the feed back things'
/// --- ---- look at the other reveal function and its rpcs 
/// </summary>
public class MutantActions_PlayerReveal : PlayerActionBase, IHudAbilityBinder
{
    public event Action<float> OnCooldownWithLengthTriggered;
    //what is below used for 
    public event Action OnCooldownCanceled;

    [Header("Resoruce System")]
    [SerializeField] private ScottsBackup_ResourceMng _staminaSystem;
    [SerializeField] private float _enegryCost = 50f;


    //Cooldown
    [Header("Ability Cooldown")]
    [SerializeField] private float _abilityCooldownLength = 30f;

    public string _targetTag;

    private bool _isOnCooldown;

    //having the duration calculated on the mutants side instead of every player is best as it means only one timer
    //active ability
    private bool _isRevealedOn;
    [SerializeField] private float _RevealDuration = 5f;


    public override bool fn_ReceiveActivationInput(bool b)
    {
        Debug.Log("MutantActions_Player: ActionInput Recived");
        //_inputRecived = b;
        //OnCooldownWithLengthTriggered?.Invoke(0.1f);
        Handle_InputRecived();

        return false;
        
    }


    //need the feedback for lacking cooldown or stamina 
    // calls a function to disable the reveal after a bit
    private void Handle_InputRecived()
    {
        //check if on cooldown
        if (_isOnCooldown || _isRevealedOn)
        {
            Debug.Log("MutantActions_Player: Action on cooldown");
            //for feedback or something
            //OnActivationFail_OnCooldown?.Invoke();
            return;
        }

        //check if theres sufficent Stamina 
        if (_staminaSystem.fn_GetCurrentValue() < _enegryCost)
        {
            Debug.Log("MutantActions_Player: not enough stamina");
            //OnActivationFail_NotEnoughEnergy?.Invoke();
            return;
        }
        //check if action is done succesfully 
        if (TryRevealPlayers())
        {
            
            _staminaSystem.fn_TryReduceValue(_enegryCost);
            Invoke("TryUnrevealPlayers", _RevealDuration);
            StartCoroutine(TryUnrevealPlayers());
            //should start after duration ends and 
            //StartCoroutine(StartCooldown(_abilityCooldownLength));
            //wait a bit before starting since the reveal will last a bit.
            //OnCooldownWithLengthTriggered?.Invoke(_abilityCooldownLength);
        } 
        else
        {
            Debug.Log("MutantActions_Player: error");
        }


    }

    /// <summary>
    /// finds all players that aren;t the client and triggers their reveal trigger on this local client if they have the receiver
    /// in there it activates a local shader and infrom the owner of the player object that they are revealed (bool)
    /// need a way to disable after time. 
    /// </summary>
    /// <returns>true</returns>
    private bool TryRevealPlayers()
    {
        foreach(ulong player in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if(player != gameObject.GetComponent<NetworkObject>().OwnerClientId)
            {
                //trigger shader
                if (NetworkManager.Singleton.ConnectedClients[player].PlayerObject.gameObject.TryGetComponent<Receiver_FluffiesReveal>(out Receiver_FluffiesReveal receiver))
                {
                    Debug.Log($"MutantActions_Player: found player {player}");
                    receiver.fn_Trigger();
                }
                
            }
        }
        //NetworkManager.Singleton.ConnectedClientsIds;
        return true;
    }

    //after the duration of the ability trys to unreveal players then starts the cooldown.
    IEnumerator TryUnrevealPlayers()
    {
        _isRevealedOn = true;

        yield return new WaitForSeconds(_RevealDuration);

        foreach (ulong player in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (player != gameObject.GetComponent<NetworkObject>().OwnerClientId)
            {
                //trigger shader
                if (NetworkManager.Singleton.ConnectedClients[player].PlayerObject.gameObject.TryGetComponent<Receiver_FluffiesReveal>(out Receiver_FluffiesReveal receiver))
                {
                    Debug.Log($"MutantActions_Player: found player {player}");
                    receiver.fn_Unreveal();
                }
            }
        }
        _isRevealedOn = false;


        //cooldown 
        OnCooldownWithLengthTriggered?.Invoke(_abilityCooldownLength);
        _isOnCooldown = true;
        yield return new WaitForSeconds(_abilityCooldownLength);

        _isOnCooldown = false;
        //StartCoroutine(StartCooldown(_abilityCooldownLength));
        //OnCooldownWithLengthTriggered?.Invoke(_abilityCooldownLength);
    }

    IEnumerator StartCooldown(float delay)
    {
        _isOnCooldown = true;
        yield return new WaitForSeconds(delay);

        _isOnCooldown = false;

        //if (ISDEBUGGING) Debug.Log("ScottsBackup_PlayerAction_RevealFluffies: Cooldown Ended");
    }


}
