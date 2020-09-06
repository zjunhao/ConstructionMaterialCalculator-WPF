using DecorationMaterialCalculator.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace DecorationMaterialCalculator.Models
{
    public class SummedItem: INotifyPropertyChanged
    {
        public SummedItem(string productName, string type, string borx)
        {
            this.ProductName = productName;
            this.Type = type;
            this.BorX = borx;
        }

        public SummedItem DeepCopy()
        {
            SummedItem clone = (SummedItem) this.MemberwiseClone();
            clone.ProductName = ProductName;
            clone.Type = Type;
            clone.BorX = BorX;
            clone.BuyPrice = BuyPrice;
            clone.SellPrice = SellPrice;
            clone.Quantity = Quantity;
            clone.Memo = Memo;
            clone.TotalAreaOrLength = TotalAreaOrLength;
            clone.TotalBuyPrice = TotalBuyPrice;
            clone.TotalSellPrice = TotalSellPrice;
            clone.PriceInputErrorMessage = PriceInputErrorMessage;

            return clone;
        }

        // Input properties
        public string ProductName { get; set; }
        public string Type { get; set; }
        public string BorX { get; set; }
        public string BuyPrice { get; set; }
        public string SellPrice { get; set; }

        // Calculated properties
        public int Quantity { get; set; }
        public string Memo { get; set; }

        private double _totalAreaOrLength;
        public double TotalAreaOrLength
        {
            get { return _totalAreaOrLength; }
            set
            {
                switch (BorX)
                {
                    case "b":
                        _totalAreaOrLength = CalculationService.GetDecimalPrecision(value) <= 3 ? value : Math.Round(value, 3);
                        break;
                    case "x":
                        _totalAreaOrLength = CalculationService.GetDecimalPrecision(value) <= 3 ? value : Math.Round(value, 3);
                        break;
                    default:
                        throw new Exception("BorX not assigned for SummedItem");
                }

            }
        }

        private double _totalBuyPrice;
        public double TotalBuyPrice
        {
            get { return _totalBuyPrice; }
            set
            {
                _totalBuyPrice = CalculationService.GetDecimalPrecision(value) <= 2 ? value : Math.Round(value, 2);
            }
        }

        private double _totalSellPrice;
        public double TotalSellPrice
        {
            get { return _totalSellPrice; }
            set
            {
                _totalSellPrice = CalculationService.GetDecimalPrecision(value) <= 2 ? value : Math.Round(value, 2); ;
            }
        }

        // Property used for binding in xaml to show error message
        private string _priceInputErrorMessage = "";
        public string PriceInputErrorMessage 
        { 
            get
            {
                return _priceInputErrorMessage;
            }
            set
            {
                _priceInputErrorMessage = value;
                NotifyPropertyChanged();
            }
        }

        // Implementation of INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }

    // Compare summedItem based on product name and type
    // name a*b (s-802 300*3600); compare using name first, then a, then b
    public class SummedItemComparer: IComparer<SummedItem>
    {
        public int Compare(SummedItem item1, SummedItem item2)
        {
            if (!item1.ProductName.Equals(item2.ProductName))
            {
                return item1.ProductName.CompareTo(item2.ProductName);
            } 
            else
            {
                // item1 type: a1 * b1; item2 type: a2 * b2
                StringParserService.ParseType(item1.Type, out int a1, out int b1);
                StringParserService.ParseType(item2.Type, out int a2, out int b2);

                return (a1 != a2) ? (a1 - a2) : (b1 - b2);
            }
        }
    }
}
