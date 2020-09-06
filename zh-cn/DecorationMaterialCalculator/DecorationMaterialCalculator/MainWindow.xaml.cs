using System.Collections.Generic;
using System.Configuration;
using System.Windows;
using ComputerIDGenerator;
using DecorationMaterialCalculator.Models;
using DecorationMaterialCalculator.Pages;
using DecorationMaterialCalculator.Services;
using static DecorationMaterialCalculator.Services.IOService;

namespace DecorationMaterialCalculator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Verify licensing valid
            string licensePath = AppConfigReadingService.GetConfigSetting("metadataFileDirectory") + "\\Licensing.txt";
            string savedLicense = IOService.ReadTextFileToString(licensePath);
            string validLicense = FingerPrint.Value();

            if (savedLicense == validLicense)
            {
                StartPage startPage = new StartPage();
                mainFrame.NavigationService.Navigate(startPage);
            }
            else
            {
                string sMessageBoxText = "程序启动失败，序列号错误";
                string sCaption = "程序启动失败";
                MessageBoxButton btnMessageBox = MessageBoxButton.OK;
                MessageBoxImage icnMessageBox = MessageBoxImage.Error;
                MessageBoxResult rsltMessageBox = MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);

                switch (rsltMessageBox)
                {
                    case MessageBoxResult.OK:
                        Application.Current.Shutdown();
                        return;
                }
            }


            //Testing();
            //Testing2();
        }

        private void Testing()
        {
            List<InputItem> inputItemList = new List<InputItem>();

            inputItemList.Add(new InputItem("s-802锦绣纹", "b", "300*3600", "2+4", "顶", "东南卧室", "0.9", "东西铺贴"));
            inputItemList.Add(new InputItem("s-802锦绣纹", "b", "300*3600", "5.9+4.7", "顶", "南西卧室", "3.6", "南北铺贴"));
            inputItemList.Add(new InputItem("s-603金橡木", "x", "100*3000", "5.9+4.7+5.9+4.7", "踢脚线", "南西卧室"));
            inputItemList.Add(new InputItem("s-603金橡木", "x", "100*3000", "5.8", "踢脚线", "北卧室"));

            List<SummedItem> summedItemList = CalculationService.MergeInputItem(inputItemList);
        }

        private void Testing2()
        {
            List<InputItem> inputItemList = new List<InputItem>();
            inputItemList.Add(new InputItem("s-802锦绣纹", "b", "300*3600", "2+4", "顶", "东南卧室", "0.9", "东西铺贴"));
            inputItemList.Add(new InputItem("s-802锦绣纹", "b", "400*3600", "5.9+4.7", "顶", "南西卧室", "3.6", "南北铺贴"));
            inputItemList.Add(new InputItem("s-803锦绣纹", "b", "300*3600", "5.9+4.7", "顶", "南西卧室", "3.6", "南北铺贴"));
            inputItemList.Add(new InputItem("s-802锦绣纹", "b", "300*3600", "5.9+4.7", "顶", "南西卧室", "3.6", "南北铺贴"));
            inputItemList.Add(new InputItem("s-802锦绣纹", "b", "300*800", "5.9+4.7", "顶", "南西卧室", "0.2", "南北铺贴"));
            inputItemList.Add(new InputItem("s-603金橡木", "x", "100*3000", "5.9+4.7+5.9+4.7", "踢脚线", "南西卧室"));
            inputItemList.Add(new InputItem("s-603金橡木", "x", "100*3000", "5.8", "踢脚线", "北卧室"));
            inputItemList.Add(new InputItem("s-802锦绣纹", "b", "400*800", "5.9+4.7", "顶", "南西卧室", "0.2", "南北铺贴"));


            List<SummedItem> summedItemList = CalculationService.MergeInputItem(inputItemList);

            foreach (SummedItem item in summedItemList)
            {
                item.BuyPrice = "15";
                item.SellPrice = "20";
                double buyPrice = StringParserService.ParsePrice(item.BuyPrice);
                double sellPrice = StringParserService.ParsePrice(item.SellPrice);

                //TODO: figure out why 15 * 46.66 gives you 656.5999999991 only here but not in csharpplayground
                item.TotalBuyPrice = CalculationService.CalculateTotalPricesByAreaOrLength(buyPrice, item.TotalAreaOrLength);
                item.TotalSellPrice = CalculationService.CalculateTotalPricesByAreaOrLength(sellPrice, item.TotalAreaOrLength);
            }

            BuckleItem buckleItem = new BuckleItem();
            buckleItem.Quantity = CalculationService.CalculateBuckleItemQuantity(summedItemList);
            buckleItem.BuyPrice = "1";
            buckleItem.SellPrice = "1.5";
            double buckleBuyPrice = StringParserService.ParsePrice(buckleItem.BuyPrice);
            double buckleSellPrice = StringParserService.ParsePrice(buckleItem.SellPrice);
            buckleItem.TotalBuyPrice = CalculationService.CalculateTotalPricesByQuantity(buckleBuyPrice, buckleItem.Quantity);
            buckleItem.TotalSellPrice = CalculationService.CalculateTotalPricesByQuantity(buckleSellPrice, buckleItem.Quantity);

            IOService.GenerateInputCheckingSheet(@"C:\Users\JZhang\Desktop\输入校对表.xlsx", inputItemList);
            IOService.GenerateConstructionDetailSheet(@"C:\Users\JZhang\Desktop\施工详单.xlsx", inputItemList);
            IOService.GenerateMaterialTotalSheet(@"C:\Users\JZhang\Desktop\材料清单1.xlsx", summedItemList, buckleItem, true);
            IOService.GenerateMaterialTotalSheet(@"C:\Users\JZhang\Desktop\材料清单2.xlsx", summedItemList, buckleItem, false);

            List<InputItem> readInput = IOService.ReadInputCheckingSheetToInputItemList(@"C:\Users\JZhang\Desktop\输入校对表.xlsx");
        }
    }
}
