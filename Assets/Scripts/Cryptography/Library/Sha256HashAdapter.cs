using System.Security.Cryptography;
using DigitalSignatureLab.Cryptography.Abstractions;

namespace DigitalSignatureLab.Cryptography.Library
{
    public class Sha256HashAdapter : IHashAlgorithm
    {
        public byte[] ComputeHash(byte[] data)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(data);
            }
        }
    }
}
