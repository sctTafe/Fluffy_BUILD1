using System;
using Unity.Collections;
using Unity.Netcode;

/// <summary>
/// Holder Strut for Player Data: ID, Name etc.
/// </summary>
[Serializable]
public struct PlayerData : IEquatable<PlayerData>, INetworkSerializable
{
    public ulong clientId;
    public FixedString64Bytes playerName;
    public FixedString64Bytes playerId;
    public bool goodTeam;

    public bool Equals(PlayerData other)
    {
        return
            clientId == other.clientId &&
            playerName == other.playerName &&
            goodTeam == other.goodTeam &&
            playerId == other.playerId;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref goodTeam);
        serializer.SerializeValue(ref playerName);
        serializer.SerializeValue(ref playerId);
    }
}


