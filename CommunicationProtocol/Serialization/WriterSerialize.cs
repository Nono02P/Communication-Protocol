using System;
using System.Collections.Generic;
using System.Text;

namespace CommunicationProtocol.Serialization
{
    public class WriterSerialize : Serializer
    {
        #region Constructor
        public WriterSerialize(int pByteBufferSize = 1200) : base(pByteBufferSize) { }
        #endregion Constructor  
        
        #region Boolean
        public override bool Serialize(ref bool pValue)
        {
            if (!Error)
            {
                uint val = 0;
                if (pValue)
                    val = 1;
                BitPacking.WriteValue(val, 1);
#if TRACE_LOG
                LogHelper.WriteToFile("Write boolean : " + val, this, Program.FileName);
#endif
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
                int value = (int)((decimal)pValue / (decimal)pResolution);
                Serialize(ref value, min, max);
            }
            return Error;
        }
        #endregion Float  

        #region Integer
        public override bool Serialize(ref int pValue, int pMin, int pMax)
        {
            if (pValue < pMin || pValue > pMax)
                Error = true;

            if (!Error)
            {
                long mappedValue = pValue - pMin;
                int requiredBits = BitsRequired(pMin, pMax);
#if TRACE_LOG
                LogHelper.WriteToFile("Write integer : " + mappedValue + " (" + requiredBits + "Bits)", this, Program.FileName);
#endif
                BitPacking.WriteValue((uint)mappedValue, requiredBits);
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
                if (string.IsNullOrEmpty(pValue))
                {
                    BitPacking.WriteValue(0, requiredBits);                             // Longueur du string (cas d'une chaine nulle ou vide)
#if TRACE_LOG
                    LogHelper.WriteToFile("Write string length : " + 0 + " (" + requiredBits + "Bits)", this, Program.FileName);
#endif
                }
                else
                {
                    byte[] data = Encoding.UTF8.GetBytes(pValue);
                    if (data.Length <= pLengthMax)
                    {
                        BitPacking.WriteValue((uint)data.Length, requiredBits);         // Longueur du string
#if TRACE_LOG
                        LogHelper.WriteToFile("Write string length : " + data.Length + " '" + pValue + "'" + " (" + requiredBits + "Bits)", this, Program.FileName);
#endif
                        for (int i = 0; i < data.Length; i++)
                        {
                            int charSize = 8;
                            BitPacking.WriteValue(data[i], charSize);                   // Caractères passés un par un
                        }
                    }
                    else
                        Error = true;
                }
            }
            return Error;
        }
        #endregion String  

        #region List of Serializable Objects
        public override bool Serialize<T>(List<T> pObjects, int pNbMaxObjects = 255, bool pAddMissingElements = false, Action<T> pOnObjectCreation = null)
        {
            if (!Error)
            {
                int nbOfObjects = pObjects.Count;

                int counterObjects = 0;
                int maxDif = 0;
                int previousIndex = 0;
                for (int i = 0; i < nbOfObjects; i++)
                {
                    T obj = pObjects[i];
                    if (obj.ShouldBeSend)
                    {
                        int dif = i - previousIndex;
                        if (dif > maxDif)
                        {
                            maxDif = dif;
                        }
                        previousIndex = i;
                        counterObjects++;
                    }
                }
#if TRACE_LOG
                LogHelper.WriteToFile("Write List<Objects> Counter : ", this, Program.FileName);
#endif
                Serialize(ref counterObjects, 0, pNbMaxObjects);                        // Nombre d'objets transmis

                if (counterObjects > 0)
                {
                    // 5 => Valeur = 0 à 31 (Nombre de bits pour encoder la différence d'index).
                    const int difBitEncoding = 5;
#if TRACE_LOG
                    LogHelper.WriteToFile("Write List<Objects> difference Bit Encoding : " + maxDif + " (" + difBitEncoding + "Bits)", this, Program.FileName);
#endif
                    BitPacking.WriteValue((uint)maxDif, difBitEncoding);                // Nombre de bits sur quoi sera encodé la différence d'index

                    previousIndex = 0;
                    if (maxDif > 0)
                    {
                        for (int i = 0; i < nbOfObjects; i++)
                        {
                            T obj = pObjects[i];
                            if (obj.ShouldBeSend)
                            {
                                int dif = i - previousIndex;
#if TRACE_LOG
                                LogHelper.WriteToFile("Write List<Objects> difference : ", this, Program.FileName);
#endif
                                Serialize(ref dif, 0, maxDif);                          // Différence d'index avec l'objet précédent
                                int objectID = dFactory.GetID(obj);
                                if (dFactory.Count() - 1 > 0)
                                {
#if TRACE_LOG
                                    LogHelper.WriteToFile("Write List<Objects> Object ID : ", this, Program.FileName);
#endif
                                    Serialize(ref objectID, 0, dFactory.Count() - 1);   // ID de l'objet (si plusieurs objets sont sérialisables
                                }
#if TRACE_LOG
                                LogHelper.WriteToFile("Write List<Objects> Data : ", this, Program.FileName);
#endif
                                obj.Serialize(this);
                                previousIndex = i;
                            }
                        }
                    }
                }
            }
            return Error;
        }

        #endregion List of Serializable Objects  
    }
}