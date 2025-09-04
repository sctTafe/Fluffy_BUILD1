using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(ScottsBackup_PlayerStealthMng))]
//[RequireComponent(typeof(Outline))]
public class Receiver_FluffiesReveal : NetworkBehaviour
{

    [SerializeField]private bool _IsRevealed = false;

    [SerializeField] private ModelOutline shader;
    private void Awake()
    {
        //shader = GetComponent<Outline>();
        shader = GetComponentInChildren<ModelOutline>();
    }
    /// <summary>
    /// Triggers the Reveal on this character
    /// enables the outline shader
    /// force unhides
    /// set the is revealed variable on the revealed player side to true to allow the player to realise its revealed 
    /// </summary>
    public void fn_Trigger(float delay = 0f)
    {
        Debug.Log("Receiver_RevealFluffies: revealed");
        
        //trigger shader for mutant side 
        
        shader.enabled = true;
        TriggerRpc(true);
        //Current Version of PlayerStealth only runs locallly
        var c = GetComponent<ScottsBackup_PlayerStealthMng>();
        if (c != null)
            c.force_unhide();
        
    }

    /// <summary>
    /// unreveals the player 
    /// disable the outline shader and then set the revealed status on the player to false (for fluffy side feedback of ability) 
    /// </summary>
    public void fn_Unreveal()
    {

        Debug.Log("Receiver_RevealFluffies: Unrevealed");
        shader.enabled = false;
        TriggerRpc(false);
    }


    //[Rpc.(SendTo.)]
    // ServerRpc - called from client, runs on owner client of networkobject
    /// <summary>
    /// sets the isrevealed variable on the fluffies side so they can know if revealed or not
    /// </summary>
    /// <param name="isrevealed">if the player is set to reveal or not</param>
    [Rpc(SendTo.Owner)]
    private void TriggerRpc(bool isrevealed)
    {
        Debug.Log("Receiver_RevealFluffies: you are revealed");
        _IsRevealed = isrevealed;
        //give revealed player feedback that they are revealed
    }
}
