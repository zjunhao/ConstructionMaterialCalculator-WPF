using DecorationMaterialCalculator.Models;
using DecorationMaterialCalculator.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Windows;
using System.Windows.Controls;

namespace DecorationMaterialCalculator.Pages
{
    /// <summary>
    /// Interaction logic for StartPage.xaml
    /// </summary>
    public partial class StartPage : Page
    {
        public StartPage()
        {
            InitializeComponent();
        }

        private void NewProjectButton_Click(object sender, RoutedEventArgs e)
        {
            ClearErrorMessage();

            // projectName:         Remington Apartment
            string projectName = newProjectNameTextBox.Text;
            // saveDirectoryPrefix: C:\Users\JZhang\Desktop\表格\Remington Apartment
            // full file path:      C:\Users\JZhang\Desktop\表格\Remington Apartment-xxxx表.xlsx
            string saveDirectoryPrefix = AppConfigReadingService.GetConfigSetting("formSaveDirectory") + "\\" + projectName;

            if (projectName == string.Empty || projectName == null)
            {
                newProjectErrMsgTextBlock.Visibility = Visibility.Visible;
                newProjectErrMsgTextBlock.Text = "工程名不能为空";
            }
            else if(IOService.FileAlreadyExists(saveDirectoryPrefix + "-输入校对表" + AppConfigReadingService.GetSpreadSheetExtention()))
            {
                newProjectErrMsgTextBlock.Visibility = Visibility.Visible;
                newProjectErrMsgTextBlock.Text = "工程名已存在";
            }
            else
            {
                List<InputItem> inputItems = new List<InputItem>();

                ItemInputPage itemInputPage = new ItemInputPage(inputItems, saveDirectoryPrefix);
                this.NavigationService.Navigate(itemInputPage);
            }
        }

        private void LoadProjectButton_Click(object sender, RoutedEventArgs e)
        {
            ClearErrorMessage();

            // projectName:         Remington Drive
            string projectName = loadProjectNameTextBox.Text;
            // saveDirectoryPrefix: C:\Users\JZhang\Desktop\表格\Remington Drive
            // full file path:      C:\Users\JZhang\Desktop\表格\Remington Drive-xxxx表.xlsx
            string saveDirectoryPrefix = AppConfigReadingService.GetConfigSetting("formSaveDirectory") + "\\" + projectName;

            try
            {
                List<InputItem> inputItems = IOService.ReadInputCheckingSheetToInputItemList(saveDirectoryPrefix + "-输入校对表" + AppConfigReadingService.GetSpreadSheetExtention());
                ItemInputPage itemInputPage = new ItemInputPage(inputItems, saveDirectoryPrefix);
                this.NavigationService.Navigate(itemInputPage);
            }
            catch (Exception ex)
            {
                loadProjectErrMsgTextBlock.Visibility = Visibility.Visible;
                loadProjectErrMsgTextBlock.Text = "工程名不存在或输入校对表未关闭导致读取失败";
            }
        }

        private void ClearErrorMessage()
        {
            newProjectErrMsgTextBlock.Text = "";
            newProjectErrMsgTextBlock.Visibility = Visibility.Visible;
            loadProjectErrMsgTextBlock.Text = "";
            loadProjectErrMsgTextBlock.Visibility = Visibility.Hidden;
        }
    }
}
