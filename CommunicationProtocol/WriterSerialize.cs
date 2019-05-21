using System;
using System.Collections.Generic;
using System.Text;

namespace CommunicationProtocol
{
    public class WriterSerialize : Serializer
    {
        public override bool Serialize(ref bool pResult, ref float pValue, float pMin, float pMax, float pResolution = 1)
        {
            base.Serialize(ref pResult, ref pValue, pMin, pMax, pResolution);
            if (pResult)
            {
                int min = (int)(pMin / pResolution);
                int max = (int)(pMax / pResolution);
                int value = (int)(pValue / pResolution);
                Serialize(ref pResult, ref value, min, max);
            }
            return pResult;
        }

        public override bool Serialize(ref bool pResult, ref int pValue, int pMin, int pMax)
        {
            if (pResult)
            {
                int mappedValue = pValue - pMin;
                dBitPacking.WriteValue((uint)mappedValue, BitsRequired(pMin, pMax));
            }
            return pResult;
        }

        public override bool Serialize(ref bool pResult, ref string pValue, int pLengthMax)
        {
            throw new NotImplementedException();
        }
    }
}
