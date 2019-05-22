
using System;

namespace CommunicationProtocol
{
    public abstract class Serializer
    {
        public BitPacking dBitPacking;

        public Serializer(int pByteBufferSize)
        {
            dBitPacking = new BitPacking(pByteBufferSize);
        }

        public abstract void ManageData(byte[] pData);
        public abstract bool Serialize(ref bool pResult, ref int pValue, int pMin, int pMax);
        public abstract bool Serialize(ref bool pResult, ref string pValue, int pLengthMax);

        public virtual bool Serialize(ref bool pResult, ref float pValue, float pMin, float pMax, float pResolution = 1)
        {
            if (pResult)
            {
                bool resolutionError = false;
                if (pResolution > 1)
                    resolutionError = true;
                else
                {
                    float workResolution = pResolution;
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

        protected uint BitsRequired(int pMin, int pMax)
        {
            if (pMin != pMax)
            {
                uint x = (uint)(pMax - pMin);
                uint a = x | (x >> 1);
                uint b = a | (a >> 2);
                uint c = b | (b >> 4);
                uint d = c | (c >> 8);
                uint e = d | (d >> 16);
                uint f = e >> 1;
                a = f - ((f >> 1) & 0x55555555);
                b = (((a >> 2) & 0x33333333) + (a & 0x33333333));
                c = (((b >> 4) + b) & 0x0f0f0f0f);
                d = c + (c >> 8);
                e = d + (d >> 16);
                return (e & 0x0000003f) + 1;
            }
            else
            {
                return 0;
            }
        }
    }
}
