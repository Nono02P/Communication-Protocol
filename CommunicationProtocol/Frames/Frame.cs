﻿using CommunicationProtocol.CRC;
using CommunicationProtocol.Factories;
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


        /// <summary>
        /// Check the sequences order and return true if the S1 is newer than S2.
        /// </summary>
        /// <param name="pS1">Sequence 1</param>
        /// <param name="pS2">Sequence 2</param>
        /// <returns>Return a boolean that is equal to true if S1 is newer than S2.</returns>
        public bool SequenceGreatherThan(uint pS1, uint pS2)
        {
            return ((pS1 > pS2) && (pS1 - pS2 <= 32768)) ||
               ((pS1 < pS2) && (pS2 - pS1 > 32768));
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
    }
}
