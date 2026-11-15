using System;
using System.Security.Cryptography;
using DigitalSignatureLab.Cryptography.Abstractions;

namespace DigitalSignatureLab.Cryptography.Library
{
    public class RsaSignatureAdapter : ISignatureAlgorithm
    {
        private const int KeySizeInBits = 2048;

        private RSA signingKey;

        public string ExportPublicKey()
        {
            return GetSigningKey().ToXmlString(false);
        }

        public byte[] SignHash(byte[] hash)
        {
            return GetSigningKey().SignHash(hash, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }

        public bool VerifyHash(byte[] hash, byte[] signature, string publicKey)
        {
            using (RSA verificationKey = LoadPublicKey(publicKey))
            {
                try
                {
                    return verificationKey.VerifyHash(hash, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                }
                catch (CryptographicException)
                {
                    return false;
                }
            }
        }

        private RSA GetSigningKey()
        {
            if (signingKey == null)
            {
                signingKey = RSA.Create();
                signingKey.KeySize = KeySizeInBits;
            }

            return signingKey;
        }

        private static RSA LoadPublicKey(string publicKey)
        {
            RSA verificationKey = RSA.Create();
            try
            {
                verificationKey.FromXmlString(publicKey);
                return verificationKey;
            }
            catch (CryptographicException exception)
            {
                verificationKey.Dispose();
                throw new FormatException("The RSA public key is not valid.", exception);
            }
        }
    }
}
