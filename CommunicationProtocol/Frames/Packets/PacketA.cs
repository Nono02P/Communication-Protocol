using CommunicationProtocol.ExtensionMethods;
using CommunicationProtocol.Serialization;
using System.Numerics;

namespace CommunicationProtocol.Frames.Packets
{
    public class PacketA : Packet
    {
        public Vector3 Position;
        public float f;
        public string comment;

        public override void Random()
        {
            Position.Randomize(new Vector3(-30, 0, 0), new Vector3(100, 700, 190));
            f = Program.Rnd.Next(100);
        }

        public override bool Serialize(Serializer pSerializer)
        {
#if TRACE_LOG
            LogHelper.WriteToFile("Serialize PacketA : ", this, Program.FileName);
            LogHelper.WriteToFile("     Position : ", this, Program.FileName);
#endif
            pSerializer.Serialize(ref Position, new Vector3(-30, 0, 0), new Vector3(100, 700, 190), 0.01f);
#if TRACE_LOG
            LogHelper.WriteToFile("     f : ", this, Program.FileName);
#endif
            pSerializer.Serialize(ref f, 0, 100f, 0.01f);
#if TRACE_LOG
            LogHelper.WriteToFile("     Comment : ", this, Program.FileName);
#endif
            pSerializer.Serialize(ref comment, 16);

            bool b = base.Serialize(pSerializer);
#if TRACE_LOG
            LogHelper.WriteToFile("End of PacketA : ", this, Program.FileName);
#endif
            return b;
        }
    }
}
