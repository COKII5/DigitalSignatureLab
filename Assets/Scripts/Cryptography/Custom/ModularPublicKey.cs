using System;
using System.Globalization;
using System.Linq;
using System.Numerics;

namespace DigitalSignatureLab.Cryptography.Custom
{
    public class ModularPublicKey
    {
        private const char PartSeparator = ':';
        private const char ExponentSeparator = ',';
        private const int ExpectedPartCount = 2;

        public ModularPublicKey(BigInteger modulus, BigInteger[] exponents)
        {
            Modulus = modulus;
            Exponents = exponents;
        }

        public BigInteger Modulus { get; }

        public BigInteger[] Exponents { get; }

        public string ToText()
        {
            string exponentsText = string.Join(ExponentSeparator.ToString(), Exponents.Select(exponent => exponent.ToString(CultureInfo.InvariantCulture)));
            return Modulus.ToString(CultureInfo.InvariantCulture) + PartSeparator + exponentsText;
        }

        public static ModularPublicKey Parse(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new FormatException("The public key is empty.");
            }

            string[] parts = text.Split(PartSeparator);
            if (parts.Length != ExpectedPartCount)
            {
                throw new FormatException("The public key must contain a modulus and its exponents.");
            }

            BigInteger modulus = ParsePositiveNumber(parts[0]);
            BigInteger[] exponents = parts[1].Split(ExponentSeparator).Select(ParsePositiveNumber).ToArray();
            return new ModularPublicKey(modulus, exponents);
        }

        private static BigInteger ParsePositiveNumber(string text)
        {
            if (!BigInteger.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out BigInteger number) || number <= 1)
            {
                throw new FormatException("The public key contains an invalid number.");
            }

            return number;
        }
    }
}
