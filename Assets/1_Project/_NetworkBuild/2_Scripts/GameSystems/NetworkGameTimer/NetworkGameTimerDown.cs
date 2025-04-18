using Unity.Netcode;
using UnityEngine;

public class NetworkGameTimerDown : INetworkGameTimer
{
    private float serverStartTime; // When the timer started (in seconds since game start)
    private NetworkVariable<float> networkMatchTime = new NetworkVariable<float>(default,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public float ElapsedTime => Time.time - serverStartTime;
    private float lastSyncTime; // var for time to sync to the network variable
    public float matchLengthMin = 10; 
    public float matchLengthSec;
    public float matchTimer => matchLengthSec - ElapsedTime;
    private bool isActive;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            serverStartTime = Time.time;
            isActive = true;
            matchLengthSec = matchLengthMin * 60;//turn mins into seconds
        }
    }

    private void Update()
    {
        if (IsServer && isActive)
        {
            // Update elapsed time periodically for syncing
            if (Time.time - lastSyncTime >= 1f) // Sync every second
            {
                networkMatchTime.Value = matchTimer;
                lastSyncTime = Time.time;
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

}