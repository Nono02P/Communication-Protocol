using CommunicationProtocol.Serialiser;

namespace CommunicationProtocol.Packets
{
    public interface IPacket
    {
        bool Serialize(Serializer pSerializer);
    }
}
