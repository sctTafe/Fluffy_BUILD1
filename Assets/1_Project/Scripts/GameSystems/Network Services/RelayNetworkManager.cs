using System.Threading.Tasks;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Collections;
using Unity.Networking.Transport;
using Unity.Services.Core.Environments;

public class RelayNetworkManager : MonoBehaviour
{
    [Header("Relay Settings")]
    [SerializeField] private string environment = "production";
    [SerializeField] private int maxConnections = 6;

    public string RelayJoinCode { get; private set; }

    private Allocation hostAllocation;
    private NativeList<NetworkConnection> serverConnections;

    private UnityTransport Transport => NetworkManager.Singleton.GetComponent<UnityTransport>();

    private async Task EnsureUnityServicesInitialized()
    {
        var options = new InitializationOptions().SetEnvironmentName(environment);
        await UnityServices.InitializeAsync(options);

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log($"Signed in. Player ID: {AuthenticationService.Instance.PlayerId}");
        }
    }

    public async Task StartHostWithRelayAsync()
    {
        await EnsureUnityServicesInitialized();

        hostAllocation = await Relay.Instance.CreateAllocationAsync(maxConnections);
        RelayJoinCode = await Relay.Instance.GetJoinCodeAsync(hostAllocation.AllocationId);

        Transport.SetRelayServerData(
            hostAllocation.RelayServer.IpV4,
            (ushort)hostAllocation.RelayServer.Port,
            hostAllocation.AllocationIdBytes,
            hostAllocation.Key,
            hostAllocation.ConnectionData
        );

        NetworkManager.Singleton.StartHost();

        // Optionally track connections
        serverConnections = new NativeList<NetworkConnection>(maxConnections, Allocator.Persistent);
    }

    public async Task StartClientWithRelayAsync(string joinCode)
    {
        await EnsureUnityServicesInitialized();

        JoinAllocation joinAlloc = await Relay.Instance.JoinAllocationAsync(joinCode);

        Transport.SetRelayServerData(
            joinAlloc.RelayServer.IpV4,
            (ushort)joinAlloc.RelayServer.Port,
            joinAlloc.AllocationIdBytes,
            joinAlloc.Key,
            joinAlloc.ConnectionData,
            joinAlloc.HostConnectionData
        );

        NetworkManager.Singleton.StartClient();
    }

    public void Shutdown()
    {
        if (serverConnections.IsCreated)
            serverConnections.Dispose();

        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.Shutdown();
        }
    }
}
