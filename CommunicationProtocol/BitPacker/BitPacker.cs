using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CommunicationProtocol
{
    public struct BitPacker
    {
        #region Constants
        private const int DEFAULT_BUFFER_SIZE = 300;
        private const int BUFFER_BIT_SIZE = 32;     // Number of bits of an element of _buffer (List<uint> => 32 bits)
        #endregion Constants

        #region Private Variables
        private uint[] _buffer;
        private ulong _temp;            // Tempory var used before push in the buffer (for easily manage the overflow).
        private int _offsetBitReaded;
        #endregion Private Variables

        #region Properties
        public int BitIndex { get; private set; }
        public int WordIndex { get; private set; }
        public int BitLength { get { return BitIndex + WordIndex * BUFFER_BIT_SIZE; } }
        public int ByteLength { get { return (int)Math.Ceiling(BitLength / (decimal)8); } }
        #endregion Properties

        #region Constructor
        public BitPacker(int pByteBufferSize)
        {
            _buffer = new uint[pByteBufferSize / 4];
            _temp = 0;
            _offsetBitReaded = 0;
            BitIndex = 0;
            WordIndex = 0;
        }

        public BitPacker(Span<uint> pBuffer, int? pBitLength = null)
        {
            _buffer = pBuffer.ToArray();
            int bitLength = 0;
            if (pBitLength.HasValue)
                bitLength = pBitLength.Value;
            else
                bitLength = pBuffer.Length * BUFFER_BIT_SIZE;
            _temp = 0;
            _offsetBitReaded = 0;
            BitIndex = bitLength % BUFFER_BIT_SIZE;
            WordIndex = bitLength / BUFFER_BIT_SIZE;
        }

        public BitPacker(uint[] pBuffer, int? pBitLength = null)
        {
            _buffer = pBuffer;
            int bitLength = 0;
            if (pBitLength.HasValue)
                bitLength = pBitLength.Value;
            else
                bitLength = pBuffer.Length * BUFFER_BIT_SIZE;
            _temp = 0;
            _offsetBitReaded = 0;
            BitIndex = bitLength % BUFFER_BIT_SIZE;
            WordIndex = bitLength / BUFFER_BIT_SIZE;
        }
        #endregion Constructor

        #region Fill buffer
        public static BitPacker FromArray(byte[] pBuffer, bool pPushTempInBuffer = true)
        {
            int length = pBuffer.Length;
            byte[] b = new byte[length + 4 - (length % 4)];
            pBuffer.CopyTo(b, 0);
            Span<uint> spanUint = MemoryMarshal.Cast<byte, uint>(new Span<byte>(b));
            BitPacker result = new BitPacker(spanUint, 8 * length);
            result._temp = spanUint[spanUint.Length - 1];
            return result;
        }

        public void WriteValue(uint pValue, int pNbOfBits)
        {
            ValidateNbOfBits(pNbOfBits);

            if (_buffer == null)
                _buffer = new uint[DEFAULT_BUFFER_SIZE];
            ulong maskNbOfBits = (1ul << pNbOfBits) - 1;
            _temp |= (pValue & maskNbOfBits) << BitIndex;
            BitIndex += pNbOfBits;
            while (BitIndex >= BUFFER_BIT_SIZE)
            {
                PushTempInBuffer();
            }
        }

        public void OverrideValue(uint pValue, int pNbOfBits, int pStartPosition = 0)
        {
            ValidateNbOfBits(pNbOfBits);

            int bufferWordIndex = pStartPosition / BUFFER_BIT_SIZE;
            int start = pStartPosition % BUFFER_BIT_SIZE;

            uint maskNbOfBits = (uint)((1ul << pNbOfBits) - 1) << start;

            _buffer[bufferWordIndex] = (_buffer[bufferWordIndex] & ~maskNbOfBits) | (maskNbOfBits & (pValue << start));
            // If the value to be written overlaps on two registers.
            if (BUFFER_BIT_SIZE < start + pNbOfBits && bufferWordIndex + 1 <= WordIndex)
            {
                int s = start - BUFFER_BIT_SIZE + pNbOfBits;
                uint maskNbOfBitsNextReg = (uint)(1ul << s) - 1;
                _buffer[bufferWordIndex + 1] = (_buffer[bufferWordIndex + 1] & ~maskNbOfBitsNextReg) | (maskNbOfBitsNextReg & (pValue >> s));
                if (bufferWordIndex + 1 == WordIndex)
                {
                    _temp = (_temp & ~maskNbOfBitsNextReg) | (maskNbOfBitsNextReg & (pValue >> s));
                }
            }
        }

        public void InsertBytes(byte[] pData, int pLength)
        {
            AlignToNextWriteByte();
            Span<byte> spanByte = new Span<byte>(pData, 0, pLength);
            int lengthInBufferIndex = BUFFER_BIT_SIZE / 8;
            Span<uint> bufferUint = new Span<uint>(_buffer, WordIndex, _buffer.Length - WordIndex);
            Span<byte> bufferByte = MemoryMarshal.Cast<uint, byte>(bufferUint).Slice(BitIndex / 8);
            spanByte.CopyTo(bufferByte);
            int bitsAdded = bufferByte.Length * 8;
            WordIndex += bitsAdded / BUFFER_BIT_SIZE;
            BitIndex = bitsAdded % BUFFER_BIT_SIZE;
        }

        public void AlignToNextWriteByte()
        {
            PushTempInBuffer();
            int remainingBits = BitIndex % 8;
            if (remainingBits > 0)
                BitIndex += 8 - remainingBits;
        }

        public void PushTempInBuffer(bool pCutEmptyEnd = false)
        {
            if (BitIndex > 0)
            {
                if (_temp > 0 || !pCutEmptyEnd)
                {
                    _buffer[WordIndex] = (uint)_temp;
                    if (BitIndex >= BUFFER_BIT_SIZE)
                    {
                        _temp >>= BUFFER_BIT_SIZE;
                        BitIndex -= BUFFER_BIT_SIZE;
                        WordIndex++;
                    }
                }
            }
        }
        #endregion Fill buffer 

        #region Emptying buffer
        public uint ReadValue(int pNbOfBits, bool pRemoveBits = true, int pStartPosition = 0)
        {
            ValidateNbOfBits(pNbOfBits);

            int bufferWordIndex = pStartPosition / BUFFER_BIT_SIZE;
            int start = (pStartPosition + _offsetBitReaded) % BUFFER_BIT_SIZE;
            Span<uint> spanBuffer = GetUintSpanBuffer();
            if ((bufferWordIndex + pNbOfBits / BUFFER_BIT_SIZE) <= spanBuffer.Length)
            {
                if (pNbOfBits <= BUFFER_BIT_SIZE)
                {
                    uint startingVal = 0;
                    if (start > 0)
                    {
                        startingVal = spanBuffer[bufferWordIndex] & (uint)((1ul << start) - 1);
                    }
                    uint maskNbOfBits = (uint)((1ul << pNbOfBits) - 1);
                    ulong result = (spanBuffer[bufferWordIndex] & maskNbOfBits << start) >> start;
                    // If the value to be written overlaps on two registers
                    if (BUFFER_BIT_SIZE < start + pNbOfBits && bufferWordIndex + 1 < spanBuffer.Length)
                    {
                        int s = start - BUFFER_BIT_SIZE + pNbOfBits;
                        result |= (spanBuffer[bufferWordIndex + 1] & ((1ul << s) - 1)) << (pNbOfBits - s);
                    }
                    if (pRemoveBits)
                    {
                        if (pStartPosition > 0)
                        {
                            uint outVal = 0;
                            for (int i = spanBuffer.Length - 1; i >= bufferWordIndex; i--)
                            {
                                uint temp = spanBuffer[i] & maskNbOfBits;
                                spanBuffer[i] >>= pNbOfBits;
                                spanBuffer[i] |= outVal << (BUFFER_BIT_SIZE - pNbOfBits);
                                outVal = temp;
                            }
                        }
                        else
                        {
                            _offsetBitReaded += pNbOfBits;
                        }
                        spanBuffer[bufferWordIndex] &= uint.MaxValue << start;
                        spanBuffer[bufferWordIndex] |= startingVal;
                        
                        BitIndex -= pNbOfBits;
                        while (BitIndex <= 0)
                        {
                            WordIndex--;
                            BitIndex += BUFFER_BIT_SIZE;
                        }
                    }
                    return (uint)result;
                }
                else
                {
                    throw new Exception("Number of bits to read exceed the maximum size of " + BUFFER_BIT_SIZE + " bits.");
                }
            }
            else
            {
                throw new Exception("Start Position + Number of bits out of range.");
            }
        }

        public void AlignToNextReadByte()
        {
            int remainingBits = _offsetBitReaded % 8;
            if (remainingBits > 0)
            {
                int bitsToRemove = 8 - remainingBits;
                _offsetBitReaded += bitsToRemove;
                BitIndex -= bitsToRemove;
            }
        }

        public byte[] ReadBytes(int pLength)
        {
            AlignToNextReadByte();
            byte[] result = GetByteBuffer(pLength);         // Should be calculated before changing the Indexes.
            int bitsToRemove = pLength * 8;
            _offsetBitReaded += bitsToRemove;
            WordIndex -= bitsToRemove / BUFFER_BIT_SIZE;
            BitIndex -= bitsToRemove % BUFFER_BIT_SIZE;
            return result;
        }

        public void RemoveFromEnd(int pNbOfBits)
        {
            if (pNbOfBits > 0)
            {
                int wordToRemove = pNbOfBits / BUFFER_BIT_SIZE;
                int bitsToRemove = pNbOfBits % BUFFER_BIT_SIZE;
                Span<uint> data = new Span<uint>(_buffer, WordIndex - wordToRemove, wordToRemove + (int)Math.Ceiling((decimal)bitsToRemove / BUFFER_BIT_SIZE));
                ulong mask = (1ul << (BitIndex - bitsToRemove)) - 1;
                if (wordToRemove > 0)
                {
                    _temp = data[0] & mask;
                }
                else
                {
                    _temp &= mask;
                }
                data.Clear();
                data[0] = (uint)_temp;
                BitIndex -= bitsToRemove;
                WordIndex -= wordToRemove;
            }
        }

        public void Clear()
        {
            if (_buffer != null)
            {
                new Span<uint>(_buffer).Clear();
            }
            else
            {
                _buffer = new uint[DEFAULT_BUFFER_SIZE];
            }
            _temp = 0;
            BitIndex = 0;
            WordIndex = 0;
        }
        #endregion Emptying buffer

        #region Transformations
        public byte[] GetByteBuffer(int? pLength = null)
        {
            return GetByteSpanBuffer(pLength).ToArray();
        }

        public Span<byte> GetByteSpanBuffer(int? pLength = null)
        {
            // Cast (uint => byte) and remove (Slice) the end if the number of byte isn't a multiple of 4.
            int size = ByteLength;
            if (pLength.HasValue)
                size = pLength.Value;

            return MemoryMarshal.Cast<uint, byte>(GetUintSpanBuffer()).Slice(0, size);
        }

        public Span<uint> GetUintSpanBuffer()
        {
            int wordStart = _offsetBitReaded / BUFFER_BIT_SIZE;
            int bitStart = _offsetBitReaded % BUFFER_BIT_SIZE;
            return new Span<uint>(_buffer, wordStart, (int)Math.Ceiling((decimal)(BitLength + bitStart) / BUFFER_BIT_SIZE));
        }

        public override string ToString()
        {
            string result = string.Empty;
            byte[] data = GetByteBuffer();
            for (int i = 0; i < data.Length; i++)
            {
                byte r = data[i];
                result = Convert.ToString(r, toBase: 2).PadLeft(8, '0') + " " + result;
            }
            return result;
        }
        #endregion Transformations

        #region Help Functions
        private void ValidateNbOfBits(int pNbOfBits)
        {
            if (pNbOfBits < 1 || pNbOfBits > 32)
                throw new Exception("Impossible to write a 32 bit type on more than 32 bits or less than 0 bits.");
        }
        #endregion Help Functions  
    }
}