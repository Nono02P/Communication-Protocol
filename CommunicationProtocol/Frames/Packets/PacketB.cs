using CommunicationProtocol.Factories;
using CommunicationProtocol.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CommunicationProtocol.Frames.Packets
{
    public class PacketB : Packet
    {
        private const int NB_MAX_OF_ACTORS = 1000;

        public List<IActor> Actors;

        protected override bool dUseSerializationCheck => true;

        public override bool Equals(IPacket other)
        {
            if (other is PacketB)
            {
                PacketB o = (PacketB)other;
                if (Actors?.Count == o.Actors?.Count)
                {
                    for (int i = 0; i < Actors.Count; i++)
                    {
                        IActor itemA = Actors[i];
                        IActor itemB = o.Actors[i];
                        if (!itemA.Equals(itemB))
                            return false;
                    }
                    return true;
                }
                return false;
            }
            return false;
        }

        public override void Random()
        {
            Actors = new List<IActor>();
            SerializerFactory factory = SerializerFactory.GetFactory();
            Random rnd = Program.Rnd;
            int nb = rnd.Next(NB_MAX_OF_ACTORS);
            for (int i = 0; i < nb; i++)
            {
                int id = rnd.Next(factory.Count());
                IBinarySerializable obj = factory.CreateInstance<IBinarySerializable>(id);
                obj.ShouldBeSend = true;
                obj.Random();
                Actors.Add((IActor)obj);
            }
        }

        protected override void SerializeData(Serializer pSerializer)
        {            
            if (Actors == null)
                Actors = new List<IActor>();

#if TRACE
            Trace.WriteLine("Serialize PacketB : ");
            Trace.Indent();
#endif
            pSerializer.Serialize(Actors, NB_MAX_OF_ACTORS, true);
#if TRACE
            Trace.Unindent();
            Trace.WriteLine("End of PacketB : ");
#endif
        }
    }
}
