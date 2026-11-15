namespace DigitalSignatureLab.Cryptography.Abstractions
{
    public interface IHashAlgorithm
    {
        byte[] ComputeHash(byte[] data);
    }
}
