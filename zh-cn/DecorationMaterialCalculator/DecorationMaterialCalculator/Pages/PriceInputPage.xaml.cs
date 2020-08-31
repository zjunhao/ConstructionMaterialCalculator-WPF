using DecorationMaterialCalculator.Models;
using DecorationMaterialCalculator.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace DecorationMaterialCalculator.Pages
{
    /// <summary>
    /// Interaction logic for PriceInputPage.xaml
    /// </summary>
    public partial class PriceInputPage : Page
    {
        public List<SummedItem> summedItemList { get; set; }
        public List<InputItem> inputItemList { get; set; }
        public BuckleItem buckleItem { get; set; }
        public readonly bool needBuckle;
        public readonly string saveDirectoryPrefix;

        public PriceInputPage(List<SummedItem> summedItemList, List<InputItem> inputItemList, string saveDirectoryPrefix)
        {
            InitializeComponent();

            this.summedItemList = summedItemList;
            this.inputItemList = inputItemList;
            this.saveDirectoryPrefix = saveDirectoryPrefix;
            this.needBuckle = NeedBuckleItem(this.summedItemList);

            if (this.needBuckle)
            {
                buckleItem = new BuckleItem();
                buckleItem.Quantity = CalculationService.CalculateBuckleItemQuantity(summedItemList);
                buckleSection.Visibility = Visibility.Visible;
            }
            else
            {
                buckleItem = null;
                buckleSection.Visibility = Visibility.Collapsed;
            }

            DataContext = this;
        }

        private void GoToProgramEndPageButton_Click(object sender, RoutedEventArgs e)
        {
            // Verify price valid and not null
            if (!VerifyPriceFields()) return;

            // Calculate total prices for general item
            foreach (SummedItem item in summedItemList)
            {
                double buyPrice = StringParserService.ParsePrice(item.BuyPrice);
                double sellPrice = StringParserService.ParsePrice(item.SellPrice);

                item.TotalBuyPrice = CalculationService.CalculateTotalPricesByAreaOrLength(buyPrice, item.TotalAreaOrLength);
                item.TotalSellPrice = CalculationService.CalculateTotalPricesByAreaOrLength(sellPrice, item.TotalAreaOrLength);
            }

            // Calculate total prices for buckle if we need buckle
            if (needBuckle)
            {
                double buckleBuyPrice = StringParserService.ParsePrice(buckleItem.BuyPrice);
                double buckleSellPrice = StringParserService.ParsePrice(buckleItem.SellPrice);
                buckleItem.TotalBuyPrice = CalculationService.CalculateTotalPricesByQuantity(buckleBuyPrice, buckleItem.Quantity);
                buckleItem.TotalSellPrice = CalculationService.CalculateTotalPricesByQuantity(buckleSellPrice, buckleItem.Quantity);
            }

            // Generate Construction Detail Sheet, Material Total Sheet (listing buy price), and Material Total Sheet (listing sell price)
            try
            {
                IOService.GenerateConstructionDetailSheet(saveDirectoryPrefix + "-施工详单" + AppConfigReadingService.GetSpreadSheetExtention(), inputItemList);
                IOService.GenerateMaterialTotalSheet(saveDirectoryPrefix + "-材料清单（商家用）" + AppConfigReadingService.GetSpreadSheetExtention(), summedItemList, buckleItem, true);
                IOService.GenerateMaterialTotalSheet(saveDirectoryPrefix + "-材料清单（客户用）" + AppConfigReadingService.GetSpreadSheetExtention(), summedItemList, buckleItem, false);

                ProgramEndPage programEndPage = new ProgramEndPage();
                this.NavigationService.Navigate(programEndPage);
            }
            catch (IOException ioex)
            {
                string sMessageBoxText = "报表生成失败，请确认您已关闭所有报表";
                string sCaption = "报表生成失败";
                MessageBoxButton btnMessageBox = MessageBoxButton.OK;
                MessageBoxImage icnMessageBox = MessageBoxImage.Error;
                MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);
            }
            catch (Exception ex)
            {
                string sMessageBoxText = "因未知原因报表生成失败，请联系软件商";
                string sCaption = "报表生成失败";
                MessageBoxButton btnMessageBox = MessageBoxButton.OK;
                MessageBoxImage icnMessageBox = MessageBoxImage.Error;
                MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);
            }
        }

        private bool VerifyPriceFields()
        {
            bool allPricesFieldsValid = true;

            foreach(SummedItem item in summedItemList)
            {
                if (String.IsNullOrEmpty(item.BuyPrice) || String.IsNullOrEmpty(item.SellPrice))
                {
                    item.PriceInputErrorMessage = "价格不能为空";
                    allPricesFieldsValid = false;
                }
                else if (!InputVerifyService.IsPriceValid(item.BuyPrice) || !InputVerifyService.IsPriceValid(item.SellPrice))
                {
                    item.PriceInputErrorMessage = "价格格式有误";
                    allPricesFieldsValid = false;
                }
                else
                {
                    item.PriceInputErrorMessage = "";
                }
            }

            if (buckleItem != null)
            {
                if (String.IsNullOrEmpty(buckleItem.BuyPrice) || String.IsNullOrEmpty(buckleItem.SellPrice))
                {
                    buckleItem.PriceInputErrorMessage = "价格不能为空";
                    allPricesFieldsValid = false;
                }
                else if (!InputVerifyService.IsPriceValid(buckleItem.BuyPrice) || !InputVerifyService.IsPriceValid(buckleItem.SellPrice))
                {
                    buckleItem.PriceInputErrorMessage = "价格格式有误";
                    allPricesFieldsValid = false;
                }
                else
                {
                    buckleItem.PriceInputErrorMessage = "";
                }
            }

            return allPricesFieldsValid;
        }

        /// <summary>
        /// If user ever input any material of "b", which means board, we then need buckle;
        /// If user does not have any board, we do not need a buckle.
        /// </summary>
        private bool NeedBuckleItem(List<SummedItem> items)
        {
            foreach(SummedItem item in items)
            {
                if (item.BorX == "b")
                    return true;
            }

            return false;
        }
    }
}
