﻿using CommunicationProtocol.Frames.Packets;
using CommunicationProtocol.Serialization;
using System.Collections.Generic;
using System.Diagnostics;

namespace CommunicationProtocol.Frames
{
    public sealed class FrameReceiver : Frame
    {
        public FrameReceiver() : base()
        {
            FrameSerializer = new ReaderSerialize();
        }

        public List<IPacket> Receive(byte[] pData)
        {
            FrameSerializer.BitPacking = BitPacker.FromArray(pData);
            uint crcValue = FrameSerializer.BitPacking.ReadValue(CrcCheck.HashSize);             // CRC
#if TRACE
            Trace.WriteLine("Read CRC : " + crcValue + " (" + CrcCheck.HashSize + "Bits)");
#endif
            dCrcParameters.Check = crcValue;

            byte[] dataCrcCalculation = FrameSerializer.BitPacking.GetByteBuffer();
            if (CrcCheck.IsRight(dataCrcCalculation))
            {
                CurrentSequence = (ushort)FrameSerializer.BitPacking.ReadValue(SEQUENCE_SIZE);          // Sequence
#if TRACE
                Trace.WriteLine("Read Sequence : " + CurrentSequence + " (" + SEQUENCE_SIZE + "Bits)");
#endif

                Debug.Assert(!Program.StopOnSequence.HasValue || CurrentSequence != Program.StopOnSequence.Value);
                List<IPacket> result = new List<IPacket>();
                int id = 0;

                // Read the packets until an error occur or if the BitLenght is less than a byte.
                // This is because when the data are sent they are always rounded at the superior byte and the end of a packet is filled with 0's.
                while (FrameSerializer.BitPacking.BitLength >= 8 && !FrameSerializer.Error)
                {
#if TRACE
                    Trace.WriteLine("Packet ID : ");
#endif
                    FrameSerializer.Serialize(ref id, 0, dFactory.Count() - 1);                  // Packet ID

                    IPacket packet = dFactory.CreateInstance<IPacket>(id);
                    packet.Header = new PacketHeader(crcValue, CurrentSequence);
#if TRACE
                    Trace.WriteLine("Packet Data : ");
#endif
                    if (packet.Serialize(FrameSerializer))                                       // Data
                        result.Add(packet);
                }
                return result;
            }
            else
            {
#if TRACE
                Trace.WriteLine("Refused packet !");
#endif
            }
            return null;
        }
    }
}