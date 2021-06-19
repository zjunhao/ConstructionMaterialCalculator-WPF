using DecorationMaterialCalculator.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DecorationMaterialCalculator.Models
{
    /// <summary>
    /// SummedItems with same ProductName and width of Type represented as 1 PriceCollector.
    /// Model used for collect prices in price input page,
    /// then populate prices back to SummedItems.
    /// Since materials with same ProductName and width of Type are always the same price,
    /// this mechanism helps in the way that user only need to input price once for all materials with same ProductName and width of Type.
    /// </summary>
    public class PriceCollector: INotifyPropertyChanged
    {
        public PriceCollector(string productName, string widthOfType)
        {
            this.ProductName = productName;
            this.WidthOfType = widthOfType;
            this.ID = productName + "_" + widthOfType;
        }

        public string ID { get; }

        public string ProductName { get; set; }

        /// <summary>
        /// For Type a*b, this is the width value, which is a.
        /// e.g., for S-802 300*3600, WidthOfType here should be 300
        /// </summary>
        public string WidthOfType { get; set; }

        public string BuyPrice { get; set; }

        public string SellPrice { get; set; }

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

    public class PriceCollectorComparer : IComparer<PriceCollector>
    {
        public int Compare(PriceCollector pc1, PriceCollector pc2)
        {
            if (!pc1.ProductName.Equals(pc2.ProductName))
            {
                return pc1.ProductName.CompareTo(pc2.ProductName);
            }
            else
            {
                return Int32.Parse(pc2.WidthOfType) - Int32.Parse(pc1.WidthOfType);
            }

        }
    }
}
