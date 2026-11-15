using System;
using System.Numerics;

namespace DigitalSignatureLab.Cryptography.Custom
{
    public static class ModularArithmetic
    {
        public static BigInteger Power(BigInteger baseValue, BigInteger exponent, BigInteger modulus)
        {
            BigInteger result = BigInteger.One;
            BigInteger currentBase = baseValue % modulus;
            BigInteger remainingExponent = exponent;
            while (remainingExponent > 0)
            {
                if (!remainingExponent.IsEven)
                {
                    result = result * currentBase % modulus;
                }

                currentBase = currentBase * currentBase % modulus;
                remainingExponent >>= 1;
            }

            return result;
        }

        public static bool AreCoprime(BigInteger firstValue, BigInteger secondValue)
        {
            BigInteger dividend = firstValue;
            BigInteger divisor = secondValue;
            while (divisor != 0)
            {
                BigInteger remainder = dividend % divisor;
                dividend = divisor;
                divisor = remainder;
            }

            return dividend == 1;
        }

        public static BigInteger Inverse(BigInteger value, BigInteger modulus)
        {
            BigInteger previousRemainder = modulus;
            BigInteger remainder = value % modulus;
            BigInteger previousCoefficient = 0;
            BigInteger coefficient = 1;
            while (remainder != 0)
            {
                BigInteger quotient = previousRemainder / remainder;
                (previousRemainder, remainder) = (remainder, previousRemainder - quotient * remainder);
                (previousCoefficient, coefficient) = (coefficient, previousCoefficient - quotient * coefficient);
            }

            if (previousRemainder != 1)
            {
                throw new ArgumentException("The value has no modular inverse.", nameof(value));
            }

            return (previousCoefficient % modulus + modulus) % modulus;
        }
    }
}
