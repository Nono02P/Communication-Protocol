using CommunicationProtocol.Frames.Packets;
using CommunicationProtocol.Serialization;
using System.Collections.Generic;
using System.Diagnostics;

namespace CommunicationProtocol.Frames
{
    public class FrameReceiver : Frame
    {
        public FrameReceiver() : base()
        {
            Serializer = new ReaderSerialize();
        }

#if TRACE_LOG
        private void Log(string pMessage, bool pEraseFile = false)
        {
            LogHelper.WriteToFile(pMessage, this, Program.FileName, pEraseFile);
        }
#endif

        public List<Packet> Receive(byte[] pData)
        {
            Serializer.BitPacking = BitPacker.FromArray(pData);
            uint crcValue = Serializer.BitPacking.ReadValue(CrcCheck.HashSize);             // CRC
#if TRACE_LOG
            Log("Read CRC : " + crcValue + " (" + CrcCheck.HashSize + "Bits)");
#endif
            dCrcParameters.Check = crcValue;

            byte[] dataCrcCalculation = Serializer.BitPacking.GetByteBuffer();
            if (CrcCheck.IsRight(dataCrcCalculation))
            {
                Sequence = (ushort)Serializer.BitPacking.ReadValue(SEQUENCE_SIZE);          // Sequence
#if TRACE_LOG
                Log("Read Sequence : " + Sequence + " (" + SEQUENCE_SIZE + "Bits)");
#endif
                Debug.Assert(!Program.StopOnSequence.HasValue || Sequence != Program.StopOnSequence.Value);
                List<Packet> result = new List<Packet>();
                int id = 0;

                // Read the packets until an error occur or if the BitLenght is less than a byte.
                // This is because when the data are sent they are always rounded at the superior byte and the end of a packet is filled with 0's.
                while (Serializer.BitPacking.BitLength >= 8 && !Serializer.Error)
                {
#if TRACE_LOG
                    Log("Packet ID : ");
#endif
                    Serializer.Serialize(ref id, 0, dFactory.Count() - 1);                  // Packet ID

                    Packet packet = dFactory.CreateInstance<Packet>(id);
#if TRACE_LOG
                    Log("Packet Data : ");
#endif
                    if (packet.Serialize(Serializer))                                       // Data
                        result.Add(packet);
                }
                return result;
            }
            else
            {
#if TRACE_LOG
                Log("Refused packet !");
#endif
            }
            return null;
        }
    }
}