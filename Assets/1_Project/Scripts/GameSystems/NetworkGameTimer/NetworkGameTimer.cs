using Unity.Netcode;
using UnityEngine;

public class NetworkGameTimer : INetworkGameTimer
{
    // Single write-once start time replicated to late joiners. No per-second updates.
    private NetworkVariable<float> startTimeSeconds = new NetworkVariable<float>(-1f,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private bool isActive;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Use server time for proper synchronization (double -> float acceptable precision for timers)
            startTimeSeconds.Value = (float)NetworkManager.ServerTime.Time;
            isActive = true;
        }
    }

    // No Update needed for syncing; kept in case future logic required
    private void Update()
    {
        // Could add server authoritative stop logic here if needed
    }

    private float GetElapsed()
    {
        if (startTimeSeconds.Value < 0f) return 0f;
        // All peers reference ServerTime for a consistent clock
        return (float)(NetworkManager.ServerTime.Time - startTimeSeconds.Value);
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
        return GetFormattedTime(GetElapsed());
    }
}
