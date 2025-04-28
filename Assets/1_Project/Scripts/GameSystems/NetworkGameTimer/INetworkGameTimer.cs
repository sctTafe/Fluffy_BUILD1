using Unity.Netcode;
using UnityEngine;

public abstract class INetworkGameTimer : NetworkBehaviour
{


    public abstract string GetFormattedTime(float time);
    public abstract string GetCurrentTimeFormatted();
}
