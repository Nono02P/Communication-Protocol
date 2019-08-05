using CommunicationProtocol.Serialization;

namespace CommunicationProtocol.Frames.Packets
{
    public struct PacketHeader
    {
        public uint Crc;
        public ushort Sequence;

        public void BeginSerialize(Serializer pSerializer)
        {
            int crc = (int)Crc;
            int sequence = Sequence;
            pSerializer.Serialize(ref crc, 0, Frame.CRC_SIZE);
            pSerializer.Serialize(ref sequence, 0, Frame.SEQUENCE_SIZE);
            Crc = (uint)crc;
            Sequence = (ushort)sequence;
        }

        public void EndSerialize(Serializer pSerializer)
        {
            if (pSerializer is WriterSerialize)
                pSerializer.BitPacking.OverrideValue(Crc, (int)Frame.CRC_SIZE);             // Write the CRC value
        }
    }
}
