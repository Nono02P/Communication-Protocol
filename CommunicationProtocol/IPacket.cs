namespace CommunicationProtocol
{
    interface IPacket
    {
        bool Serialize(Serializer pSerializer);
    }
}
