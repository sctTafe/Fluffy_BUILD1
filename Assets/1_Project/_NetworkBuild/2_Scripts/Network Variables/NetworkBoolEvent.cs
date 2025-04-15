using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class NetworkBoolEvent : NetworkBehaviour
{
    // Network Variable
    public NetworkVariable<bool> IsTrueNV = new NetworkVariable<bool>();

    // Local Variables
    public UnityEvent OnTrue;
    public UnityEvent OnFalse;

    override public void OnNetworkSpawn()
    {
        Debug.Log(" OnNetworkSpawn Called");

        base.OnNetworkSpawn();

        Start_IsClient();
        Start_IsServer();
    }

    private void OnEnable()
    {
        IsTrueNV.OnValueChanged += Handle_OnBoolValueChange;
    }
    private void OnDisable()
    {
        IsTrueNV.OnValueChanged -= Handle_OnBoolValueChange;
    }


    /// <summary>
    /// Local Function Call to Trigger 
    /// </summary>
    public void fn_ClientSideActivationCall()
    {
        FlipValue_ServerRPC();
    }


    private void Start_IsServer()
    {
        if (!IsServer)
            return;

        IsTrueNV.Value = false; //Set False On Start
    }

    private void Start_IsClient()
    {
        if (!IsClient)
            return;

        OnFalse?.Invoke(); //Set False On Start
    }

    //Called to run on the server
    //[ServerRpc]
    [Rpc(SendTo.Server)]
    private void FlipValue_ServerRPC()
    {
        var currentValue = IsTrueNV.Value;
        IsTrueNV.Value = !currentValue;
        Debug.Log("SimpleNetVarScripts: Value Updated");
    }

    /// <summary>
    /// Called on Client Side when value changes 
    /// </summary>
    private void Handle_OnBoolValueChange(bool previousValue, bool newValue)
    {
        if (newValue == true)
        {
            OnTrue?.Invoke();
        }
        else
        {
            OnFalse?.Invoke();
        }
    }
}
