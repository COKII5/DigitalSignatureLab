using DigitalSignatureLab.Cryptography.Abstractions;

namespace DigitalSignatureLab.Cryptography.Custom
{
    public class CustomHashAdapter : IHashAlgorithm
    {
        private readonly RoundMixHash roundMixHash;

        public CustomHashAdapter(RoundMixHash roundMixHash)
        {
            this.roundMixHash = roundMixHash;
        }

        public byte[] ComputeHash(byte[] data)
        {
            return roundMixHash.ComputeHash(data);
        }
    }
}
