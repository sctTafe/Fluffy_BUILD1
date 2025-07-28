using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(ScottsBackup_PlayerStealthMng))]
public class Receiver_FluffiesReveal : NetworkBehaviour
{

    bool _IsRevealed = false;

    /// <summary>
    /// Triggers the Reveal on this character
    /// </summary>
    public void fn_Trigger(float delay = 0f)
    {
        Debug.Log("Receiver_RevealFluffies: revealed");
        TriggerRpc();
        //trigger shader for mutant side 


        //Current Version of PlayerStealth only runs locallly
        var c = GetComponent<ScottsBackup_PlayerStealthMng>();
        if (c != null)
            c.force_unhide();
        
    }

    //[Rpc.(SendTo.)]
    // ServerRpc - called from client, runs on owner client of networkobject
    [Rpc(SendTo.Owner)]
    private void TriggerRpc()
    {
        Debug.Log("Receiver_RevealFluffies: you are revealed");
        _IsRevealed = true;
    }
}
