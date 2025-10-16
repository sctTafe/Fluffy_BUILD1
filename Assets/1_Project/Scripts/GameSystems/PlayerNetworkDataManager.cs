using Unity.Netcode;
using System;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Script for updating the player data
/// 
/// </summary>
public class PlayerNetworkDataManager : NetworkSingleton<PlayerNetworkDataManager>
{
    private const bool isDebuggingOn = true;

    /// <summary>
    /// Event Call for when any PlayerData changes
    /// </summary>
    public event EventHandler OnPlayerDataNetworkListChanged;

    /// <summary>
    /// Event Call for when teams change
    /// </summary>
    public event EventHandler OnTeamsChanged;

    public event EventHandler OnPlayerDataNVChanged;


    // - Network Varaibles -   

    // Server NV
    private NetworkVariable<int> mutantCountNV = new NetworkVariable<int>();
    private NetworkList<PlayerData> playerDataNetworkList; //Must be initialized later 

    // - Local Variables -
    private string playerName_local; // local Player Name
    private const string PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER = "PlayerNameMultiplayer";


    private bool _isSetUpComplete;

    private bool _isDisconnected; // Keeping track of if data is stale

    #region Unity Native Functions
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Destroy any duplicate
            return;
        }

        DontDestroyOnLoad(gameObject);


        // Initialized network List (must be done here)
        playerDataNetworkList = new NetworkList<PlayerData>();

        playerDataNetworkList.OnListChanged += Handle_PlayerDataNetworkList_OnListChanged;

        mutantCountNV.OnValueChanged += Handle_teamMonstersCountNVValueChange;

        _isSetUpComplete = true;
    }

    private void OnDisable()
    {
        if (_isSetUpComplete)
        {
            playerDataNetworkList.OnListChanged -= Handle_PlayerDataNetworkList_OnListChanged;
            mutantCountNV.OnValueChanged -= Handle_teamMonstersCountNVValueChange;
            NetworkManager.Singleton.OnClientConnectedCallback -= Handle_OnClientConnectedCallback_ServerReaction;
            NetworkManager.Singleton.OnClientDisconnectCallback -= Handle_OnClientDisconnectCallback_ServerReaction;
        }
    }


    override public void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (isDebuggingOn) Debug.Log("PlayerNetworkDataManager: OnNetworkSpawn");

        _isDisconnected = false; // Reset on spawn  - list should be refreshed

        // On new clients joining the game
        NetworkManager.Singleton.OnClientConnectedCallback += Handle_OnClientConnectedCallback_ServerReaction;
        // On new clients leaving the game
        NetworkManager.Singleton.OnClientDisconnectCallback += Handle_OnClientDisconnectCallback_ServerReaction;

        //NetworkManager.Singleton.ConnectionApprovalCallback += Handle_NetworkManagerConnectionApprovalCallback;


        if (IsClient)
        {
            // Invoke a local update of team realated values on joining the network. I.e. Initalisation update of local values
            OnTeamsChanged?.Invoke(this, new EventArgs());
        }

    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (isDebuggingOn) Debug.Log("PlayerNetworkDataManager: OnNetworkDespawn - Cleaning up");

        // Clean up callbacks
        NetworkManager.Singleton.OnClientConnectedCallback -= Handle_OnClientConnectedCallback_ServerReaction;
        NetworkManager.Singleton.OnClientDisconnectCallback -= Handle_OnClientDisconnectCallback_ServerReaction;

        // CRITICAL: Clear stale data on disconnect
        if (IsClient && !IsServer)
        {
            // Client-side cleanup - the NetworkList will be read-only but we can prepare for next connection
            if (isDebuggingOn) Debug.Log($"Client disconnecting with {playerDataNetworkList.Count} stale entries");

            // Note: We cannot clear the NetworkList as a client (it's read-only) - But we can prevent other
            // code from using stale data
            _isDisconnected = true;
        }
    }




    #endregion END: Unity Native Functions


    #region Public Functions

    /// <summary>
    ///  Runs Data Cleaning and refreshment on the server when returning to lobby post game
    /// </summary>
    public void fn_ReturnToLobbyPostGame()
    {
        // Only Run on Server/Host
        if (!IsServer && !IsHost)
            return;

        //1) Clear Current States
        VerifyAndCleanPlayerDataList(); // Remove any exited players
        ResetAllPlayersToGoodTeam(); // Reset All players teams to fluffy
    }


    public void fn_SwitchTeamToggle() => LocalClient_ToggleTeam();

    public bool fn_GetLocalClientTeam()
    {
        if (!IsDataValid())
        {
            Debug.LogWarning("Attempting to read stale network data!");
            return true; // Return a safe default
        }
        return GetLocalClientTeam();
    }

    public int fn_GetTotalMutantPlayers()
    {
        if (!IsDataValid())
        {
            Debug.LogWarning("Attempting to read stale network data!");
            return 0;
        }
        return mutantCountNV.Value;
    }




    public bool fn_GetClientTeamByClientID(ulong id) => GetClientTeamByClientID(id);

    public void fn_SelectMonsterOnStart() => SelectMonsterOnStart();

    public void fn_ClearPlayerDataManager()
    {
        if (IsClient)
        {
            Debug.LogWarning("Player Data Manger 'Clear' Called by Client - Disallowed!");
            return;
        }

        Debug.Log("Player Data Manger 'Clear' Called!");
        if (playerDataNetworkList != null)
        {
            if (playerDataNetworkList.Count > 0)
            {
                playerDataNetworkList.Clear();
            }
        }
    }

    #endregion END: Public Functions


    #region CallBack Functions
    /// <summary>
    /// Called
    /// </summary>
    private void Handle_PlayerDataNetworkList_OnListChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        // NOTE: Called on clients and server

        if (isDebuggingOn) Debug.Log($"PlayerNetworkDataManager: PlayerDataNetworkList.OnValueChange Callback; ");
        //if (isDebuggingOn) Debug.Log($"PlayerNetworkDataManager: Change Event = {changeEvent} ");
        if ((isDebuggingOn))
        {
            foreach (var player in playerDataNetworkList)
            {
                Debug.Log($"PNDL Callback Info:  ClientID {player.clientId}, Team {player.goodTeam} ");
            }
        }

        if (IsServer)
        {
            // Update Count Of Total Monsters
            UpdateTotalMutantPlayersNVServerRpc();
        }

        OnPlayerDataNetworkListChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Does: Inform Clients that teamMonstersCountNV has changed value
    /// </summary>
    private void Handle_teamMonstersCountNVValueChange(int previousValue, int newValue)
    {
        if (isDebuggingOn) Debug.Log($"PlayerNetworkDataManager: MonstersCountNV.OnValueChange Callback; Total Mutant Count Updated: Mutant Team = {newValue}");
        OnTeamsChanged?.Invoke(this, new EventArgs());
    }


    #region Sub Region: NetworkManager Callbacks

    private void Handle_OnClientConnectedCallback_ServerReaction(ulong clientId)
    {
        if (IsServer)
        {
            // Create & Add New Player
            playerDataNetworkList.Add(new PlayerData
            {
                clientId = clientId,
                playerName = "",
                goodTeam = true
            });

            if (isDebuggingOn) Debug.Log($"PlayerNetworkDataManager: ClientConnection - New Total Number of PlayerData Sets: {playerDataNetworkList.Count}");
        }
    }

    private void Handle_OnClientDisconnectCallback_ServerReaction(ulong clientId)
    {
        if (IsServer)
        {
            // Remove Player
            int indexPos = -1;

            for (var i = 0; i < playerDataNetworkList.Count; i++)
            {


                if (playerDataNetworkList[i].clientId == clientId)
                {
                    indexPos = i;
                    break;
                }
            }

            // Is Match
            if (indexPos >= 0)   // Changed from '>'
            {
                playerDataNetworkList.RemoveAt(indexPos);
            }

            if (isDebuggingOn) Debug.Log($"PlayerNetworkDataManager: ClientDisConnection - New Total Number of PlayerData Sets: {playerDataNetworkList.Count}");
        }
    }


    /// <summary>
    /// Allows client code to decide whether or not to allow incoming client connection
    /// </summary>
    private void Handle_NetworkManagerConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        // IF NEEDED: Code for potentially rejecting clients 
    }
    #endregion END: Sub Region: NetworkManager Callbacks

    #endregion END: CallBack Functions

    #region Player Team
    // - Client Side Functions -

    private bool GetLocalClientTeam()
    {
        if (isDebuggingOn)
        {
            ulong id = NetworkManager.Singleton.LocalClientId;
            if (isDebuggingOn) Debug.Log($"PlayerNetworkDataManager: GetLocalClientTeam; Local Clinet ID = {id}, current team alignment = {GetPlayerDataFromClientId(id).goodTeam}");
        }
        return GetPlayerDataFromClientId(NetworkManager.Singleton.LocalClientId).goodTeam;
    }

    private bool GetClientTeamByClientID(ulong id)
    {
        return GetPlayerDataFromClientId(id).goodTeam;
    }

    private void LocalClient_ToggleTeam()
    {
        if (isDebuggingOn) Debug.Log("LocalClient_ToggleTeam Called");
        // Retrive the current team of the local player
        bool isGoodieCurrently = GetPlayerDataFromClientId(NetworkManager.Singleton.LocalClientId).goodTeam;
        SetPlayerTeam(!isGoodieCurrently); //Set the opposite value (!)
    }


    /// <summary>
    /// Set player team: If called by clinet they can only update their own, (Host & Server can override, with a 'clinetID')
    /// </summary>
    public void SetPlayerTeam(bool isGoodie, ulong? clinetID = null)
    {
        // Note: Called Client Side

        if (clinetID == null)
        {
            // Update local client team
            UpdatePlayerTeamServerRpc(NetworkManager.Singleton.LocalClientId, isGoodie);
        }
        else
        {
            // Host Or Server, can change the team of any player
            if (IsHost || IsServer)
                UpdatePlayerTeamServerRpc(clinetID.Value, isGoodie);
        }
    }


    // - Server Side Functions -

    /// <summary>
    /// Server RPC: Updates the Player Name in the PlayerDataList
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void UpdatePlayerTeamServerRpc(ulong clinetID, bool isGoodie)
    {
        // Implementation and efficiency note - this is a very inefficient solution as we need to
        //      retransmit out the full PlayerData Strut each time a value is updated in it.


        // Note: Called On Server
        if (isDebuggingOn) Debug.Log($"UpdatePlayerTeamServerRpc: {clinetID}, Team updated to: isGoodie( {isGoodie} )");


        // Get PlayerData Strut
        var playerData = GetPlayerDataFromClientId(clinetID);
        PlayerData playerdataTemp = new PlayerData
        {
            clientId = playerData.clientId,
            playerName = playerData.playerName,
            playerId = playerData.playerId,
            goodTeam = isGoodie
        };

        // Reassigning PlayerData Strut to PlayerData NetworkList
        int playerDataListIndex = GetPlayerDataListIndexFromClientId(clinetID);
        playerDataNetworkList[playerDataListIndex] = playerdataTemp;

        //playerDataNetworkList.RemoveAt(playerDataListIndex);
        //playerDataNetworkList.Insert(playerDataListIndex, playerdataTemp);
    }


    /// <summary>
    /// Update the Total Mutant Players Count NV
    /// </summary>
    [ServerRpc]
    public void UpdateTotalMutantPlayersNVServerRpc()
    {
        int count = 0;
        foreach (var playerData in playerDataNetworkList)
        {
            if (playerData.goodTeam == false)
            {
                count++;
            }
        }

        // If Value is different to currently held value, update it
        if (mutantCountNV.Value != count)
        {
            mutantCountNV.Value = count;
        }
    }



    #endregion END: Player Team


    #region Player Name

    /// <summary>
    /// Returns local player name
    /// </summary>
    public string GetPlayerName()
    {
        return playerName_local;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerNameServerRpc(string playerName, ServerRpcParams serverRpcParams = default)
    {
        int playerDataListIndex = GetPlayerDataListIndexFromClientId(serverRpcParams.Receive.SenderClientId);

        PlayerData playerData = playerDataNetworkList[playerDataListIndex];

        playerData.playerName = playerName;

        playerDataNetworkList[playerDataListIndex] = playerData;
    }


    /// <summary>
    /// Set player name
    /// </summary>
    /// <param name="playerName">string to set name as</param>
    /// <param name="clinetID">leave null for local player</param>
    public void SetPlayerName(string playerName, ulong? clinetID = null)
    {

        // If network game is active
        if (NetworkManager.Singleton.IsListening)
        {
            // -> Network Started
            if (clinetID == null)
            {
                // Update local player name
                UpdatePlayerNameServerRpc(NetworkManager.Singleton.LocalClientId, playerName);
            }
            else
            {
                // Host can change the name of any player
                if (IsHost || IsServer)
                    UpdatePlayerNameServerRpc(clinetID.Value, playerName);
            }
        }
        else
        {
            // -> Network Not Started
            this.playerName_local = playerName;
            PlayerPrefs.SetString(PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER, playerName);
        }
    }

    /// <summary>
    /// Updates the Player Name in the PlayerDataList
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void UpdatePlayerNameServerRpc(ulong clinetID, string newName)
    {
        var playerData = GetPlayerDataFromClientId(clinetID);
        playerData.playerName = newName;
        int playerDataListIndex = GetPlayerDataListIndexFromClientId(clinetID);
        playerDataNetworkList[playerDataListIndex] = playerData;
        //Debug.Log("player name, in player networklist updated");
    }
    #endregion End Player Name





    #region Player Data Network List

    public int GetPlayerDataListIndexFromClientId(ulong clientId)
    {
        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            if (playerDataNetworkList[i].clientId == clientId)
            {
                return i;
            }
        }
        return -1;
    }
    public PlayerData GetPlayerDataFromClientId(ulong clientId)
    {
        foreach (PlayerData playerData in playerDataNetworkList)
        {
            if (playerData.clientId == clientId)
            {
                return playerData;
            }
        }
        return default;
    }
    #endregion END: Player Data Network List

    #region Monster Selection

    /// <summary>
    /// Randomly assigns a single player to the monster team
    /// </summary>
    private void SelectMonsterOnStart()
    {
        if (IsHost)
        {
            ulong monsterClientID;
            if (mutantCountNV.Value == 0)
            {
                monsterClientID = playerDataNetworkList[UnityEngine.Random.Range(0, playerDataNetworkList.Count)].clientId;
            }
            else
            {
                List<PlayerData> teamMonstersCandidates = new List<PlayerData>();

                foreach (var player in playerDataNetworkList)
                {
                    if (player.goodTeam == false) teamMonstersCandidates.Add(player);
                }

                monsterClientID = teamMonstersCandidates[UnityEngine.Random.Range(0, teamMonstersCandidates.Count)].clientId;
            }
            AssignTeams(monsterClientID);
        }
    }

    /// <summary>
    /// Assigns the specified player to the monster team, and the rest to player team
    /// </summary>
    /// <param name="monsterClientID">The clientId of the selected Monster</param>
    private void AssignTeams(ulong monsterClientID)
    {
        if (IsHost)
        {
            foreach (var player in playerDataNetworkList)
            {
                if (player.clientId == monsterClientID) SetPlayerTeam(false, player.clientId);
                else SetPlayerTeam(true, player.clientId);
            }
        }
    }

    #endregion END: Monster Selection


    private void Handle_PlayerDataChange(PlayerData oldValue, PlayerData newValue)
    {
        OnPlayerDataNVChanged?.Invoke(this, EventArgs.Empty);
    }




    /// <summary>
    /// Verifies all players in playerDataNetworkList are still connected and removes disconnected players
    /// Should only be called by the Server/Host
    /// </summary>
    public void VerifyAndCleanPlayerDataList()
    {
        if (!IsServer)
        {
            Debug.LogWarning("VerifyAndCleanPlayerDataList can only be called by the Server!");
            return;
        }

        if (playerDataNetworkList == null || playerDataNetworkList.Count == 0)
        {
            if (isDebuggingOn) Debug.Log("PlayerDataNetworkList is empty, nothing to verify.");
            return;
        }

        List<int> indicesToRemove = new List<int>();

        
        // Check each player in the list
        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            ulong clientId = playerDataNetworkList[i].clientId;

            bool isCurrentClient = false;
            int countOfCoppies = 0;

            foreach (var clientID_nm in NetworkManager.Singleton.ConnectedClientsIds)
            {
                // Check if this client is still connected
                if (clientId == clientID_nm)
                {
                    isCurrentClient = true;
                    countOfCoppies++;
                }                   
            }

            if (countOfCoppies > 1)
                Debug.LogError("Multiple instances of the same player in the PlayerDataList!!! THis is very broken!");

            // If not founc in the current ID list
            if (isCurrentClient == false)
            {            
                if (isDebuggingOn) Debug.Log($"Client {clientId} is no longer connected. Marking for removal.");
                indicesToRemove.Add(i);
            }          
        }

       
        // Remove disconnected players (iterate backwards to maintain correct indices)
        for (int i = indicesToRemove.Count - 1; i >= 0; i--)
        {
            if (isDebuggingOn) Debug.Log($"Removing disconnected player at index {indicesToRemove[i]}, ClientID: {playerDataNetworkList[indicesToRemove[i]].clientId}");
            playerDataNetworkList.RemoveAt(indicesToRemove[i]);
        }

        if (indicesToRemove.Count > 0)
        {
            if (isDebuggingOn) Debug.Log($"Verification complete. Removed {indicesToRemove.Count} disconnected player(s). Current player count: {playerDataNetworkList.Count}");
        }
        else
        {
            if (isDebuggingOn) Debug.Log($"Verification complete. All players are still connected. Current player count: {playerDataNetworkList.Count}");
        }
    }

    /// <summary>
    /// Resets all players to the good team (goodTeam = true)
    /// Should only be called by the Server/Host
    /// </summary>
    public void ResetAllPlayersToGoodTeam()
    {
        if (!IsServer)
        {
            Debug.LogWarning("ResetAllPlayersToGoodTeam can only be called by the Server!");
            return;
        }

        if (playerDataNetworkList == null || playerDataNetworkList.Count == 0)
        {
            if (isDebuggingOn) Debug.Log("PlayerDataNetworkList is empty, nothing to reset.");
            return;
        }

        if (isDebuggingOn) Debug.Log($"Resetting all {playerDataNetworkList.Count} players to good team...");

        // Iterate through all players and set goodTeam to true
        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            PlayerData playerData = playerDataNetworkList[i];

            // Only update if the player is not already on the good team
            if (!playerData.goodTeam)
            {
                playerData.goodTeam = true;
                playerDataNetworkList[i] = playerData;

                if (isDebuggingOn) Debug.Log($"Reset ClientID {playerData.clientId} ({playerData.playerName}) to good team.");
            }
        }

        if (isDebuggingOn) Debug.Log("All players have been reset to the good team.");

        // Update the mutant count (should now be 0)
        UpdateTotalMutantPlayersNVServerRpc();
    }



    /// <summary>
    /// Returns true if we're connected and data is valid
    /// </summary>
    public bool IsDataValid()
    {
        return NetworkManager.Singleton != null &&
               NetworkManager.Singleton.IsConnectedClient &&
               !_isDisconnected;
    }

}
