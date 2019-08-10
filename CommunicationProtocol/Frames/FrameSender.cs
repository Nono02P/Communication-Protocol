using CommunicationProtocol.CRC;
using CommunicationProtocol.Frames.Packets;
using CommunicationProtocol.Serialization;
using System;
using System.Diagnostics;

namespace CommunicationProtocol.Frames
{
    public class FrameSender : Frame
    {
        /// <summary>
        /// Maximum Transmission Unit in bytes.
        /// </summary>
        private const int MTU = 1024;
        private bool _shouldClean;

        public FrameSender() : base()
        {
            FrameSerializer = new WriterSerialize();
            //PrepareFrame();
        }

#if TRACE_LOG
        private void Log(string pMessage, bool pEraseFile = false)
        {
            LogHelper.WriteToFile(pMessage, this, Program.FileName, pEraseFile);
        }
#endif

        private PacketHeader PrepareFrame(bool pClearBitPacker = false)
        {
            _shouldClean = false;
            if (pClearBitPacker)
                FrameSerializer.BitPacking.Clear();

            PacketHeader header = new PacketHeader(0, CurrentSequence);             // Write an empty CRC to reserve the 32 first bits to the CRC.
            header.BeginSerialize(FrameSerializer);
            return header;
        }

        public bool ProcessPacket(Packet pPacket)
        {
            pPacket.Header = PrepareFrame(_shouldClean);                                            // CRC + Sequence
         
            Debug.Assert(!Program.StopOnSequence.HasValue || CurrentSequence != Program.StopOnSequence.Value);
            int bitCounter = FrameSerializer.BitPacking.BitLength;
            int id = dFactory.GetID(pPacket);
#if TRACE_LOG
            Log("Packet ID : ");
#endif
            FrameSerializer.Serialize(ref id, 0, dFactory.Count() - 1);                              // Packet ID
#if TRACE_LOG
            Log("Packet Data : ");
#endif
            if (!pPacket.Serialize(FrameSerializer))                                                 // Data
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
                        FrameSerializer.BitPacking.Clear();
                        FrameSerializer.Error = false;
                        return false;
                    }
                }
            }

            if (FrameSerializer.BitPacking.ByteLength > MTU)
                SplitPacketIntoFragments();
            else
                AddCrcOnHeader(pPacket.Header);
            
            CurrentSequence++;
            _shouldClean = true;
            return !FrameSerializer.Error;
        }

        private FragmentedPacket[] SplitPacketIntoFragments()
        {
            int normalHeaderSize = (CRC_SIZE + SEQUENCE_SIZE) / 8;
            int fragmentedHeaderSize = normalHeaderSize +                                           // CRC + Sequence
                (int)Math.Ceiling((decimal)(FrameSerializer.BitsRequired(0, dFactory.Count() - 1) + // Number of bits required to send the packet type.
                FragmentedPacket.FRAGMENT_HEADER_SIZE) / 8);                                        // Fragment ID + Number of fragments
            int dataSize = MTU - fragmentedHeaderSize;
            int nbOfFragments = (int)Math.Ceiling((FrameSerializer.BitPacking.ByteLength - normalHeaderSize) / (decimal)(MTU - fragmentedHeaderSize));
            Span<byte> data = FrameSerializer.BitPacking.GetByteSpanBuffer();
            Span<byte> dataWithoutHeader = data.Slice(normalHeaderSize);
            FragmentedPacket[] packets = new FragmentedPacket[nbOfFragments];
            for (int i = 0; i < nbOfFragments; i++)
            {
                FrameSerializer.BitPacking = new BitPacker();
                packets[i] = new FragmentedPacket();
                packets[i].Header = new PacketHeader(0, CurrentSequence);
                packets[i].Header.BeginSerialize(FrameSerializer);                      // CRC + Sequence
                int id = dFactory.GetID<IPacket>(packets[i]);
                FrameSerializer.Serialize(ref id, 0, dFactory.Count() - 1);             // Packet ID
                packets[i].FragmentID = i;
                packets[i].NumberOfFragments = nbOfFragments;

                int length = dataSize;
                if (i == nbOfFragments - 1)
                    length = dataWithoutHeader.Length % (MTU - fragmentedHeaderSize);

                packets[i].Data = dataWithoutHeader.Slice(dataSize * i, length).ToArray();
                packets[i].Serialize(FrameSerializer);                                  // Data
                AddCrcOnHeader(packets[i].Header);
            }
            return packets;
        }

        private void AddCrcOnHeader(PacketHeader pHeader)
        {
            FrameSerializer.BitPacking.PushTempInBuffer();

            byte[] data = FrameSerializer.BitPacking.GetByteBuffer();
            int crcByteLength = CrcCheck.HashSize / 8;

            CrcCheck.ComputeHash(data, crcByteLength, data.Length - crcByteLength);
            pHeader.Crc = (uint)CrcHelper.FromBigEndian(CrcCheck.Hash, CrcCheck.HashSize);
            pHeader.EndSerialize(FrameSerializer);
        }

        public byte[] Send()
        {
            /*
            FrameSerializer.BitPacking.PushTempInBuffer();

            byte[] data = FrameSerializer.BitPacking.GetByteBuffer();
            int crcByteLength = CrcCheck.HashSize / 8;

            CrcCheck.ComputeHash(data, crcByteLength, data.Length - crcByteLength);
            dCrcValue = (int)CrcHelper.FromBigEndian(CrcCheck.Hash, CrcCheck.HashSize);
            
            FrameSerializer.BitPacking.OverrideValue((uint)dCrcValue, CrcCheck.HashSize);            // Write the CRC at the frame start (reserved area)
            */

            return FrameSerializer.BitPacking.GetByteBuffer();
        }
    }
}