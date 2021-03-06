﻿using System;
using System.Collections.Generic;
using System.Text;

namespace CommunicationProtocol.Serialization
{
    public class WriterSerialize : Serializer
    {
        #region Constructor
        public WriterSerialize(int pByteBufferSize = 1024 * 255) : base(pByteBufferSize) { }
        #endregion Constructor  

        #region Byte Alignment
        public override void AlignToNextByte()
        {
            BitPacking.AlignToNextWriteByte();
        }
        #endregion Byte Alignment

        #region Serialization Functions

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

        #region Byte Array
        public override bool Serialize(ref byte[] pData, int pLength)
        {
            BitPacking.InsertBytes(pData, pLength);
            return true;
        }
        #endregion Byte Array  

        #region Integer
        public override bool Serialize(ref int pValue, int pMin, int pMax)
        {
            if (pValue < pMin || pValue > pMax)
                Error = true;

            if (!Error)
            {
                uint mappedValue = (uint)(pValue - pMin);
                int requiredBits = BitsRequired(pMin, pMax);
#if TRACE_LOG
                LogHelper.WriteToFile("Write integer : " + mappedValue + " (" + requiredBits + "Bits)", this, Program.FileName);
#endif
                BitPacking.WriteValue(mappedValue, requiredBits);
            }
            return Error;
        }

        public override bool Serialize(ref int pValue, int pNbOfBits)
        {
            if (!Error)
            {
#if TRACE_LOG
                LogHelper.WriteToFile("Write integer : " + pValue + " (" + pNbOfBits + "Bits)", this, Program.FileName);
#endif
                BitPacking.WriteValue((uint)pValue, pNbOfBits);
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
                int value = (int)((decimal)pValue / (decimal)pResolution);
                Serialize(ref value, min, max);
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
                if (string.IsNullOrEmpty(pValue))
                {
                    BitPacking.WriteValue(0, requiredBits);                             // String length (If the string is empty or null)
#if TRACE_LOG
                    LogHelper.WriteToFile("Write string length : " + 0 + " (" + requiredBits + "Bits)", this, Program.FileName);
#endif
                }
                else
                {
                    byte[] data = Encoding.UTF8.GetBytes(pValue);
                    if (data.Length <= pLengthMax)
                    {
                        BitPacking.WriteValue((uint)data.Length, requiredBits);         // String length (If the string isn't empty or null)
#if TRACE_LOG
                        LogHelper.WriteToFile("Write string length : " + data.Length + " '" + pValue + "'" + " (" + requiredBits + "Bits)", this, Program.FileName);
#endif
                        for (int i = 0; i < data.Length; i++)
                        {
                            int charSize = 8;
                            BitPacking.WriteValue(data[i], charSize);                   // byte (char) passed one by one
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
                int difBitSize = 0;
                int previousIndex = 0;
                for (int i = 0; i < nbOfObjects; i++)
                {
                    T obj = pObjects[i];
                    if (obj.ShouldBeSend)
                    {
                        int dif = i - previousIndex;
                        if (dif > difBitSize)
                        {
                            difBitSize = dif;
                        }
                        previousIndex = i;
                        counterObjects++;
                    }
                }

                if (counterObjects == 0)
                    Error = true;   // No objects to send, the package does not need to be sent.
                else
                {
#if TRACE_LOG
                    LogHelper.WriteToFile("Write List<Objects> Counter : ", this, Program.FileName);
#endif
                    Serialize(ref counterObjects, 0, pNbMaxObjects);                            // Number of transmitted objects
#if TRACE_LOG
                    LogHelper.WriteToFile("Write List<Objects> difference Bit Encoding : " + difBitSize + " (" + difBitEncoding + "Bits)", this, Program.FileName);
#endif
                    BitPacking.WriteValue((uint)difBitSize, d_INDEX_DIFFERENCE_BIT_ENCODING);   // Number of bits on which will be encoded the index difference
                    previousIndex = 0;
                    for (int i = 0; i < nbOfObjects; i++)
                    {
                        T obj = pObjects[i];
                        if (obj.ShouldBeSend)
                        {
                            if (difBitSize > 0)
                            {
                                int dif = i - previousIndex;
#if TRACE_LOG
                                LogHelper.WriteToFile("Write List<Objects> difference : ", this, Program.FileName);
#endif
                                Serialize(ref dif, 0, difBitSize);                              // Index difference with the previous object.
#if TRACE_LOG
                                LogHelper.WriteToFile("Write List<Objects> index : " + i, this, Program.FileName);
#endif
                            }

                            int objectID = dFactory.GetID(obj);
                            if (dFactory.Count() - 1 > 0)
                            {
#if TRACE_LOG
                                LogHelper.WriteToFile("Write List<Objects> Object ID : ", this, Program.FileName);
#endif
                                Serialize(ref objectID, 0, dFactory.Count() - 1);   // Object ID (If there is more than 1 serializable object)
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
            return Error;
        }
        #endregion List of Serializable Objects  

        #endregion Serialization Functions
    }
}