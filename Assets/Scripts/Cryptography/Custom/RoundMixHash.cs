using System;

namespace DigitalSignatureLab.Cryptography.Custom
{
    public class RoundMixHash
    {
        private const int StateWords = 8;
        private const int BytesPerWord = 4;
        private const int BitsPerByte = 8;
        private const int BitsPerWord = 32;
        private const int BlockBytes = StateWords * BytesPerWord;
        private const int LengthFieldBytes = 8;
        private const int RoundCount = 12;
        private const int NeighbourDistance = 1;
        private const int DistantWordDistance = 3;
        private const int FoldShiftBits = 15;
        private const byte PaddingMarker = 0x80;
        private const uint MixMultiplier = 0xA3B1C2D5;
        private const uint RoundConstantStep = 0x6C8E9CF5;

        private static readonly uint[] InitialState =
        {
            0x5A17C3E9, 0x9B2D4F61, 0x13E8A7C5, 0xC64B9D2F,
            0x7F3A1E8B, 0x2E9C6D47, 0xB81F5A3D, 0x46D0E972
        };

        private static readonly int[] RotationOffsets = { 5, 11, 17, 23, 7, 13, 19, 29 };

        private static readonly int[] WordPermutation = { 3, 6, 1, 4, 7, 2, 5, 0 };

        public byte[] ComputeHash(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            uint[] state = (uint[])InitialState.Clone();
            byte[] paddedData = Pad(data);
            for (int offset = 0; offset < paddedData.Length; offset += BlockBytes)
            {
                CompressBlock(state, ReadBlockWords(paddedData, offset));
            }

            return WriteStateBytes(state);
        }

        private static byte[] Pad(byte[] data)
        {
            int minimumLength = data.Length + 1 + LengthFieldBytes;
            int paddedLength = (minimumLength + BlockBytes - 1) / BlockBytes * BlockBytes;
            byte[] paddedData = new byte[paddedLength];
            Array.Copy(data, paddedData, data.Length);
            paddedData[data.Length] = PaddingMarker;

            ulong bitLength = (ulong)data.Length * BitsPerByte;
            for (int index = 0; index < LengthFieldBytes; index++)
            {
                paddedData[paddedLength - 1 - index] = (byte)(bitLength >> (BitsPerByte * index));
            }

            return paddedData;
        }

        private static uint[] ReadBlockWords(byte[] paddedData, int offset)
        {
            uint[] blockWords = new uint[StateWords];
            for (int wordIndex = 0; wordIndex < StateWords; wordIndex++)
            {
                uint word = 0;
                for (int byteIndex = 0; byteIndex < BytesPerWord; byteIndex++)
                {
                    word = (word << BitsPerByte) | paddedData[offset + wordIndex * BytesPerWord + byteIndex];
                }

                blockWords[wordIndex] = word;
            }

            return blockWords;
        }

        private static void CompressBlock(uint[] state, uint[] blockWords)
        {
            uint[] chainingValue = (uint[])state.Clone();
            for (int round = 0; round < RoundCount; round++)
            {
                SubstituteWords(state, blockWords, round);
                DiffuseWords(state);
                PermuteWords(state);
            }

            for (int index = 0; index < StateWords; index++)
            {
                state[index] += chainingValue[index] ^ blockWords[index];
            }
        }

        private static void SubstituteWords(uint[] state, uint[] blockWords, int round)
        {
            uint roundConstant = RoundConstantStep * (uint)(round + 1);
            for (int index = 0; index < StateWords; index++)
            {
                uint mixedWord = (state[index] ^ blockWords[(index + round) % StateWords]) * MixMultiplier;
                state[index] = (mixedWord ^ (mixedWord >> FoldShiftBits)) + roundConstant;
            }
        }

        private static void DiffuseWords(uint[] state)
        {
            for (int index = 0; index < StateWords; index++)
            {
                uint neighbourWord = state[(index + NeighbourDistance) % StateWords];
                uint distantWord = state[(index + DistantWordDistance) % StateWords];
                state[index] += RotateLeft(neighbourWord, RotationOffsets[index]) ^ distantWord;
            }
        }

        private static void PermuteWords(uint[] state)
        {
            uint[] previousOrder = (uint[])state.Clone();
            for (int index = 0; index < StateWords; index++)
            {
                state[index] = previousOrder[WordPermutation[index]];
            }
        }

        private static uint RotateLeft(uint value, int bits)
        {
            return (value << bits) | (value >> (BitsPerWord - bits));
        }

        private static byte[] WriteStateBytes(uint[] state)
        {
            byte[] digest = new byte[BlockBytes];
            for (int wordIndex = 0; wordIndex < StateWords; wordIndex++)
            {
                for (int byteIndex = 0; byteIndex < BytesPerWord; byteIndex++)
                {
                    int shift = BitsPerByte * (BytesPerWord - 1 - byteIndex);
                    digest[wordIndex * BytesPerWord + byteIndex] = (byte)(state[wordIndex] >> shift);
                }
            }

            return digest;
        }
    }
}
