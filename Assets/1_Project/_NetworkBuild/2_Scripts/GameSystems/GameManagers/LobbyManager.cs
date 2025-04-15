using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LobbyManager : NetworkSingleton<LobbyManager>
{
    public event EventHandler OnStateChanged;
    public event EventHandler OnReadyChanged;

    // Network Variable 
    private NetworkVariable<int> numberOfPlayersNV = new NetworkVariable<int>();
    private NetworkVariable<int> numberOfReadyPlayersNV = new NetworkVariable<int>();

    // local dictionary
    private Dictionary<ulong, bool> playerReadyDictionary;
    private bool isLocalPlayerReady;

	PlayerNetworkDataManager playerNetworkDataManager;

	#region Unity Native Functions
	private void Awake()
    {
        playerReadyDictionary = new Dictionary<ulong, bool>();
    }

	private void Start()
	{
		playerNetworkDataManager = PlayerNetworkDataManager.Instance;
	}

	public override void OnNetworkSpawn()
    {
        Debug.Log("LobbyManager: OnNetworkSpawn");
        base.OnNetworkSpawn();

        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
            NetworkManager.Singleton.OnClientConnectedCallback += Server_OnPlayerJoinedEvent;
            Server_UpdatePlayerValues();
        }

        if (IsClient)
        {
            numberOfPlayersNV.OnValueChanged += Handle_ValuesUpdate;
            numberOfReadyPlayersNV.OnValueChanged += Handle_ValuesUpdate;
        }
    }

    private void OnDisable()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= SceneManager_OnLoadEventCompleted;
            NetworkManager.Singleton.OnClientConnectedCallback -= Server_OnPlayerJoinedEvent;
        }

        if (IsClient)
        {
            numberOfPlayersNV.OnValueChanged -= Handle_ValuesUpdate;
            numberOfReadyPlayersNV.OnValueChanged -= Handle_ValuesUpdate;
        }
    }
    #endregion END: Unity Native Functions

    #region Public Functions
    public int fn_GetNumberOfPlayersInLobby()
    {
        return numberOfPlayersNV.Value;
    }
    public int fn_GetNumberOfReadyPlayersInLobby()
    {
        return numberOfReadyPlayersNV.Value;
    }

    public void fn_PlayerReadyToggle()
    {
        TogglePlayerReadyServerRpc();
    }

    public void fn_StartGame()
    {
        if (IsHost)
        {
            Debug.Log("Host Trying to Start Game");

            // If all players ready, can switch to next scene
            if (fn_GetNumberOfPlayersInLobby() == fn_GetNumberOfReadyPlayersInLobby())
            {
                playerNetworkDataManager.fn_SelectMonsterOnStart();
				NetworkSceneManager.Instance.fn_GoToScene("5_Game");
            }
            else
            {
                Debug.Log("Players not all ready!");
            }

        }
    }
    #endregion END: Public Functions



    #region Players Readiness 

    // Runs only on the server
    [ServerRpc(RequireOwnership = false)]
    private void TogglePlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        ulong senderClientID = serverRpcParams.Receive.SenderClientId;

        if (playerReadyDictionary.ContainsKey(senderClientID))
        {
            playerReadyDictionary[senderClientID] = !playerReadyDictionary[senderClientID];
        }
        else
        {
            playerReadyDictionary[senderClientID] = true;
        }

        UpdateClientPLayerReadyDictionaries_ClientRpc(senderClientID, playerReadyDictionary[senderClientID]);
        Server_UpdatePlayerValues();
    }


    // Client RPC is sent to the clients from the server to notify them of change
    [ClientRpc]
    private void UpdateClientPLayerReadyDictionaries_ClientRpc(ulong clientId, bool state)
    {
        playerReadyDictionary[clientId] = state;
    }

    private void CheckIfAllPlayersReady()
    {
        // Only need to run on server
        if (IsServer)
        {
            bool allClientsReady = true;
            foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                if (!playerReadyDictionary.ContainsKey(clientId) || !playerReadyDictionary[clientId])
                {
                    // This player is NOT ready
                    allClientsReady = false;
                    break;
                }
            }

            if (allClientsReady)
            {
                //state.Value = State.CountdownToStart;
            }
        }
    }
    #endregion END: Players Readiness 

    #region RPC Calls

    /// <summary>
    /// Server Only
    /// </summary>
    [ServerRpc]
    private void UpdateTotalPlayersValue_ServerRPC()
    {
        numberOfPlayersNV.Value = NetworkManager.Singleton.ConnectedClients.Count;
    }
    [ServerRpc]
    private void UpdatePlayerReadyValues_ServerRPC()
    {
        int rdyCount = 0;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (playerReadyDictionary.ContainsKey(clientId) && playerReadyDictionary[clientId])
            {
                rdyCount++;
            }
        }
        numberOfReadyPlayersNV.Value = rdyCount;
    }
    /// <summary>
    /// Server Only
    /// </summary>
    private void Server_UpdatePlayerValues()
    {
        UpdateTotalPlayersValue_ServerRPC();
        UpdatePlayerReadyValues_ServerRPC();
        PlayerReadyValuesUpdated_ClientRpc();
    }

    // Called on All clients
    [ClientRpc]
    private void PlayerReadyValuesUpdated_ClientRpc()
    {
        OnReadyChanged?.Invoke(this, EventArgs.Empty);
        Debug.Log($"PlayerReadyValuesUpdated_ClientRpc Called: fn_GetNumberOfPlayersInLobby = {fn_GetNumberOfPlayersInLobby()} fn_GetNumberOfReadyPlayersInLobby = {fn_GetNumberOfReadyPlayersInLobby()} /n");

    }
    #endregion END: RCP Calls

    #region Joining and Load Event Responces

    private void Handle_ValuesUpdate(int previousValue, int newValue)
    {
        Debug.Log($"Handle_ValuesUpdate: Called with prviouse value = {previousValue} & new = {newValue}");
        OnReadyChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Called On Scene Load
    /// </summary>
    private void SceneManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
    }

    /// <summary>
    /// Called On Player Join
    /// </summary>
    private void Server_OnPlayerJoinedEvent(ulong clientId)
    {
        Server_UpdatePlayerValues();
    }
    #endregion END: Joining and Load Events

}
