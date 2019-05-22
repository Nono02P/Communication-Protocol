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
            dBitPacking = BitPacking.FromArray(pData);
        }

        #region Serialisation
        public override bool Serialize(ref bool pResult, ref float pValue, float pMin, float pMax, float pResolution = 1)
        {
            base.Serialize(ref pResult, ref pValue, pMin, pMax, pResolution);
            if (pResult)
            {
                int min = (int)((decimal)pMin / (decimal)pResolution);
                int max = (int)((decimal)pMax / (decimal)pResolution);
                int value = (int)((decimal)pValue / (decimal)pResolution);
                Serialize(ref pResult, ref value, min, max);
            }
            return pResult;
        }

        public override bool Serialize(ref bool pResult, ref int pValue, int pMin, int pMax)
        {
            if (pValue < pMin || pValue > pMax)
                pResult = false;

            if (pResult)
            {
                int mappedValue = pValue - pMin;
                dBitPacking.WriteValue((uint)mappedValue, BitsRequired(pMin, pMax));
            }
            return pResult;
        }

        public override bool Serialize(ref bool pResult, ref string pValue, int pLengthMax)
        {
            if (pResult)
            {
                byte[] data = Encoding.UTF8.GetBytes(pValue);
                if (data.Length <= pLengthMax)
                {
                    dBitPacking.WriteValue((uint)data.Length, BitsRequired(0, pLengthMax));
                    for (int i = 0; i < data.Length; i++)
                    {
                        byte d = data[i];
                        dBitPacking.WriteValue(d, 8);
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
    }
}
