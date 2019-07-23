using CommunicationProtocol.Frames.Packets;
using CommunicationProtocol.Serialization;
using System;
using System.Collections.Generic;

namespace CommunicationProtocol.Frames
{
    public class FrameReceiver : Frame
    {
        public FrameReceiver() : base()
        {
#if TRACE_LOG
            Log("Init Receiver");
#endif
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

            dCrcParameters.Check = crcValue;

            byte[] dataCrcCalculation = Serializer.BitPacking.GetByteBuffer();
            if (CrcCheck.IsRight(dataCrcCalculation))
            {
                Sequence = (ushort)Serializer.BitPacking.ReadValue(SEQUENCE_SIZE);          // Sequence
                List<Packet> result = new List<Packet>();
                int id = 0;

                // Lis les paquets tant qu'aucune erreur ne se produit à la désérialisation 
                // et tant qu'il reste au moins 1 octet (parce que la fin d'un paquet se termine toujours par des 0 pour combler à l'octet supérieur).
                while (Serializer.BitPacking.BitLength >= 8 && !Serializer.Error)
                {
                    Serializer.Serialize(ref id, 0, dFactory.Count() - 1);                  // ID de paquet

                    Packet packet = dFactory.CreateInstance<Packet>(id);
                    if (packet.Serialize(Serializer))                                       // Data
                        result.Add(packet);
                }
                return result;
            }
            else
                Console.WriteLine("Paquet refusé !");
            return null;
        }
    }
}