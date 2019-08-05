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

        private PacketHeader PrepareFrame()
        {
            _shouldClean = false;
            Serializer.BitPacking.Clear();
            PacketHeader header = new PacketHeader();
            header.Sequence = CurrentSequence;
            header.BeginSerialize(Serializer);
            return header;
        }

        public bool InsertPacket(Packet pPacket)
        {
            if (_shouldClean)
                pPacket.Header = PrepareFrame();
            else
                pPacket.Header = new PacketHeader() { Sequence = CurrentSequence };

            Debug.Assert(!Program.StopOnSequence.HasValue || CurrentSequence != Program.StopOnSequence.Value);
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
                SplitPacketIntoFragments();
            
            return !Serializer.Error;
        }

        private void SplitPacketIntoFragments()
        {
            int nbOfFragments = (int)Math.Ceiling(Serializer.BitPacking.ByteLength / (decimal)MTU);
            for (int i = 0; i < nbOfFragments; i++)
            {

            }
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
            
            CurrentSequence++;
            _shouldClean = true;
            return data;
        }
    }
}