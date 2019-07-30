using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace CommunicationProtocol.CRC
{
    public class Crc : HashAlgorithm
    {
        #region Private Variables
        private readonly ulong _mask;
        private readonly ulong[] _table = new ulong[256];
        private ulong _currentValue;
        #endregion Private Variables

        #region Properties
        public Parameters Parameters { get; private set; }
        public override int HashSize { get { return Parameters.HashSize; } }
        public override bool CanTransformMultipleBlocks
        {
            get
            {
                return base.CanTransformMultipleBlocks;
            }
        }
        #endregion Properties

        #region Constructor
        public Crc(Parameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException("parameters");

            Parameters = parameters;

            _mask = ulong.MaxValue >> (64 - HashSize);

            Init();
        }
        #endregion Constructor

        public ulong[] GetTable()
        {
            ulong[] res = new ulong[_table.Length];
            Array.Copy(_table, res, _table.Length);
            return res;
        }

        public override void Initialize()
        {
            _currentValue = Parameters.RefOut ? CrcHelper.ReverseBits(Parameters.Init, HashSize) : Parameters.Init;
        }

        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            _currentValue = ComputeCrc(_currentValue, array, ibStart, cbSize);
        }

        protected override byte[] HashFinal()
        {
            return CrcHelper.ToBigEndianBytes(_currentValue ^ Parameters.XorOut);
        }

        private void Init()
        {
            CreateTable();
            Initialize();
        }

        #region Main functions

        private ulong ComputeCrc(ulong init, byte[] data, int offset, int length)
        {
            ulong crc = init;

            if (Parameters.RefOut)
            {
                for (int i = offset; i < offset + length; i++)
                {
                    crc = _table[(crc ^ data[i]) & 0xFF] ^ (crc >> 8);
                    crc &= _mask;
                }
            }
            else
            {
                int toRight = HashSize - 8;
                toRight = toRight < 0 ? 0 : toRight;
                for (int i = offset; i < offset + length; i++)
                {
                    crc = _table[((crc >> toRight) ^ data[i]) & 0xFF] ^ (crc << 8);
                    crc &= _mask;
                }
            }

            return crc;
        }

        private void CreateTable()
        {
            for (int i = 0; i < _table.Length; i++)
                _table[i] = CreateTableEntry(i);
        }

        private ulong CreateTableEntry(int index)
        {
            ulong r = (ulong)index;

            if (Parameters.RefIn)
                r = CrcHelper.ReverseBits(r, HashSize);
            else if (HashSize > 8)
                r <<= HashSize - 8;

            ulong lastBit = 1ul << (HashSize - 1);
            
            for (int i = 0; i < 8; i++)
            {
                if ((r & lastBit) != 0)
                    r = (r << 1) ^ Parameters.Poly;
                else
                    r <<= 1;
            }

            if (Parameters.RefIn)
                r = CrcHelper.ReverseBits(r, HashSize);

            return r & _mask;
        }

        #endregion Main functions

        #region Test functions
        public static CheckResult[] CheckAll()
        {
            Dictionary<CrcAlgorithms, Parameters> parameters = CrcStdParams.StandartParameters;

            List<CheckResult> result = new List<CheckResult>();
            foreach (KeyValuePair<CrcAlgorithms, Parameters> parameter in parameters)
            {
                Crc crc = new Crc(parameter.Value);

                result.Add(new CheckResult()
                {
                    Parameter = parameter.Value,
                    Table = crc.GetTable()
                });
            }

            return result.ToArray();
        }

        public bool IsRight(byte[] bytes)
        {
            byte[] hashBytes = ComputeHash(bytes);
            ulong hash = CrcHelper.FromBigEndian(hashBytes, HashSize);
            return hash == Parameters.Check;
        } 

        public class CheckResult
        {
            public Parameters Parameter { get; set; }
            public ulong[] Table { get; set; }
        }
        #endregion Test functions
    }
}