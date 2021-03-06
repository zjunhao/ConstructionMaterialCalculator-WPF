﻿using DecorationMaterialCalculator.Models;
using DecorationMaterialCalculator.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;


namespace DecorationMaterialCalculator.Pages
{
    /// <summary>
    /// Interaction logic for ItemInputPage.xaml
    /// </summary>
    public partial class ItemInputPage : Page
    {
        public List<string> productNameList { get; set; }
        public List<string> positionList { get; set; }
        public List<string> roomList { get; set; }
        public List<string> typeList { get; set; }
        public List<string> applydirectionList { get; set; }

        public ObservableCollection<InputItem> inputItemOC { get; set; }

        public readonly string saveDirectoryPrefix;

        public ItemInputPage(List<InputItem> inputItems, string saveDirectoryPrefix)
        {
            InitializeComponent();

            LoadListsFromTextFile();

            PopulateComboBoxes();

            this.saveDirectoryPrefix = saveDirectoryPrefix;
            this.inputItemOC = new ObservableCollection<InputItem>(inputItems);

            DataContext = this;
        }

        private void LoadListsFromTextFile()
        {
            string resourcePath = AppConfigReadingService.GetConfigSetting("metadataFileDirectory");

            productNameList = IOService.ReadTextFileToList(resourcePath + "\\产品名称.txt");
            positionList = IOService.ReadTextFileToList(resourcePath + "\\位置.txt");
            roomList = IOService.ReadTextFileToList(resourcePath + "\\房间.txt");
            applydirectionList = IOService.ReadTextFileToList(resourcePath + "\\铺贴方向.txt");
        }

        private void PopulateComboBoxes()
        {
            productNameComboBox.ItemsSource = productNameList;
            positionComboBox.ItemsSource = positionList;
            roomComboBox.ItemsSource = roomList;
            applyDirectionComboBox.ItemsSource = applydirectionList;
        }

        private void GoToPriceInputPageButton_Click(object sender, RoutedEventArgs e)
        {
            // Warn user if any field on left column is not clear but filled with value
            if (!AllInputFieldsClear())
            {
                string sMessageBoxText = "左侧输入栏仍有值未保存，选择继续将丢失这些值，确定继续？";
                string sCaption = "输入栏有未保存值";
                MessageBoxButton btnMessageBox = MessageBoxButton.YesNo;
                MessageBoxImage icnMessageBox = MessageBoxImage.Warning;
                MessageBoxResult rsltMessageBox = MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);

                switch (rsltMessageBox)
                {
                    case MessageBoxResult.Yes:
                        break;

                    case MessageBoxResult.No:
                        return;                        
                }
            }

            List<InputItem> inputItemList = inputItemOC.ToList();
            List<SummedItem> summedItemList = CalculationService.MergeInputItem(inputItemList);

            try
            {
                IOService.GenerateInputCheckingSheet(saveDirectoryPrefix + "-输入校对表" + AppConfigReadingService.GetSpreadSheetExtention(), inputItemList);

                PriceInputPage priceInputPage = new PriceInputPage(summedItemList, inputItemList, saveDirectoryPrefix);
                this.NavigationService.Navigate(priceInputPage);
            }
            catch (IOException ioex)
            {
                string sMessageBoxText = "校对表生成失败，无法进入价格输入页面，请确认您已关闭所有报表";
                string sCaption = "无法进入价格输入页面";
                MessageBoxButton btnMessageBox = MessageBoxButton.OK;
                MessageBoxImage icnMessageBox = MessageBoxImage.Error;
                MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);
            }
            catch (Exception ex)
            {
                string sMessageBoxText = "因未知原因校对表生成失败，请联系软件商";
                string sCaption = "校对表生成失败";
                MessageBoxButton btnMessageBox = MessageBoxButton.OK;
                MessageBoxImage icnMessageBox = MessageBoxImage.Error;
                MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);
            }
        }

        private void SaveAndCloseButton_Click(object sender, RoutedEventArgs e)
        {
            // Warn user if any field on left column is not clear but filled with value
            if (!AllInputFieldsClear())
            {
                string sMessageBoxText = "左侧输入栏仍有值未保存，选择继续将丢失这些值，确定继续？";
                string sCaption = "输入栏有未保存值";
                MessageBoxButton btnMessageBox = MessageBoxButton.YesNo;
                MessageBoxImage icnMessageBox = MessageBoxImage.Warning;
                MessageBoxResult rsltMessageBox = MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);

                switch (rsltMessageBox)
                {
                    case MessageBoxResult.Yes:
                        break;

                    case MessageBoxResult.No:
                        return;
                }
            }

            try
            {
                IOService.GenerateInputCheckingSheet(saveDirectoryPrefix + "-输入校对表" + AppConfigReadingService.GetSpreadSheetExtention(), inputItemOC.ToList());
                
                Application.Current.Shutdown();
            }
            catch (IOException ioex)
            {
                string sMessageBoxText = "进度保存失败，请确认您已关闭所有报表";
                string sCaption = "进度保存失败";
                MessageBoxButton btnMessageBox = MessageBoxButton.OK;
                MessageBoxImage icnMessageBox = MessageBoxImage.Error;
                MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);
            }
            catch (Exception ex)
            {
                string sMessageBoxText = "因未知原因进度保存失败，请联系软件商";
                string sCaption = "进度保存失败";
                MessageBoxButton btnMessageBox = MessageBoxButton.OK;
                MessageBoxImage icnMessageBox = MessageBoxImage.Error;
                MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!VerifyInputFields()) return;

            string room = roomComboBox.SelectedItem as string;
            string position = positionComboBox.SelectedItem as string;
            string borx = bRadioButton.IsChecked == true ? "b" : "x";
            string productName = productNameComboBox.SelectedItem as string;
            string type = typeTextBox.Text;
            string size = sizeTextBox.Text;
            string unitLength = unitLengthTextBox.Visibility == Visibility.Visible ? unitLengthTextBox.Text : null;
            string applydirection = applyDirectionTextBlock.Visibility == Visibility.Visible ? applyDirectionComboBox.SelectedItem as string : null;

            InputItem newInputItem = new InputItem(productName, borx, type, size, position, room, unitLength, applydirection);
            inputItemOC.Add(newInputItem);

            ClearInputFields();
        }

        private void DeleteInputItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (inputItemListView.SelectedItems.Count > 0)
            {
                // Cast to array to avoid selectedItems list change while looping through it
                var selectedItems = inputItemListView.SelectedItems.Cast<Object>().ToArray();
                foreach (var selectedItem in selectedItems)
                {
                    inputItemOC.Remove((InputItem)selectedItem);
                }
            }
        }

        private void PositionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedPosition = (sender as ComboBox).SelectedItem as string;
            if (selectedPosition == "顶")
            {
                applyDirectionTextBlock.Visibility = Visibility.Visible;
                applyDirectionComboBox.Visibility = Visibility.Visible;
            }
            else
            {
                applyDirectionTextBlock.Visibility = Visibility.Hidden;
                applyDirectionComboBox.Visibility = Visibility.Hidden;
                applyDirectionComboBox.SelectedItem = null;

                applyDirectionErrMsgTextblock.Visibility = Visibility.Hidden;
                applyDirectionErrMsgTextblock.Text = "";
            }
        }

        private void BRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            unitLengthTextBlock.Visibility = Visibility.Visible;
            unitLengthTextBox.Visibility = Visibility.Visible;
        }

        private void XRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            unitLengthTextBlock.Visibility = Visibility.Hidden;
            unitLengthTextBox.Visibility = Visibility.Hidden;
            unitLengthTextBox.Text = null;

            unitLengthErrMsgTextblock.Visibility = Visibility.Hidden;
            unitLengthErrMsgTextblock.Text = "";
        }

        private void ClearInputFields()
        {
            roomComboBox.SelectedItem = null;
            positionComboBox.SelectedItem = null;
            bRadioButton.IsChecked = false;
            xRadioButton.IsChecked = false;
            productNameComboBox.SelectedItem = null;
            typeTextBox.Text = null;
            sizeTextBox.Text = null;
            unitLengthTextBlock.Visibility = Visibility.Hidden;
            unitLengthTextBox.Visibility = Visibility.Hidden;
            unitLengthTextBox.Text = null;
            applyDirectionTextBlock.Visibility = Visibility.Hidden;
            applyDirectionComboBox.Visibility = Visibility.Hidden;
            applyDirectionComboBox.SelectedItem = null;
        }

        /// <summary>
        /// Verify input fields and give notification if input fields not valid
        /// </summary>
        /// <returns>Return true if all fields are valid, false otherwise</returns>
        private bool VerifyInputFields()
        {
            bool inputFieldsValid = true;

            if (roomComboBox.SelectedItem == null)
            {
                roomErrMsgTextblock.Visibility = Visibility.Visible;
                roomErrMsgTextblock.Text = "房间不能为空";
                inputFieldsValid = false;
            }
            else
            {
                roomErrMsgTextblock.Visibility = Visibility.Hidden;
                roomErrMsgTextblock.Text = "";
            }

            if (positionComboBox.SelectedItem == null)
            {
                positionErrMsgTextblock.Visibility = Visibility.Visible;
                positionErrMsgTextblock.Text = "位置不能为空";
                inputFieldsValid = false;
            }
            else
            {
                positionErrMsgTextblock.Visibility = Visibility.Hidden;
                positionErrMsgTextblock.Text = "";
            }

            if (bRadioButton.IsChecked == false && xRadioButton.IsChecked == false)
            {
                bOrXErrMsgTextblock.Visibility = Visibility.Visible;
                bOrXErrMsgTextblock.Text = "板或线不能为空";
                inputFieldsValid = false;
            }
            else
            {
                bOrXErrMsgTextblock.Visibility = Visibility.Hidden;
                bOrXErrMsgTextblock.Text = "";
            }

            if (productNameComboBox.SelectedItem == null)
            {
                productNameErrMsgTextblock.Visibility = Visibility.Visible;
                productNameErrMsgTextblock.Text = "产品名称不能为空";
                inputFieldsValid = false;
            }
            else
            {
                productNameErrMsgTextblock.Visibility = Visibility.Hidden;
                productNameErrMsgTextblock.Text = "";
            }

            if (typeTextBox.Text == string.Empty)
            {
                typeErrMsgTextblock.Visibility = Visibility.Visible;
                typeErrMsgTextblock.Text = "规格不能为空";
                inputFieldsValid = false;
            }
            else if (!InputVerifyService.IsTypeValid(typeTextBox.Text))
            {
                typeErrMsgTextblock.Visibility = Visibility.Visible;
                typeErrMsgTextblock.Text = "规格格式有误";
                inputFieldsValid = false;
            }
            else
            {
                typeErrMsgTextblock.Visibility = Visibility.Hidden;
                typeErrMsgTextblock.Text = "";
            }

            if (sizeTextBox.Text == string.Empty)
            {
                sizeErrMsgTextblock.Visibility = Visibility.Visible;
                sizeErrMsgTextblock.Text = "尺寸不能为空";
                inputFieldsValid = false;
            }
            else if (!InputVerifyService.IsSizeValid(sizeTextBox.Text))
            {
                sizeErrMsgTextblock.Visibility = Visibility.Visible;
                sizeErrMsgTextblock.Text = "仅允许整数，小数，加号，减号，不含空格";
                inputFieldsValid = false;
            }
            else
            {
                sizeErrMsgTextblock.Visibility = Visibility.Hidden;
                sizeErrMsgTextblock.Text = "";
            }

            if (unitLengthTextBox.Visibility == Visibility.Visible)
            {
                if (unitLengthTextBox.Text == string.Empty)
                {
                    unitLengthErrMsgTextblock.Visibility = Visibility.Visible;
                    unitLengthErrMsgTextblock.Text = "单片长度不能为空";
                    inputFieldsValid = false;
                }
                else if (!InputVerifyService.IsUnitLengthValid(unitLengthTextBox.Text))
                {
                    unitLengthErrMsgTextblock.Visibility = Visibility.Visible;
                    unitLengthErrMsgTextblock.Text = "单片长度格式有误";
                    inputFieldsValid = false;
                }
                else
                {
                    unitLengthErrMsgTextblock.Visibility = Visibility.Hidden;
                    unitLengthErrMsgTextblock.Text = "";
                }
            }

            if (applyDirectionComboBox.Visibility == Visibility.Visible)
            {
                if (applyDirectionComboBox.SelectedItem == null)
                {
                    applyDirectionErrMsgTextblock.Visibility = Visibility.Visible;
                    applyDirectionErrMsgTextblock.Text = "铺贴方向不能为空";
                    inputFieldsValid = false;
                }
                else
                {
                    applyDirectionErrMsgTextblock.Visibility = Visibility.Hidden;
                    applyDirectionErrMsgTextblock.Text = "";
                }
            }

            return inputFieldsValid;
        }

        /// <summary>
        /// Check whether all input fields are cleared without any value.
        /// </summary>
        private bool AllInputFieldsClear()
        {
            return roomComboBox.SelectedItem == null &&
                positionComboBox.SelectedItem == null &&
                bRadioButton.IsChecked == false && 
                xRadioButton.IsChecked == false &&
                productNameComboBox.SelectedItem == null &&
                String.IsNullOrEmpty(typeTextBox.Text) &&
                String.IsNullOrEmpty(sizeTextBox.Text) &&
                String.IsNullOrEmpty(unitLengthTextBox.Text) &&
                applyDirectionComboBox.SelectedItem == null;
        }
    }
}
