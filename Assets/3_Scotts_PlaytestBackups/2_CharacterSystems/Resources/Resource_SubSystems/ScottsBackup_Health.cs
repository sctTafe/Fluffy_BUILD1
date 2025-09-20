using UnityEngine;
using Unity.Netcode;
using System;
using UnityEngine.Events;

public class ScottsBackup_Health : NetworkBehaviour
{
    private const bool ISDEBUGGING = true;
    
    public UnityEvent _OnHealthChangeUp;
    public UnityEvent _OnHealthChangeDown;
    public UnityEvent _OnDeath;

    ScottsBackup_ResourceMng scottsBackup_ResourceMng;

    private void Start()
    {
        //this.enabled = false;
        //return;

        if (IsOwner)
        {
            TryGetHealthSystem();
            Bind();
        }

    }

    private void OnDisable()
    {
        UnBind();
    }
    public override void OnDestroy()
    {
        UnBind();
        base.OnDestroy();      
    }


    private void Bind()
    {
        if (scottsBackup_ResourceMng == null)
            return;

        scottsBackup_ResourceMng._OnValueChangeUp += Handle_OnValueChangeUp;
        scottsBackup_ResourceMng._OnExhaustionTriggered += Handle_OnDeath;
    }

    private void UnBind()
    {
        if (scottsBackup_ResourceMng == null)
            return;

        scottsBackup_ResourceMng._OnValueChangeUp -= Handle_OnValueChangeUp;
        scottsBackup_ResourceMng._OnExhaustionTriggered -= Handle_OnDeath;
    }


    private void Handle_OnDeath()
    {
        // Death!

        // -- Network --
        SendOnDeathRpc();

        // -- Local --
        if (ISDEBUGGING) Debug.Log("ScottsBackup_Health: Death!");
        _OnDeath?.Invoke();
        MainGameManager.Instance.fn_KillPlayerAndSpawnGhost(NetworkManager.Singleton.LocalClientId, this.gameObject.transform.position);
    }

    private void Handle_OnValueChangeUp(bool b)
    {
        // -- Network --
        SendOnHealthChangeUpBoolRpc(b);

        // -- Local --
        //Not needed as the client RPC talks to the local version as well!
        /*
        if (b)
        {
            // Health Went Up
            if (ISDEBUGGING) Debug.Log("ScottsBackup_Health: Health Went Up!");
            _OnHealthChangeUp?.Invoke();  
        }
        else
        {
            //Health Went Down
            if (ISDEBUGGING) Debug.Log("ScottsBackup_Health: Health Went Down!");
            _OnHealthChangeDown?.Invoke(); 
        }
        */
    }


    #region Network RPCs

    // --- Health Chanage ---
    // ServerRpc - called from a client, runs on server
    [Rpc(SendTo.Server)]
    private void SendOnHealthChangeUpBoolRpc(bool b)
    {
        SendOnHealthChangeUpBoolClientRpc(b);
    }

    // ClientRpc - called from server, runs on all clients
    [Rpc(SendTo.ClientsAndHost)]
    private void SendOnHealthChangeUpBoolClientRpc(bool b)
    {
        if (b)
        {
            _OnHealthChangeUp?.Invoke();
            if (ISDEBUGGING) Debug.Log("ScottsBackup_Health: ClientRPC - Health Went Up!");          
        }
        else
        {
            _OnHealthChangeDown?.Invoke();
            if (ISDEBUGGING) Debug.Log("ScottsBackup_Health: ClientRPC - Health Went Down!");
        }
    }

    // --- Death ---
    // ServerRpc - called from client, runs on server
    [Rpc(SendTo.Server)]
    private void SendOnDeathRpc()
    {
        SendOnDeathClientRpc();
    }

    // ClientRpc - called from server, runs on all clients
    [Rpc(SendTo.ClientsAndHost)]
    private void SendOnDeathClientRpc()
    {
        _OnDeath?.Invoke();

        if (ISDEBUGGING)
            Debug.Log("ScottsBackup_Health: ClientRPC - OnDeath Triggered!");
    }

    #endregion



    private void TryGetHealthSystem()
    {
        ScottsBackup_ResourceMng[] resourceManagers = this.GetComponents<ScottsBackup_ResourceMng>();
        foreach (var resMng in resourceManagers)
        {
            if (resMng.ResourceTypeID == ScottsBackup_ResourceMng.ResourceType.Health)
            {
                scottsBackup_ResourceMng = resMng;
            }
        }

        if(scottsBackup_ResourceMng == null)
        {
            Debug.LogError("ScottsBackup_Health: Failed to find 'Health' (ScottsBackup_ResourceMng)");
        }

        if (ISDEBUGGING) Debug.Log("ScottsBackup_Health: TryGetHealthSystem Sucess");
    }
}
