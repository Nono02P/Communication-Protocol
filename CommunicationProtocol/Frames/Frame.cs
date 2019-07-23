using CommunicationProtocol.CRC;
using CommunicationProtocol.Frames.Packets;
using CommunicationProtocol.Serialization;

namespace CommunicationProtocol.Frames
{
    /*  Not fragmented Packet Type :
     *  [CRC-32  (32 bits)]
     *  [Sequence (16 bits)]
     *  [Packet Type (Variable depending of number of packet types)]
     *  [Packet Data (Variable length)]
     *  [Packet End Serialization Check (32 bits)]
     * ====================================================================================
     *  Fragmented Packet Type :
     *  [CRC-32  (32 bits)]
     *  [Sequence (16 bits)]
     *  [Packet Type = 0 (Variable depending of number of packet types)]
     *  [Fragment ID (8 bits)]
     *  [Fragment index (8 bits)]
     *  [Packet Type = 0 (Variable depending of number of packet types)]
     *  [Packet Data (Variable length)]
     *  [Packet End Serialization Check (32 bits)]
     */

    public abstract class Frame
    {
        protected const int SEQUENCE_SIZE = 16;

        protected int dCrcValue;
        protected PacketFactory dFactory;
        protected Parameters dCrcParameters;

        public ushort Sequence { get; protected set; }
        public Crc CrcCheck { get; protected set; }
        public Serializer Serializer { get; protected set; }

        public Frame()
        {
            dCrcParameters = CrcStdParams.StandartParameters[CrcAlgorithms.Crc32];
            CrcCheck = new Crc(dCrcParameters);
            dFactory = PacketFactory.GetFactory();
        }
    }
}
