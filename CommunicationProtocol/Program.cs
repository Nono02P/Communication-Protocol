#undef DEBUG
//#undef TRACE
using CommunicationProtocol.CRC;
using CommunicationProtocol.Factories;
using CommunicationProtocol.Frames;
using CommunicationProtocol.Frames.Packets;
using CommunicationProtocol.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;

namespace CommunicationProtocol
{
    class Program
    {
        private enum eListener : byte
        {
            Sender, 
            Receiver
        }

        private static bool _continue = true;
        private static TextWriterTraceListener _sender;
        private static TextWriterTraceListener _receiver;
        
        public static Random Rnd = new Random(1);
        public static int? StopOnSequence = null;
        
        static void Main(string[] args)
        {
#if DEBUG
            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
#endif
#if TRACE
            Directory.CreateDirectory("Log/");
            /*
            _sender = new TextWriterTraceListener(File.CreateText("Log/Sender.log"));
            _receiver = new TextWriterTraceListener(File.CreateText("Log/Receiver.Log"));
            */
            _sender = new TextWriterTraceListener(File.CreateText("Log/Sender.log"));
            _receiver = new TextWriterTraceListener(File.CreateText("Log/Receiver.Log"));

#endif

            TestFrames();
            _sender.Flush();
            _sender.Close();
            _sender.Dispose();

            _receiver.Flush();
            _receiver.Close();
            _receiver.Dispose();

            //TestSerializerPacketA();
            //TestSerializerPacketB();
            //TestBitPacker();
            //TestCRC();
            //Devlog();
            Console.Read();

        }

        private static void SwapListeners(eListener pDesiredListener)
        {
#if TRACE
            switch (pDesiredListener)
            {
                case eListener.Sender:
                    //Trace.Flush();
                    Trace.Listeners.Remove(_receiver);
                    Trace.Listeners.Add(_sender);
                    break;
                case eListener.Receiver:
                    //Trace.Flush();
                    Trace.Listeners.Remove(_sender);
                    Trace.Listeners.Add(_receiver);
                    break;
                default:
                    break;
            }
#endif
        }

        private static void Devlog()
        {
            BitPacker bitPacker = new BitPacker();
            bitPacker.WriteValue(80, 7);
            bitPacker.WriteValue(3850, 12);
            bitPacker.WriteValue(750, 10);

            bitPacker.PushTempInBuffer();
            Console.WriteLine(bitPacker.ToString());

            byte[] data = bitPacker.GetByteBuffer();

            Console.WriteLine("Reader Creation : ");
            BitPacker reader = BitPacker.FromArray(data);
            Console.WriteLine(reader.ReadValue(7));
            Console.WriteLine(reader.ReadValue(12));
            Console.WriteLine(reader.ReadValue(10));
        }

        #region Frames
        private static void TestFrames()
        {
            FrameSender sender = new FrameSender();
            ulong counter = 0;
            while (_continue)
            {
#if TRACE
                SwapListeners(eListener.Sender);
#endif
                List<Packet> sendPackets = new List<Packet>();
                while (sendPackets.Count == 0) //for (int i = 0; i < 1; i++)
                {
                    //Packet p = GetPacketB(); 
                    Packet p = RandomPacket();
                    if (sender.ProcessPacket(p))
                        sendPackets.Add(p);
                }

                byte[] data = sender.Send();

                FrameReceiver receiver = new FrameReceiver();
#if TRACE
                SwapListeners(eListener.Receiver);
#endif
                List<IPacket> readPackets = receiver.Receive(data);
                Debug.Assert(sendPackets.Count == readPackets.Count);
                for (int i = 0; i < sendPackets.Count; i++)
                {
                    IPacket itemA = sendPackets[i];
                    IPacket itemB = readPackets[i];
                    if (!(itemB is FragmentedPacket))
                        Debug.Assert(itemA.Equals(itemB));
                }
                Console.WriteLine(counter);
                counter++;
            }
        }
        #endregion Frames  

        #region Packets
        private static Packet RandomPacket()
        {
            PacketFactory factory = PacketFactory.GetFactory();
            int id = 1 + Rnd.Next(factory.Count() - 1);         // Exclusion of the fragmented packet (ID 0)
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

            bufferSender.AlignToNextWriteByte();
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
                Console.WriteLine("Refused packet !");
        }
        #endregion CRC  
    }
}