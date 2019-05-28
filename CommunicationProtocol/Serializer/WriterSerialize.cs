using System;
using System.Text;

namespace CommunicationProtocol
{
    public class WriterSerialize : Serializer
    {
        public WriterSerialize(int pByteBufferSize = 1200) : base(pByteBufferSize) { }

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
                int value = (int)((decimal)pValue / (decimal)pResolution);
                Serialize(ref pBitCounter, ref pResult, ref value, min, max);
            }
            return pResult;
        }

        public override bool Serialize(ref int pBitCounter, ref bool pResult, ref int pValue, int pMin, int pMax)
        {
            if (pValue < pMin || pValue > pMax)
                pResult = false;

            if (pResult)
            {
                int mappedValue = pValue - pMin;
                int bitCounter = BitsRequired(pMin, pMax);
                dBitPacking.WriteValue((uint)mappedValue, bitCounter);
                pBitCounter += bitCounter;
            }
            return pResult;
        }

        public override bool Serialize(ref int pBitCounter, ref bool pResult, ref string pValue, int pLengthMax)
        {
            if (pResult)
            {
                byte[] data = Encoding.UTF8.GetBytes(pValue);
                if (data.Length <= pLengthMax)
                {
                    int bitCounter = BitsRequired(0, pLengthMax);
                    dBitPacking.WriteValue((uint)data.Length, bitCounter);
                    pBitCounter += bitCounter;
                    for (int i = 0; i < data.Length; i++)
                    {
                        int charSize = 8;
                        dBitPacking.WriteValue(data[i], charSize);
                        pBitCounter += charSize;
                    }
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
