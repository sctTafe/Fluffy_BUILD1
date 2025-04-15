using Unity.Netcode;
using UnityEngine;

public class NetworkGameTimer : NetworkBehaviour
{
    private float serverStartTime; // When the timer started (in seconds since game start)
    private NetworkVariable<float> networkElapsedTime = new NetworkVariable<float>(default,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public float ElapsedTime => Time.time - serverStartTime;
    private float lastSyncTime; // var for time to sync to the network variable

    private bool isActive;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            serverStartTime = Time.time;
            isActive = true;
        }
    }

    private void Update()
    {
        if (IsServer && isActive)
        {
            // Update elapsed time periodically for syncing
            if (Time.time - lastSyncTime >= 1f) // Sync every second
            {
                networkElapsedTime.Value = ElapsedTime;
                lastSyncTime = Time.time;
            }
        }

    }

    public string GetFormattedTime(float time)
    {
        int hours = Mathf.FloorToInt(time / 3600);
        int minutes = Mathf.FloorToInt((time % 3600) / 60);
        int seconds = Mathf.FloorToInt(time % 60);

        return $"{hours:00}:{minutes:00}:{seconds:00}";
    }

    public string GetCurrentTimeFormatted()
    {
        float displayTime = IsServer ? ElapsedTime : networkElapsedTime.Value;
        return GetFormattedTime(displayTime);
    }

}
