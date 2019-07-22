using CommunicationProtocol.CRC;
using CommunicationProtocol.Frames.Packets;
using CommunicationProtocol.Serialization;

namespace CommunicationProtocol.Frames
{
    /*  Not fragmented Packet Type.
     *  [CRC-32  (32 bits)]
     *  [Sequence (16 bits)]
     *  [Packet Type (3 bits)]
     *  [Packet Data (Variable length)]
     *  [Packet End Serialization Check (32 bits)]
     * ====================================================================================
     *  Fragmented Packet Type.
     *  [CRC-32  (32 bits)]
     *  [Sequence (16 bits)]
     *  [Packet Type = 0 (3 bits)]
     *  [Fragment ID (8 bits)]
     *  [Fragment index (8 bits)]
     *  [Packet Type = 0 (3 bits)]
     *  [Packet Data (Variable length)]
     *  [Packet End Serialization Check (32 bits)]
     */

    public abstract class Frame
    {
        protected int dCrcValue;
        protected PacketFactory dFactory;

        public short Sequence { get; protected set; }
        public Crc CrcCheck { get; protected set; }
        public Serializer Serializer { get; protected set; }

        public Frame()
        {
            Parameters parameters = CrcStdParams.StandartParameters[CrcAlgorithms.Crc32];
            CrcCheck = new Crc(parameters);
            dFactory = PacketFactory.GetFactory();
        }
    }
}
