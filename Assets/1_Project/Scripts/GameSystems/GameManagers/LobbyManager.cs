using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class LobbyManager : NetworkSingleton<LobbyManager>
{
    public event EventHandler OnStateChanged;
    public event EventHandler OnReadyChanged;

    // Local Unity Events
    // Used for enableing and disabling other UI Eellemtns 
    public UnityEvent OnLocalPlayerReady;
    public UnityEvent OnLocalPlayerNotReady;

    public bool _isLoadingPlaytestingScene = false;
    [SerializeField] private string _testGameSceneName = "X_Game_Playtesting";

    // Network Variable 
    private NetworkVariable<int> numberOfPlayersNV = new NetworkVariable<int>();
    private NetworkVariable<int> numberOfReadyPlayersNV = new NetworkVariable<int>();

    // local dictionary
    private Dictionary<ulong, bool> playerReadyDictionary;
    private bool isLocalPlayerReady = false;

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
            NetworkManager.Singleton.OnClientDisconnectCallback += Handle_OnClientDisconnected;
            numberOfPlayersNV.OnValueChanged += Handle_ValuesUpdate;
            numberOfReadyPlayersNV.OnValueChanged += Handle_ValuesUpdate;
        }
    }

    private void OnDisable()
    {
        TurnOffMusic_ServerRPC();

        if (IsServer)
        {
            //if (NetworkManager.Singleton == null)
            //    return;

            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= SceneManager_OnLoadEventCompleted;
            NetworkManager.Singleton.OnClientConnectedCallback -= Server_OnPlayerJoinedEvent;
        }

        if (IsClient)
        {
            numberOfPlayersNV.OnValueChanged -= Handle_ValuesUpdate;
            numberOfReadyPlayersNV.OnValueChanged -= Handle_ValuesUpdate;
            NetworkManager.Singleton.OnClientDisconnectCallback -= Handle_OnClientDisconnected;
        }
    }
    #endregion END: Unity Native Functions

    #region Public Functions

    public void fn_UseTestingScene(bool useTestingScene) =>
        _isLoadingPlaytestingScene = useTestingScene;
    
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
        UpdateLocalClientUIEvents();
    }

    private void UpdateLocalClientUIEvents()
    {
        isLocalPlayerReady = !isLocalPlayerReady;
        if (isLocalPlayerReady) 
        {
                OnLocalPlayerReady?.Invoke();
        } 
        else 
        {
                OnLocalPlayerNotReady?.Invoke();
        }
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

                if (!_isLoadingPlaytestingScene)
                {
                    NetworkSceneManager.Instance.fn_GoToScene("5_Game");
                }
                else
                {
                    NetworkSceneManager.Instance.fn_GoToScene(_testGameSceneName);
                }			
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

    [Rpc(SendTo.Server)]
    private void TurnOffMusic_ServerRPC()
    {
        TurnOffMusic_ClientRPC();
    }
    [Rpc(SendTo.ClientsAndHost)]
    private void TurnOffMusic_ClientRPC()
    {
        Sounds_BackgroundMusic.Instance.fn_StopBackgroundMusicTrack();
    }


    #endregion END: RCP Calls

    #region Joining and Load Event Responces

    private void Handle_OnClientDisconnected(ulong clientId)
    {
        // If this client got disconnected from the server
        if (clientId == NetworkManager.Singleton.LocalClientId && !IsHost)
        {
            Debug.LogWarning("Disconnected from host. Returning to main menu.");

            NetworkSceneManager.Instance.fn_Disconnect();
        }
    }

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
