using System;
using CRC;

namespace CommunicationProtocol
{
    class Program
    {
        static void Main(string[] args)
        {
            // Sender side
            // ============================================================================================
            BitPacking bufferSender = new BitPacking();
            bufferSender.WriteValue(0x39, 8);
            bufferSender.WriteValue(0x2B, 6);
            Console.WriteLine("Data envoyées sans CRC :" + bufferSender.ToString());

            Parameters parameters = CrcStdParams.StandartParameters[CrcAlgorithms.Crc32];
            Crc crcSender = new Crc(parameters);
            crcSender.ComputeHash(bufferSender.GetByteBuffer());
            uint value = (uint)CrcHelper.FromBigEndian(crcSender.Hash, crcSender.HashSize);

            bufferSender.AlignToNextByte();
            bufferSender.WriteValue(value, 32);

            Console.WriteLine("Data envoyées avec CRC :" + bufferSender.ToString());

            // Trucage des données pour test du CRC
            bufferSender = new BitPacking();
            bufferSender.WriteValue(0x38, 8);
            bufferSender.WriteValue(0x2B, 6);

            bufferSender.AlignToNextByte();
            bufferSender.WriteValue(value, 32);
            bufferSender.PushTempInBuffer();
            // Console.WriteLine("Valeur du CRC :" + Convert.ToString(value, toBase: 16));

            Console.WriteLine("=================================================================");

            // Receiver side
            // ============================================================================================
            BitPacking bufferReceiver = BitPacking.FromArray(bufferSender.GetByteBuffer());
            Console.WriteLine("Réception data avec CRC :" + bufferReceiver.ToString());
            uint crcValue = bufferReceiver.ReadValue(32, true, bufferReceiver.Length - 32);
            Console.WriteLine("Réception data sans CRC :" + bufferReceiver.ToString());

            Crc crcReceiver = new Crc(parameters);
            parameters.Check = crcValue;

            if (crcReceiver.IsRight(bufferReceiver.GetByteBuffer()))
                Console.WriteLine("Paquet accepté !");
            else
                Console.WriteLine("Paquet refusé !");

            Console.Read();
        }
    }
}
