using System;
using System.Linq;
using System.Numerics;

namespace DigitalSignatureLab.Cryptography.Custom
{
    public class ChainedModularSigner
    {
        private const int PrimeBitLength = 256;
        private const int HashBlockBytes = 8;
        private const int BitsPerByte = 8;
        private const int ByteMask = 0xFF;

        private static readonly BigInteger[] PublicExponents = { 65537, 65539, 65543, 65551 };
        private static readonly BigInteger BlockMask = (BigInteger.One << (HashBlockBytes * BitsPerByte)) - 1;

        private readonly PrimeGenerator primeGenerator;

        public ChainedModularSigner(PrimeGenerator primeGenerator)
        {
            this.primeGenerator = primeGenerator;
        }

        public ModularKeyPair GenerateKeyPair()
        {
            while (true)
            {
                BigInteger firstPrime = primeGenerator.GeneratePrime(PrimeBitLength);
                BigInteger secondPrime = primeGenerator.GeneratePrime(PrimeBitLength);
                BigInteger totient = (firstPrime - 1) * (secondPrime - 1);
                if (firstPrime != secondPrime && PublicExponents.All(exponent => ModularArithmetic.AreCoprime(exponent, totient)))
                {
                    BigInteger[] privateExponents = PublicExponents.Select(exponent => ModularArithmetic.Inverse(exponent, totient)).ToArray();
                    ModularPublicKey publicKey = new ModularPublicKey(firstPrime * secondPrime, PublicExponents);
                    return new ModularKeyPair(publicKey, privateExponents);
                }
            }
        }

        public byte[] Sign(byte[] hash, ModularKeyPair keyPair)
        {
            BigInteger modulus = keyPair.PublicKey.Modulus;
            int blockCount = keyPair.PrivateExponents.Length;
            if (hash == null || hash.Length != blockCount * HashBlockBytes)
            {
                throw new ArgumentException("The hash length does not match the key.", nameof(hash));
            }

            int signatureBlockBytes = GetSignatureBlockBytes(modulus);
            byte[] signature = new byte[blockCount * signatureBlockBytes];
            BigInteger chainValue = modulus & BlockMask;
            for (int blockIndex = 0; blockIndex < blockCount; blockIndex++)
            {
                BigInteger hashBlock = ReadUnsigned(hash, blockIndex * HashBlockBytes, HashBlockBytes);
                BigInteger signatureBlock = ModularArithmetic.Power(hashBlock ^ chainValue, keyPair.PrivateExponents[blockIndex], modulus);
                WriteUnsigned(signatureBlock, signature, blockIndex * signatureBlockBytes, signatureBlockBytes);
                chainValue = signatureBlock & BlockMask;
            }

            return signature;
        }

        public byte[] RecoverHash(byte[] signature, ModularPublicKey publicKey)
        {
            BigInteger modulus = publicKey.Modulus;
            int blockCount = publicKey.Exponents.Length;
            int signatureBlockBytes = GetSignatureBlockBytes(modulus);
            if (signature == null || signature.Length != blockCount * signatureBlockBytes)
            {
                throw new FormatException("The signature length does not match the public key.");
            }

            byte[] recoveredHash = new byte[blockCount * HashBlockBytes];
            BigInteger chainValue = modulus & BlockMask;
            for (int blockIndex = 0; blockIndex < blockCount; blockIndex++)
            {
                BigInteger signatureBlock = ReadUnsigned(signature, blockIndex * signatureBlockBytes, signatureBlockBytes);
                BigInteger chainedBlock = ModularArithmetic.Power(signatureBlock, publicKey.Exponents[blockIndex], modulus);
                WriteUnsigned((chainedBlock ^ chainValue) & BlockMask, recoveredHash, blockIndex * HashBlockBytes, HashBlockBytes);
                chainValue = signatureBlock & BlockMask;
            }

            return recoveredHash;
        }

        private static int GetSignatureBlockBytes(BigInteger modulus)
        {
            byte[] modulusBytes = modulus.ToByteArray();
            bool hasSignByte = modulusBytes[modulusBytes.Length - 1] == 0;
            return hasSignByte ? modulusBytes.Length - 1 : modulusBytes.Length;
        }

        private static BigInteger ReadUnsigned(byte[] source, int offset, int length)
        {
            BigInteger value = BigInteger.Zero;
            for (int index = 0; index < length; index++)
            {
                value = (value << BitsPerByte) | source[offset + index];
            }

            return value;
        }

        private static void WriteUnsigned(BigInteger value, byte[] target, int offset, int length)
        {
            BigInteger remainingValue = value;
            for (int index = length - 1; index >= 0; index--)
            {
                target[offset + index] = (byte)(remainingValue & ByteMask);
                remainingValue >>= BitsPerByte;
            }
        }
    }
}
