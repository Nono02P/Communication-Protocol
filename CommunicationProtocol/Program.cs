using CommunicationProtocol.CRC;
using CommunicationProtocol.Frames;
using CommunicationProtocol.Frames.Packets;
using CommunicationProtocol.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;


namespace CommunicationProtocol
{
    class Program
    {
        public static string FileName;
        public static Random Rnd = new Random(1);
        public static int? StopOnSequence = null;

        static void Main(string[] args)
        {
            TestFrames();
            //TestSerializerPacketA();
            //TestSerializerPacketB();
            //TestBitPacker();
            //TestCRC();
            Console.Read();
        }

        #region Frames
        private static void TestFrames()
        {
#if TRACE_LOG
            FileName = "Receiver";
            LogHelper.WriteToFile("Starting", "Program", FileName, true);
            FileName = "Sender";
            LogHelper.WriteToFile("Starting", "Program", FileName, true);
#endif
            FrameSender sender = new FrameSender();
            ulong counter = 0;
            while (true)
            {
                FileName = "Sender";
#if TRACE_LOG
                LogHelper.WriteToFile("Prepare to sending packet.", "Program", FileName, true);
#endif
                List<Packet> sendPackets = new List<Packet>();
                for (int i = 0; i < 1; i++)
                {
                    //Packet p = GetPacketB(); 
                    Packet p = RandomPacket();
                    if (sender.InsertPacket(p))
                        sendPackets.Add(p);
                }

                byte[] data = sender.Send();
                string message = string.Empty;
                for (int i = data.Length - 1; i >= 0; i--)
                {
                    message += data[i] + " ";
                }
                Console.WriteLine(message);

                FileName = "Receiver";
                FrameReceiver receiver = new FrameReceiver();
#if TRACE_LOG
                LogHelper.WriteToFile("Prepare to receive packet.", "Program", FileName, true);
#endif
                List<Packet> packets = receiver.Receive(data);
                Debug.Assert(sendPackets.Count == packets.Count);
                counter++;
            }
        }
        #endregion Frames  

        #region Packets
        private static Packet RandomPacket()
        {
            PacketFactory factory = PacketFactory.GetFactory();
            int id = Rnd.Next(factory.Count());
            Packet p = factory.CreateInstance<Packet>(id);
            p.Random();
            return p;
        }

        private static Packet GetPacketA()
        {
            PacketA p = new PacketA();
            p.Position = new Vector3(50.55f, 80.1f, 18.6666f);
            p.f = 78.654426f;
            p.comment = "Test packet A";
            return p;
        }

        private static Packet GetPacketB()
        {
            PacketB p = new PacketB();
            p.Actors = new List<IActor>();
            Tank t = new Tank() { Name = "Toto", Life = 100, Position = new Vector2(150), ShouldBeSend = true };
            t.Shoot();
            p.Actors.Add(t);
            for (int i = 0; i < 50; i++)
            {
                p.Actors.Add(new Loot() { NbOfAmmo = 0, AmmoType = eAmmoType.Grenada, Position = new Vector2(150), IsActive = true, ShouldBeSend = true });
            }
            return p;
        }

        private static void TestSerializerPacketB()
        {
            Serializer sender = new WriterSerialize();
            List<IActor> senderList = new List<IActor>();
            Tank t = new Tank() { Name = "Toto", Life = 100, Position = new Vector2(150), ShouldBeSend = true };
            t.Shoot();
            senderList.Add(t);
            senderList.Add(new Loot() { NbOfAmmo = 1, AmmoType = eAmmoType.Grenada, Position = new Vector2(150), IsActive = true, ShouldBeSend = true });
            senderList.Add(new Tank() { Name = "Tata", Life = 80, Position = new Vector2(100), ShouldBeSend = true });
            senderList.Add(new Loot() { AmmoType = eAmmoType.Bullet, Position = new Vector2(360), IsActive = true, ShouldBeSend = true });
            senderList.Add(new Tank() { Life = 50, Position = new Vector2(50), ShouldBeSend = true });
            
            PacketB sendedPacket = new PacketB() { Actors = senderList };
            bool sendingAuthorized = sendedPacket.Serialize(sender);
            sender.BitPacking.PushTempInBuffer();

            byte[] dataSending = sender.BitPacking.GetByteBuffer();

            if (sendingAuthorized)
            {
                Serializer receiver = new ReaderSerialize();
                receiver.BitPacking = BitPacker.FromArray(dataSending);
                List<IActor> receiverList = new List<IActor>();
                PacketB receivedPacket = new PacketB() { Actors = receiverList };
                bool isValid = receivedPacket.Serialize(receiver);
            }
        }

        private static void TestSerializerPacketA()
        {
            Serializer sender = new WriterSerialize();
            PacketA sendedPacket = new PacketA() { Position = new Vector3(-29.158f, 50.735f, 150.2875f), f = 100.191f, comment = "Je suis CON !!" };
            bool sendingAuthorized = sendedPacket.Serialize(sender);
            sendedPacket = new PacketA() { Position = new Vector3(50, 100, 40), f = 45.02f, comment = "Je suis un test." };
            sendingAuthorized |= sendedPacket.Serialize(sender);
            sender.BitPacking.PushTempInBuffer();
            byte[] data = sender.BitPacking.GetByteBuffer();
            if (sendingAuthorized)
            {
                Serializer receiver = new ReaderSerialize();
                receiver.BitPacking = BitPacker.FromArray(data);
                PacketA receivedPacket = new PacketA();
                PacketA receivedPacket2 = new PacketA();
                bool isValid = receivedPacket.Serialize(receiver);
                isValid = receivedPacket2.Serialize(receiver);
            }
        }
        #endregion Packets  

        #region BitPacker
        private static void TestBitPacker()
        {
            BitPacker bitPacker = new BitPacker();
            bitPacker.WriteValue(80, 32);
            bitPacker.WriteValue(0x800800, 32);

            Console.WriteLine(bitPacker.ToString());
            bitPacker.OverrideValue(4160749567, 32, 16);
            Console.WriteLine(bitPacker.ToString());
            bitPacker.PushTempInBuffer();
            Console.WriteLine(bitPacker.ToString());
        }
        #endregion BitPacker  

        #region CRC
        private static void TestCRC()
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
        #endregion CRC  
    }
}