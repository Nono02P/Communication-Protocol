using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;

namespace CommunicationProtocol.Serialization
{
    public abstract class Serializer
    {
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
        public abstract bool Serialize(ref bool pValue);
        public abstract bool Serialize(ref int pValue, int pMin, int pMax);
        public abstract bool Serialize(ref string pValue, int pLengthMax);
        public abstract bool Serialize<T>(List<T> pObjects, int pNbMaxObjects = 255, bool pAddMissingElements = false, Action<T> pOnObjectCreation = null) where T : IBinarySerializable;
        
        #region Float
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

        // TODO : Gérer une liste de types primitifs (int, float, string, etc).
        //public abstract bool Serialize<T>(List<T> pType);

        #region Vectors / Quaternion
        public bool Serialize(ref Vector2 pValue, Vector2 pMin, Vector2 pMax, float pResolution = 1)
        {
            Serialize(ref pValue.X, pMin.X, pMax.X, pResolution);
            Serialize(ref pValue.Y, pMin.Y, pMax.Y, pResolution);
            return Error;
        }

        public bool Serialize(ref Vector3 pValue, Vector3 pMin, Vector3 pMax, float pResolution = 1)
        {
            Serialize(ref pValue.X, pMin.X, pMax.X, pResolution);
            Serialize(ref pValue.Y, pMin.Y, pMax.Y, pResolution);
            Serialize(ref pValue.Z, pMin.Z, pMax.Z, pResolution);
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

        public bool Serialize(ref Quaternion pValue, Quaternion pMin, Quaternion pMax, float pResolution = 1)
        {
            Serialize(ref pValue.X, pMin.X, pMax.X, pResolution);
            Serialize(ref pValue.Y, pMin.Y, pMax.Y, pResolution);
            Serialize(ref pValue.Z, pMin.Z, pMax.Z, pResolution);
            Serialize(ref pValue.W, pMin.W, pMax.W, pResolution);
            return Error;
        }
        #endregion Vectors / Quaternion

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