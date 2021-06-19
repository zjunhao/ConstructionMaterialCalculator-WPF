using DecorationMaterialCalculator.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DecorationMaterialCalculator.Services
{
    public static class CalculationService
    {
        #region Public Methods

        public static List<SummedItem> MergeInputItem(List<InputItem> inputItemList)
        {
            List<SummedItem> summedItemList = new List<SummedItem>();
            Dictionary<string, List<InputItem>> itemDict = new Dictionary<string, List<InputItem>>();

            // Get dict of "s-802 300*3600 x" -> List<InputItem> that has same product name and type
            foreach (InputItem item in inputItemList)
            {
                string inputItemKey = item.ProductName + " " + item.Type + " " + item.BorX;
                if (itemDict.ContainsKey(inputItemKey)) 
                {
                    itemDict[inputItemKey].Add(item);
                }
                else
                {
                    List<InputItem> similarInputItemList = new List<InputItem>();
                    similarInputItemList.Add(item);
                    itemDict.Add(inputItemKey, similarInputItemList);
                }
            }

            // Loop through dictionary, for each key value pair, merge the List<InputItem> into one SummedItem
            foreach (KeyValuePair<string, List<InputItem>> entry in itemDict)
            {
                SummedItem summedItem = new SummedItem(entry.Value[0].ProductName, entry.Value[0].Type, entry.Value[0].BorX);
                int quantity = 0;
                StringBuilder memosb = new StringBuilder();

                // Sum up quantity and memo (Southeast bedroom top 19 + kitchen top 16 ...)
                foreach(InputItem item in entry.Value)
                {
                    quantity += item.Quantity;
                    memosb.Append(item.Room + item.Position + item.Quantity + "+");
                }
                memosb.Remove(memosb.Length-1, 1);

                summedItem.Quantity = quantity;
                summedItem.Memo = memosb.ToString();

                // Calculate TotalArea (for b) or TotalLength (for x)
                StringParserService.ParseType(summedItem.Type, out int a, out int b);
                switch(summedItem.BorX)
                {
                    case "b":
                        CalculateTotalAreaForB(a, b, summedItem.Quantity, out double totalArea);
                        summedItem.TotalAreaOrLength = totalArea;
                        break;
                    case "x":
                        CalculateTotalLengthForX(a, b, summedItem.Quantity, out double totalLength);
                        summedItem.TotalAreaOrLength = totalLength;
                        break;
                    default:
                        throw new Exception("B or X not assigned to summedItem");
                }

                summedItemList.Add(summedItem);
            }

            return summedItemList;
        }

        public static double CalculateTotalPricesByAreaOrLength(double price, double totalAreaOrLength)
        {
            return price * totalAreaOrLength;
        }

        public static double CalculateTotalPricesByQuantity(double price, int quantity)
        {
            return price * quantity;
        }

        public static double CalculateSummedUpSize(string expression)
        {
            expression = expression.Trim();
            if (expression.Length < 1) throw new Exception("Expression is empty");
            if (expression[0] == '-') expression = "0" + expression;  //-1.2 get changed to 0-1.2 so algorithm below still work

            StringBuilder digitReaderSB = new StringBuilder();
            char sign = '+';
            double lastNumber;
            double res = 0;

            foreach (char c in expression.ToCharArray())
            {
                if (c != '+' && c != '-')
                {
                    digitReaderSB.Append(c);
                }
                else
                {
                    lastNumber = double.Parse(digitReaderSB.ToString());
                    digitReaderSB.Clear();
                    AddOrSubtract(sign, lastNumber, ref res);
                    sign = c;
                }
            }

            AddOrSubtract(sign, double.Parse(digitReaderSB.ToString()), ref res);

            return res;
        }

        public static int CalculateInputItemQuantity(InputItem inputItem)
        {
            switch (inputItem.BorX)
            {
                case "b":
                    return CalculateQuantityForB(inputItem);
                case "x":
                    return CalculateQuantityForX(inputItem);
                default:
                    throw new Exception("B or X not selected");
            }
        }

        public static int CalculateBuckleItemQuantity(List<SummedItem> items)
        {
            int res = 0;
            foreach(SummedItem item in items)
            {
                if (item.BorX == "b")
                {
                    StringParserService.ParseType(item.Type, out int a, out int b);
                    res += (int) Math.Ceiling(1.0 * b * item.Quantity / 600);
                }
            }
            return res;
        }

        public static int GetDecimalPrecision(double value)
        {
            string strnum = value.ToString();
            if (!strnum.Contains(".")) 
                return 0;
            else 
                return strnum.Split('.')[1].Length;
        }

        /// <summary>
        /// Use PriceCollector to collect price for SummedItems with same ProductName and width of Type,
        /// then populate collected prices back to SummedItems
        /// </summary>
        public static List<PriceCollector> SummedItemsToPriceCollectors(List<SummedItem> summedItems)
        {
            List<PriceCollector> priceCollectors = new List<PriceCollector>();
            HashSet<string> collectorIDs = new HashSet<string>();

            foreach(SummedItem summedItem in summedItems)
            {
                StringParserService.ParseType(summedItem.Type, out int widthOfType, out int lengthOfType);
                string id = summedItem.ProductName + "_" + widthOfType;
                if (!collectorIDs.Contains(id))
                {
                    PriceCollector collector = new PriceCollector(summedItem.ProductName, widthOfType.ToString());
                    priceCollectors.Add(collector);
                    collectorIDs.Add(id);
                }
            }

            return priceCollectors;
        }

        public static void PopulateSummedItemsPriceFromPriceCollectors(List<PriceCollector> priceCollectors, ref List<SummedItem> summedItems)
        {
            Dictionary<string, string[]> pricesDict = new Dictionary<string, string[]>();
            foreach(PriceCollector priceCollector in priceCollectors)
            {
                pricesDict.Add(priceCollector.ID, new string[] { priceCollector.BuyPrice, priceCollector.SellPrice });
            }

            foreach(SummedItem summedItem in summedItems)
            {
                StringParserService.ParseType(summedItem.Type, out int widthOfType, out int lengthOfType);
                string itemID = summedItem.ProductName + "_" + widthOfType;
                
                if (!pricesDict.ContainsKey(itemID))
                {
                    throw new Exception("PriceCollectors does not cover all SummedItems");
                }

                summedItem.BuyPrice = pricesDict[itemID][0];
                summedItem.SellPrice = pricesDict[itemID][1];
            }
        }

        #endregion

        #region Private Methods
        private static int CalculateQuantityForX(InputItem item)
        {
            double size = item.SummedUpSize;   // size in meter
            StringParserService.ParseType(item.Type, out int a, out int b);  // a, b in millimeter
            double bInMeter = b / 1000.0;   

            return (int) Math.Ceiling(size / bInMeter);
        }

        private static int CalculateQuantityForB(InputItem item)
        {
            double size = item.SummedUpSize;   // size in meter
            double unitLength = StringParserService.ParseUnitLength(item.UnitLength);  // unitLength in meter
            StringParserService.ParseType(item.Type, out int a, out int b);    // a, b in millimeter
            double aInMeter = a / 1000.0, bInMeter = b / 1000.0;

            return (int) Math.Ceiling(size / aInMeter / Math.Floor(bInMeter / unitLength));
        }

        private static void CalculateTotalLengthForX(int a, int b, int quantity, out double totalLength)
        {
            totalLength = b * quantity / 1000.0;
        }

        private static void CalculateTotalAreaForB(int a, int b, int quantity, out double totalArea)
        {
            totalArea = a * b * quantity / 1000000.0;
        }

        private static void AddOrSubtract(char sign, double b, ref double a)
        {
            switch (sign)
            {
                case '+':
                    a += b;
                    break;
                case '-':
                    a -= b;
                    break;
                default:
                    throw new Exception("Size expression contains sign other than + and -");
            }
        }

        #endregion
    }
}
