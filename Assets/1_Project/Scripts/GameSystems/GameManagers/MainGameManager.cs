using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Primarily Handles Server Side Management of the Main Game
/// 
/// NOTE: Only the server can spawn network objects
/// </summary>
public class MainGameManager : NetworkSingleton<MainGameManager>
{
    
    [SerializeField] private Transform _playerPrefab;
    [SerializeField] private Transform _enemyPrefab;
    [SerializeField] private Transform _ghostPrefab;

    [SerializeField] private Vector3 _GoodSpawnArea = Vector3.zero;
    [SerializeField] private Vector3 _BadSpawnArea;
    [SerializeField] private float _spawnRadius = 10f;

    [Header("UI GameObjects")]
    //[SerializeField] private GameObject _friendlyUI;
    //[SerializeField] private GameObject _mutantUI;
    [SerializeField] private GameObject _endScreen;
    [SerializeField] private GameObject _endScreen_FluffyWin;
    [SerializeField] private GameObject _endScreen_MutantWin;



    // Client Side Varaibles

    // Server Side Varaibles
    List<ulong> _MutantPlayerIDs;
    List<ulong> _FluffyPlayerIDs;


    #region Unity Native Functions
    //private void Awake()
    //{
    //}
    //void Start()
    //{
    //}
    //void Update()
    //{        
    //}

    void Start()
	{
		_BadSpawnArea = new Vector3(-10, 30, 50);
	}

    public override void OnNetworkSpawn()
    {
        //Debug.Log("PreGameLobbyManager: OnNetworkSpawn");
        if (IsServer)
        {
            // Create Lists
            _MutantPlayerIDs = new List<ulong>();
            _FluffyPlayerIDs = new List<ulong>();

            // Invoked after a scene load finishes across the networl: once all clients have reported back that they’ve completed the
            //         scene load (successfully or with failure), and the server/host has finished coordinating the scene transition.
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted_ServerSideReaction;

            //Invoked whenever a client disconnects from the server/host.
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect_ServerSideReaction;
            
        }
        if (IsClient)
        {
            //Invoked whenever a client disconnects from the server/host.
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect_ClientSideReaction;
            PlayerNetworkDataManager playerNetworkDataManager = PlayerNetworkDataManager.Instance;
            ulong localClientID = NetworkManager.Singleton.LocalClientId;
        }
    }



    public void OnDisable()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= SceneManager_OnLoadEventCompleted_ServerSideReaction;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect_ServerSideReaction;
        }

        if (IsClient)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect_ClientSideReaction;
        }           
    }
    #endregion END: Unity Native Functions





    #region Joining and Load Event Responces
    /// <summary>
    /// Called On Scene Load
    /// DOSE: Instantiate player Prefabs & Stores which playerIs are mutant or fluffy
    /// </summary>
    private void SceneManager_OnLoadEventCompleted_ServerSideReaction(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        // SERVER ONLY
        Debug.Log("MainGameManger: All Clients Have Loaded the Scene");


        PlayerNetworkDataManager PDNM = PlayerNetworkDataManager.Instance;
        Quaternion spawnRot = Quaternion.identity;

        
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (PDNM.fn_GetClientTeamByClientID(clientId) == true)
            {
                _FluffyPlayerIDs.Add(clientId);

                Vector3 spawnPos = GetRandomPointAround(_GoodSpawnArea, _spawnRadius);             
                //Transform playerTransform = Instantiate(_playerPrefab);
                Transform playerTransform = Instantiate(_playerPrefab, spawnPos, spawnRot);
                playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
                playerTransform.gameObject.name = $"Player ({clientId}) MainGame Playable Character";
            }
            else 
            {
                _MutantPlayerIDs.Add(clientId);

                Vector3 spawnPos = GetRandomPointAround(_BadSpawnArea, _spawnRadius);
                Transform playerTransform = Instantiate(_enemyPrefab, spawnPos, spawnRot);
                //Transform playerTransform = Instantiate(_enemyPrefab);
                playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
                playerTransform.gameObject.name = $"Player ({clientId}) MainGame Playable Enemy";
            }
        }

        Vector3 GetRandomPointAround(Vector3 center, float radius)
        {
            Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * radius;
            return center + new Vector3(randomCircle.x, 0f, randomCircle.y); // assuming Y-up
        }
    }

    #endregion END: Joining and Load Events

    #region End Game
    public void fn_EndGame(bool isEndTriggeredByFluffies)
    {
        fn_DespawnAllPlayers();

        EnableEndScreenClientRPC(isEndTriggeredByFluffies);

        StartCoroutine(WaitThenChangeScene());
    }


    IEnumerator WaitThenChangeScene()
    {
        Debug.Log("Waiting for 1.5 seconds...");
        yield return new WaitForSeconds(1.5f);
        Debug.Log("Done waiting!");
        NetworkSceneManager.Instance.fn_GoToScene("4_Lobby");
    }


    /// <summary>
    /// Turns On The Win end UI
    /// </summary>
    [ClientRpc]
    private void EnableEndScreenClientRPC(bool isFluffyWin)
    {
        _endScreen.gameObject.SetActive(true);
        if (isFluffyWin) 
            _endScreen_FluffyWin.gameObject.SetActive(true);
        else
            _endScreen_MutantWin.gameObject.SetActive(true); 
    }

    #endregion END: End Game

    #region Spawn & Despawn Network Objects

    public void fn_KillPlayer(ulong networkObjectID)
    {
        DespawnNetworkObjectRPC(networkObjectID);
    }
    public void fn_KillPlayerAndSpawnGhost(ulong clientId, Vector3 pos)
    {
        Debug.Log("MainGameManager, fn_KillPlayerAndSpawnGhost Called");
        
        // Try Get the NetworkClient based on the clientID, then get back their primaryNO destory it, and clreate a new one (Ghost)
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var networkClient))
        {
            var thePlayerObject = networkClient.PlayerObject; // This is the main NetworkObject associated with the client
            ulong objectNetworkId = thePlayerObject.NetworkObjectId;
            
            DespawnNetworkObjectRPC(objectNetworkId);
            DelayedSpawnGhost(clientId, pos, 2f);
        }
    }

    private void DelayedSpawnGhost(ulong ghostPlayer, Vector3 pos, float waitTime)
    {
        Debug.Log($"MainGameManager, DelayedSpawnGhost Called at position: {pos} ");
        StartCoroutine(WaitThenSummonGhost(waitTime, ghostPlayer, pos));       
    }

    IEnumerator WaitThenSummonGhost(float waitTime, ulong ghostPlayer, Vector3 pos)
    {
        yield return new WaitForSeconds(waitTime);
        fn_SpawnGhost(ghostPlayer, pos);
    }

    /// <summary>
    /// Spawns a ghost player prefab for the requesting player
    /// </summary>
    /// <param name="ghostPlayer">requesting ClientID</param>
    /// <param name="pos">Spawn Position in world</param>
    public void fn_SpawnGhost(ulong ghostPlayer, Vector3 pos)
    {
            SpawnGhostRPC(ghostPlayer, pos);
    }


    [Rpc(SendTo.Server)]
    private void SpawnGhostRPC(ulong ghostPlayer, Vector3 pos)
    {
        Transform playerTransform = Instantiate(_ghostPrefab, pos, Quaternion.identity);
        playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(ghostPlayer, true);
        playerTransform.gameObject.name = $"Player ({ghostPlayer}) MainGame Playable Ghost";
    }

    public void fn_DespawnAllPlayers()
    {
        Debug.Log("MainGameManager: fn_DespawnPlayers called");

        if (!IsServer) 
        {
            Debug.LogError("This should only be called by the Server!");
            return;
        }
            

        foreach (var clientPair in NetworkManager.Singleton.ConnectedClients)
        {
            var clientId = clientPair.Key;
            var playerObject = clientPair.Value.PlayerObject;

            if (playerObject != null && playerObject.IsSpawned)
            {
                // Despawn the player object (and optionally destroy it)
                playerObject.Despawn(true);
            }
        }
    }

    [Rpc(SendTo.Server)]
    private void DespawnNetworkObjectRPC(ulong networkObjectID)
    {
        Debug.Log($"DespawnNetworkObjectRPC called on {networkObjectID}");
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectID, out NetworkObject target_object))
        {
            target_object.Despawn();
        }
    }
    #endregion END: Spawn Network Objects

    #region Pause Game
    /// <summary>
    /// Event Driven Pause Handler
    /// NOTE: Could Put this In its own Class
    /// </summary>
    public event EventHandler OnLocalGamePaused;
    public event EventHandler OnLocalGameUnpaused;
    private NetworkVariable<bool> isGamePaused = new NetworkVariable<bool>(false);
    private Dictionary<ulong, bool> playerPausedDictionary;
    private bool isLocalGamePaused = false;

    public void TogglePauseGame()
    {
        isLocalGamePaused = !isLocalGamePaused;
        if (isLocalGamePaused)
        {
            PauseGameServerRpc();

            OnLocalGamePaused?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            UnpauseGameServerRpc();

            OnLocalGameUnpaused?.Invoke(this, EventArgs.Empty);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PauseGameServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playerPausedDictionary[serverRpcParams.Receive.SenderClientId] = true;

        TestGamePausedState();
    }

    [ServerRpc(RequireOwnership = false)]
    private void UnpauseGameServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playerPausedDictionary[serverRpcParams.Receive.SenderClientId] = false;

        TestGamePausedState();
    }

    private void TestGamePausedState()
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (playerPausedDictionary.ContainsKey(clientId) && playerPausedDictionary[clientId])
            {
                // This player is paused
                isGamePaused.Value = true;
                return;
            }
        }

        // All players are unpaused
        isGamePaused.Value = false;
    }
    #endregion END: Pause Game

    private void OnClientDisconnect_ClientSideReaction(ulong clientId)
    {

        Debug.Log("OnClientDisconnect Called.");

        // Only execute if this is the local client being disconnected
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.LogWarning("Local client has been disconnected.");

            // Check if the server (host) was the one that disconnected
            // If so, all clients will also be disconnected
            if (!NetworkManager.Singleton.IsServer)
            {
                Debug.Log("Host disconnected. Returning to main menu.");
                NetworkSceneManager.Instance.fn_Disconnect();
            }
        }   
    }



    private void OnClientDisconnect_ServerSideReaction(ulong clientId)
    {
        Debug.Log("OnClientDisconnect Called - On Server.");

        // Update Count
            // TODO

        // If the client playing the Mutant Drops/Quits
        if (_MutantPlayerIDs.Contains(clientId))
        {
            _MutantPlayerIDs.Remove(clientId);

            if(_MutantPlayerIDs.Count < 1)
            {
                //Trigger Fluffies Win
                fn_EndGame(true);
            }
        }
    }
}





