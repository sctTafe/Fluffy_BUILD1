using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayManager : Singleton<RelayManager> {
    
    [SerializeField]
    private string environment = "production";  //"production" is the default unity services environment 

    [SerializeField]
    private int maxNumberOfConnections = 6;

    public bool IsRelayEnabled => Transport != null && Transport.Protocol == UnityTransport.ProtocolType.RelayUnityTransport;

    public UnityTransport Transport => NetworkManager.Singleton.gameObject.GetComponent<UnityTransport>();


    /*
    // Control vars
    bool isHost;
    bool isPlayer;

    void OnDestroy() {
        // Cleanup objects upon exit
        if (isHost) {
            serverConnections.Dispose();
        }
        else if (isPlayer) {
            playerDriver.Dispose();
        }
    }
    */

    // GUI vars
    string _playerId = "Not signed in";
    // Allocation response objects
    Allocation _hostAllocation;
    // UTP vars
    NativeList<NetworkConnection> _serverConnections;

    private string _relayJoinCode;

    public string RelayJoinCode
    {
        get { return _relayJoinCode; }
    }

    public async Task<RelayHostData> SetupRelay() {

        InitializationOptions options = new InitializationOptions()
            .SetEnvironmentName(environment);

        await UnityServices.InitializeAsync(options);

        //NOTE: STEP AuthenticationService  - Sign In
        if (!AuthenticationService.Instance.IsSignedIn) {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            _playerId = AuthenticationService.Instance.PlayerId;
            Debug.Log($"Signed in. Player ID: {_playerId}");
        }

        //NOTE: STEP Allocation
        //TODO: Add the Regions stuff in, maybe...
        Debug.Log("Host - Creating an allocation. Upon success, I have 10 seconds to BIND to the Relay server that I've allocated.");
        _hostAllocation = await Relay.Instance.CreateAllocationAsync(maxNumberOfConnections);

        // -NOTE: new section - particialy taken from Unity Relay Examples
        // Initialize NetworkConnection list for the server (Host).
        // This list object manages the NetworkConnections which represent connected players.
        _serverConnections = new NativeList<NetworkConnection>(maxNumberOfConnections, Allocator.Persistent);

        
        // capture the relay host Data
        RelayHostData relayHostData = new RelayHostData {
            Key = _hostAllocation.Key,
            Port = (ushort)_hostAllocation.RelayServer.Port,
            AllocationID = _hostAllocation.AllocationId,
            AllocationIDBytes = _hostAllocation.AllocationIdBytes,
            IPv4Address = _hostAllocation.RelayServer.IpV4,
            ConnectionData = _hostAllocation.ConnectionData
        };


        relayHostData.JoinCode = await Relay.Instance.GetJoinCodeAsync(relayHostData.AllocationID);
        
        _relayJoinCode = relayHostData.JoinCode; // Store Code incase it needs to be recalled later

        Transport.SetRelayServerData(relayHostData.IPv4Address, relayHostData.Port, relayHostData.AllocationIDBytes,
                relayHostData.Key, relayHostData.ConnectionData);

        return relayHostData;
    }

    public async Task<RelayJoinData> JoinRelay(string joinCode) {
        
        InitializationOptions options = new InitializationOptions()
            .SetEnvironmentName(environment);

        await UnityServices.InitializeAsync(options);

        if (!AuthenticationService.Instance.IsSignedIn) {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        JoinAllocation allocation = await Relay.Instance.JoinAllocationAsync(joinCode);

        RelayJoinData relayJoinData = new RelayJoinData {
            Key = allocation.Key,
            Port = (ushort)allocation.RelayServer.Port,
            AllocationID = allocation.AllocationId,
            AllocationIDBytes = allocation.AllocationIdBytes,
            ConnectionData = allocation.ConnectionData,
            HostConnectionData = allocation.HostConnectionData,
            IPv4Address = allocation.RelayServer.IpV4,
            JoinCode = joinCode
        };

        Transport.SetRelayServerData(relayJoinData.IPv4Address, relayJoinData.Port, relayJoinData.AllocationIDBytes,
            relayJoinData.Key, relayJoinData.ConnectionData, relayJoinData.HostConnectionData);



        return relayJoinData;
    }
}
