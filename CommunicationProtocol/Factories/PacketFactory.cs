using CommunicationProtocol.Factories;
using CommunicationProtocol.Frames.Packets;
using System;
using System.Collections.Generic;

namespace CommunicationProtocol.Factories
{
    public class PacketFactory : Factory
    {
        private static PacketFactory _instance;

        public static PacketFactory GetFactory()
        {
            if (_instance == null)
                _instance = new PacketFactory();
            return _instance;
        }

        protected override void InitialiseListID()
        {
            dTypes = new List<Type>();
            dTypes.Add(typeof(PacketA));
            dTypes.Add(typeof(PacketB));
        }

        public new T CreateInstance<T>(int pID) where T : Packet
        {
            return base.CreateInstance<T>(pID);
        }

        public new int GetID<T>(T pType) where T : Packet
        {
            return base.GetID(pType);
        }
    }
}