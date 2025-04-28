using System;
using Unity.Netcode;
using UnityEngine;

public class LobbySharedJoinCode : NetworkSingleton<LobbySharedJoinCode>
{
    public event EventHandler OnRelayCodeChanged;
    public string relayJoinCode;
    private NetworkVariable<NetworkString> RelayJoinCodeNV = new NetworkVariable<NetworkString>();
    

    public override void OnNetworkSpawn()
    {
        Debug.Log("LobbySharedJoinCode: OnNetworkSpawn");

        if (IsServer)
        {
            relayJoinCode = RelayManager.Instance.RelayJoinCode;
            Debug.Log($"RelayJoinCode: {relayJoinCode}");
            RelayJoinCodeNV.Value = relayJoinCode;
        }
        if (IsClient)
        {
            RelayJoinCodeNV.OnValueChanged += Handle_OnRelayCodeChange;
            relayJoinCode = RelayJoinCodeNV.Value;
        }
    }

    private void Handle_OnRelayCodeChange(NetworkString previousValue, NetworkString newValue)
    {
        relayJoinCode = RelayJoinCodeNV.Value;
        OnRelayCodeChanged?.Invoke(this, new EventArgs());
    }

    private void OnDisable()
    {
        if (IsClient)
        {
            RelayJoinCodeNV.OnValueChanged -= Handle_OnRelayCodeChange;
        }
    }
}
