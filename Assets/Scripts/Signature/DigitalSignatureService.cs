using System;
using System.Text;
using DigitalSignatureLab.Cryptography.Abstractions;

namespace DigitalSignatureLab.Signature
{
    public class DigitalSignatureService
    {
        private readonly IHashAlgorithm hashAlgorithm;
        private readonly ISignatureAlgorithm signatureAlgorithm;

        public DigitalSignatureService(CryptoProvider provider, IHashAlgorithm hashAlgorithm, ISignatureAlgorithm signatureAlgorithm)
        {
            Provider = provider;
            this.hashAlgorithm = hashAlgorithm;
            this.signatureAlgorithm = signatureAlgorithm;
        }

        public CryptoProvider Provider { get; }

        public SignedMessage Sign(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException("The message cannot be empty.", nameof(message));
            }

            byte[] signature = signatureAlgorithm.SignHash(ComputeHash(message));
            return new SignedMessage
            {
                message = message,
                signatureBase64 = Convert.ToBase64String(signature),
                publicKey = signatureAlgorithm.ExportPublicKey(),
                provider = Provider.ToString()
            };
        }

        public VerificationResult Verify(SignedMessage signedMessage)
        {
            byte[] hash = ComputeHash(signedMessage.message);
            byte[] signature = Convert.FromBase64String(signedMessage.signatureBase64);
            bool isValid = signatureAlgorithm.VerifyHash(hash, signature, signedMessage.publicKey);
            return new VerificationResult(isValid, ToHex(hash));
        }

        public string ComputeHashHex(string message)
        {
            return ToHex(ComputeHash(message));
        }

        private byte[] ComputeHash(string message)
        {
            return hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(message));
        }

        private static string ToHex(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", string.Empty);
        }
    }
}
