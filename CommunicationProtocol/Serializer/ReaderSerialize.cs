﻿using System;
using System.Text;

namespace CommunicationProtocol
{
    public class ReaderSerialize : Serializer
    {
        public ReaderSerialize(int pByteBufferSize = 1200) : base(pByteBufferSize) { }


        public override void ManageData(byte[] pData)
        {
            dBitPacking.Clear();
            dBitPacking = BitPacker.FromArray(pData);
        }

        #region Serialisation
        public override bool Serialize(ref int pBitCounter, ref bool pResult, ref float pValue, float pMin, float pMax, float pResolution = 1)
        {
            base.Serialize(ref pBitCounter, ref pResult, ref pValue, pMin, pMax, pResolution);
            if (pResult)
            {
                int min = (int)((decimal)pMin / (decimal)pResolution);
                int max = (int)((decimal)pMax / (decimal)pResolution);
                int value = 0;
                Serialize(ref pBitCounter, ref pResult, ref value, min, max);
                pValue = (float)(value * (decimal)pResolution);
            }
            return pResult;
        }

        public override bool Serialize(ref int pBitCounter, ref bool pResult, ref int pValue, int pMin, int pMax)
        {
            if (pResult)
            {
                int value = (int)dBitPacking.ReadValue(BitsRequired(pMin, pMax)) + pMin;
                if (value >= pMin && value <= pMax)
                    pValue = value;
                else
                    pResult = false;
            }
            return pResult;
        }

        public override bool Serialize(ref int pBitCounter, ref bool pResult, ref string pValue, int pLengthMax)
        {
            if (pResult)
            {
                int length = (int)dBitPacking.ReadValue(BitsRequired(0, pLengthMax));
                
                if (length <= pLengthMax)
                {
                    byte[] data = new byte[length];
                    for (int i = 0; i < data.Length; i++)
                    {
                        data[i] = (byte)dBitPacking.ReadValue(8);
                    }
                    pValue = Encoding.UTF8.GetString(data);
                }
                else
                    pResult = false;
            }
            return pResult;
        }
        #endregion Serialisation

        #region Fin de paquet
        public override bool EndOfPacket(ref bool pResult, ref int pCheckValue, int pNbOfBits)
        {
            throw new NotImplementedException();
        }
        #endregion Fin de paquet

        public override bool Serialize<T>(ref int pBitCounter, ref bool pResult, T pType)
        {
            throw new NotImplementedException();
        }
    }
}