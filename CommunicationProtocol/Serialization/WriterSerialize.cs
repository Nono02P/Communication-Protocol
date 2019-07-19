using System;
using System.Collections.Generic;
using System.Text;

namespace CommunicationProtocol.Serialization
{
    public class WriterSerialize : Serializer
    {
        public int BitCounter { get; private set; }

        public WriterSerialize(int pByteBufferSize = 1200) : base(pByteBufferSize) { }

        public void ResetBitCounter()
        {
            BitCounter = 0;
        }

        public override void ManageData(byte[] pData)
        {
            dBitPacking.Clear();
            dBitPacking = BitPacker.FromArray(pData);
        }

        #region Serialisation
        public override bool Serialize(ref bool pValue)
        {
            if (!Error)
            {
                uint val = 0;
                if (pValue)
                    val = 1;
                dBitPacking.WriteValue(val, 1);
                BitCounter++;
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
                int value = (int)((decimal)pValue / (decimal)pResolution);
                Serialize(ref value, min, max);
            }
            return Error;
        }

        public override bool Serialize(ref int pValue, int pMin, int pMax)
        {
            if (pValue < pMin || pValue > pMax)
                Error = true;

            if (!Error)
            {
                int mappedValue = pValue - pMin;
                int bitCounter = BitsRequired(pMin, pMax);
                dBitPacking.WriteValue((uint)mappedValue, bitCounter);
                BitCounter += bitCounter;
            }
            return Error;
        }

        public override bool Serialize(ref string pValue, int pLengthMax)
        {
            if (!Error)
            {
                byte[] data = Encoding.UTF8.GetBytes(pValue);
                if (data.Length <= pLengthMax)
                {
                    int bitCounter = BitsRequired(0, pLengthMax);
                    dBitPacking.WriteValue((uint)data.Length, bitCounter);
                    BitCounter += bitCounter;
                    for (int i = 0; i < data.Length; i++)
                    {
                        int charSize = 8;
                        dBitPacking.WriteValue(data[i], charSize);
                        BitCounter += charSize;
                    }
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
                int nbOfObjects = pObjects.Count;

                int counterObjects = 0;
                int maxDif = 0;
                int previousIndex = 0;
                for (int i = 0; i < nbOfObjects; i++)
                {
                    T obj = pObjects[i];
                    if (obj.ShouldBeSend) // && !obj.LockSending)
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

                Serialize(ref counterObjects, 0, pNbMaxObjects);

                const int difBitEncoding = 5; // Valeur = 0 à 31 (Nombre de bits pour encoder la différence d'index).
                dBitPacking.WriteValue((uint)maxDif, difBitEncoding);
                BitCounter += 5;

                previousIndex = 0;
                for (int i = 0; i < nbOfObjects; i++)
                {
                    T obj = pObjects[i];
                    if (obj.ShouldBeSend) // && !obj.LockSending)
                    {
                        int dif = i - previousIndex;
                        Serialize(ref dif, 0, maxDif);
                        int objectID = dFactory.GetID(obj);
                        Serialize(ref objectID, 0, dFactory.Count() - 1);
                        obj.Serialize(this);
                        previousIndex = i;
                    }
                }
            }
            return Error;
        }

        #endregion Serialisation

        #region Fin de paquet
        public override bool EndOfPacket(ref bool Error, ref int pCheckValue, int pNbOfBits)
        {
            throw new NotImplementedException();
        }
        #endregion Fin de paquet
    }
}
