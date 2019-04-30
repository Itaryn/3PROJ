using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KittyCoins.Packages
{
    public static class StringExtension
    {
        public static bool IsLowerHex(this string a, string b)
        {
            return a.CompareHex(b) == "<";
        }
        public static bool IsGreaterHex(this string a, string b)
        {
            return a.CompareHex(b) == ">";
        }
        public static bool IsEqualHex(this string a, string b)
        {
            return a.CompareHex(b) == "=";
        }

        public static string CompareHex(this string a, string b)
        {
            var i = 0;

            if (a.Length < b.Length)
                a = (new string('0', b.Length) + a).Substring(a.Length);
            else if (a.Length > b.Length)
                b = (new string('0', a.Length) + b).Substring(b.Length);

            while (a.Length >= i + 8)
            {
                var partA = Convert.ToInt64(a.Substring(i, i + 8), 16);
                var partB = Convert.ToInt64(b.Substring(i, i + 8), 16);

                if (partA < partB)
                    return "<";
                if (partA > partB)
                    return ">";

                i += 8;
            }

            var lastA = Convert.ToInt64(a.Substring(i), 16);
            var lastB = Convert.ToInt64(b.Substring(i), 16);

            if (lastA < lastB)
                return "<";
            if (lastA > lastB)
                return ">";

            return "=";
        }

        public static string RemoveHex(this string hex, int number)
        {
            var i = hex.Length;
            var reste = 0;
            while (i - 8 >= 0)
            {
                var partA = Convert.ToInt64(hex.Substring(i - 8, 8), 16);

                if (partA > 0)
                {
                    var modifPart = (partA - 1).ToString("X");
                    var modifPartHex = (new string('0', 8) + modifPart).Substring(modifPart.Length);
                    return hex.Substring(0, i - 8) + modifPartHex + new string('F', reste);
                }

                reste += 8;

                i -= 8;
            }

            var lastA = Convert.ToInt64(hex.Substring(0, i), 16);

            if (lastA > 0)
            {
                var modifPart = (lastA - 1).ToString("X");
                var modifPartHex = (new string('0', i) + modifPart).Substring(modifPart.Length);
                return modifPartHex + new string('F', reste);
            }

            return hex;
        }
    }
}
