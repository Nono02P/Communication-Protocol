﻿using CommunicationProtocol.Serialization;
using System;
using System.Collections.Generic;

namespace CommunicationProtocol.Frames.Packets
{
    public class PacketB : Packet
    {
        public List<IActor> Actors;

        public override void Random()
        {
            Actors = new List<IActor>();
            SerializerFactory factory = SerializerFactory.GetFactory();
            Random rnd = Program.Rnd;
            for (int i = 0; i < rnd.Next(20); i++)
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

            pSerializer.Serialize(Actors, 255, true);
            
            return base.Serialize(pSerializer);
        }
    }
}
