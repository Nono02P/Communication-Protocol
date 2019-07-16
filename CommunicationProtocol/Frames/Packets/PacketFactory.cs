using CommunicationProtocol.Factories;
using System;
using System.Collections.Generic;

namespace CommunicationProtocol.Frames.Packets
{
    public class PacketFactory : Factory
    {
        protected override void InitialiseListID()
        {
            dTypes = new List<Type>();
            dTypes.Add(typeof(FragmentedPacket));
            dTypes.Add(typeof(PacketA));
            dTypes.Add(typeof(PacketB));
        }

        public new T CreateInstance<T>(int pID) where T : IPacket
        {
            return base.CreateInstance<T>(pID);
        }

        public new int GetID<T>(T pType) where T : IPacket
        {
            return base.GetID(pType);
        }
    }
}