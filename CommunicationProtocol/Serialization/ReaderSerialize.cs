using System;
using System.Collections.Generic;
using System.Text;

namespace CommunicationProtocol.Serialization
{
    public class ReaderSerialize : Serializer
    {
        public ReaderSerialize(int pByteBufferSize = 1200) : base(pByteBufferSize) { }
        
        #region Serialisation
        public override bool Serialize(ref bool pValue)
        {
            if (!Error)
            {
                uint val = BitPacking.ReadValue(1);
                pValue = val == 1;
            }
            return Error;
        }

        public override bool Serialize(ref float pValue, float pMin, float pMax, float pResolution = 1)
        {
            base.Serialize(ref pValue, pMin, pMax, pResolution);
            if (!Error)
            {
                int min = (int)((decimal)pMin / (decimal)pResolution);
                int max = (int)((decimal)pMax / (decimal)pResolution);
                int value = 0;
                Serialize(ref value, min, max);
                pValue = (float)(value * (decimal)pResolution);
            }
            return Error;
        }

        public override bool Serialize(ref int pValue, int pMin, int pMax)
        {
            if (!Error)
            {
                int value = (int)BitPacking.ReadValue(BitsRequired(pMin, pMax)) + pMin;
                if (value >= pMin && value <= pMax)
                    pValue = value;
                else
                    Error = true;
            }
            return Error;
        }

        public override bool Serialize(ref string pValue, int pLengthMax)
        {
            if (!Error)
            {
                int length = (int)BitPacking.ReadValue(BitsRequired(0, pLengthMax));
                
                if (length <= pLengthMax)
                {
                    byte[] data = new byte[length];
                    for (int i = 0; i < data.Length; i++)
                    {
                        data[i] = (byte)BitPacking.ReadValue(8);
                    }
                    pValue = Encoding.UTF8.GetString(data);
                }
                else
                    Error = true;
            }
            return Error;
        }

        public override bool Serialize<T>(List<T> pObjects, int pNbMaxObjects = 255, bool pAddMissingElements = false, Action<T> pOnObjectCreation = null)
        {
            if (!Error)
            {
                int nbOfObjects = 0;
                Serialize(ref nbOfObjects, 0, pNbMaxObjects);

                if (nbOfObjects > pNbMaxObjects)
                    Error = true;
                
                const int difBitEncoding = 5; // Valeur = 0 à 31 (Nombre de bits pour encoder la différence d'index).
                int difBitSize = (int)BitPacking.ReadValue(difBitEncoding);

                int index = 0;
                for (int i = 0; i < nbOfObjects; i++)
                {
                    index += (int)BitPacking.ReadValue(difBitSize);

                    int objectID = 0;
                    Serialize(ref objectID, 0, dFactory.Count() - 1);
                    T obj = default(T);
                    if (pObjects.Count > index)
                    {
                        obj = pObjects[index];
                        if (obj.GetType() == dFactory.GetType(objectID))
                            obj.Serialize(this);
                        else
                            Error = true;
                    }
                    else
                    {
                        obj = dFactory.CreateInstance<T>(objectID);
                        pOnObjectCreation?.Invoke(obj);
                        obj.Serialize(this);
                        if (pAddMissingElements && pObjects.Count == index && !Error)
                            pObjects.Add(obj);
                        else
                            Error = true;
                    }
                }
            }
            return Error;
        }
        #endregion Serialisation
    }
}