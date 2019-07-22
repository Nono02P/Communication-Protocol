﻿using CommunicationProtocol.CRC;
using CommunicationProtocol.Frames.Packets;
using CommunicationProtocol.Serialization;
using System;

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

        private void PrepareFrame()
        {
            _shouldClean = false;
            Serializer.BitPacking.Clear();
            Serializer.BitPacking.WriteValue(0, CrcCheck.HashSize);         // Ecriture d'un CRC vide
            Serializer.BitPacking.WriteValue((uint)Sequence, 16);           // Ecriture de l'index de séquence
        }

        public bool InsertPacket(Packet pPacket)
        {
            if (_shouldClean)
                PrepareFrame();

            int id = dFactory.GetID(pPacket);
            Serializer.Serialize(ref id, 0, dFactory.Count() - 1);              // ID de paquet

            pPacket.Serialize(Serializer);                                  // Data

            if (Serializer.BitPacking.ByteLength > MTU)
            {
                throw new Exception("Call the fragmentation code here.");
            }
            return !Serializer.Error;
        }

        public byte[] Send()
        {
            Serializer.BitPacking.PushTempInBuffer();

            byte[] data = Serializer.BitPacking.GetByteBuffer();
            int crcByteLength = CrcCheck.HashSize / 8;

            CrcCheck.ComputeHash(data, crcByteLength, data.Length - crcByteLength);
            dCrcValue = (int)CrcHelper.FromBigEndian(CrcCheck.Hash, CrcCheck.HashSize);

            /*
            for (int i = 0; i < CrcCheck.HashSize / 8; i++)
            {
                data[i] = CrcCheck.Hash[7 - i];
            }
            */
            Serializer.BitPacking.OverrideValue((uint)dCrcValue, CrcCheck.HashSize);            // Ecriture du CRC au début de la frame (zone réservée)
            data = Serializer.BitPacking.GetByteBuffer();
            //
            Sequence++;
            _shouldClean = true;
            return data;
        }
    }
}