using System;
using System.Numerics;

namespace DigitalSignatureLab.Cryptography.Custom
{
    public class PrimeGenerator
    {
        private const int MillerRabinRounds = 24;
        private const int BitsPerByte = 8;
        private const byte HighestBitMask = 0x80;
        private const byte LowestBitMask = 0x01;
        private const int SmallestWitness = 2;

        private static readonly int[] SmallPrimes = { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71 };

        private readonly Random random;

        public PrimeGenerator(Random random)
        {
            this.random = random;
        }

        public BigInteger GeneratePrime(int bitLength)
        {
            while (true)
            {
                BigInteger candidate = GenerateOddCandidate(bitLength);
                if (IsProbablePrime(candidate))
                {
                    return candidate;
                }
            }
        }

        private BigInteger GenerateOddCandidate(int bitLength)
        {
            int byteLength = bitLength / BitsPerByte;
            byte[] candidateBytes = new byte[byteLength + 1];
            random.NextBytes(candidateBytes);
            candidateBytes[byteLength] = 0;
            candidateBytes[byteLength - 1] |= HighestBitMask;
            candidateBytes[0] |= LowestBitMask;
            return new BigInteger(candidateBytes);
        }

        private bool IsProbablePrime(BigInteger candidate)
        {
            foreach (int smallPrime in SmallPrimes)
            {
                if (candidate % smallPrime == 0)
                {
                    return candidate == smallPrime;
                }
            }

            BigInteger oddFactor = candidate - 1;
            int twoExponent = 0;
            while (oddFactor.IsEven)
            {
                oddFactor >>= 1;
                twoExponent++;
            }

            for (int round = 0; round < MillerRabinRounds; round++)
            {
                if (!PassesMillerRabinRound(GenerateWitness(candidate), oddFactor, twoExponent, candidate))
                {
                    return false;
                }
            }

            return true;
        }

        private BigInteger GenerateWitness(BigInteger candidate)
        {
            byte[] witnessBytes = candidate.ToByteArray();
            random.NextBytes(witnessBytes);
            witnessBytes[witnessBytes.Length - 1] = 0;
            BigInteger witnessRange = candidate - 2 * SmallestWitness + 1;
            return new BigInteger(witnessBytes) % witnessRange + SmallestWitness;
        }

        private static bool PassesMillerRabinRound(BigInteger witness, BigInteger oddFactor, int twoExponent, BigInteger candidate)
        {
            BigInteger candidateMinusOne = candidate - 1;
            BigInteger value = ModularArithmetic.Power(witness, oddFactor, candidate);
            if (value == 1 || value == candidateMinusOne)
            {
                return true;
            }

            for (int squaring = 1; squaring < twoExponent; squaring++)
            {
                value = value * value % candidate;
                if (value == candidateMinusOne)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
