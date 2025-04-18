using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine.Samples;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;
using static UnityEditor.PlayerSettings;

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
    [SerializeField] private Vector3 _BadSpawnArea = Vector3.forward * 5;
    [SerializeField] private float _spawnRadius = 5f;

    [Header("UI GameObjects")]
    [SerializeField] private GameObject _friendlyUI;
    [SerializeField] private GameObject _mutantUI;
    [SerializeField] private GameObject _endScreen;
    [SerializeField] private GameObject _endScreen_FluffyWin;
    [SerializeField] private GameObject _endScreen_MutantWin;



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
    public override void OnNetworkSpawn()
    {
        //Debug.Log("PreGameLobbyManager: OnNetworkSpawn");
        if (IsServer)
        {
            //NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback; // NOT YET IMPLMENTED IN THIS VERSION - STILL NEEDS TO BE
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
            //NetworkManager.Singleton.OnClientConnectedCallback += Server_OnPlayerJoinedEvent; // NOT YET IMPLMENTED IN THIS VERSION - STILL NEEDS TO BE
        }
        if (IsClient)
        {
            PlayerNetworkDataManager playerNetworkDataManager = PlayerNetworkDataManager.Instance;
            ulong localClientID = NetworkManager.Singleton.LocalClientId;
            if (playerNetworkDataManager.fn_GetClientTeamByClientID(localClientID)) _friendlyUI.SetActive(true);
            else _mutantUI.SetActive(true);
        }
    }
    public void OnDisable()
    {
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= SceneManager_OnLoadEventCompleted;
    }


    #endregion END: Unity Native Functions

    #region Joining and Load Event Responces
    /// <summary>
    /// Called On Scene Load
    /// DOSE: Instantiate player Prefabs
    /// </summary>
    private void SceneManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        PlayerNetworkDataManager PDNM = PlayerNetworkDataManager.Instance;
        Quaternion spawnRot = Quaternion.identity;

        Debug.Log("MainGameManger: OnLoadEventComplete Called");
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (PDNM.fn_GetClientTeamByClientID(clientId) == true)
            {
                Vector3 spawnPos = GetRandomPointAround(_GoodSpawnArea, _spawnRadius);             
                //Transform playerTransform = Instantiate(_playerPrefab);
                Transform playerTransform = Instantiate(_playerPrefab, spawnPos, spawnRot);
                playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
                playerTransform.gameObject.name = $"Player ({clientId}) MainGame Playable Character";
            }
            else 
            {
                Vector3 spawnPos = GetRandomPointAround(_GoodSpawnArea, _spawnRadius);
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

    public void fn_DelayedSpawnGhost(ulong ghostPlayer, Vector3 pos, float waitTime)
    {
        if (IsOwner)
        {
            StartCoroutine(WaitThenSummonGhost(waitTime, ghostPlayer, pos));
        }
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
        if (IsOwner)
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


}





