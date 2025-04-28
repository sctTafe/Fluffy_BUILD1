using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class NetworkGameTimerDown : INetworkGameTimer
{
    public UnityEvent OnTimmerTrigger;

    private float serverStartTime; // When the timer started (in seconds since game start)
    private NetworkVariable<float> networkMatchTime = new NetworkVariable<float>(default,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public float ElapsedTime => Time.time - serverStartTime;
    private float lastSyncTime; // var for time to sync to the network variable
    public float matchLengthMin = 10; 
    public float matchLengthSec;
    public float matchTimer => matchLengthSec - ElapsedTime;
    private bool isActive;

    // checking if all clients are in scene
    private HashSet<ulong> clientsLoadedScene = new HashSet<ulong>();
    string currentSceneName;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            currentSceneName = SceneManager.GetActiveScene().name;
            NetworkManager.SceneManager.OnLoadComplete += OnSceneLoaded;
        }
    }
    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkManager.SceneManager.OnLoadComplete -= OnSceneLoaded;
        }
    }
    private void Update()
    {
        if (IsServer && isActive)
        {
            // Update elapsed time periodically for syncing
            if (Time.time - lastSyncTime >= 1f) // Sync every second
            {
                CheckForTimeExhausted();
                networkMatchTime.Value = matchTimer;
                lastSyncTime = Time.time;
            }
        }
    }
    private void StartMatchTimer()
    {
        serverStartTime = Time.time;
        matchLengthSec += matchLengthMin * 60;
        isActive = true;
        lastSyncTime = Time.time;
    }

    private void OnSceneLoaded(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        // Only run once, when all clients finish loading the game scene
        if (!IsServer) return;

        if (sceneName == currentSceneName) // Replace with your actual game scene name
        {
            clientsLoadedScene.Add(clientId);

            // Start the match when all clients are ready
            if (clientsLoadedScene.Count == NetworkManager.ConnectedClientsIds.Count)
            {
                Debug.Log("All Players Loaded Into Current Scene");
                StartMatchTimer();
            }
        }
    }



    public override string GetFormattedTime(float time)
    {
        int hours = Mathf.FloorToInt(time / 3600);
        int minutes = Mathf.FloorToInt((time % 3600) / 60);
        int seconds = Mathf.FloorToInt(time % 60);

        return $"{hours:00}:{minutes:00}:{seconds:00}";
    }

    public override string GetCurrentTimeFormatted()
    {
        float displayTime = IsServer ? matchTimer : networkMatchTime.Value;
        return GetFormattedTime(displayTime);
    }

    private void CheckForTimeExhausted() 
    {
        if(matchTimer <= 0)
        {
            isActive = false;
            OnTimmerTrigger?.Invoke();
        }
    }

}