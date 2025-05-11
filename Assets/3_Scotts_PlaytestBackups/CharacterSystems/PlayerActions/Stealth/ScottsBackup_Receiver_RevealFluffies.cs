using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(PlayerStealth))]
public class ScottsBackup_Receiver_RevealFluffies : NetworkBehaviour
{
    private const bool ISDEBUGGING = true;

    public UnityEvent _onReavealTriggered;

    /// <summary>
    /// Triggers the Reveal on this character
    /// </summary>
    internal void fn_Trigger(float delay = 0f)
    {
        if (ISDEBUGGING) Debug.Log("RevealFluffies_Receiver: fn_Trigger Called!");
        TriggerRpc(delay);

        //Current Version of PlayerStealth only runs locallly
        var c = GetComponent<PlayerStealth>();
        if (c != null)
            c.force_unhide();

    }

    #region Network RPCs
    // ServerRpc - called from client, runs on server
    [Rpc(SendTo.Server)]
    private void TriggerRpc(float delay = 0f)
    {
        SendOnAttackedClientRpc(delay);
    }

    // ClientRpc - called from server, runs on all clients
    [Rpc(SendTo.ClientsAndHost)]
    private void SendOnAttackedClientRpc(float delay = 0f)
    {
        _onReavealTriggered?.Invoke();

        if (ISDEBUGGING) Debug.Log("RevealFluffies_Receiver: ClientRPC _onReavealTriggered Called!");
    }

    #endregion

}
