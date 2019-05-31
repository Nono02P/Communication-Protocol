using System;
using System.Collections.Generic;
using System.Text;

namespace CommunicationProtocol.Serialiser
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


        public override bool Serialize<T>(ref int pBitCounter, ref bool pResult, List<T> pObjects, int pNbMaxObjects = 255)
        {
            if (pResult)
            {
                int nbOfObjects = 0;
                Serialize(ref pBitCounter, ref pResult, ref nbOfObjects, 0, pNbMaxObjects);

                if (nbOfObjects > pNbMaxObjects)
                    pResult = false;
                if (pObjects.Count < nbOfObjects)
                    pResult = false;    // TODO : Une fois l'instancieur créé, supprimer cette erreur de sérialisation.

                const int difBitEncoding = 5; // Valeur = 0 à 31 (Nombre de bits pour encoder la différence d'index).
                int difBitSize = (int)dBitPacking.ReadValue(difBitEncoding);

                int index = 0;
                for (int i = 0; i < nbOfObjects; i++)
                {
                    index += (int)dBitPacking.ReadValue(difBitSize);
                    // TODO : Lire le type de l'objet
                    // enumerationObjects = (int)dBitPacking.ReadValue();
                    T obj = default(T);
                    if (pObjects.Count > index)
                    {
                        // inListObjectType = getObjectType(enumerationObjets)
                        obj = pObjects[index];
                        // Comparer obj.GetType() et inListObjectType
                        // Si identique, désérialiser, sinon pResult = false;
                        obj.Serialize(this, ref pBitCounter, ref pResult);
                    }
                    else
                    {
                        // TODO : L'objet n'existe pas dans la liste, prévoir d'instancier l'objet de type enumerationObjects
                        // obj = new object();
                        // Ajouter l'objet à la liste
                        // pObjects.Add(obj);
                    }
                }
            }
            return pResult;
        }
    }
}
