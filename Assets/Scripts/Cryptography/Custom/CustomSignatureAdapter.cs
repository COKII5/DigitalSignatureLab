using System.Linq;
using DigitalSignatureLab.Cryptography.Abstractions;

namespace DigitalSignatureLab.Cryptography.Custom
{
    public class CustomSignatureAdapter : ISignatureAlgorithm
    {
        private readonly ChainedModularSigner signer;

        private ModularKeyPair keyPair;

        public CustomSignatureAdapter(ChainedModularSigner signer)
        {
            this.signer = signer;
        }

        public string ExportPublicKey()
        {
            return GetKeyPair().PublicKey.ToText();
        }

        public byte[] SignHash(byte[] hash)
        {
            return signer.Sign(hash, GetKeyPair());
        }

        public bool VerifyHash(byte[] hash, byte[] signature, string publicKey)
        {
            byte[] recoveredHash = signer.RecoverHash(signature, ModularPublicKey.Parse(publicKey));
            return recoveredHash.SequenceEqual(hash);
        }

        private ModularKeyPair GetKeyPair()
        {
            if (keyPair == null)
            {
                keyPair = signer.GenerateKeyPair();
            }

            return keyPair;
        }
    }
}
