using System;
using System.Collections.Generic;

namespace CommunicationProtocol
{
    public struct BitPacking
    {
        #region Constantes
        private const int BUFFER_BIT_SIZE = 32;     // Nombre de bits dépendant du type de données de la liste _buffer (List<uint> => 32 bits)
        #endregion

        #region Variables privées
        private List<uint> _buffer;
        private ulong _temp;
        #endregion

        #region Propriétés
        public int BitIndex { get; private set; }
        public int WordIndex { get; private set; }
        public int Length { get { return BitIndex + WordIndex * BUFFER_BIT_SIZE; } }
        #endregion

        #region Constructeur
        public BitPacking(List<uint> pBuffer, int pBitLength)
        {
            _buffer = pBuffer;
            _temp = 0;
            BitIndex = pBitLength % BUFFER_BIT_SIZE;
            WordIndex = pBitLength / BUFFER_BIT_SIZE;
        }

        public BitPacking(uint[] pBuffer, int pBitLength)
        {
            _buffer = new List<uint>();
            _buffer.AddRange(pBuffer);
            _temp = 0;
            BitIndex = pBitLength % BUFFER_BIT_SIZE;
            WordIndex = pBitLength / BUFFER_BIT_SIZE;
        }
        #endregion

        #region Remplissage buffer
        public static BitPacking FromArray(byte[] pBuffer, bool pPushTempInBuffer = true)
        {
            BitPacking result = new BitPacking();
            result._buffer = new List<uint>();
            int nbOfBytePerBufferElement = BUFFER_BIT_SIZE / 8;
            int length = (int)Math.Ceiling((double)pBuffer.Length / nbOfBytePerBufferElement);
            result.WordIndex = (int)Math.Floor((double)pBuffer.Length / nbOfBytePerBufferElement);
            result.BitIndex = (pBuffer.Length * 8) % BUFFER_BIT_SIZE;
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
                    result._buffer.Add(val);
            }
            if (pPushTempInBuffer)
                result.PushTempInBuffer();
            return result;
        }

        public void WriteValue(uint pValue, uint pNbOfBits)
        {
            if (_buffer == null)
                _buffer = new List<uint>();
            if (pNbOfBits > 32)
                throw new Exception("Impossible to write a 32 bit type on more than 32 bits.");
            _temp |= (ulong)(pValue & (uint.MaxValue >> (32 - (int)pNbOfBits))) << BitIndex;
            BitIndex += (int)pNbOfBits;
            while (BitIndex > BUFFER_BIT_SIZE)
            {
                PushTempInBuffer();
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
                    _buffer.Add((uint)_temp);
                    if (BitIndex < BUFFER_BIT_SIZE)
                    {
                        _temp >>= BitIndex;
                        //BitIndex += 8 - BitIndex;
                    }
                    else
                    {
                        _temp >>= BUFFER_BIT_SIZE;
                        BitIndex -= BUFFER_BIT_SIZE;
                        WordIndex++;
                    }
                }
            }
        }
        #endregion

        #region Vidage buffer
        public uint ReadValue(int pNbOfBits, bool pRemoveBits = true, int pStartPosition = 0)
        {
            int bufferIndex = pStartPosition / BUFFER_BIT_SIZE;
            pStartPosition %= BUFFER_BIT_SIZE;
            if ((bufferIndex + pNbOfBits / BUFFER_BIT_SIZE) < _buffer.Count)
            {
                if (pNbOfBits <= BUFFER_BIT_SIZE)
                {
                    uint startingVal = 0;
                    if (pStartPosition > 0)
                    {
                        startingVal = _buffer[bufferIndex] & (uint.MaxValue >> (32 - pStartPosition));
                    }
                    ulong result = (_buffer[bufferIndex] & (uint.MaxValue >> (32 - pNbOfBits)) << pStartPosition) >> pStartPosition;
                    // Si la valeur a lire se chevauche sur deux registres.
                    if (pStartPosition > 0 && bufferIndex + 1 < _buffer.Count)
                    {
                        result |= (_buffer[bufferIndex + 1] & (uint.MaxValue >> (32 - pNbOfBits + pStartPosition))) << pStartPosition;
                    }
                    if (pRemoveBits)
                    {
                        uint outVal = 0;
                        for (int i = _buffer.Count - 1; i >= bufferIndex; i--)
                        {
                            uint temp = _buffer[i] & (uint.MaxValue >> (32 - pNbOfBits));
                            _buffer[i] >>= pNbOfBits;
                            _buffer[i] |= outVal << (32 - pNbOfBits);
                            outVal = temp;
                        }
                        _buffer[bufferIndex] &= (uint.MaxValue << pStartPosition);
                        _buffer[bufferIndex] |= startingVal;
                        BitIndex -= pNbOfBits;
                        while (BitIndex <= 0)
                        {
                            WordIndex--;
                            _buffer.RemoveAt(_buffer.Count - 1);
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
                _buffer.Clear();
            else
                _buffer = new List<uint>();
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
            int lengthInByte = (int)Math.Ceiling((double)Length / 8);
            byte[] result = new byte[lengthInByte];
            
            for (int i = 0; i < _buffer.Count; i++)
            {
                uint currentInt = _buffer[i];
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

        public uint[] GetUIntBuffer()
        {
            return _buffer.ToArray();
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