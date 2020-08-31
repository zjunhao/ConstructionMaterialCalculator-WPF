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
                newProjectErrMsgTextBlock.Text = "Project name cannot be blank";
            }
            else if(IOService.FileAlreadyExists(saveDirectoryPrefix + "-InputCheckingSheet" + AppConfigReadingService.GetSpreadSheetExtention()))
            {
                newProjectErrMsgTextBlock.Visibility = Visibility.Visible;
                newProjectErrMsgTextBlock.Text = "Project name already exists";
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
                List<InputItem> inputItems = IOService.ReadInputCheckingSheetToInputItemList(saveDirectoryPrefix + "-InputCheckingSheet" + AppConfigReadingService.GetSpreadSheetExtention());
                ItemInputPage itemInputPage = new ItemInputPage(inputItems, saveDirectoryPrefix);
                this.NavigationService.Navigate(itemInputPage);
            }
            catch (Exception ex)
            {
                loadProjectErrMsgTextBlock.Visibility = Visibility.Visible;
                loadProjectErrMsgTextBlock.Text = "Project name does not exist or the form is not closed causing loading to fail";
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
