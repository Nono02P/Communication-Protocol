using CommunicationProtocol.Serialization;

namespace CommunicationProtocol.Frames.Packets
{
    public interface IPacket
    {
        PacketHeader Header { get; set; }

        bool Serialize(Serializer pSerializer);
    }
}
