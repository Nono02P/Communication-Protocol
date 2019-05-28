using CRC;
using System;
using System.Numerics;

namespace CommunicationProtocol
{
    class Program
    {
        static void Main(string[] args)
        {
            TestSerializer();
            Console.Read();
        }

        static void TestSerializer()
        {
            Serializer sender = new WriterSerialize();
            PacketA sendedPacket = new PacketA() { Position = new Vector3(-29.158f, 50.735f, 150.2875f), f = 100.191f, comment = "Je suis CON !!" };
            bool sendingAuthorized = sendedPacket.Serialize(sender);
            sendedPacket = new PacketA() { Position = new Vector3(50, 100, 40), f = 45.02f, comment = "Je suis un test." };
            sendingAuthorized |= sendedPacket.Serialize(sender);
            sender.dBitPacking.PushTempInBuffer();

            if (sendingAuthorized)
            {
                Serializer receiver = new ReaderSerialize();
                receiver.dBitPacking = sender.dBitPacking;
                PacketA receivedPacket = new PacketA();
                PacketA receivedPacket2 = new PacketA();
                bool isValid = receivedPacket.Serialize(receiver);
                isValid = receivedPacket2.Serialize(receiver);
            }
        }

        static void TestCRC()
        {
            // Sender side
            // ============================================================================================
            BitPacker bufferSender = new BitPacker();
            bufferSender.WriteValue(0x39, 6);
            bufferSender.WriteValue(0x2B, 6);
            Console.WriteLine("Data envoyées sans CRC :" + bufferSender.ToString());

            Parameters parameters = CrcStdParams.StandartParameters[CrcAlgorithms.Crc32];
            Crc crcSender = new Crc(parameters);
            crcSender.ComputeHash(bufferSender.GetByteBuffer());
            uint value = (uint)CrcHelper.FromBigEndian(crcSender.Hash, crcSender.HashSize);

            bufferSender.AlignToNextByte();
            bufferSender.WriteValue(value, 32);

            /*
            Console.WriteLine("Data envoyées avec CRC :" + bufferSender.ToString());

            // Trucage des données pour test du CRC
            bufferSender = new BitPacking();
            bufferSender.WriteValue(0x39, BitsRequired(0, 63));
            bufferSender.WriteValue(0x2B, BitsRequired(0, 63));

            bufferSender.AlignToNextByte();
            bufferSender.WriteValue(value, (uint)crcSender.HashSize);
            bufferSender.PushTempInBuffer();
            // Console.WriteLine("Valeur du CRC :" + Convert.ToString(value, toBase: 16));
            
            Console.WriteLine("=================================================================");
            */
            // Receiver side
            // ============================================================================================
            BitPacker bufferReceiver = BitPacker.FromArray(bufferSender.GetByteBuffer());
            //Console.WriteLine("Réception data avec CRC :" + bufferReceiver.ToString());

            Crc crcReceiver = new Crc(parameters);

            uint crcValue = bufferReceiver.ReadValue(crcReceiver.HashSize, true, bufferReceiver.BitLength - 32);
            Console.WriteLine("Réception data sans CRC :" + bufferReceiver.ToString());

            parameters.Check = crcValue;

            if (crcReceiver.IsRight(bufferReceiver.GetByteBuffer()))
                Console.WriteLine("Paquet accepté !");
            else
                Console.WriteLine("Paquet refusé !");
        }
        /*
        public static uint BitsRequired4(int pMin, int pMax)
        {
            if (pMin != pMax)
            {
                uint x = (uint)(pMax - pMin);
                uint counter = 0;
                while ((x & 0x80000000) != 0x80000000 && x > 0)
                {
                    x <<= 1;
                    counter++;
                }
                return 32 - counter;
            }
            else
            {
                return 0;
            }
        }

        public static uint BitsRequired2(int pMin, int pMax)
        {
            if (pMin != pMax)
            {
                uint x = (uint)(pMax - pMin);
                uint counter = 0;
                while ((x & 0x80000000) != 0x80000000)
                {
                    x <<= 1;
                    counter++;
                    if (x <= 0)
                        break;
                }
                return 32 - counter;
            }
            else
            {
                return 0;
            }
        }

	    public static uint BitsRequired3(int pMin, int pMax)
        {
            if (pMin != pMax)
            {
                uint x = (uint)(pMax - pMin);
		        uint counter = 0;
		        for (int i = 0; i < 32; i++)
		        {
		            if ((x & 0x80000000) == 0x80000000)
		            {
		                counter = (uint)(32 - i);
		                break;
		            }
                    x <<= 1;
                }
		        return counter;
            }
            else
            {
                return 0;
            }
        }
        */

    }
}
