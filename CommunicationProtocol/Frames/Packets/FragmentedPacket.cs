using CommunicationProtocol.Factories;
using CommunicationProtocol.Serialization;

namespace CommunicationProtocol.Frames.Packets
{
    public class FragmentedPacket : IPacket
    {
        public int FragmentID;
        public int NumberOfFragments;
        public int PacketID;

        public PacketHeader Header { get; set; }

        public bool Serialize(Serializer pSerializer)
        {
            pSerializer.Serialize(ref FragmentID, 0, 255);
            pSerializer.Serialize(ref NumberOfFragments, 0, 255);
            pSerializer.Serialize(ref PacketID, 0, PacketFactory.GetFactory().Count() - 1);

            return !pSerializer.Error;
        }
    }
}