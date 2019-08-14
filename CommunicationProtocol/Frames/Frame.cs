using CommunicationProtocol.CRC;
using CommunicationProtocol.Factories;
using CommunicationProtocol.Serialization;
using System;

namespace CommunicationProtocol.Frames
{
    #region Normal Packet trame
    /*  [CRC-32  (32 bits)]
     *  [Sequence (16 bits)]
     *  [Packet Type (Variable depending of number of packet types)]
     *  [Packet Data (Variable length)]
     *  [Packet End Serialization Check (32 bits)]
    */
    #endregion Normal Packet trame  
    
    #region Fragmented Packet trame
    /*  [CRC-32  (32 bits)]
     *  [Sequence (16 bits)]
     *  [Packet Type = 0 (Variable depending of number of packet types)]
     *  [Fragment ID (8 bits)]
     *  [Number of Fragments (8 bits)]
     *  [Packet Type = 0 (Variable depending of number of packet types)]
     *  [Pad zero bits to nearest byte]
     *  [Fragment Data (Variable length)]
     *      Fragment Data with an exemple of packet splitted in 3 Fragments :
     *          1-> [Packet Type (Variable depending of number of packet types)]
     *          1-> [Splitted Packet Data (Variable length)]
     *          2-> [Splitted Packet Data (Variable length)]
     *          3-> [Splitted Packet Data (Variable length)]
     *          3-> [Packet End Serialization Check (32 bits)]
    */
    #endregion Fragmented Packet trame
    
    public abstract class Frame
    {
        public static int CRC_SIZE = 32;
        public static int SEQUENCE_SIZE = 16;

        protected PacketFactory dFactory;
        protected Parameters dCrcParameters;

        public ushort CurrentSequence { get; protected set; }
        public Crc CrcCheck { get; protected set; }
        public Serializer FrameSerializer { get; protected set; }

        public Frame()
        {
            dCrcParameters = CrcStdParams.StandartParameters[CrcAlgorithms.Crc32];
            CrcCheck = new Crc(dCrcParameters);
            dFactory = PacketFactory.GetFactory();
        }

        /// <summary>
        /// Check the sequences order and return true if the S1 is newer than S2.
        /// </summary>
        /// <param name="pS1">Sequence 1</param>
        /// <param name="pS2">Sequence 2</param>
        /// <returns>Return a boolean that is equal to true if S1 is newer than S2.</returns>
        public bool SequenceGreatherThan(uint pS1, uint pS2)
        {
            ulong mask = 1ul << (SEQUENCE_SIZE - 1);
            return ((pS1 > pS2) && (pS1 - pS2 <= mask)) ||
               ((pS1 < pS2) && (pS2 - pS1 > mask));
        }

        /// <summary>
        /// Check the sequences order and return true if the S1 is older than S2.
        /// </summary>
        /// <param name="pS1">Sequence 1</param>
        /// <param name="pS2">Sequence 2</param>
        /// <returns>Return a boolean that is equal to true if S1 is older than S2.</returns>
        public bool SequenceLessThan(uint pS1, uint pS2)
        {
            return SequenceGreatherThan(pS2, pS1);
        }

        public int SequenceDifference (uint pS1, uint pS2)
        {
            if (Math.Abs(pS1 - pS2) >= (long)(1ul << (SEQUENCE_SIZE - 1)))
            {
                if (pS1 > pS2)
                    pS2 += (uint)1ul << SEQUENCE_SIZE;
                else
                    pS1 += (uint)1ul << SEQUENCE_SIZE;
            }
            return (int)(pS1 - pS2);
        }
    }
}