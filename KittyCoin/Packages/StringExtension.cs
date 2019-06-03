using System.Globalization;
using System.Numerics;

namespace KittyCoin.Packages
{
    public static class StringExtension
    {
        public static bool IsLowerHex(this string a, string b)
        {
            var bA = BigInteger.Parse("0" + a, NumberStyles.AllowHexSpecifier);
            var bB = BigInteger.Parse("0" + b, NumberStyles.AllowHexSpecifier);
            return BigInteger.Compare(bA, bB) == -1;
        }
        public static bool IsGreaterHex(this string a, string b)
        {
            var bA = BigInteger.Parse("0" + a, NumberStyles.AllowHexSpecifier);
            var bB = BigInteger.Parse("0" + b, NumberStyles.AllowHexSpecifier);
            return BigInteger.Compare(bA, bB) == 1;
        }
        public static bool IsEqualHex(this string a, string b)
        {
            var bA = BigInteger.Parse("0" + a, NumberStyles.AllowHexSpecifier);
            var bB = BigInteger.Parse("0" + b, NumberStyles.AllowHexSpecifier);
            return BigInteger.Compare(bA, bB) == 0;
        }

        public static string RemoveHex(this string hex, int number)
        {
            var bHex = BigInteger.Parse("0" + hex, NumberStyles.AllowHexSpecifier);
            var result = BigInteger.Subtract(bHex, number).ToString("X");
            return (new string('0', hex.Length) + result).Substring(result.Length);
        }

        public static string MultiplyHex(this string hex, double number)
        {
            if (number < 0)
                return hex;

            var bHex = BigInteger.Parse("00000" + hex, NumberStyles.AllowHexSpecifier);
            var result = BigInteger.Divide(BigInteger.Multiply(bHex, (int)(number * 1000)), 1000).ToString("X");
            return (new string('0', hex.Length) + result).Substring(result.Length);
        }
    }
}
