using System;
using System.Collections.Generic;
using System.Numerics;

namespace CommunicationProtocol.Serialization
{
    public abstract class Serializer
    {
        public BitPacker dBitPacking;
        public bool Error { get; protected set; }
        
        public Serializer(int pByteBufferSize)
        {
            dBitPacking = new BitPacker(pByteBufferSize);
        }

        public abstract void ManageData(byte[] pData);
        public abstract bool Serialize(ref bool pValue);
        public abstract bool Serialize(ref int pValue, int pMin, int pMax);
        public abstract bool Serialize(ref string pValue, int pLengthMax);
        public abstract bool Serialize<T>(List<T> pObjects, int pNbMaxObjects = 255, bool pAddMissingElements = false) where T : IBinarySerializable;

        /*
        public bool Serialize<T>(T pValue, Enum pEnum) where T : IConvertible
        {
            switch (pEnum.GetTypeCode())
            {
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                    Array EnumValues = Enum.GetValues(pEnum.GetType());
                    int min = int.MaxValue;
                    int max = 0;
                    for (int i = 0; i < EnumValues.Length; i++)
                    {
                        int value = (int)EnumValues.GetValue(i);
                        if (value < min)
                            min = value;
                        if (value > max)
                            max = value;
                    }
                    int valueOut = (int)pValue.ToInt32(null);
                    Serialize(ref valueOut, min, max);
                    break;
                case TypeCode.String:
                    break;
                default:
                    break;
            }
            return Error;
        }
        */

        // TODO : Gérer une liste de types primitifs (int, float, string, etc).
        //public abstract bool Serialize<T>(List<T> pType);

        public bool Serialize(ref Vector2 pValue, Vector2 pMin, Vector2 pMax, float pResolution = 1)
        {
            Serialize(ref pValue.X, pMin.X, pMax.X, pResolution);
            Serialize(ref pValue.Y, pMin.Y, pMax.Y, pResolution);
            return Error;
        }

        public bool Serialize(ref Vector3 pValue, Vector3 pMin, Vector3 pMax, float pResolution = 1)
        {
            Serialize(ref pValue.X, pMin.X, pMax.X, pResolution);
            Serialize(ref pValue.Y, pMin.Y, pMax.Y, pResolution);
            Serialize(ref pValue.Z, pMin.Z, pMax.Z, pResolution);
            return Error;
        }

        public bool Serialize(ref Vector4 pValue, Vector4 pMin, Vector4 pMax, float pResolution = 1)
        {
            Serialize(ref pValue.X, pMin.X, pMax.X, pResolution);
            Serialize(ref pValue.Y, pMin.Y, pMax.Y, pResolution);
            Serialize(ref pValue.Z, pMin.Z, pMax.Z, pResolution);
            Serialize(ref pValue.W, pMin.W, pMax.W, pResolution);
            return Error;
        }

        public bool Serialize(ref Quaternion pValue, Quaternion pMin, Quaternion pMax, float pResolution = 1)
        {
            Serialize(ref pValue.X, pMin.X, pMax.X, pResolution);
            Serialize(ref pValue.Y, pMin.Y, pMax.Y, pResolution);
            Serialize(ref pValue.Z, pMin.Z, pMax.Z, pResolution);
            Serialize(ref pValue.W, pMin.W, pMax.W, pResolution);
            return Error;
        }

        public virtual bool Serialize(ref float pValue, float pMin, float pMax, float pResolution = 1)
        {
            if (!Error)
            {
                bool resolutionError = false;
                if (pResolution > 1)
                    resolutionError = true;
                else
                {
                    decimal workResolution = (decimal)pResolution;
                    while (workResolution < 1 && !resolutionError)
                    {
                        workResolution *= 10;
                        if (workResolution > 1)
                            resolutionError = true;
                    }
                }
                if (resolutionError)
                {
                    Error = true;
                    throw new Exception("The resolution should be a number like 1, 0.1, 0.01, etc... The value is : " + pResolution);
                }
            }
            return Error;
        }

        public abstract bool EndOfPacket(ref bool Error, ref int pCheckValue, int pNbOfBits);

        protected int BitsRequired(int pMin, int pMax)
        {
            if (pMin != pMax)
            {
                uint x = (uint)(pMax - pMin);
                x = x | (x >> 1);
                x = x | (x >> 2);
                x = x | (x >> 4);
                x = x | (x >> 8);
                x = x | (x >> 16);
                x = x >> 1;
                x = x - ((x >> 1) & 0x55555555);
                x = ((x >> 2) & 0x33333333) + (x & 0x33333333);
                x = ((x >> 4) + x) & 0x0f0f0f0f;
                x = x + (x >> 8);
                x = x + (x >> 16);
                return (int)(x & 0x0000003f) + 1;
            }
            else
            {
                return 0;
            }
        }
    }
}
