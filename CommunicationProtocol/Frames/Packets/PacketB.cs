using CommunicationProtocol.Factories;
using CommunicationProtocol.Serialization;
using System;
using System.Collections.Generic;

namespace CommunicationProtocol.Frames.Packets
{
    public class PacketB : Packet
    {
        public List<IActor> Actors;

        public override bool Equals(Packet other)
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
            int nb = rnd.Next(25);
            for (int i = 0; i < nb; i++)
            {
                int id = rnd.Next(factory.Count());
                IBinarySerializable obj = factory.CreateInstance<IBinarySerializable>(id);
                obj.ShouldBeSend = true;
                obj.Random();
                Actors.Add((IActor)obj);
            }
        }

        public override bool Serialize(Serializer pSerializer)
        {            
            if (Actors == null)
                Actors = new List<IActor>();

#if TRACE_LOG
            LogHelper.WriteToFile("Serialize PacketB : ", this, Program.FileName);
#endif
            pSerializer.Serialize(Actors, 255, true);

            bool b = base.Serialize(pSerializer);
#if TRACE_LOG
            LogHelper.WriteToFile("End of PacketB : ", this, Program.FileName);
#endif
            return b;
        }
    }
}
