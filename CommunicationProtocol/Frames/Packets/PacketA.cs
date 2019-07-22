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
            pSerializer.Serialize(ref Position, new Vector3(-30, 0, 0), new Vector3(100, 700, 190), 0.01f);
            pSerializer.Serialize(ref f, 0, 100.2f, 0.01f);
            pSerializer.Serialize(ref comment, 16);

            return base.Serialize(pSerializer);
        }
    }
}
