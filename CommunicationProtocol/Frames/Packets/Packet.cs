﻿using CommunicationProtocol.Serialization;
using System.Diagnostics;

namespace CommunicationProtocol.Frames.Packets
{
    public abstract class Packet : IPacket
    {
        private const int SERIALIZATION_CHECK_VALUE = 1431655766;
        private const int SERIALIZATION_CHECK_SIZE = 32;

        protected abstract bool dUseSerializationCheck { get; }

        public PacketHeader Header { get; set; }

        public abstract bool Equals(IPacket other);
        public abstract void Random();

        public bool Serialize(Serializer pSerializer)
        {
            BeginSerialize(pSerializer);
            SerializeData(pSerializer);
            return EndSerialize(pSerializer);
        }

        private void BeginSerialize(Serializer pSerializer)
        {
            // PacketFactory factory = PacketFactory.GetFactory();
            // Header.BeginSerialize(pSerializer);
        }

        protected abstract void SerializeData(Serializer pSerializer);

        private bool EndSerialize(Serializer pSerializer)
        {
            bool result = !dUseSerializationCheck;
            if (dUseSerializationCheck)
            {
                int checkValue = SERIALIZATION_CHECK_VALUE;
#if TRACE_LOG
            LogHelper.WriteToFile("End Serialization Check :", this, Program.FileName);
#endif
                pSerializer.Serialize(ref checkValue, SERIALIZATION_CHECK_SIZE);

                Debug.Assert(checkValue == SERIALIZATION_CHECK_VALUE);
                result = checkValue == SERIALIZATION_CHECK_VALUE;
            }
            if (pSerializer is WriterSerialize)
                pSerializer.BitPacking.PushTempInBuffer();

            return !pSerializer.Error && result;
        }
    }
}