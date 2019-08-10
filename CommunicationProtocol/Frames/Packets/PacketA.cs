using CommunicationProtocol.ExtensionMethods;
using CommunicationProtocol.Serialization;
using System.Numerics;

namespace CommunicationProtocol.Frames.Packets
{
    public class PacketA : Packet
    {
        private const float _POSITION_RESOLUTION = 0.01f;
        private const float _F_RESOLUTION = 0.01f;

        protected override bool dUseSerializationCheck => true;

        public Vector3 Position;
        public float f;
        public string comment;
        
        public override bool Equals(IPacket other)
        {
            if (other is PacketA)
            {
                PacketA o = (PacketA)other;
                bool checkString = false;
                if (string.IsNullOrEmpty(comment) && string.IsNullOrEmpty(o.comment))
                    checkString = true;
                else
                    checkString = comment == o.comment;
                return Position.ApplyResolution(_POSITION_RESOLUTION) == o.Position.ApplyResolution(_POSITION_RESOLUTION) &
                    f.ApplyResolution(_F_RESOLUTION) == o.f.ApplyResolution(_F_RESOLUTION) &
                    checkString;
            }
            return false;
        }
        
        public override void Random()
        {
            Position.Randomize(new Vector3(-30, 0, 0), new Vector3(100, 700, 190));
            f = Program.Rnd.Next(100);
        }

        protected override void SerializeData(Serializer pSerializer)
        {
#if TRACE_LOG
            LogHelper.WriteToFile("Serialize PacketA : ", this, Program.FileName);
            LogHelper.WriteToFile("     Position : ", this, Program.FileName);
#endif
            pSerializer.Serialize(ref Position, new Vector3(-30, 0, 0), new Vector3(100, 700, 190), _POSITION_RESOLUTION);
#if TRACE_LOG
            LogHelper.WriteToFile("     f : ", this, Program.FileName);
#endif
            pSerializer.Serialize(ref f, 0, 100f, _F_RESOLUTION);
#if TRACE_LOG
            LogHelper.WriteToFile("     Comment : ", this, Program.FileName);
#endif
            pSerializer.Serialize(ref comment, 16);
#if TRACE_LOG
            LogHelper.WriteToFile("End of PacketA : ", this, Program.FileName);
#endif
        }
    }
}
