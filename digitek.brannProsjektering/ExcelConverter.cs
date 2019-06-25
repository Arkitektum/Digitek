using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using digitek.brannProsjektering.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using OfficeOpenXml.Table;

namespace digitek.brannProsjektering
{


    /// <summary>
    /// 
    /// </summary>
    public class ExcelConverter
    {

        /// <summary>
        /// 
        /// </summary>
        public static string StartCell = "B5";


        public static void AddDataToTabel( ref ExcelWorksheet excelWorksheet, ExcelTable excelTable, JArray jsonArray)
        {
            var columnsList = GetListOfKeyFromJArray(jsonArray);
            var tableStartAdress = excelTable.Address.Start.Address;
            var i = 0;
            foreach (var jsonObject in jsonArray.Children<JObject>())
            {
                i++;
                var j = 0;
                foreach (var columnName in columnsList)
                {
                    j++;
                    var jObjectProperyValue = jsonObject[columnName].ToString();
                    excelWorksheet.Cells[AddRowAndColumnToCellAddress(tableStartAdress, i, j)].Value = jObjectProperyValue;
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jsonArray"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static ExcelTable AddTableToWorkSheet(ref ExcelWorksheet excelWorksheet, JArray jsonArray, string tableName)
        {
            var columnsList = GetListOfKeyFromJArray(jsonArray);
            var startTableCellAddress = AddRowAndColumnToCellAddress(StartCell, 4, 0);
            var endTableCellAddress = AddRowAndColumnToCellAddress(startTableCellAddress, jsonArray.Count, columnsList.Count -1);
            ExcelTable table;
            using (ExcelRange rng = excelWorksheet.Cells[$"{startTableCellAddress}:{endTableCellAddress}"])
            {
                table = excelWorksheet.Tables.Add(rng, tableName);
            }
            table.ShowHeader = true;
            table.ShowFilter = true;
            return table;
        }

        public static void AddHeadersToExcelTable(ExcelTable table, JArray jsonArray)
        {
            var columnsList = GetListOfKeyFromJArray(jsonArray);

            //Set Columns position & name
            var i = 0;
            foreach (var property in columnsList)
            {
                table.Columns[i].Name = string.Concat(property);
                i++;
            }
           
        }


        public static void AddWorksheetInfo(ref ExcelWorksheet excelWorksheet,string Name, string ejecutionId)
        {
            excelWorksheet.Cells[StartCell].Value = "Navn:";
            excelWorksheet.Cells[AddRowAndColumnToCellAddress(StartCell, 0, 1)].Value = Name;
            excelWorksheet.Cells[AddRowAndColumnToCellAddress(StartCell, 1, 0)].Value = "ejecutionId:";
            excelWorksheet.Cells[AddRowAndColumnToCellAddress(StartCell, 1, 1)].Value = ejecutionId;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="ws"></param>
        /// <returns></returns>
        public JArray ExcelToJsonArray(ExcelWorksheet ws)
        {
            var jsonList = new List<object>();
            var table = ws.Tables.FirstOrDefault();
            var jsonArray = new JArray();

            if (table != null)
            {
                var startColumn = table.Address.Start.Column;
                var startRow = table.Address.Start.Row;
                var dictionaryList = new List<Dictionary<string, string>>();

                for (int i = startRow + 1; i <= table.Address.End.Row; i++)
                {
                    var valuesDictionary = new Dictionary<string, string>();
                    for (int j = startColumn; j <= table.Address.End.Column; j++)
                    {
                        var cellName = string.Concat(GetColumnName(j), i);
                        var objectValue = ws.Cells[cellName].Value;
                        var objectName = ws.Cells[string.Concat(GetColumnName(j), startRow)].Value;
                        valuesDictionary.Add(objectName.ToString(), objectValue?.ToString());
                    }
                    dictionaryList.Add(valuesDictionary);
                }
                jsonArray = JArray.Parse(JsonConvert.SerializeObject(dictionaryList.ToArray()));
            }
            return jsonArray;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jsonArray"></param>
        /// <returns></returns>
        public static List<string> GetListOfKeyFromJArray(JArray jsonArray)
        {
            List<string> namesList = new List<string>();
            var jsonDefaultObject = jsonArray.Children<JObject>().FirstOrDefault();
            foreach (JProperty prop in jsonDefaultObject.Properties())
            {
                namesList.Add(prop.Name);
            }

            return namesList;
        }
        public static string AddRowAndColumnToCellAddress(string address, int row, int column)
        {


            var addressAndWorkSheet = address.Split("!");

            var cellAddress = addressAndWorkSheet.Length > 1 ? addressAndWorkSheet[1] : addressAndWorkSheet[0];

            var dictionaryKeyIndex = GetRowAndColumIndex(cellAddress);

            if (dictionaryKeyIndex.Any())
            {
                var newaddress = $"{GetColumnName(dictionaryKeyIndex["column"] + column)}{dictionaryKeyIndex["row"] + row}";
                return addressAndWorkSheet.Length > 1 ? $"{addressAndWorkSheet[0]}!{newaddress}" : newaddress;
            }
            return null;
        }
        public static Dictionary<string, int> GetRowAndColumIndex(string address)
        {

            if (!string.IsNullOrEmpty(address))
            {

                var addressAndWorkSheet = address.Split("!");

                var cellAddress = addressAndWorkSheet.Length > 1 ? addressAndWorkSheet[1] : addressAndWorkSheet[0];


                Dictionary<string, int> dictionay = new Dictionary<string, int>();

                var column = string.Empty;
                var row = string.Empty;

                foreach (char c in cellAddress)
                {
                    if (char.IsLetter(c))
                        column += c;
                    if (char.IsNumber(c))
                        row += c;
                }
                int rowNumber;
                int.TryParse(row, out rowNumber);

                dictionay.Add("row", rowNumber);
                dictionay.Add("column", GetColumnIndex(column));
                if (addressAndWorkSheet.Length > 1)
                {
                    dictionay.Add("WorkSheet", 0);
                }
                return dictionay;
            }
            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static int GetColumnIndex(string columnName)
        {
            var index = 0;
            for (int i = 0; i < columnName.Length; i++)
            {
                index *= 26;
                index += (columnName[i] - 'A' + 1);
            }

            return index;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static string GetColumnName(int index)
        {
            int dividend = index;
            string columnName = String.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo) + columnName;
                dividend = (int)((dividend - modulo) / 26);
            }

            return columnName;
        }

    }
}

