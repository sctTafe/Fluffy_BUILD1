using System.Collections;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class TestNetworkManager : MonoBehaviour
{
    public string ConnectAddress = "127.0.0.1";
    private float connectionTimeout = 0.5f;

    public GameObject humanPrefab;
    public GameObject mutantPrefab;

    public bool spawnAsMutant = false;

    public bool isHost = true;

    void Start()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager not found in the scene!");
            return;
        }

        // Register the ConnectionApprovalCallback globally
        //NetworkManager.Singleton.ConnectionApprovalCallback = ApproveConnection;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

        if (!isHost)
        StartClientAttempt();
        else
            NetworkManager.Singleton.StartHost();

    }

    private void StartClientAttempt()
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.ConnectionData.Address = ConnectAddress;
        Debug.Log("Attempting to start as client...");

        NetworkManager.Singleton.NetworkConfig.ConnectionData = new byte[] { (byte)(spawnAsMutant ? 1 : 0) };
        NetworkManager.Singleton.StartClient();
    }

    private void OnClientConnected(ulong clientId)
    {
        // Ensure this only runs on the server
        if (!NetworkManager.Singleton.IsServer) return;

        Debug.Log($"Client {clientId} connected. Deciding their player type...");

        // For testing: alternate prefab type between clients
        bool isMutant = (clientId % 2 == 0); // Example logic: even client IDs are mutants

        // Select the appropriate prefab
        GameObject playerPrefab = isMutant ? mutantPrefab : humanPrefab;

        // Instantiate and spawn the player
        GameObject playerInstance = Instantiate(playerPrefab, GetSpawnPosition(), Quaternion.identity);
        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);

        Debug.Log($"Spawned {(isMutant ? "Mutant" : "Human")} for Client {clientId}.");
    }




    private IEnumerator ConnectionTimeoutCoroutine()
    {
        float timer = 0f;
        while (timer < connectionTimeout)
        {
            if (NetworkManager.Singleton.IsConnectedClient)
            {
                Debug.Log("Successfully connected as client.");
                yield break;
            }
            timer += Time.deltaTime;
            yield return null;
        }

        Debug.Log("Client connection timed out. Shutting down client and starting as host.");
        NetworkManager.Singleton.Shutdown();
        yield return null;
        NetworkManager.Singleton.StartHost();

        Debug.Log($"Server started: {NetworkManager.Singleton.IsServer}");
    }

    /* //This will be useful later maybe, turn on approval check in network manager
    private void ApproveConnection(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        ulong clientId = request.ClientNetworkId;
        bool isMutant = request.Payload.Length > 0 && request.Payload[0] == 1;

        GameObject playerPrefab = isMutant ? mutantPrefab : humanPrefab;
        Vector3 spawnPosition = GetSpawnPosition();
        Quaternion spawnRotation = Quaternion.identity;

        GameObject playerInstance = Instantiate(playerPrefab, spawnPosition, spawnRotation);
        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);

        Debug.Log($"Spawned {(isMutant ? "Mutant" : "Human")} for Client {clientId}");

        response.Approved = true;
        response.CreatePlayerObject = false; // Don't use default prefab
    }
    */

    private Vector3 GetSpawnPosition()
    {
        return new Vector3(Random.Range(-5, 5), 0, Random.Range(-5, 5));
    }
}