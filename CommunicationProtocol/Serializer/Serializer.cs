﻿using System;
using System.Numerics;

namespace CommunicationProtocol
{
    public abstract class Serializer
    {
        public BitPacker dBitPacking;

        public Serializer(int pByteBufferSize)
        {
            dBitPacking = new BitPacker(pByteBufferSize);
        }

        public abstract void ManageData(byte[] pData);
        public abstract bool Serialize(ref int pBitCounter, ref bool pResult, ref int pValue, int pMin, int pMax);
        public abstract bool Serialize(ref int pBitCounter, ref bool pResult, ref string pValue, int pLengthMax);
        public abstract bool Serialize<T>(ref int pBitCounter, ref bool pResult, T pType) where T : IPacket;

        public bool Serialize(ref int pBitCounter, ref bool pResult, ref Vector2 pValue, Vector2 pMin, Vector2 pMax, float pResolution = 1)
        {
            Serialize(ref pBitCounter, ref pResult, ref pValue.X, pMin.X, pMax.X, pResolution);
            Serialize(ref pBitCounter, ref pResult, ref pValue.Y, pMin.Y, pMax.Y, pResolution);
            return pResult;
        }

        public bool Serialize(ref int pBitCounter, ref bool pResult, ref Vector3 pValue, Vector3 pMin, Vector3 pMax, float pResolution = 1)
        {
            Serialize(ref pBitCounter, ref pResult, ref pValue.X, pMin.X, pMax.X, pResolution);
            Serialize(ref pBitCounter, ref pResult, ref pValue.Y, pMin.Y, pMax.Y, pResolution);
            Serialize(ref pBitCounter, ref pResult, ref pValue.Z, pMin.Z, pMax.Z, pResolution);
            return pResult;
        }

        public bool Serialize(ref int pBitCounter, ref bool pResult, ref Vector4 pValue, Vector4 pMin, Vector4 pMax, float pResolution = 1)
        {
            Serialize(ref pBitCounter, ref pResult, ref pValue.X, pMin.X, pMax.X, pResolution);
            Serialize(ref pBitCounter, ref pResult, ref pValue.Y, pMin.Y, pMax.Y, pResolution);
            Serialize(ref pBitCounter, ref pResult, ref pValue.Z, pMin.Z, pMax.Z, pResolution);
            Serialize(ref pBitCounter, ref pResult, ref pValue.W, pMin.W, pMax.W, pResolution);
            return pResult;
        }

        public bool Serialize(ref int pBitCounter, ref bool pResult, ref Quaternion pValue, Quaternion pMin, Quaternion pMax, float pResolution = 1)
        {
            Serialize(ref pBitCounter, ref pResult, ref pValue.X, pMin.X, pMax.X, pResolution);
            Serialize(ref pBitCounter, ref pResult, ref pValue.Y, pMin.Y, pMax.Y, pResolution);
            Serialize(ref pBitCounter, ref pResult, ref pValue.Z, pMin.Z, pMax.Z, pResolution);
            Serialize(ref pBitCounter, ref pResult, ref pValue.W, pMin.W, pMax.W, pResolution);
            return pResult;
        }

        public virtual bool Serialize(ref int pBitCounter, ref bool pResult, ref float pValue, float pMin, float pMax, float pResolution = 1)
        {
            if (pResult)
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
                    pResult = false;
                    throw new Exception("The resolution should be a number like 1, 0.1, 0.01, etc... The value is : " + pResolution);
                }
            }
            return pResult;
        }

        public abstract bool EndOfPacket(ref bool pResult, ref int pCheckValue, int pNbOfBits);

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
                x = (((x >> 2) & 0x33333333) + (x & 0x33333333));
                x = (((x >> 4) + x) & 0x0f0f0f0f);
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