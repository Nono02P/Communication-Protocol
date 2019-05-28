namespace CommunicationProtocol
{
    public interface IPacket
    {
        bool Serialize(Serializer pSerializer);
    }
}
