using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CommunicationProtocol.Serialization
{
    public class ReaderSerialize : Serializer
    {
        #region Constructor
        public ReaderSerialize(int pByteBufferSize = 1024 * 255) : base(pByteBufferSize) { }
        #endregion Constructor  

        #region Byte Alignment
        public override void AlignToNextByte()
        {
            BitPacking.AlignToNextReadByte();
        }
        #endregion Byte Alignment

        #region Serialization Functions

        #region Boolean
        public override bool Serialize(ref bool pValue)
        {
            if (!Error)
            {
                uint val = BitPacking.ReadValue(1);
                pValue = val == 1;
#if TRACE
                Trace.Indent();
                Trace.WriteLine("Read boolean : " + val);
                Trace.Unindent();
#endif
            }
            return Error;
        }
        #endregion Boolean  

        #region Byte Array
        public override bool Serialize(ref byte[] pData, int pLength)
        {
            BitPacking.ReadBytes(pLength).CopyTo(pData, 0);
            return pData != null;
        }
        #endregion Byte Array  

        #region Integer
        public override bool Serialize(ref int pValue, int pMin, int pMax)
        {
            if (!Error)
            {
                int requiredBits = BitsRequired(pMin, pMax);
                uint mappedValue = BitPacking.ReadValue(requiredBits);
                int value = (int)mappedValue + pMin;
#if TRACE
                Trace.Indent();
                Trace.WriteLine("Read integer : " + mappedValue + " (" + requiredBits + "Bits)");
                Trace.Unindent();
#endif

                if (value >= pMin && value <= pMax)
                    pValue = value;
                else
                    Error = true;
            }
            return Error;
        }

        public override bool Serialize(ref int pValue, int pNbOfBits)
        {
            if (!Error)
            {
                pValue = (int)BitPacking.ReadValue(pNbOfBits);
#if TRACE
                Trace.Indent();
                Trace.WriteLine("Read integer : " + pValue + " (" + pNbOfBits + "Bits)");
                Trace.Unindent();
#endif
            }
            return Error;
        }
        #endregion Integer  

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
        
        #region String
        public override bool Serialize(ref string pValue, int pLengthMax)
        {
            if (!Error)
            {
                int requiredBits = BitsRequired(0, pLengthMax);
                int length = (int)BitPacking.ReadValue(requiredBits);
#if TRACE
                Trace.Indent();
                Trace.WriteLine("Read string length : " + length + " (" + requiredBits + "Bits)");
#endif

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
#if TRACE
                        Trace.WriteLine("Read string data : " + pValue);
#endif
                    }
                    else
                    {
                        pValue = string.Empty;
                    }
                }
                else
                    Error = true;
#if TRACE
                Trace.Unindent();
#endif
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
#if TRACE
                Trace.Indent();
                Trace.WriteLine("Read List<Objects> Counter : ");
#endif
                Serialize(ref nbOfObjects, 0, pNbMaxObjects);

                if (nbOfObjects > pNbMaxObjects)
                    Error = true;

                if (nbOfObjects > 0)
                {
                    int difBitSize = (int)BitPacking.ReadValue(d_INDEX_DIFFERENCE_BIT_ENCODING);    // Number of bits on which will be encoded the index difference
#if TRACE
                    Trace.Indent();
                    Trace.WriteLine("Read List<Objects> difference Bit Encoding : " + difBitSize + " (" + d_INDEX_DIFFERENCE_BIT_ENCODING + "Bits)");
                    Trace.Indent();
#endif
                    int index = 0;
                    for (int i = 0; i < nbOfObjects; i++)
                    {
                        if (difBitSize > 0)
                        {
                            int dif = (int)BitPacking.ReadValue(difBitSize);
                            index += dif;                                               // Index difference with the previous object.
#if TRACE
                            Trace.WriteLine("Read List<Objects> difference : ");
                            Trace.WriteLine("Read integer : " + dif + " (" + difBitSize + "Bits)");
                            Trace.WriteLine("Read List<Objects> index : " + index);
#endif
                        }

                        int objectID = 0;
                        if (dFactory.Count() - 1 > 0)
                        {
#if TRACE
                            Trace.WriteLine("Read List<Objects> Object ID : ");
#endif
                            Serialize(ref objectID, 0, dFactory.Count() - 1);           // Object ID (If there is more than 1 serializable object)
                        }
                        T obj = default(T);
                        if (pObjects.Count > index)
                        {
                            obj = pObjects[index];
                            if (obj.GetType() == dFactory.GetType(objectID))
                            {
#if TRACE
                                Trace.WriteLine("Read List<Objects> Data on object already in list : ");
#endif
                                Error |= obj.Serialize(this);
                            }
                            else
                                Error = true;
                        }
                        else
                        {
                            obj = dFactory.CreateInstance<T>(objectID);
                            pOnObjectCreation?.Invoke(obj);

#if TRACE
                            Trace.WriteLine("Read List<Objects> Data on Created object : ");
#endif
                            Error |= obj.Serialize(this);
                            if (pAddMissingElements && pObjects.Count == index && !Error)
                                pObjects.Add(obj);
                            else
                                Error = true;
                        }
                    }
                }
#if TRACE
                Trace.IndentLevel -= 2;
#endif
            }
            return Error;
        }
        #endregion List of Serializable Objects  

        #endregion Serialization Functions
    }
}