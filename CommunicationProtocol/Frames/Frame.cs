using CommunicationProtocol.CRC;

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
     *  [Packet Data (Variable length)]
     *  [Packet End Serialization Check (32 bits)]
     */

    public class Frame
    {
        public int Sequence { get; private set; }
        public Crc CrcCheck { get; private set; }


    }
}