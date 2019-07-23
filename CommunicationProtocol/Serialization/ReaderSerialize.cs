using System;
using System.Collections.Generic;
using System.Text;

namespace CommunicationProtocol.Serialization
{
    public class ReaderSerialize : Serializer
    {
        #region Constructor
        public ReaderSerialize(int pByteBufferSize = 1200) : base(pByteBufferSize) { }
        #endregion Constructor  
        
        #region Boolean
        public override bool Serialize(ref bool pValue)
        {
            if (!Error)
            {
                uint val = BitPacking.ReadValue(1);
                pValue = val == 1;
                LogHelper.WriteToFile("Read boolean : " + val, this, Program.FileName);
            }
            return Error;
        }
        #endregion Boolean  

        #region Float
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
        #endregion Float  

        #region Integer
        public override bool Serialize(ref int pValue, int pMin, int pMax)
        {
            if (!Error)
            {
                int requiredBits = BitsRequired(pMin, pMax);
                int value = (int)BitPacking.ReadValue(requiredBits) + pMin;
                LogHelper.WriteToFile("Read integer : " + value + "(" + requiredBits + "Bits)", this, Program.FileName);
                if (value >= pMin && value <= pMax)
                    pValue = value;
                else
                    Error = true;
            }
            return Error;
        }
        #endregion Integer  

        #region String
        public override bool Serialize(ref string pValue, int pLengthMax)
        {
            if (!Error)
            {
                int requiredBits = BitsRequired(0, pLengthMax);
                int length = (int)BitPacking.ReadValue(requiredBits);
                LogHelper.WriteToFile("Read string length : " + length + "(" + requiredBits + "Bits)", this, Program.FileName);

                if (length <= pLengthMax)
                {
                    if (length > 0)
                    {
                        byte[] data = new byte[length];
                        for (int i = 0; i < data.Length; i++)
                        {
                            data[i] = (byte)BitPacking.ReadValue(8);
                        }
                        pValue = Encoding.UTF8.GetString(data);
                        LogHelper.WriteToFile("Read string data : " + pValue, this, Program.FileName);
                    }
                    else
                    {
                        pValue = string.Empty;
                    }
                }
                else
                    Error = true;
            }
            return Error;
        }
        #endregion String  

        #region List of Serializable Objects
        public override bool Serialize<T>(List<T> pObjects, int pNbMaxObjects = 255, bool pAddMissingElements = false, Action<T> pOnObjectCreation = null)
        {
            if (!Error)
            {
                int nbOfObjects = 0;
                Serialize(ref nbOfObjects, 0, pNbMaxObjects);
                LogHelper.WriteToFile("Read List<Objects> Counter : " + nbOfObjects + "(" + BitsRequired(0, pNbMaxObjects) + "Bits)", this, Program.FileName);

                if (nbOfObjects > pNbMaxObjects)
                    Error = true;

                if (nbOfObjects > 0)
                {
                    // 5 => Valeur = 0 à 31 (Nombre de bits pour encoder la différence d'index).
                    const int difBitEncoding = 5; 
                    int difBitSize = (int)BitPacking.ReadValue(difBitEncoding);         // Nombre de bits sur quoi sera encodé la différence d'index
                    LogHelper.WriteToFile("Read List<Objects> difference Bit Encoding : " + difBitSize + " (" + difBitEncoding + "Bits)", this, Program.FileName);

                    int index = 0;
                    for (int i = 0; i < nbOfObjects; i++)
                    {
                        if (difBitSize > 0)
                        {
                            int dif = (int)BitPacking.ReadValue(difBitSize);
                            index += dif;                                               // Différence d'index avec l'objet précédent
                            LogHelper.WriteToFile("Read List<Objects> difference : " + dif + " index : " + index + "(" + difBitSize + "Bits)", this, Program.FileName);
                        }

                        int objectID = 0;
                        if (dFactory.Count() - 1 > 0)
                        {
                            Serialize(ref objectID, 0, dFactory.Count() - 1);           // ID de l'objet (si plusieurs objets sont sérialisables
                            LogHelper.WriteToFile("Read List<Objects> Object ID : " + objectID + "(" + BitsRequired(0, dFactory.Count() - 1) + "Bits)", this, Program.FileName);
                        }
                        T obj = default(T);
                        if (pObjects.Count > index)
                        {
                            obj = pObjects[index];
                            if (obj.GetType() == dFactory.GetType(objectID))
                            {
                                LogHelper.WriteToFile("Read List<Objects> Data on object already in list : ", this, Program.FileName);
                                obj.Serialize(this);
                            }
                            else
                                Error = true;
                        }
                        else
                        {
                            obj = dFactory.CreateInstance<T>(objectID);
                            pOnObjectCreation?.Invoke(obj);
                            LogHelper.WriteToFile("Read List<Objects> Data on Created object : ", this, Program.FileName);
                            obj.Serialize(this);
                            if (pAddMissingElements && pObjects.Count == index && !Error)
                                pObjects.Add(obj);
                            else
                                Error = true;
                        }
                    }
                }
            }
            return Error;
        }
        #endregion List of Serializable Objects  
    }
}