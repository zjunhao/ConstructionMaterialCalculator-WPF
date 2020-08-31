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
}
