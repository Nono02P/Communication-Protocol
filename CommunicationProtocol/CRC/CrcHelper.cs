using System;

namespace CommunicationProtocol.CRC
{
    public static class CrcHelper
    {
        #region internal

        #region reverseBits
        internal static ulong ReverseBits(ulong ul, int valueLength)
        {
            ulong newValue = 0;

            for (int i = valueLength - 1; i >= 0; i--)
            {
                newValue |= (ul & 1) << i;
                ul >>= 1;
            }

            return newValue;
        }

        #endregion reverseBits

        #region ToBigEndian

        internal static byte[] ToBigEndianBytes(uint value)
        {
            byte[] result = BitConverter.GetBytes(value);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(result);

            return result;
        }

        internal static byte[] ToBigEndianBytes(ushort value)
        {
            byte[] result = BitConverter.GetBytes(value);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(result);

            return result;
        }

        internal static byte[] ToBigEndianBytes(ulong value)
        {
            byte[] result = BitConverter.GetBytes(value);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(result);

            return result;
        }

        #endregion ToBigEndian

        #region FromBigEndian

        internal static ushort FromBigToUInt16(byte[] buffer, int start)
        {
            return (ushort)(buffer[start] << 8 | buffer[start + 1]);
        }

        internal static uint FromBigToUInt32(byte[] buffer, int start)
        {
            return (uint)(buffer[start] << 24 | buffer[start + 1] << 16 | buffer[start + 2] << 8 | buffer[start + 3]);
        }

        internal static ulong FromBigToUInt64(byte[] buffer, int start)
        {
            ulong result = 0;
            for (int i = 0; i < 8; i++)
            {
                result |= ((ulong)buffer[i]) << (64 - 8 * (i + 1));
            }

            return result;
        }

        #endregion FromBigEndian

        #endregion internal

        #region public

        /// <summary>
        /// Use this method for convert hash from byte array to UInt16 value.
        /// </summary>
        public static ushort ToUInt16(byte[] hash)
        {
            return FromBigToUInt16(hash, 0);
        }

        /// <summary>
        /// Use this method for convert hash from byte array to UInt32 value.
        /// </summary>
        public static uint ToUInt32(byte[] hash)
        {
            return FromBigToUInt32(hash, 0);
        }

        #endregion public

        public static ulong FromBigEndian(byte[] hashBytes, int hashSize)
        {
            ulong result = 0;
            for (int i = 0; i < 8; i++)
            {
                result |= ((ulong)hashBytes[i]) << (64 - 8 * (i + 1));
            }

            return result;
        }
    }
}