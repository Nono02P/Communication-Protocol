using CommunicationProtocol.Serialization;

namespace CommunicationProtocol.Frames.Packets
{
    public interface IPacket
    {
        bool Serialize(Serializer pSerializer);
    }
}
