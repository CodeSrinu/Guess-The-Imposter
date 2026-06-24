using System;
using System.Collections;
using Unity.Collections;
using Unity.Netcode;
public struct PlayerNetworkData: INetworkSerializable, IEquatable<PlayerNetworkData>
{
    public FixedString64Bytes name;
    public bool hasGivenClue;
    public bool hasVoted;
    public bool isEliminated;


    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref name);
        serializer.SerializeValue(ref hasGivenClue);
        serializer.SerializeValue(ref hasVoted);
        serializer.SerializeValue(ref isEliminated);
    }

    public bool Equals(PlayerNetworkData other)
    {
        return name == other.name && hasGivenClue == other.hasGivenClue &&
            hasVoted == other.hasVoted && isEliminated == other.isEliminated;
    }



}

