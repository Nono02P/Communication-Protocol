using CommunicationProtocol.Serialization;

namespace CommunicationProtocol.Frames.Packets
{
    public class FragmentedPacket : IPacket
    {
        public const int FRAGMENT_HEADER_SIZE = 2 * 8;      // 8 bits for index of fragmented packet + 8 bits for number of fragmented packets

        public int FragmentID;
        public int NumberOfFragments;
        public byte[] Data;

        public PacketHeader Header { get; set; }

        public bool Equals(IPacket other)
        {
            if (other is FragmentedPacket)
            {
                FragmentedPacket o = (FragmentedPacket)other;
                bool dataIsEquals = true;
                if (Data != null && o.Data != null)
                {
                    if (Data.Length != o.Data.Length)
                        dataIsEquals = false;
                    int i = 0;
                    while (dataIsEquals && i < Data.Length)
                    {
                        if (Data[i] != o.Data[i])
                            dataIsEquals = false;
                        i++;
                    }
                }
                return FragmentID == o.FragmentID && 
                    NumberOfFragments == o.NumberOfFragments &&
                    dataIsEquals;
            }
            return false;
        }

        public bool Serialize(Serializer pSerializer)
        {
            pSerializer.Serialize(ref FragmentID, FRAGMENT_HEADER_SIZE / 2);
            pSerializer.Serialize(ref NumberOfFragments, FRAGMENT_HEADER_SIZE / 2);
            if (Data == null)
            {
                Data = new byte[pSerializer.BitPacking.ByteLength];
            }
            pSerializer.Serialize(ref Data, Data.Length);
            
            return !pSerializer.Error;
        }
    }
}