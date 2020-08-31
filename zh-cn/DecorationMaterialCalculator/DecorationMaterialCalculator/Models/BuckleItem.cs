using DecorationMaterialCalculator.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace DecorationMaterialCalculator.Models
{
    /// <summary>
    /// The way buckle is calculated is different than the other items.
    /// So create a new class just for buckle to address buckle
    /// </summary>
    public class BuckleItem: INotifyPropertyChanged
    {
        public BuckleItem()
        {

        }

        public string ProductName 
        { 
            get
            {
                return "卡扣";
            }
        }
        public int Quantity { get; set; }
        public string BuyPrice { get; set; }
        public string SellPrice { get; set; }

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
