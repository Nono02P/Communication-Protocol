using CommunicationProtocol.CRC;
using CommunicationProtocol.Frames.Packets;
using CommunicationProtocol.Serialization;
using System;
using System.Diagnostics;

namespace CommunicationProtocol.Frames
{
    public class FrameSender : Frame
    {
        private const int MTU = 1200;
        private bool _shouldClean;

        public FrameSender() : base()
        {
            Serializer = new WriterSerialize();
            PrepareFrame();
        }

#if TRACE_LOG
        private void Log(string pMessage, bool pEraseFile = false)
        {
            LogHelper.WriteToFile(pMessage, this, Program.FileName, pEraseFile);
        }
#endif

        private void PrepareFrame()
        {
            _shouldClean = false;
            Serializer.BitPacking.Clear();
            Serializer.BitPacking.WriteValue(0, CrcCheck.HashSize);         // Write an empty CRC
#if TRACE_LOG
            Log("Reserve Empty CRC : " + 0 + " (" + CrcCheck.HashSize + "Bits)");
#endif
            Serializer.BitPacking.WriteValue(Sequence, SEQUENCE_SIZE);      // Write the Sequence index
#if TRACE_LOG
            Log("Sequence : " + Sequence + " (" + SEQUENCE_SIZE + "Bits)");
#endif
        }

        public bool InsertPacket(Packet pPacket)
        {
            if (_shouldClean)
                PrepareFrame();

            Debug.Assert(!Program.StopOnSequence.HasValue || Sequence != Program.StopOnSequence.Value);
            int bitCounter = Serializer.BitPacking.BitLength;
            int id = dFactory.GetID(pPacket);
#if TRACE_LOG
            Log("Packet ID : ");
#endif
            Serializer.Serialize(ref id, 0, dFactory.Count() - 1);                              // Packet ID
#if TRACE_LOG
            Log("Packet Data : ");
#endif
            if (!pPacket.Serialize(Serializer))                                                 // Data
            {
#if DEBUG
                if (pPacket is PacketB)
#endif
                {
#if DEBUG
                    PacketB p = (PacketB)pPacket;
                    if (p.Actors.Count == 0)
#endif
                    {
                        int dif = Serializer.BitPacking.BitLength - bitCounter;
                        Serializer.BitPacking.RemoveFromEnd(dif);
                        Serializer.Error = false;
                        return false;
                    }
                }
            }

            if (Serializer.BitPacking.ByteLength > MTU)
            {
                throw new Exception("Call the fragmentation code here.");
            }
            return !Serializer.Error;
        }

        public byte[] Send()
        {
            Serializer.BitPacking.PushTempInBuffer();

            byte[] data = Serializer.BitPacking.GetByteBuffer();
            int crcByteLength = CrcCheck.HashSize / 8;

            CrcCheck.ComputeHash(data, crcByteLength, data.Length - crcByteLength);
            dCrcValue = (int)CrcHelper.FromBigEndian(CrcCheck.Hash, CrcCheck.HashSize);
            
            Serializer.BitPacking.OverrideValue((uint)dCrcValue, CrcCheck.HashSize);            // Write the CRC at the frame start (reserved area)
            data = Serializer.BitPacking.GetByteBuffer();
            
            Sequence++;
            _shouldClean = true;
            return data;
        }
    }
}