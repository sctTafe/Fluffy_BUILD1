using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Scott_SceneLoadTracker : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Canvas loadingCanvas;

    private NetworkVariable<int> playersLoaded = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private int totalPlayersExpected;

    private void Start()
    {
        // Both Client & Server
        totalPlayersExpected = NetworkManager.Singleton.ConnectedClientsList.Count;
        playersLoaded.OnValueChanged += OnPlayersLoadedChanged;
        

        // Server Only
        if (IsServer)
        {           
            NetworkManager.Singleton.SceneManager.OnLoadComplete += OnClientLoadedScene;                     
        }

        // Client Only
        if (IsClient) 
        {
            // Subscribe to the "all players finished loading" event
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnAllPlayersLoaded; 
        }
    }

    private void UpdateStatusText(int currentLoaded)
    {
        if (statusText != null)
        {
            statusText.text = $"Loading... {currentLoaded} / {totalPlayersExpected} players ready";
        }
    }

    private void OnPlayersLoadedChanged(int oldValue, int newValue)
    {
        Debug.Log($"Players Loaded: {newValue}/{totalPlayersExpected}");
        UpdateStatusText(newValue);
    }

    private void OnAllPlayersLoaded(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (statusText != null)
        {
            statusText.text = $"All {clientsCompleted.Count} players loaded!";
        }

        // Disable the canvas
        if (loadingCanvas != null)
        {
            loadingCanvas.gameObject.SetActive(false);
        }
    }

    private void OnClientLoadedScene(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        if (IsServer)
        {
            playersLoaded.Value++;
        }
    }
}
