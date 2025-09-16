using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class NetworkGameTimerDown : INetworkGameTimer
{
    public UnityEvent OnTimmerTrigger;

    // Replicated once: match start server time and total length in seconds.
    private NetworkVariable<float> startTimeSeconds = new NetworkVariable<float>(-1f,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<int> matchLengthSeconds = new NetworkVariable<int>(0,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // Optional: server can periodically push a correction for significant drift (disabled by default)
    [SerializeField] private bool enablePeriodicResync = false;
    [SerializeField] private float resyncInterval = 30f;
    [SerializeField] private float resyncDriftThreshold = 0.5f; // seconds

    private float lastResyncSent;

    public float matchLengthMin = 10; // designer set minutes
    public float matchLengthSec; // designer set extra seconds

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
        if (!isActive) return;
        if (IsServer)
        {
            // Timer end check server side
            if (GetRemainingTime() <= 0f)
            {
                isActive = false;
                OnTimmerTrigger?.Invoke();
            }

            if (enablePeriodicResync && Time.time - lastResyncSent >= resyncInterval)
            {
                lastResyncSent = Time.time;
                // Force a tiny write to correct potential drift by toggling start time slightly if needed (rare)
                // Instead of writing every second, only write if we detect client drift via a lightweight RPC (omitted for simplicity)
            }
        }
    }

    private void StartMatchTimer()
    {
        if (!IsServer) return;
        float serverNow = (float)NetworkManager.ServerTime.Time;
        startTimeSeconds.Value = serverNow;
        int totalSeconds = Mathf.RoundToInt(matchLengthMin * 60f + matchLengthSec);
        matchLengthSeconds.Value = totalSeconds;
        isActive = true;
        lastResyncSent = Time.time;
    }

    private void OnSceneLoaded(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        // Only run once, when all clients finish loading the game scene
        if (!IsServer) return;

        if (sceneName == currentSceneName)
        {
            clientsLoadedScene.Add(clientId);

            // Start the match when all clients are ready
            if (clientsLoadedScene.Count == NetworkManager.ConnectedClientsIds.Count && !isActive)
            {
                Debug.Log("All Players Loaded Into Current Scene");
                StartMatchTimer();
            }
        }
    }

    private float GetRemainingTime()
    {
        if (startTimeSeconds.Value < 0f) return 0f;
        double serverNow = NetworkManager.ServerTime.Time;
        double endTime = startTimeSeconds.Value + matchLengthSeconds.Value;
        return (float)(endTime - serverNow);
    }

    public override string GetFormattedTime(float time)
    {
        time = Mathf.Max(0f, time);
        int hours = Mathf.FloorToInt(time / 3600);
        int minutes = Mathf.FloorToInt((time % 3600) / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        return $"{hours:00}:{minutes:00}:{seconds:00}";
    }

    public override string GetCurrentTimeFormatted()
    {
        return GetFormattedTime(GetRemainingTime());
    }
}