using CommunicationProtocol.Factories;
using System;
using System.Collections.Generic;

namespace CommunicationProtocol.Serialization
{
    public class SerializerFactory : Factory
    {
        //private static List<Type> _types;

        protected override void InitialiseListID()
        {
            dTypes = new List<Type>();
            dTypes.Add(typeof(Tank));
            dTypes.Add(typeof(Loot));
        }

        public new T CreateInstance<T>(int pID) where T : IBinarySerializable
        {
            return base.CreateInstance<T>(pID);
            /*
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
            */
        }

        /*
        public static Type GetType(int pID)
        {
            if (_types == null)
                InitialiseListID();
            if (pID >= _types.Count)
                throw new Exception("The ID " + pID + " doesn't exist in the current list.");
            else
                return _types[pID];
        }
        */

        public new int GetID<T>(T pType) where T : IBinarySerializable
        {
            return base.GetID(pType);
            /*
            if (_types == null)
                InitialiseListID();
            return _types.FindIndex(item => item == pType.GetType());
            */
        }
        /*
        public static int Count()
        {
            if (_types == null)
                InitialiseListID();
            return _types.Count;
        }
        */
    }
}
