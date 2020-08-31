using System;
using System.Text.RegularExpressions;

namespace DecorationMaterialCalculator.Services
{
    public static class InputVerifyService
    {
        /// <summary>
        /// Valid type "300*3600".
        /// </summary>
        public static bool IsTypeValid(string text)
        {
            Regex regex = new Regex(@"^\d+\*\d+$");
            return regex.IsMatch(text);
        }

        /// <summary>
        /// Valid size "2.59+3.69+2.59+3.69-1.2".
        /// "*", "/", " " not allowed
        /// </summary>
        public static bool IsSizeValid(string text)
        {
            Regex regex = new Regex(@"^-?(\d+(\.\d+)?[-\+])*\d+(\.\d+)?$");
            return regex.IsMatch(text);
        }
        
        /// <summary>
        /// Valid UnitLength "3.6", "1.2".
        /// </summary>
        public static bool IsUnitLengthValid(string text)
        {
            Regex regex = new Regex(@"^\d+(\.\d+)?$");
            return regex.IsMatch(text);
        }

        /// <summary>
        /// Valid price "0", "100 ", " 1.1", " 1.12 ".
        /// </summary>
        public static bool IsPriceValid(string priceStr)
        {
            Regex regex = new Regex(@"^\s*\d+(\.\d{1,2})?\s*$"); ;
            return regex.IsMatch(priceStr);
        }
    }
}
