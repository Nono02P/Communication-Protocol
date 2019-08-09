using CommunicationProtocol.Factories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;

namespace CommunicationProtocol.Serialization
{
    public abstract class Serializer
    {
        #region Constants
        // 5 => Value can be 0 to 31 (Number of bits necessary to encode the index difference for objects serialization).
        protected const int d_INDEX_DIFFERENCE_BIT_ENCODING = 5;
        #endregion Constants  

        #region Private Variables
        private bool _error;
        #endregion Private Variables  

        #region Protected Variables
        protected SerializerFactory dFactory;
        #endregion Protected Variables

        #region Public Fields
        public BitPacker BitPacking;// { get; set; }
        #endregion Public Fields

        #region Properties
        public bool Error { get { return _error; } set { _error = value; } } // Debug.Assert(!value); } }
        #endregion Properties  

        #region Constructor
        public Serializer(int pByteBufferSize)
        {
            BitPacking = new BitPacker(pByteBufferSize);
            dFactory = SerializerFactory.GetFactory();
        }
        #endregion Constructor  

        #region Serialization Functions

        #region Boolean
        public abstract bool Serialize(ref bool pValue);
        #endregion Boolean  

        #region Byte Array
        public abstract bool Serialize(ref byte[] pData, int pLength);
        #endregion Byte Array  

        #region Integer
        public abstract bool Serialize(ref int pValue, int pMin, int pMax);
        public abstract bool Serialize(ref int pValue, int pNbOfBits);
        public bool Serialize(ref int pValue, int pMin, int pMax, ref bool pShouldBeSerialized)
        {
            Serialize(ref pShouldBeSerialized);
            if (pShouldBeSerialized)
                Serialize(ref pValue, pMin, pMax);
            return Error;
        }
        #endregion Integer  

        #region Float
        public bool Serialize(ref float pValue, float pMin, float pMax, ref bool pShouldBeSerialized, float pResolution = 1)
        {
            Serialize(ref pShouldBeSerialized);
            if (pShouldBeSerialized)
                Serialize(ref pValue, pMin, pMax, pResolution);
            return Error;
        }

        public virtual bool Serialize(ref float pValue, float pMin, float pMax, float pResolution = 1)
        {
            if (!Error)
            {
                bool resolutionError = false;
                if (pResolution > 1)
                    resolutionError = true;
                else
                {
                    decimal workResolution = (decimal)pResolution;
                    while (workResolution < 1 && !resolutionError)
                    {
                        workResolution *= 10;
                        if (workResolution > 1)
                            resolutionError = true;
                    }
                }
                if (resolutionError)
                {
                    Error = true;
                    throw new Exception("The resolution should be a number like 1, 0.1, 0.01, etc... The value is : " + pResolution);
                }
            }
            return Error;
        }
        #endregion Float  

        #region String
        public abstract bool Serialize(ref string pValue, int pLengthMax);
        public bool Serialize(ref string pValue, int pLengthMax, ref bool pShouldBeSerialized)
        {
            Serialize(ref pShouldBeSerialized);
            if (pShouldBeSerialized)
                Serialize(ref pValue, pLengthMax);
            return Error;
        }
        #endregion String

        // TODO : Manage a list of primitives types (int, float, string, etc).

        #region List of objects
        public abstract bool Serialize<T>(List<T> pObjects, int pNbMaxObjects = 255, bool pAddMissingElements = false, Action<T> pOnObjectCreation = null) where T : IBinarySerializable;
        #endregion List of objects  

        #region Vector2
        public bool Serialize(ref Vector2 pValue, Vector2 pMin, Vector2 pMax, ref bool pShouldBeSerialized, float pResolution = 1)
        {
            Serialize(ref pShouldBeSerialized);
            if (pShouldBeSerialized)
                Serialize(ref pValue, pMin, pMax, pResolution);
            return Error;
        }

        public bool Serialize(ref Vector2 pValue, Vector2 pMin, Vector2 pMax, float pResolution = 1)
        {
            Serialize(ref pValue.X, pMin.X, pMax.X, pResolution);
            Serialize(ref pValue.Y, pMin.Y, pMax.Y, pResolution);
            return Error;
        }
        #endregion Vector2  

        #region Vector3
        public bool Serialize(ref Vector3 pValue, Vector3 pMin, Vector3 pMax, ref bool pShouldBeSerialized, float pResolution = 1)
        {
            Serialize(ref pShouldBeSerialized);
            if (pShouldBeSerialized)
                Serialize(ref pValue, pMin, pMax, pResolution);
            return Error;
        }

        public bool Serialize(ref Vector3 pValue, Vector3 pMin, Vector3 pMax, float pResolution = 1)
        {
            Serialize(ref pValue.X, pMin.X, pMax.X, pResolution);
            Serialize(ref pValue.Y, pMin.Y, pMax.Y, pResolution);
            Serialize(ref pValue.Z, pMin.Z, pMax.Z, pResolution);
            return Error;
        }
        #endregion Vector3  

        #region Vector4
        public bool Serialize(ref Vector4 pValue, Vector4 pMin, Vector4 pMax, ref bool pShouldBeSerialized, float pResolution = 1)
        {
            Serialize(ref pShouldBeSerialized);
            if (pShouldBeSerialized)
                Serialize(ref pValue, pMin, pMax, pResolution);
            return Error;
        }

        public bool Serialize(ref Vector4 pValue, Vector4 pMin, Vector4 pMax, float pResolution = 1)
        {
            Serialize(ref pValue.X, pMin.X, pMax.X, pResolution);
            Serialize(ref pValue.Y, pMin.Y, pMax.Y, pResolution);
            Serialize(ref pValue.Z, pMin.Z, pMax.Z, pResolution);
            Serialize(ref pValue.W, pMin.W, pMax.W, pResolution);
            return Error;
        }
        #endregion Vector4  

        #region Quaternion
        public bool Serialize(ref Quaternion pValue, Quaternion pMin, Quaternion pMax, ref bool pShouldBeSerialized, float pResolution = 1)
        {
            Serialize(ref pShouldBeSerialized);
            if (pShouldBeSerialized)
                Serialize(ref pValue, pMin, pMax, pResolution);
            return Error;
        }

        public bool Serialize(ref Quaternion pValue, Quaternion pMin, Quaternion pMax, float pResolution = 1)
        {
            Serialize(ref pValue.X, pMin.X, pMax.X, pResolution);
            Serialize(ref pValue.Y, pMin.Y, pMax.Y, pResolution);
            Serialize(ref pValue.Z, pMin.Z, pMax.Z, pResolution);
            Serialize(ref pValue.W, pMin.W, pMax.W, pResolution);
            return Error;
        }
        #endregion Quaternion  

        #endregion Serialization Functions  

        #region Help Functions
        public int BitsRequired(int pMin, int pMax)
        {
            if (pMin != pMax)
            {
                uint x = (uint)(pMax - pMin);
                x = x | (x >> 1);
                x = x | (x >> 2);
                x = x | (x >> 4);
                x = x | (x >> 8);
                x = x | (x >> 16);
                x = x >> 1;
                x = x - ((x >> 1) & 0x55555555);
                x = ((x >> 2) & 0x33333333) + (x & 0x33333333);
                x = ((x >> 4) + x) & 0x0f0f0f0f;
                x = x + (x >> 8);
                x = x + (x >> 16);
                return (int)(x & 0x0000003f) + 1;
            }
            else
            {
                return 0;
            }
        }
        #endregion Help Functions 
    }
}