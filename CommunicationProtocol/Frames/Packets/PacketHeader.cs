using CommunicationProtocol.Serialization;
using System;
using System.Diagnostics;

namespace CommunicationProtocol.Frames.Packets
{
    public class PacketHeader : IEquatable<PacketHeader>
    {
        public uint Crc;
        public ushort Sequence;

        public PacketHeader(uint pCrcValue, ushort pSequence)
        {
            Crc = pCrcValue;
            Sequence = pSequence;
        }

        public void BeginSerialize(Serializer pSerializer)
        {
            int crc = (int)Crc;
            int sequence = Sequence;
#if TRACE
            Trace.WriteLine("Serialize PacketHeader :");
            Trace.Indent();
            Trace.WriteLine("CRC :");
#endif
            pSerializer.Serialize(ref crc, Frame.CRC_SIZE);

#if TRACE
            Trace.WriteLine("Sequence :");
#endif
            pSerializer.Serialize(ref sequence, Frame.SEQUENCE_SIZE);
            Crc = (uint)crc;
            Sequence = (ushort)sequence;

#if TRACE
            Trace.Unindent();
            Trace.WriteLine("End of PacketHeader :");
#endif
        }

        public void EndSerialize(Serializer pSerializer)
        {
            if (pSerializer is WriterSerialize)
                pSerializer.BitPacking.OverrideValue(Crc, (int)Frame.CRC_SIZE);             // Write the CRC value
        }

        public bool Equals(PacketHeader other)
        {
            return Crc == other.Crc && 
                Sequence == other.Sequence;
        }
    }
}
