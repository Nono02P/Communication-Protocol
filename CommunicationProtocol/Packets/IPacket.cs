using CommunicationProtocol.Serialization;

namespace CommunicationProtocol.Packets
{
    public interface IPacket
    {
        bool Serialize(Serializer pSerializer);
    }
}
