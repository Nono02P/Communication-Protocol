using System;
using System.Collections.Generic;
using System.Reflection;

namespace CommunicationProtocol.Serialization
{
    public static class SerializerFactory
    {
        private static List<Type> _types;
        
        private static void InitialiseListID()
        {
            _types = new List<Type>();
            _types.Add(typeof(Tank));
            _types.Add(typeof(Loot));
        }

        public static T CreateInstance<T>(int pID) where T : IBinarySerializable
        {
            if (_types == null)
                InitialiseListID();
            if (pID >= _types.Count)
                throw new Exception("The ID " + pID + " doesn't exist in the current list.");
            else
            {
                Type type = _types[pID];
                object result = new object();
                return (T)type.InvokeMember(string.Empty, BindingFlags.CreateInstance, null, result, new object[] { });
            }
        }

        public static Type GetType(int pID)
        {
            if (_types == null)
                InitialiseListID();
            if (pID >= _types.Count)
                throw new Exception("The ID " + pID + " doesn't exist in the current list.");
            else
                return _types[pID];
        }

        public static int GetID<T>(T pType) where T : IBinarySerializable
        {
            if (_types == null)
                InitialiseListID();
            return _types.FindIndex(item => item == pType.GetType());
        }

        public static int Count()
        {
            if (_types == null)
                InitialiseListID();
            return _types.Count;
        }
    }
}
