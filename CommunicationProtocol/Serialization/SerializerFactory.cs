using CommunicationProtocol.Factories;
using System;
using System.Collections.Generic;

namespace CommunicationProtocol.Serialization
{
    public class SerializerFactory : Factory
    {
        private static SerializerFactory _instance;

        public static SerializerFactory GetFactory()
        {
            if (_instance == null)
                _instance = new SerializerFactory();
            return _instance;
        }

        protected override void InitialiseListID()
        {
            dTypes = new List<Type>();
            dTypes.Add(typeof(Tank));
            dTypes.Add(typeof(Bullet));
            dTypes.Add(typeof(Loot));
        }

        public new T CreateInstance<T>(int pID) where T : IBinarySerializable
        {
            return base.CreateInstance<T>(pID);
        }

        public new int GetID<T>(T pType) where T : IBinarySerializable
        {
            return base.GetID(pType);
        }
    }
}
