using System;

namespace DigitalSignatureLab.Signature
{
    [Serializable]
    public class SignedMessage
    {
        public string message;
        public string signatureBase64;
        public string publicKey;
        public string provider;

        public bool HasRequiredFields()
        {
            return !string.IsNullOrEmpty(message)
                && !string.IsNullOrEmpty(signatureBase64)
                && !string.IsNullOrEmpty(publicKey)
                && !string.IsNullOrEmpty(provider);
        }
    }
}
