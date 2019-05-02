using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace KittyCoins.Packages
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

            if (number < 1)
                return hex.DivideHex(1 / number);

            var bHex = BigInteger.Parse("0" + hex, NumberStyles.AllowHexSpecifier);
            var result = BigInteger.Multiply(bHex, (int)(number * 1000)).ToString("X").DivideHex(1000);
            return (new string('0', hex.Length) + result).Substring(result.Length);
        }

        public static string MultiplyHex(this string hex, int number)
        {
            if (number < 0)
                return hex;

            var bHex = BigInteger.Parse("0" + hex, NumberStyles.AllowHexSpecifier);
            var result = BigInteger.Multiply(bHex, number).ToString("X");
            return (new string('0', hex.Length) + result).Substring(result.Length);
        }

        private static string DivideHex(this string hex, double number)
        {
            var bHex = BigInteger.Parse("0" + hex, NumberStyles.AllowHexSpecifier);
            var result = BigInteger.Divide(bHex, (int)(number * 1000)).ToString("X").DivideHex(1000);
            return (new string('0', hex.Length) + result).Substring(result.Length);
        }

        private static string DivideHex(this string hex, int number)
        {
            var bHex = BigInteger.Parse("0" + hex, NumberStyles.AllowHexSpecifier);
            var result = BigInteger.Divide(bHex, number).ToString("X");
            return (new string('0', hex.Length) + result).Substring(result.Length);
        }
    }
}
