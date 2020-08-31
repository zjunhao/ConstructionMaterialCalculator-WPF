using System;
using System.Collections.Generic;
using System.Text;

namespace DecorationMaterialCalculator.Services
{
    public static class StringParserService
    {
        /// <summary>
        /// Parse type for InputItem
        /// </summary>
        /// <param name="typeStr">"300*3600"</param>
        /// <param name="a">300</param>
        /// <param name="b">3600</param>
        public static void ParseType(string typeStr, out int a, out int b)
        {
            String pattern = @"^\s*(\d+)\s*[*]\s*(\d+)\s*$";
            var match = System.Text.RegularExpressions.Regex.Matches(typeStr, pattern)[0];
            a = Int32.Parse(match.Groups[1].Value);
            b = Int32.Parse(match.Groups[2].Value);
        }

        public static double ParseUnitLength(string unitLength)
        {
            return double.Parse(unitLength);
        }

        public static double ParsePrice(string priceStr)
        {
            return double.Parse(priceStr);
        }

        public static double StringToDouble(string str)
        {
            return double.Parse(str);
        }

        public static int StringToInt(string str)
        {
            return int.Parse(str);
        }
    }
}
