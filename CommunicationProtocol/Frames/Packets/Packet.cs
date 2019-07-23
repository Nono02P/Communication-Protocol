using CommunicationProtocol.Serialization;

namespace CommunicationProtocol.Frames.Packets
{
    public abstract class Packet
    {
        protected const int SerializationCheck = -1431655766;

        public abstract void Random();
        public virtual bool Serialize(Serializer pSerializer)
        {
            int checkValue = SerializationCheck;
            LogHelper.WriteToFile("End Serialization Check :", this, Program.FileName);
            pSerializer.Serialize(ref checkValue, int.MinValue, int.MaxValue);
            return !pSerializer.Error && checkValue == SerializationCheck;
        }
    }
}
