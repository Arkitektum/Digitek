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
                    var jObjectProperyValue = jsonObject[columnName].ToString();
                    excelWorksheet.Cells[AddRowAndColumnToCellAddress(tableStartAdress, i, j)].Value = jObjectProperyValue;
                    j++;
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
                        var cellName = string.Concat(GetColumnLetter(j), i);
                        var objectValue = ws.Cells[cellName].Value;
                        var objectName = ws.Cells[string.Concat(GetColumnLetter(j), startRow)].Value;
                        valuesDictionary.Add(objectName.ToString(), objectValue?.ToString());
                    }
                    dictionaryList.Add(valuesDictionary);
                }
                jsonArray = JArray.Parse(JsonConvert.SerializeObject(dictionaryList.ToArray()));
            }
            return jsonArray;
        }


        /// <summary>
        /// Get all table cell adress and values from a range of columns
        /// </summary>
        /// <param name="worksheet"></param>
        /// <param name="columnsIndexs"></param>
        /// <param name="variableId"></param>
        /// <returns></returns>
        public static Dictionary<int, Dictionary<string, object>> GetTableCellsAdressAndValue(ExcelWorksheet worksheet, string[] columnsIndexs, bool variableId = false)
        {
            var table = worksheet.Tables.FirstOrDefault();
            var tableStartRow = table.Address.Start.Row;
            var dictionary = new Dictionary<int, Dictionary<string, object>>();

            int startRow = variableId ? tableStartRow + 2 : tableStartRow + 1;
            var startColumn = GetRange(columnsIndexs, out var end);

            if (!CheckColumnRange(table, startColumn, end)) return dictionary;

            for (int i = startRow; i <= table.Address.End.Row; i++)
            {
                var valuesDictionary = new Dictionary<string, object>();
                for (int j = startColumn; j <= end; j++)
                {
                    var cellName = string.Concat(GetColumnLetter(j), i);
                    try
                    {
                        var cellValue = table.WorkSheet.Cells[cellName].Value;
                        valuesDictionary.Add(cellName, cellValue);

                    }
                    catch (Exception exception)
                    {
                        var mes = exception.Message;
                    }
                }
                dictionary.Add(i, valuesDictionary);
            }

            return dictionary;
        }

        /// <summary>
        /// Get last Table column adress and value, check the range to see if the DMN have annotation or not.
        /// </summary>
        /// <param name="ws"></param>
        /// <param name="table"></param>
        /// <param name="inputsIndex"></param>
        /// <param name="outputsIndex"></param>
        /// <param name="variableId"></param>
        /// <returns></returns>
        public static Dictionary<int, string> GetTableAnnotationsCellsValue(ExcelWorksheet worksheet, string[] inputsIndex, string[] outputsIndex, bool variableId)
        {
            var dictionary = new Dictionary<int, string>();

            var table = worksheet.Tables.FirstOrDefault();
            var tableStartRow = table.Address.Start.Row;

            var startRow = variableId ? tableStartRow + 2 : tableStartRow + 1;

            var tableSize = table.Columns.Count;

            if (tableSize != outputsIndex.Count() + inputsIndex.Count() + 1)
                return dictionary;

            var start = GetRange(inputsIndex, out var end);
            if (!CheckColumnRange(table, start, end)) return dictionary;
            for (var i = startRow; i <= table.Address.End.Row; i++)
            {
                var cellName = string.Concat(GetColumnLetter(table.Address.End.Column), i);
                var cellValue = table.WorkSheet.Cells[cellName].Value;

                dictionary.Add(i, cellValue?.ToString());
            }
            return dictionary;
        }

        /// <summary>
        /// Get the value type for the each column
        /// </summary>
        /// <param name="ws"></param>
        /// <param name="table"></param>
        /// <param name="columnsIndexs"></param>
        /// <param name="variableId"></param>
        /// <returns></returns>
        public static Dictionary<int, string> GetTableColumnsType(ExcelWorksheet worksheet, string[] columnsIndexs, bool variableId = false)
        {
            var table = worksheet.Tables.FirstOrDefault();
            var tableStartRow = table.Address.Start.Row;
            var columnsTypes = new Dictionary<int, string>();
            int startRow = variableId ? tableStartRow + 2 : tableStartRow + 1;
            
            // get column range from leters (Excel Adress)
            var startColumn = GetRange(columnsIndexs, out var endColumn);

            if (!CheckColumnRange(table, startColumn, endColumn)) return columnsTypes;
            int row = 0;

            //Loop
            for (int col = startColumn; col <= endColumn; col++)
            {
             
                var cellValueType = GetRowsValueTypes(col, startRow, worksheet);
                columnsTypes.Add(row, cellValueType);
                row++;
            }

            return columnsTypes;
        }

        /// <summary>
        /// have to check if all types in all rows have the same value type
        /// </summary>
        /// <param name="columnIndex"></param>
        /// <param name="startRow"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        public static string GetRowsValueTypes(int columnIndex, int startRow, ExcelWorksheet worksheet)
        {
            string type = null;

            var table = worksheet.Tables.FirstOrDefault();

            for (int row = startRow; row <= table.Address.End.Row; row++)
            {
                var cellName = string.Concat(GetColumnLetter(columnIndex), row);
                
                // get cell value type
                var cellValue =worksheet.Cells[cellName].Value;
                var typeTemp = GetCellValueType(cellValue);
                // if type is null set value to type, first loop
                type = string.IsNullOrEmpty(type) ? typeTemp : type;

                //Compare the type and standardize for the entire column
                type = StandardizeColumnType(type, typeTemp);
            }
            return type;
        }


        private static string StandardizeColumnType(string type, string typeTemp)
        {
            if (type != typeTemp && !string.IsNullOrEmpty(typeTemp))
            {
                if (type == "integer")
                {
                    switch (typeTemp)
                    {
                        case "double":
                            type = "double";
                            break;
                        case "long":
                            type = "long";
                            break;
                        default:
                            type = "string";
                            break;
                    }
                }

                if (type == "double" || type == "long")
                {
                    switch (typeTemp)
                    {
                        case "double":
                        case "long":
                        case "integer":
                            type = "double";
                            break;
                        case "string":
                        case "boolean":
                            type = "string";
                            break;
                    }
                }

                if (type == "boolean")
                    type = "string";
            }
            return type;
        }


        private static string GetCellValueType(object cellValue)
        {
            //cellValue = cellValue.ToString().Trim();
            if (cellValue == null || string.IsNullOrEmpty(cellValue.ToString())) return string.Empty;
            //if (string.IsNullOrEmpty(cellValue.ToString()))return String.Empty;

            string cellValueString = DmnConverter.GetComparisonNumber(cellValue.ToString()) ?? cellValue.ToString();

            var cellRangeNumber = DmnConverter.GetRangeNumber(cellValue.ToString());
            if (cellRangeNumber != null)
            {
                var type1 = GetCellValueType(cellRangeNumber[0]);
                var type2 = GetCellValueType(cellRangeNumber[1]);

                if (type1 != type2)
                    return StandardizeColumnType(type1, type2);
                cellValueString = cellRangeNumber[0];
            }

            if (int.TryParse(cellValueString, out var intType)) return "integer";
            if (long.TryParse(cellValueString, out var longType)) return "long";
            if (double.TryParse(cellValueString, out var doubleType)) return "double";
            if (bool.TryParse(cellValueString, out var booleanType)) return "boolean";

            return "string";

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="jsonArray"></param>
        /// <returns></returns>
        public static List<string> GetListOfKeyFromJArray(JArray jsonArray)
        {
            var namesList = new List<string>();
            var jsonDefaultObject = jsonArray.Children<JObject>().FirstOrDefault();
            if (jsonDefaultObject != null)
                namesList.AddRange(jsonDefaultObject.Properties().Select(prop => prop.Name));

            return namesList;
        }


        /// <summary>
        /// Get the range from the excel columns index
        /// </summary>
        /// <param name="columnIndexs"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static int GetRange(string[] columnIndexs, out int end)
        {
            var columnIndexsOrderBy = columnIndexs.OrderBy(d => d).ToArray();
            var start = GetColumnIndex(columnIndexsOrderBy[0]);
            end = GetColumnIndex(columnIndexsOrderBy.Last());
            return start;
        }

        /// <summary>
        /// check if the range is containd in the excel table
        /// </summary>
        /// <param name="table"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static bool CheckColumnRange(ExcelTable table, int start, int end)
        {
            var tableStartColumn = table.Address.Start.Column;
            var tableEndColumn = table.Address.End.Column;
            return Enumerable.Range(tableStartColumn, tableEndColumn).Contains(start) &&
                   Enumerable.Range(tableStartColumn, tableEndColumn).Contains(end);
        }


        public static Dictionary<string, string[]> GetColumnRagngeInLeters(ExcelTable table, int inputsColumnsCount, int outputsColumnsCount)
        {
            var dictionary = new Dictionary<string, string[]>();
            var start = table.Address.Start.Column;
            var end = table.Address.End.Column;

            if ((inputsColumnsCount + outputsColumnsCount) > (end - (start - 1)))
                return null;

            var outputsStart = start + inputsColumnsCount;
            var inputsColumnIndexes = new List<string>();
            for (int i = start; i < outputsStart; i++)
            {
                inputsColumnIndexes.Add(GetColumnLetter(i));
            }
            var outputsColumnIndexes = new List<string>();
            for (int i = outputsStart; i < outputsStart + outputsColumnsCount; i++)
            {
                outputsColumnIndexes.Add(GetColumnLetter(i));
            }
            dictionary.Add("inputsIndex", inputsColumnIndexes.ToArray());
            dictionary.Add("outputsIndex", outputsColumnIndexes.ToArray());
            return dictionary;
        }


        public static string AddRowAndColumnToCellAddress(string address, int row, int column)
        {


            var addressAndWorkSheet = address.Split("!");

            var cellAddress = addressAndWorkSheet.Length > 1 ? addressAndWorkSheet[1] : addressAndWorkSheet[0];

            var dictionaryKeyIndex = GetRowAndColumIndex(cellAddress);

            if (dictionaryKeyIndex.Any())
            {
                var newaddress = $"{GetColumnLetter(dictionaryKeyIndex["column"] + column)}{dictionaryKeyIndex["row"] + row}";
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
        public static string GetColumnLetter(int index)
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="columnsIndexs"></param>
        /// <param name="haveId"></param>
        /// <returns></returns>
        public static Dictionary<string, Dictionary<string, string>> GetTableHeader(ExcelWorksheet worksheet, string[] columnsIndexs, bool haveId = false)
        {
            var table = worksheet.Tables.FirstOrDefault();
            var tableStartRow = table.Address.Start.Row;
            Dictionary<string, Dictionary<string, string>> dictionary = new Dictionary<string, Dictionary<string, string>>();

            var start = GetRange(columnsIndexs, out var end);
            if (!CheckColumnRange(table, start, end)) return dictionary;

            for (int col = start; col <= end; col++)
            {
                Dictionary<string, string> headerDictionary = new Dictionary<string, string>();
                var cellName = string.Concat(GetColumnLetter(col), tableStartRow);
                var cellValue = table.WorkSheet.Cells[cellName].Value;
                if (haveId)
                {
                    var cellAddress = string.Concat(GetColumnLetter(col), tableStartRow + 1);
                    var cellIdValue = table.WorkSheet.Cells[cellAddress].Value;
                    headerDictionary.Add(cellValue.ToString(), cellIdValue.ToString());
                }
                else
                {
                    headerDictionary.Add(cellValue.ToString(), string.Empty);
                }
                dictionary.Add(cellName, headerDictionary);
            }

            return dictionary;
        }
    }
}

