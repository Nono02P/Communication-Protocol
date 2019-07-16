using System;
using System.Collections.Generic;
using System.Reflection;

namespace CommunicationProtocol.Factories
{
    public abstract class Factory
    {
        protected List<Type> dTypes;

        protected abstract void InitialiseListID();

        protected T CreateInstance<T>(int pID)
        {
            if (dTypes == null)
                InitialiseListID();
            if (pID >= dTypes.Count)
                throw new Exception("The ID " + pID + " doesn't exist in the current list.");
            else
            {
                Type type = dTypes[pID];
                object result = new object();
                return (T)type.InvokeMember(string.Empty, BindingFlags.CreateInstance, null, result, new object[] { });
            }
        }

        public Type GetType(int pID)
        {
            if (dTypes == null)
                InitialiseListID();
            if (pID >= dTypes.Count)
                throw new Exception("The ID " + pID + " doesn't exist in the current list.");
            else
                return dTypes[pID];
        }

        protected int GetID<T>(T pType)
        {
            if (dTypes == null)
                InitialiseListID();
            return dTypes.FindIndex(item => item == pType.GetType());
        }

        public int Count()
        {
            if (dTypes == null)
                InitialiseListID();
            return dTypes.Count;
        }
    }
}