using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DecorationMaterialCalculator.Models;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace DecorationMaterialCalculator.Services
{
    public static class IOService
    {
        /// <summary>
        /// Given file name you want to open, return a list of string corresponds to each line in file
        /// </summary>
        public static List<string> ReadTextFileToList(string fileName)
        {
            List<string> strList = new List<string>();

            try
            {
                using (StreamReader streamreader = new StreamReader(fileName))
                {
                    Regex allSpaceRegex = new Regex(@"^\s*$");
                    string fileContent = streamreader.ReadToEnd();
                    string[] itemArr = Regex.Split(fileContent, "\r\n");

                    foreach(string item in itemArr)
                    {
                        if (item != string.Empty && !allSpaceRegex.IsMatch(item))
                            strList.Add(item);
                    }
                }
            }
            catch (FileNotFoundException ex)
            {
                
            }

            return strList;
        }

        /// <summary>
        /// Read all contents in txt file to a large string
        /// </summary>
        public static string ReadTextFileToString(string fileName)
        {
            using (StreamReader streamreader = new StreamReader(fileName))
            {
                string fileContent = streamreader.ReadToEnd();
                return fileContent;
            }
        }

        public static bool FileAlreadyExists(string filepath)
        {
            return File.Exists(filepath);
        }

        /// <summary>
        /// Generate sheet used for saving all current input for next time use, records in the sheet contains all information of InputItem
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="inputItems"></param>
        public static void GenerateInputCheckingSheet(string filepath, List<InputItem> inputItems)
        {
            // Create a spreadsheet document by supplying the filepath.
            // By default, AutoSave = true, Editable = true, and Type = xlsx.
            SpreadsheetDocument spreadSheet = SpreadsheetDocument.Create(filepath, SpreadsheetDocumentType.Workbook);

            // Add a WorkbookPart to the document.
            WorkbookPart workbookPart = spreadSheet.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();

            // Add Sheets to the Workbook.
            Sheets sheets = spreadSheet.WorkbookPart.Workbook.AppendChild<Sheets>(new Sheets());

            // Create new worksheet part with actual data
            WorksheetPart newWorksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            Worksheet newWorksheet = new Worksheet();
            SheetData newSheetData = new SheetData();

            FillSheetDataWithInputItemListAllFields(newSheetData, inputItems);

            newWorksheet.Append(newSheetData);
            newWorksheetPart.Worksheet = newWorksheet;

            // Create new sheet metadata corresponds to the new worksheet created above
            Sheet newSheet = new Sheet()
            {
                Id = workbookPart.GetIdOfPart(newWorksheetPart),
                SheetId = 1,
                Name = "输入校对表"
            };
            sheets.Append(newSheet);

            // Save the new worksheet.
            newWorksheet.Save();
            workbookPart.Workbook.Save();

            // Close the document.
            spreadSheet.Close();
        }

        /// <summary>
        /// Read the saved InputCheckingSheet, put each record on sheet into a InputItem
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public static List<InputItem> ReadInputCheckingSheetToInputItemList(string filepath)
        {
            List<InputItem> inputItems = new List<InputItem>();

            using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(filepath, false))
            {
                WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;
                WorksheetPart worksheetPart = workbookPart.WorksheetParts.First();
                SheetData sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();

                foreach (Row r in sheetData.Elements<Row>())
                {
                    if (r.RowIndex == 1u) continue; // skip first row

                    var cells = r.Elements<Cell>().ToList();
                    InputItem inputItem = new InputItem();

                    inputItem.ProductName = cells[1].CellValue.Text;
                    inputItem.BorX = cells[2].CellValue.Text;
                    inputItem.Type = cells[3].CellValue.Text;
                    inputItem.Size = cells[4].CellValue.Text;
                    inputItem.Position = cells[5].CellValue.Text;
                    inputItem.Room = cells[6].CellValue.Text;
                    inputItem.UnitLength = cells[7].CellValue.Text != string.Empty ? cells[7].CellValue.Text : null;
                    inputItem.ApplyDirection = cells[8].CellValue.Text != string.Empty ? cells[8].CellValue.Text : null;
                    inputItem.SummedUpSize = StringParserService.StringToDouble(cells[9].CellValue.Text);
                    inputItem.Quantity = StringParserService.StringToInt(cells[10].CellValue.Text);

                    inputItems.Add(inputItem);
                }
            }
            
            return inputItems;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="inputItems"></param>
        public static void GenerateConstructionDetailSheet(string filepath, List<InputItem> inputItems)
        {
            // Create a spreadsheet document by supplying the filepath.
            // By default, AutoSave = true, Editable = true, and Type = xlsx.
            SpreadsheetDocument spreadSheet = SpreadsheetDocument.Create(filepath, SpreadsheetDocumentType.Workbook);

            // Add a WorkbookPart to the document.
            WorkbookPart workbookPart = spreadSheet.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();

            // Add Sheets to the Workbook.
            Sheets sheets = spreadSheet.WorkbookPart.Workbook.AppendChild<Sheets>(new Sheets());

            // Create new worksheet part with actual data
            WorksheetPart newWorksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            Worksheet newWorksheet = new Worksheet();
            SheetData newSheetData = new SheetData();

            FillSheetDataWithInputItemList(newSheetData, inputItems);

            newWorksheet.Append(newSheetData);
            newWorksheetPart.Worksheet = newWorksheet;

            // Create new sheet metadata corresponds to the new worksheet created above
            Sheet newSheet = new Sheet()
            {
                Id = workbookPart.GetIdOfPart(newWorksheetPart),
                SheetId = 1,
                Name = "施工详单"
            };
            sheets.Append(newSheet);

            // Save the new worksheet.
            newWorksheet.Save();
            workbookPart.Workbook.Save();

            // Close the document.
            spreadSheet.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="summedItems"></param>
        /// <param name="buckleItem">null means we don't need buckle</param>
        /// <param name="buyPriceNotSellPrice"> Set to true will generate sheet with buy price showing on it; Set to false will generate sheet with sell price showing on it </param>
        public static void GenerateMaterialTotalSheet(string filepath, List<SummedItem> summedItems, BuckleItem buckleItem, bool buyPriceNotSellPrice)
        {
            // Create a spreadsheet document by supplying the filepath.
            // By default, AutoSave = true, Editable = true, and Type = xlsx.
            SpreadsheetDocument spreadSheet = SpreadsheetDocument.Create(filepath, SpreadsheetDocumentType.Workbook);

            // Add a WorkbookPart to the document.
            WorkbookPart workbookPart = spreadSheet.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();

            // Add Sheets to the Workbook.
            Sheets sheets = spreadSheet.WorkbookPart.Workbook.AppendChild<Sheets>(new Sheets());

            // Create new worksheet part with actual data
            WorksheetPart newWorksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            Worksheet newWorksheet = new Worksheet();
            SheetData newSheetData = new SheetData();

            FillSheetDataWithSummedItemListAndBuckleItem(newSheetData, summedItems, buckleItem, buyPriceNotSellPrice);

            newWorksheet.Append(newSheetData);
            newWorksheetPart.Worksheet = newWorksheet;

            // Create new sheet metadata corresponds to the new worksheet created above
            Sheet newSheet = new Sheet()
            {
                Id = workbookPart.GetIdOfPart(newWorksheetPart),
                SheetId = 1,
                Name = "材料清单"
            };
            sheets.Append(newSheet);

            // Save the new worksheet.
            newWorksheet.Save();
            workbookPart.Workbook.Save();

            // Close the document.
            spreadSheet.Close();
        }


        #region Helper methods
        private static void FillSheetDataWithInputItemList(SheetData sheetData, List<InputItem> inputItems)
        {
            Row rHeader = new Row() { RowIndex = (UInt32Value)1u };
            Cell cHeader1 = new Cell();
            cHeader1.DataType = CellValues.String;
            cHeader1.CellValue = new CellValue("序号");
            Cell cHeader2 = new Cell();
            cHeader2.DataType = CellValues.String;
            cHeader2.CellValue = new CellValue("产品名称");
            Cell cHeader3 = new Cell();
            cHeader3.DataType = CellValues.String;
            cHeader3.CellValue = new CellValue("规格");
            Cell cHeader4 = new Cell();
            cHeader4.DataType = CellValues.String;
            cHeader4.CellValue = new CellValue("数量");
            Cell cHeader5 = new Cell();
            cHeader5.DataType = CellValues.String;
            cHeader5.CellValue = new CellValue("位置");
            Cell cHeader6 = new Cell();
            cHeader6.DataType = CellValues.String;
            cHeader6.CellValue = new CellValue("备注");
            Cell cHeader7 = new Cell();
            cHeader7.DataType = CellValues.String;
            cHeader7.CellValue = new CellValue("铺贴方向");
            rHeader.Append(cHeader1);
            rHeader.Append(cHeader2);
            rHeader.Append(cHeader3);
            rHeader.Append(cHeader4);
            rHeader.Append(cHeader5);
            rHeader.Append(cHeader6);
            rHeader.Append(cHeader7);
            sheetData.Append(rHeader);

            uint rowIndex = 2u;
            foreach (InputItem item in inputItems)
            {
                Row row = new Row() { RowIndex = (UInt32Value)rowIndex };
                Cell cell1 = new Cell();
                cell1.DataType = CellValues.Number;
                cell1.CellValue = new CellValue((rowIndex - 1).ToString());
                Cell cell2 = new Cell();
                cell2.DataType = CellValues.String;
                cell2.CellValue = new CellValue(item.ProductName);
                Cell cell3 = new Cell();
                cell3.DataType = CellValues.String;
                cell3.CellValue = new CellValue(item.Type);
                Cell cell4 = new Cell();
                cell4.DataType = CellValues.Number;
                cell4.CellValue = new CellValue(item.Quantity.ToString());
                Cell cell5 = new Cell();
                cell5.DataType = CellValues.String;
                cell5.CellValue = new CellValue(item.Position);
                Cell cell6 = new Cell();
                cell6.DataType = CellValues.String;
                cell6.CellValue = new CellValue(item.Room);
                Cell cell7 = new Cell();
                cell7.DataType = CellValues.String;
                cell7.CellValue = new CellValue(item.ApplyDirection);
                row.Append(cell1);
                row.Append(cell2);
                row.Append(cell3);
                row.Append(cell4);
                row.Append(cell5);
                row.Append(cell6);
                row.Append(cell7);
                sheetData.Append(row);

                rowIndex++;
            }
        }

        private static void FillSheetDataWithInputItemListAllFields(SheetData sheetData, List<InputItem> inputItems)
        {
            Row rHeader = new Row() { RowIndex = (UInt32Value)1u };
            Cell cHeader1 = new Cell();
            cHeader1.DataType = CellValues.String;
            cHeader1.CellValue = new CellValue("序号");
            Cell cHeader2 = new Cell();
            cHeader2.DataType = CellValues.String;
            cHeader2.CellValue = new CellValue("产品名称");
            Cell cHeader3 = new Cell();
            cHeader3.DataType = CellValues.String;
            cHeader3.CellValue = new CellValue("板或线");
            Cell cHeader4 = new Cell();
            cHeader4.DataType = CellValues.String;
            cHeader4.CellValue = new CellValue("规格");
            Cell cHeader5 = new Cell();
            cHeader5.DataType = CellValues.String;
            cHeader5.CellValue = new CellValue("尺寸");
            Cell cHeader6 = new Cell();
            cHeader6.DataType = CellValues.String;
            cHeader6.CellValue = new CellValue("位置");
            Cell cHeader7 = new Cell();
            cHeader7.DataType = CellValues.String;
            cHeader7.CellValue = new CellValue("房间");
            Cell cHeader8 = new Cell();
            cHeader8.DataType = CellValues.String;
            cHeader8.CellValue = new CellValue("单片长");
            Cell cHeader9 = new Cell();
            cHeader9.DataType = CellValues.String;
            cHeader9.CellValue = new CellValue("铺贴方向");
            Cell cHeader10 = new Cell();
            cHeader10.DataType = CellValues.String;
            cHeader10.CellValue = new CellValue("总尺寸");
            Cell cHeader11 = new Cell();
            cHeader11.DataType = CellValues.String;
            cHeader11.CellValue = new CellValue("数量");

            rHeader.Append(cHeader1);
            rHeader.Append(cHeader2);
            rHeader.Append(cHeader3);
            rHeader.Append(cHeader4);
            rHeader.Append(cHeader5);
            rHeader.Append(cHeader6);
            rHeader.Append(cHeader7);
            rHeader.Append(cHeader8);
            rHeader.Append(cHeader9);
            rHeader.Append(cHeader10);
            rHeader.Append(cHeader11);
            sheetData.Append(rHeader);

            uint rowIndex = 2u;
            foreach (InputItem item in inputItems)
            {
                Row row = new Row() { RowIndex = (UInt32Value)rowIndex };
                Cell cell1 = new Cell();
                cell1.DataType = CellValues.Number;
                cell1.CellValue = new CellValue((rowIndex - 1).ToString());
                Cell cell2 = new Cell();
                cell2.DataType = CellValues.String;
                cell2.CellValue = new CellValue(item.ProductName);
                Cell cell3 = new Cell();
                cell3.DataType = CellValues.String;
                cell3.CellValue = new CellValue(item.BorX);
                Cell cell4 = new Cell();
                cell4.DataType = CellValues.String;
                cell4.CellValue = new CellValue(item.Type);
                Cell cell5 = new Cell();
                cell5.DataType = CellValues.String;
                cell5.CellValue = new CellValue(item.Size);
                Cell cell6 = new Cell();
                cell6.DataType = CellValues.String;
                cell6.CellValue = new CellValue(item.Position);
                Cell cell7 = new Cell();
                cell7.DataType = CellValues.String;
                cell7.CellValue = new CellValue(item.Room);
                Cell cell8 = new Cell();
                cell8.DataType = CellValues.String;
                cell8.CellValue = new CellValue(item.UnitLength);
                Cell cell9 = new Cell();
                cell9.DataType = CellValues.String;
                cell9.CellValue = new CellValue(item.ApplyDirection);
                Cell cell10 = new Cell();
                cell10.DataType = CellValues.String;
                cell10.CellValue = new CellValue(item.SummedUpSize.ToString());
                Cell cell11 = new Cell();
                cell11.DataType = CellValues.String;
                cell11.CellValue = new CellValue(item.Quantity.ToString());
                row.Append(cell1);
                row.Append(cell2);
                row.Append(cell3);
                row.Append(cell4);
                row.Append(cell5);
                row.Append(cell6);
                row.Append(cell7);
                row.Append(cell8);
                row.Append(cell9);
                row.Append(cell10);
                row.Append(cell11);
                sheetData.Append(row);

                rowIndex++;
            }
        }

        private static void FillSheetDataWithSummedItemListAndBuckleItem(SheetData sheetData, List<SummedItem> summedItems, BuckleItem buckleItem, bool buyPriceNotSellPrice)
        {
            // Fill in header
            Row rHeader = new Row() { RowIndex = (UInt32Value)1u };
            Cell cHeader1 = new Cell();
            cHeader1.DataType = CellValues.String;
            cHeader1.CellValue = new CellValue("序号");
            Cell cHeader2 = new Cell();
            cHeader2.DataType = CellValues.String;
            cHeader2.CellValue = new CellValue("产品名称");
            Cell cHeader3 = new Cell();
            cHeader3.DataType = CellValues.String;
            cHeader3.CellValue = new CellValue("规格");
            Cell cHeader4 = new Cell();
            cHeader4.DataType = CellValues.String;
            cHeader4.CellValue = new CellValue("数量");
            Cell cHeader5 = new Cell();
            cHeader5.DataType = CellValues.String;
            cHeader5.CellValue = new CellValue("平方");
            Cell cHeader6 = new Cell();
            cHeader6.DataType = CellValues.String;
            cHeader6.CellValue = new CellValue("单价");
            Cell cHeader7 = new Cell();
            cHeader7.DataType = CellValues.String;
            cHeader7.CellValue = new CellValue("合计");
            Cell cHeader8 = new Cell();
            cHeader8.DataType = CellValues.String;
            cHeader8.CellValue = new CellValue("位置");
            Cell cHeader9 = new Cell();
            cHeader9.DataType = CellValues.String;
            cHeader9.CellValue = new CellValue("备注");
            rHeader.Append(cHeader1);
            rHeader.Append(cHeader2);
            rHeader.Append(cHeader3);
            rHeader.Append(cHeader4);
            rHeader.Append(cHeader5);
            rHeader.Append(cHeader6);
            rHeader.Append(cHeader7);
            rHeader.Append(cHeader8);
            rHeader.Append(cHeader9);
            sheetData.Append(rHeader);

            // Fill in summedItemList
            uint rowIndex = 2u;
            foreach (SummedItem item in summedItems)
            {
                Row row = new Row() { RowIndex = (UInt32Value)rowIndex };
                Cell cell1 = new Cell();
                cell1.DataType = CellValues.Number;
                cell1.CellValue = new CellValue((rowIndex - 1).ToString());
                Cell cell2 = new Cell();
                cell2.DataType = CellValues.String;
                cell2.CellValue = new CellValue(item.ProductName);
                Cell cell3 = new Cell();
                cell3.DataType = CellValues.String;
                cell3.CellValue = new CellValue(item.Type);
                Cell cell4 = new Cell();
                cell4.DataType = CellValues.Number;
                cell4.CellValue = new CellValue(item.Quantity.ToString());
                Cell cell5 = new Cell();
                cell5.DataType = CellValues.Number;
                cell5.CellValue = new CellValue(item.TotalAreaOrLength.ToString());
                Cell cell6 = new Cell();
                cell6.DataType = CellValues.String;
                cell6.CellValue = new CellValue(buyPriceNotSellPrice ? item.BuyPrice: item.SellPrice);
                Cell cell7 = new Cell();
                cell7.DataType = CellValues.String;
                cell7.CellValue = new CellValue(buyPriceNotSellPrice ? item.TotalBuyPrice.ToString() : item.TotalSellPrice.ToString());
                Cell cell8 = new Cell();
                cell8.DataType = CellValues.String;
                cell8.CellValue = new CellValue(item.Memo);
                Cell cell9 = new Cell();
                cell9.DataType = CellValues.String;
                cell9.CellValue = new CellValue("");
                row.Append(cell1);
                row.Append(cell2);
                row.Append(cell3);
                row.Append(cell4);
                row.Append(cell5);
                row.Append(cell6);
                row.Append(cell7);
                row.Append(cell8);
                row.Append(cell9);
                sheetData.Append(row);

                rowIndex++;
            }

            // Fill in buckleItem if we need buckle
            if (buckleItem != null)
            {
                Row rBuckle = new Row() { RowIndex = (UInt32Value)rowIndex };
                Cell cBuckle1 = new Cell();
                cBuckle1.DataType = CellValues.Number;
                cBuckle1.CellValue = new CellValue((rowIndex - 1).ToString());
                Cell cBuckle2 = new Cell();
                cBuckle2.DataType = CellValues.String;
                cBuckle2.CellValue = new CellValue(buckleItem.ProductName);
                Cell cBuckle3 = new Cell();
                cBuckle3.DataType = CellValues.String;
                cBuckle3.CellValue = new CellValue("");
                Cell cBuckle4 = new Cell();
                cBuckle4.DataType = CellValues.Number;
                cBuckle4.CellValue = new CellValue(buckleItem.Quantity.ToString());
                Cell cBuckle5 = new Cell();
                cBuckle5.DataType = CellValues.Number;
                cBuckle5.CellValue = new CellValue("");
                Cell cBuckle6 = new Cell();
                cBuckle6.DataType = CellValues.String;
                cBuckle6.CellValue = new CellValue(buyPriceNotSellPrice ? buckleItem.BuyPrice : buckleItem.SellPrice);
                Cell cBuckle7 = new Cell();
                cBuckle7.DataType = CellValues.String;
                cBuckle7.CellValue = new CellValue(buyPriceNotSellPrice ? buckleItem.TotalBuyPrice.ToString() : buckleItem.TotalSellPrice.ToString());
                Cell cBuckle8 = new Cell();
                cBuckle8.DataType = CellValues.String;
                cBuckle8.CellValue = new CellValue("");
                Cell cBuckle9 = new Cell();
                cBuckle9.DataType = CellValues.String;
                cBuckle9.CellValue = new CellValue("");
                rBuckle.Append(cBuckle1);
                rBuckle.Append(cBuckle2);
                rBuckle.Append(cBuckle3);
                rBuckle.Append(cBuckle4);
                rBuckle.Append(cBuckle5);
                rBuckle.Append(cBuckle6);
                rBuckle.Append(cBuckle7);
                rBuckle.Append(cBuckle8);
                rBuckle.Append(cBuckle9);
                sheetData.Append(rBuckle);

                rowIndex++;
            }

        }
        #endregion
    }


}