using CommunicationProtocol.CRC;
using CommunicationProtocol.Frames.Packets;
using CommunicationProtocol.Serialization;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace CommunicationProtocol
{
    class Program
    {
        static void Main(string[] args)
        {
            BitPacker bitPacker = new BitPacker();
            bitPacker.WriteValue(80, 32);
            bitPacker.WriteValue(0x800800, 32);

            //bitPacker.PushTempInBuffer();

            Console.WriteLine(bitPacker.ToString());
            bitPacker.OverrideValue(4160749567, 32, 16);
            Console.WriteLine(bitPacker.ToString());
            //TestSerializerPacketB();
            Console.Read(); 
        }

        static void TestSerializerPacketB()
        {
            Serializer sender = new WriterSerialize();
            List<IActor> senderList = new List<IActor>();
            senderList.Add(new Tank() { Name = "Toto", Life = 100, Position = new Vector2(150), ShouldBeSend = true });
            senderList.Add(new Loot() { NbOfAmmo = 1, AmmoType = Loot.eAmmoType.Grenada, Position = new Vector2(150), IsActive = true, ShouldBeSend = true });
            senderList.Add(new Tank() { Name = "Tata", Life = 80, Position = new Vector2(100), ShouldBeSend = true });
            senderList.Add(new Loot() { AmmoType = Loot.eAmmoType.Bullet, Position = new Vector2(360), IsActive = true, ShouldBeSend = true });
            senderList.Add(new Tank() { Life = 50, Position = new Vector2(50), ShouldBeSend = true });
            
            PacketB sendedPacket = new PacketB() { Actors = senderList };
            bool sendingAuthorized = sendedPacket.Serialize(sender);
            sender.dBitPacking.PushTempInBuffer();

            if (sendingAuthorized)
            {
                Serializer receiver = new ReaderSerialize();
                receiver.dBitPacking = BitPacker.FromArray(sender.dBitPacking.GetByteBuffer());
                List<IActor> receiverList = new List<IActor>();
                PacketB receivedPacket = new PacketB() { Actors = receiverList };
                bool isValid = receivedPacket.Serialize(receiver);
            }
        }

        static void TestSerializerPacketA()
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
    }
}
