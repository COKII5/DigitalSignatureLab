using System;
using DigitalSignatureLab.Cryptography.Custom;
using DigitalSignatureLab.Cryptography.Library;

namespace DigitalSignatureLab.Signature
{
    public static class SignatureServiceFactory
    {
        public static DigitalSignatureService Create(CryptoProvider provider)
        {
            switch (provider)
            {
                case CryptoProvider.Library:
                    return new DigitalSignatureService(provider, new Sha256HashAdapter(), new RsaSignatureAdapter());
                case CryptoProvider.Custom:
                    ChainedModularSigner signer = new ChainedModularSigner(new PrimeGenerator(new Random()));
                    return new DigitalSignatureService(provider, new CustomHashAdapter(new RoundMixHash()), new CustomSignatureAdapter(signer));
                default:
                    throw new ArgumentOutOfRangeException(nameof(provider));
            }
        }
    }
}
