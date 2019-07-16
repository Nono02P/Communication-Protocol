using CommunicationProtocol.Serialization;

namespace CommunicationProtocol.Frames.Packets
{
    public class FragmentedPacket : IPacket
    {
        private int _fragmentID;
        private int _numberOfFragments;

        public bool Serialize(Serializer pSerializer)
        {
            pSerializer.Serialize(ref _fragmentID, 0, 255);
            pSerializer.Serialize(ref _numberOfFragments, 0, 255);
            return !pSerializer.Error;
        }
    }
}