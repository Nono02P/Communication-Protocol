using CommunicationProtocol.Serialization;
using System;

namespace CommunicationProtocol.Frames.Packets
{
    public interface IPacket : IEquatable<IPacket>
    {
        PacketHeader Header { get; set; }

        bool Serialize(Serializer pSerializer);
    }
}
