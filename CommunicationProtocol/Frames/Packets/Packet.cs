using CommunicationProtocol.Serialization;
using System;
using System.Diagnostics;

namespace CommunicationProtocol.Frames.Packets
{
    public abstract class Packet : IEquatable<Packet>
    {
        protected const int SerializationCheck = 1431655766;

        public abstract bool Equals(Packet other);
        public abstract void Random();
        public virtual bool Serialize(Serializer pSerializer)
        {
            int checkValue = SerializationCheck;
#if TRACE_LOG
            LogHelper.WriteToFile("End Serialization Check :", this, Program.FileName);
#endif
            pSerializer.Serialize(ref checkValue, int.MinValue, int.MaxValue);
            Debug.Assert(checkValue == SerializationCheck);
            return !pSerializer.Error && checkValue == SerializationCheck;
        }
    }
}
