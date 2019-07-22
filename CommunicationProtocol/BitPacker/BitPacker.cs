using System;
using System.Collections.Generic;

namespace CommunicationProtocol
{
    public struct BitPacker
    {
        #region Constantes
        private const int DEFAULT_BUFFER_SIZE = 300;
        private const int BUFFER_BIT_SIZE = 32;     // Nombre de bits dépendant du type de données de la liste _buffer (List<uint> => 32 bits)
        #endregion

        #region Variables privées
        private uint[] _buffer;
        private ulong _temp;            // Tampon utilisé avant de pousser dans le buffer
        private int _offsetBitReaded;
        #endregion

        #region Propriétés
        public int BitIndex { get; private set; }
        public int WordIndex { get; private set; }
        public int BitLength { get { return BitIndex + WordIndex * BUFFER_BIT_SIZE; } }
        public int ByteLength { get { return (int)Math.Ceiling(BitLength / (decimal)8); } }
        #endregion

        #region Constructeur
        public BitPacker(int pByteBufferSize)
        {
            _buffer = new uint[pByteBufferSize / 4];
            _temp = 0;
            _offsetBitReaded = 0;
            BitIndex = 0;
            WordIndex = 0;
        }

        public BitPacker(List<uint> pBuffer, int pBitLength)
        {
            _buffer = pBuffer.ToArray();
            _temp = 0;
            _offsetBitReaded = 0;
            BitIndex = pBitLength % BUFFER_BIT_SIZE;
            WordIndex = pBitLength / BUFFER_BIT_SIZE;
        }

        public BitPacker(uint[] pBuffer, int pBitLength)
        {
            _buffer = pBuffer;
            _temp = 0;
            _offsetBitReaded = 0;
            BitIndex = pBitLength % BUFFER_BIT_SIZE;
            WordIndex = pBitLength / BUFFER_BIT_SIZE;
        }
        #endregion

        #region Remplissage buffer
        public static BitPacker FromArray(byte[] pBuffer, bool pPushTempInBuffer = true)
        {
            BitPacker result = new BitPacker();
            int nbOfBytePerBufferElement = BUFFER_BIT_SIZE / 8;
            int length = (int)Math.Ceiling((double)pBuffer.Length / nbOfBytePerBufferElement);
            result._buffer = new uint[length];
            result.WordIndex = (int)Math.Floor((double)pBuffer.Length / nbOfBytePerBufferElement);
            result.BitIndex = pBuffer.Length * 8 % BUFFER_BIT_SIZE;
            for (int i = 0; i < length; i++)
            {
                bool shouldBeAdded = true;
                uint val = 0;
                for (int j = 0; j < nbOfBytePerBufferElement; j++)
                {
                    int indexBuffer = i * nbOfBytePerBufferElement + j;
                    if (indexBuffer >= pBuffer.Length)
                    {
                        shouldBeAdded = false;
                        result._temp = val;
                        break;
                    }
                    byte b = pBuffer[indexBuffer];
                    val |= (uint)b << j * 8;
                }
                if (shouldBeAdded)
                    result._buffer[i] = val;
            }
            if (pPushTempInBuffer)
                result.PushTempInBuffer();
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
            while (BitIndex > BUFFER_BIT_SIZE)
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
            // Si la valeur à écrire se chevauche sur deux registres.
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

        public void AlignToNextByte()
        {
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
        #endregion

        private void ValidateNbOfBits(int pNbOfBits)
        {
            if (pNbOfBits < 1 || pNbOfBits > 32)
                throw new Exception("Impossible to write a 32 bit type on more than 32 bits.");
        }

        #region Vidage buffer
        public uint ReadValue(int pNbOfBits, bool pRemoveBits = true, int pStartPosition = 0)
        {
            ValidateNbOfBits(pNbOfBits);

            int bufferWordIndex = pStartPosition / BUFFER_BIT_SIZE;
            int start = (pStartPosition + _offsetBitReaded) % BUFFER_BIT_SIZE;
            Span<uint> spanBuffer = GetSpanBuffer();
            if ((bufferWordIndex + pNbOfBits / BUFFER_BIT_SIZE) < spanBuffer.Length)
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
                    // Si la valeur à lire se chevauche sur deux registres.
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

        public void Clear()
        {
            if (_buffer != null)
                GetSpanBuffer().Clear();
            else
                _buffer = new uint[DEFAULT_BUFFER_SIZE];
            _temp = 0;
            BitIndex = 0;
            WordIndex = 0;
        }
        #endregion

        #region Transformations
        public byte[] GetByteBuffer()
        {
            int counter = 0;
            int nbOfBytePerBufferElement = BUFFER_BIT_SIZE / 8;
            int lengthInByte = (int)Math.Ceiling((double)BitLength / 8);
            byte[] result = new byte[lengthInByte];

            Span<uint> span = GetSpanBuffer();

            for (int i = 0; i < span.Length; i++)
            {
                uint currentInt = span[i];
                for (int j = 0; j < nbOfBytePerBufferElement; j++)
                {
                    int byteIndex = i * nbOfBytePerBufferElement + j;
                    if (byteIndex >= lengthInByte)
                        break;
                    result[byteIndex] = (byte)((currentInt & (0xFF << j * 8)) >> j * 8);
                    counter++;
                }
            }
            for (int i = 0; i < 8; i++)
            {
                if (counter + i >= result.Length)
                    break;
                result[counter + i] = (byte)((_temp & (ulong)(0xFF << i * 8)) >> i * 8);
            }
            return result;
        }

        public Span<uint> GetSpanBuffer()
        {
            int start = _offsetBitReaded / BUFFER_BIT_SIZE;
            return new Span<uint>(_buffer, start, (int)Math.Ceiling((decimal)BitLength / BUFFER_BIT_SIZE)); // - start);
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
        #endregion
    }
}