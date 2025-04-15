using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine.Samples;
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

    [SerializeField] private Vector3 _GoodSpawnArea = Vector3.zero;
    [SerializeField] private Vector3 _BadSpawnArea = Vector3.forward * 5;
    [SerializeField] private float _spawnRadius = 5f;

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


    public void fn_EndGame()
    {
        fn_DespawnPlayers();
        StartCoroutine(WaitThenChangeScene());
    }

    IEnumerator WaitThenChangeScene()
    {
        Debug.Log("Waiting for 1.5 seconds...");
        yield return new WaitForSeconds(1.5f);
        Debug.Log("Done waiting!");
        NetworkSceneManager.Instance.fn_GoToScene("4_Lobby");

        // You can add any logic you want to happen after the wait here
    }


    #region Spawn & Despawn Network Objects

    //#region SUB REGION - Handle SO Network Objects
    //[SerializeField] private NetworkObjectsListType1SO networkObjectsListType1SO; //List of network objects, for providing the index value


    //public void fn_SpawnNetworkObjectType1(NetworkObjectsType1SO networkObjectType1SO)
    //{
    //    SpawnNetworkObjectType1ServerRpc(GetType1NetObjSOIndex(networkObjectType1SO));
    //}
    //public void fn_SpawnNetworkObjectType1(NetworkObjectsType1SO networkObjectType1SO, NetworkObject toBeParentNetObj)
    //{
    //    if (networkObjectType1SO == null)
    //        Debug.LogWarning("networkObjectType1SO is Null!");
    //    if (toBeParentNetObj == null)
    //        Debug.LogWarning("toBeParentNetObj is Null!");

    //    SpawnNetworkObjectType1ServerRpc(GetType1NetObjSOIndex(networkObjectType1SO), toBeParentNetObj.NetworkObjectId);
    //}

    //public void fn_SpawnNetworkObjectType1(NetworkObjectsType1SO networkObjectType1SO, INetworkObjectType1Parent networkObjectParent)
    //{
    //    SpawnNetworkObjectType1ServerRpc(GetType1NetObjSOIndex(networkObjectType1SO), networkObjectParent.GetNetworkObject());
    //}

    ///// <summary>
    ///// 
    ///// </summary>
    ///// <param name="netObjType1SOIndex"> Index of the Network Object to be spawned </param>
    ///// <param name="parentNetworkObjectRef"> NetworkObjectRefrence to the parent NetworkObject</param>
    //[ServerRpc(RequireOwnership = false)]
    //private void SpawnNetworkObjectType1ServerRpc(int netObjType1SOIndex, NetworkObjectReference parentNetworkObjectRef)
    //{
    //    NetworkObjectsType1SO type1NetworkObjectSO = GetType1NetObjSOFromIndex(netObjType1SOIndex);


    //    parentNetworkObjectRef.TryGet(out NetworkObject NetObjType1ParentNetworkObject);
    //    INetworkObjectType1Parent kitchenObjectParent = NetObjType1ParentNetworkObject.GetComponent<INetworkObjectType1Parent>();


    //    if (kitchenObjectParent.HasKitchenObject())
    //    {
    //        // Parent already spawned an object
    //        return;
    //    }

    //    Transform NetObjType1Prefab = Instantiate(type1NetworkObjectSO.prefab);

    //    NetworkObject netObjType1NetObj = NetObjType1Prefab.GetComponent<NetworkObject>();
    //    netObjType1NetObj.Spawn(true);

    //    NetworkObjectType1 netObjT1 = NetObjType1Prefab.GetComponent<NetworkObjectType1>();
    //    netObjT1.SetNetworkObjectParent(kitchenObjectParent);
    //}

    //[ServerRpc(RequireOwnership = false)]
    //private void SpawnNetworkObjectType1ServerRpc(int netObjType1SOIndex)
    //{
    //    NetworkObjectsType1SO type1NetworkObjectSO = GetType1NetObjSOFromIndex(netObjType1SOIndex);

    //    Transform NetObjType1Prefab = Instantiate(type1NetworkObjectSO.prefab);

    //    NetworkObject netObjType1NetObj = NetObjType1Prefab.GetComponent<NetworkObject>();
    //    netObjType1NetObj.Spawn(true);
    //}


    //[ServerRpc(RequireOwnership = false)]
    //private void SpawnNetworkObjectType1ServerRpc(int netObjType1SOIndex, ulong parentNetObjId)
    //{
    //    Debug.Log($" SpawnNetworkObjectType1ServerRpc with parent id: {parentNetObjId}");
    //    NetworkObjectsType1SO type1NetworkObjectSO = GetType1NetObjSOFromIndex(netObjType1SOIndex);

    //    // Create new object
    //    Transform NetObjType1Prefab = Instantiate(type1NetworkObjectSO.prefab);
    //    NetworkObject netObjType1NetObj = NetObjType1Prefab.GetComponent<NetworkObject>();

    //    // Add it to the network
    //    netObjType1NetObj.Spawn(true);

    //    //Set parent object
    //    NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(parentNetObjId, out NetworkObject toBeParentNetObj);
    //    if (toBeParentNetObj != null) {         
    //        netObjType1NetObj.transform.parent = toBeParentNetObj.transform;
    //    }
    //    else
    //    {
    //        Debug.LogWarning("Couldnot find parent network ID");
    //    }
    //}




    //public int GetType1NetObjSOIndex(NetworkObjectsType1SO type1NetObjSO)
    //{
    //    return networkObjectsListType1SO.notworkObjectType1SOList.IndexOf(type1NetObjSO);
    //}

    //public NetworkObjectsType1SO GetType1NetObjSOFromIndex(int kitchenObjectSOIndex)
    //{
    //    return networkObjectsListType1SO.notworkObjectType1SOList[kitchenObjectSOIndex];
    //}
    //#endregion END: SUB REGION - Handle SO Network Objects

    public void fn_DespawnPlayers()
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





