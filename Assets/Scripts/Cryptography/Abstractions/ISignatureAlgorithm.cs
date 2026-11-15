namespace DigitalSignatureLab.Cryptography.Abstractions
{
    public interface ISignatureAlgorithm
    {
        string ExportPublicKey();

        byte[] SignHash(byte[] hash);

        bool VerifyHash(byte[] hash, byte[] signature, string publicKey);
    }
}
