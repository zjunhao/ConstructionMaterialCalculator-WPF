using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace DecorationMaterialCalculator.Services
{
    public class AppConfigReadingService
    {
        private static string _spreadSheetExtention;

        public static string GetSpreadSheetExtention()
        {
            if (!String.IsNullOrEmpty(_spreadSheetExtention)) return _spreadSheetExtention;

            try
            {
                string str = ConfigurationManager.AppSettings["spreadSheetExtension"];

                if (str == "xlsx" || str == ".xlsx") 
                    _spreadSheetExtention = ".xlsx";
                else if (str == "xls" || str == ".xls") 
                    _spreadSheetExtention = ".xls";
                else 
                    _spreadSheetExtention = ".xlsx";

                return _spreadSheetExtention;
            }
            catch
            {
                _spreadSheetExtention = ".xlsx";
                return _spreadSheetExtention;
            }

        }

        public static string GetConfigSetting(string key)
        {
            string value = ConfigurationManager.AppSettings[key];
            return value;
        }
    }
}
