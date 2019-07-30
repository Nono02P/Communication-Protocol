using CommunicationProtocol.Serialization;

namespace CommunicationProtocol.Frames.Packets
{
    public class FragmentedPacket : Packet
    {
        private int _fragmentID;
        private int _numberOfFragments;

        public override bool Equals(Packet other)
        {
            throw new System.NotImplementedException();
        }

        public override void Random()
        {
            throw new System.NotImplementedException();
        }

        public override bool Serialize(Serializer pSerializer)
        {
            pSerializer.Serialize(ref _fragmentID, 0, 255);
            pSerializer.Serialize(ref _numberOfFragments, 0, 255);

            return base.Serialize(pSerializer);
        }
    }
}