namespace DigitalSignatureLab.Signature
{
    public class VerificationResult
    {
        public VerificationResult(bool isValid, string computedHashHex)
        {
            IsValid = isValid;
            ComputedHashHex = computedHashHex;
        }

        public bool IsValid { get; }

        public string ComputedHashHex { get; }
    }
}
