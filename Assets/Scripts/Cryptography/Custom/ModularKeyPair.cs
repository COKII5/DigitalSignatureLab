using System.Numerics;

namespace DigitalSignatureLab.Cryptography.Custom
{
    public class ModularKeyPair
    {
        public ModularKeyPair(ModularPublicKey publicKey, BigInteger[] privateExponents)
        {
            PublicKey = publicKey;
            PrivateExponents = privateExponents;
        }

        public ModularPublicKey PublicKey { get; }

        public BigInteger[] PrivateExponents { get; }
    }
}
