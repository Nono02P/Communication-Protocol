using System;
using System.Collections.Generic;
using System.Text;

namespace CommunicationProtocol
{
    public class ReaderSerialize : Serializer
    {
        public override bool Serialize(ref bool pResult, ref float pValue, float pMin, float pMax, float pResolution = 1)
        {
            base.Serialize(ref pResult, ref pValue, pMin, pMax, pResolution);
            if (pResult)
            {
                int min = (int)(pMin / pResolution);
                int max = (int)(pMax / pResolution);
                //float value = (dBitPacking.ReadValue((int)BitsRequired(min, max)) + pMin) * pResolution;
                int value = 0;
                Serialize(ref pResult, ref value, min, max);
                pValue = value * pResolution;
            }
            return pResult;
        }

        public override bool Serialize(ref bool pResult, ref int pValue, int pMin, int pMax)
        {
            if (pResult)
            {
                int value = (int)dBitPacking.ReadValue((int)BitsRequired(pMin, pMax)) + pMin;
                if (value >= pMin && value <= pMax)
                    pValue = value;
                else
                    pResult = false;
            }
            return pResult;
        }

        public override bool Serialize(ref bool pResult, ref string pValue, int pLengthMax)
        {
            throw new NotImplementedException();
        }
    }
}
