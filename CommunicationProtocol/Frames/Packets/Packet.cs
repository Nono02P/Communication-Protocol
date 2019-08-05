using CommunicationProtocol.Serialization;
using System;
using System.Diagnostics;

namespace CommunicationProtocol.Frames.Packets
{
    public abstract class Packet : IPacket, IEquatable<Packet>
    {
        protected const int SerializationCheck = 1431655766;

        public PacketHeader Header { get; set; }
        
        public abstract bool Equals(Packet other);
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
            int checkValue = SerializationCheck;
#if TRACE_LOG
            LogHelper.WriteToFile("End Serialization Check :", this, Program.FileName);
#endif
            pSerializer.Serialize(ref checkValue, int.MinValue, int.MaxValue);
            Debug.Assert(checkValue == SerializationCheck);
            Header.EndSerialize(pSerializer);
            return !pSerializer.Error && checkValue == SerializationCheck;
        }
    }
}