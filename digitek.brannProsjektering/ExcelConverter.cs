using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using digitek.brannProsjektering.Models;
using digitek.brannProsjektering.Models.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using OfficeOpenXml.Style;
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
        public static string _StartCell = "B5";

        //----JSon to Excel
        public static void AddDataToTabel(ref ExcelWorksheet excelWorksheet, ExcelTable excelTable, JArray jsonArray)
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
            var startTableCellAddress = AddRowAndColumnToCellAddress(_StartCell, 4, 0);
            var endTableCellAddress = AddRowAndColumnToCellAddress(startTableCellAddress, jsonArray.Count, columnsList.Count - 1);
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


        public static void AddWorksheetInfo(ref ExcelWorksheet excelWorksheet, string Name, string ejecutionId)
        {
            excelWorksheet.Cells[_StartCell].Value = "Navn:";
            excelWorksheet.Cells[AddRowAndColumnToCellAddress(_StartCell, 0, 1)].Value = Name;
            excelWorksheet.Cells[AddRowAndColumnToCellAddress(_StartCell, 1, 0)].Value = "ejecutionId:";
            excelWorksheet.Cells[AddRowAndColumnToCellAddress(_StartCell, 1, 1)].Value = ejecutionId;
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


        //------ Functions for converting Excel Files to DMN


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
        /// Get the range from the excel columns index
        /// </summary>
        /// <param name="columnIndexs"></param>
        /// <param name="end"></param>
        /// <returns></returns>
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
                var cellValue = worksheet.Cells[cellName].Value;
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


        /// <summary>
        /// Get the type from the value and return an standard string eith the type
        /// </summary>
        /// <param name="cellValue"></param>
        /// <returns></returns>
        public static string GetCellValueType(object cellValue)
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

        /// <summary>
        /// Get the start and end of the excel columns in numbers
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


        //------ DMN to Excel

        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="inputsColumnsCount"></param>
        /// <param name="outputsColumnsCount"></param>
        /// <returns></returns>
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


        public static void AddTableTitle(string tableName, ExcelWorksheet wsSheet1, string hitPolicy, string tableId)
        {
            wsSheet1.Cells["B1"].Value = "DMN Navn:";
            wsSheet1.Cells["C1"].Value = tableName;
            wsSheet1.Cells["C1"].Style.Locked = false;

            wsSheet1.Cells["B2"].Value = "DMN id:";
            wsSheet1.Cells["C2"].Value = tableId;

            wsSheet1.Cells["B3"].Value = "Hit policy:";
            wsSheet1.Cells["C3"].Value = hitPolicy;
            wsSheet1.Cells["C3"].Style.Locked = false;

            wsSheet1.Cells["B1"].Style.Font.Size = 12;
            wsSheet1.Cells["B1"].Style.Font.Bold = true;
            wsSheet1.Cells["B1"].Style.Font.Italic = true;

            wsSheet1.Cells["B2"].Style.Font.Size = 12;
            wsSheet1.Cells["B2"].Style.Font.Bold = true;
            wsSheet1.Cells["B2"].Style.Font.Italic = true;

            wsSheet1.Cells["B3"].Style.Font.Size = 12;
            wsSheet1.Cells["B3"].Style.Font.Bold = true;
            wsSheet1.Cells["B3"].Style.Font.Italic = true;
        }


        public static void AddTableInputOutputTitle(ExcelWorksheet wsSheet, tDecisionTable decisionTable)
        {
            var totalInput = decisionTable.input.Count();
            var totalOutput = decisionTable.output.Count();

            //input
            var endInputsCellAddress = AddRowAndColumnToCellAddress(_StartCell, 0, totalInput - 1);
            using (ExcelRange rng = wsSheet.Cells[$"{_StartCell}:{endInputsCellAddress}"])
            {
                InputOutputTitleFormat(rng, "Input");
            }

            //Output
            var startOutputsCellAddress = AddRowAndColumnToCellAddress(endInputsCellAddress, 0, 1);
            var endOutputsCellAddress = AddRowAndColumnToCellAddress(endInputsCellAddress, 0, totalOutput + 1);
            using (ExcelRange rng = wsSheet.Cells[$"{startOutputsCellAddress}:{endOutputsCellAddress}"])
            {
                InputOutputTitleFormat(rng, "Output");
            }

        }
        private static void InputOutputTitleFormat(ExcelRange excelRange, string text)
        {
            excelRange.Value = text;
            excelRange.Style.Font.Size = 12;
            excelRange.Style.Font.Bold = true;
            excelRange.Style.Font.Italic = true;
            excelRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            excelRange.Merge = true;
            excelRange.Style.Border.BorderAround(ExcelBorderStyle.Thin);
        }

        public static void CreateExcelTableFromDecisionTable(tDecisionTable decisionTable, ExcelWorksheet wsSheet, string tableName)
        {
            // palse Table in Excel


            // Calculate size of the table
            var totalInput = decisionTable.input.Count();
            var totalOutput = decisionTable.output.Count();
            var totalRules = decisionTable.rule.Count();

            // Create Excel table Header
            var startTableCellAddress = AddRowAndColumnToCellAddress(_StartCell, 1, 0);
            var endTableCellAddress = AddRowAndColumnToCellAddress(startTableCellAddress, totalRules + 1, totalInput + totalOutput);
            using (ExcelRange rng = wsSheet.Cells[$"{startTableCellAddress}:{endTableCellAddress}"])
            {
                ExcelTable table = wsSheet.Tables.Add(rng, tableName);

                //Set Columns Adress and values
                var i = 0;
                foreach (var inputClause in decisionTable.input)
                {
                    var headerCellAddress = AddRowAndColumnToCellAddress(startTableCellAddress, 0, i);
                    wsSheet.Cells[headerCellAddress].Value = inputClause.label;
                    AddContentStyleToCell(wsSheet, headerCellAddress);

                    //add input variableId name
                    var cellAdress = AddRowAndColumnToCellAddress(startTableCellAddress, 1, i);
                    wsSheet.Cells[cellAdress].Value = inputClause.inputExpression?.Item?.ToString();
                    AddIdStyleToCell(wsSheet, cellAdress);

                    i++;
                }

                foreach (var outputClause in decisionTable.output)
                {
                    var headerCellAddress = AddRowAndColumnToCellAddress(startTableCellAddress, 0, i);
                    wsSheet.Cells[headerCellAddress].Value = outputClause.label;
                    AddContentStyleToCell(wsSheet, headerCellAddress);

                    // Add Output variableId name
                    var cellAdress = AddRowAndColumnToCellAddress(startTableCellAddress, 1, i);

                    var variableId = outputClause.name ?? "";
                    wsSheet.Cells[cellAdress].Value = variableId;
                    AddIdStyleToCell(wsSheet, cellAdress);
                    i++;
                }

                // Add empty cell for annotation
                table.Columns[i].Name = "Annotation";

                wsSheet.Cells[AddRowAndColumnToCellAddress(startTableCellAddress, 1, i)].Value = "";
                AddIdStyleToCell(wsSheet, AddRowAndColumnToCellAddress(startTableCellAddress, 1, i));

                //table.ShowHeader = false;
                //table.ShowFilter = true;
                //table.ShowTotal = true;
            }

            for (int ruleCount = 0; ruleCount < decisionTable.rule.Length; ruleCount++)
            {
                var inputEntryLength = decisionTable.rule[ruleCount].inputEntry.Length;
                var outputEntryLength = decisionTable.rule[ruleCount].outputEntry.Length;

                //Add inputs values to table
                for (int inputsCount = 0; inputsCount < inputEntryLength; inputsCount++)
                {
                    var value = decisionTable.rule[ruleCount].inputEntry[inputsCount].text;
                    var cellAdress = AddRowAndColumnToCellAddress(startTableCellAddress, ruleCount + 2, inputsCount);
                    wsSheet.Cells[cellAdress].Value = value;
                    AddContentStyleToCell(wsSheet, cellAdress);

                    wsSheet.Cells[AddRowAndColumnToCellAddress(startTableCellAddress, ruleCount + 2, inputsCount)].Style.Locked = false;
                }

                //Add outpus content values to Table

                for (int outputsCount = 0; outputsCount < outputEntryLength; outputsCount++)
                {
                    var value = decisionTable.rule[ruleCount].outputEntry[outputsCount].Item?.ToString();
                    var cellAdress = AddRowAndColumnToCellAddress(startTableCellAddress, ruleCount + 2, inputEntryLength + outputsCount);
                    wsSheet.Cells[cellAdress].Value = value;
                    AddContentStyleToCell(wsSheet, cellAdress);
                }

                var annotationValue = decisionTable.rule[ruleCount].description;
                var annotationCellAdress = AddRowAndColumnToCellAddress(startTableCellAddress, ruleCount + 2, inputEntryLength + outputEntryLength);
                wsSheet.Cells[annotationCellAdress].Value = annotationValue;
                AddContentStyleToCell(wsSheet, annotationCellAdress);
            }
            wsSheet.Protection.AllowInsertRows = true;
            wsSheet.Protection.AllowDeleteRows = true;
            wsSheet.Protection.IsProtected = true;
            //wsSheet1.Protection.AllowSelectLockedCells = false;
            wsSheet.Cells[wsSheet.Dimension.Address].AutoFitColumns();
        }

        private static void AddIdStyleToCell(ExcelWorksheet wsSheet, string cellAddress)
        {
            var color = Color.FromArgb(250, 199, 111);
            wsSheet.Cells[cellAddress].Style.Fill.PatternType = ExcelFillStyle.Solid;
            wsSheet.Cells[cellAddress].Style.Fill.BackgroundColor.SetColor(color);
            wsSheet.Cells[cellAddress].Style.Locked = true;
        }
        private static void AddContentStyleToCell(ExcelWorksheet wsSheet, string cellAddress)
        {
            wsSheet.Cells[cellAddress].Style.Locked = false;
        }
        private static void AddLockedContentStyleToCell(ExcelWorksheet wsSheet, string cellAddress)
        {
            wsSheet.Cells[cellAddress].Style.Locked = true;
        }

        //----- Data Dictionary

        public static void CreateDmnExcelTableDataDictionary(IEnumerable<DmnInfo> dmns, ExcelWorksheet wsSheet, string tableName, string[] objectPropertyNames)
        {
            // place Table in Excel
            var dmnInfos = dmns as DmnInfo[] ?? dmns.ToArray();
            CreateExcelTable(wsSheet, dmnInfos.Count(), objectPropertyNames.Length - 1, tableName);

            //Create Excel table  and set Header
            AddTableHeader(wsSheet, objectPropertyNames);

            for (int rowIndex = 0; rowIndex < dmnInfos.Count(); rowIndex++)
            {
                AddExcelTableRowData(dmnInfos[rowIndex], rowIndex, wsSheet, objectPropertyNames);
            }

            wsSheet.Cells[wsSheet.Dimension.Address].AutoFitColumns();
        }
        public static void CreateVariablesExcelTableDataDictionary(List<VariablesInfo> variablesInfos, ExcelWorksheet wsSheet, string tableName, string[] objectPropertyNames)
        {
            // place Table in Excel

            CreateExcelTable(wsSheet, variablesInfos.Count(), objectPropertyNames.Length - 1, tableName);

            //Create Excel table  and set Header
            AddTableHeader(wsSheet, objectPropertyNames);

            for (int rowIndex = 0; rowIndex < variablesInfos.Count(); rowIndex++)
            {
                AddExcelTableRowData(variablesInfos[rowIndex], rowIndex, wsSheet, objectPropertyNames);
            }

            wsSheet.Cells[wsSheet.Dimension.Address].AutoFitColumns();
        }
        public static void CreateDMNAndVariablesExcelTableDataDictionary(IEnumerable<DmnInfo> dmns, ExcelWorksheet wsSheet, string tableName, string[] objectPropertyNames)
        {
            // place Table in Excel
            var dmnInfos = dmns as DmnInfo[] ?? dmns.ToArray();

            var totalInputs = dmnInfos.SelectMany(d => d.InputVariablesInfo).ToList();
            var totalOutputs = dmnInfos.SelectMany(d => d.OutputVariablesInfo).ToList();

            var totalRows = totalInputs.Count + totalOutputs.Count;
            CreateExcelTable(wsSheet, totalRows, objectPropertyNames.Length - 1, tableName);

            //Create Excel table  and set Header
            AddTableHeader(wsSheet, objectPropertyNames);



            var rowIndex = 0;
            foreach (var dmnInfo in dmnInfos)
            {
                foreach (var variablesInfo in dmnInfo.InputVariablesInfo)
                {
                    AddVariableInfoExcelTableRowData(dmnInfo, variablesInfo, rowIndex, wsSheet, objectPropertyNames);
                    rowIndex++;
                }
                foreach (var variablesInfo in dmnInfo.OutputVariablesInfo)
                {
                    AddVariableInfoExcelTableRowData(dmnInfo, variablesInfo, rowIndex, wsSheet, objectPropertyNames);
                    rowIndex++;
                }
            }

            wsSheet.Cells[wsSheet.Dimension.Address].AutoFitColumns();
        }
        public static void CreateSummaryExcelTableDataDictionary(IEnumerable<BpmnInfo> bpmns, ExcelWorksheet wsSheet, string tableName, string[] objectPropertyNames)
        {
            var TotalDmns = bpmns.SelectMany(bp => bp.DmnInfos).ToList();
            var totalDmnImputst = TotalDmns.SelectMany(d => d.InputVariablesInfo).ToList().Count;
            var totalDmnsOutpust = TotalDmns.SelectMany(d => d.OutputVariablesInfo).ToList().Count;
            var totalRows = totalDmnImputst + totalDmnsOutpust;

            //var totalRows = totalInputs.Count + totalOutputs.Count;
            CreateExcelTable(wsSheet, totalRows, objectPropertyNames.Length - 1, tableName);

            //Create Excel table  and set Header
            AddTableHeader(wsSheet, objectPropertyNames);

            var rowIndex = 0;
            foreach (var bpmn in bpmns)
            {

                foreach (var dmnInfo in bpmn.DmnInfos)
                {
                    foreach (var variablesInfo in dmnInfo.InputVariablesInfo)
                    {
                        AddSummaryRowInfoToExcelTable(bpmn, dmnInfo, variablesInfo, rowIndex, wsSheet, objectPropertyNames, "input");
                        rowIndex++;
                    }
                    foreach (var variablesInfo in dmnInfo.OutputVariablesInfo)
                    {
                        AddSummaryRowInfoToExcelTable(bpmn, dmnInfo, variablesInfo, rowIndex, wsSheet, objectPropertyNames, "output");
                        rowIndex++;
                    }
                }

            }
            wsSheet.Cells[wsSheet.Dimension.Address].AutoFitColumns();
        }

        private static void CreateExcelTable(ExcelWorksheet wsSheet, int rowNumber, int propertiesNamesCount, string tableName)
        {
            var startTableCellAddress = AddRowAndColumnToCellAddress(_StartCell, 0, 0);
            var endTableCellAddress = AddRowAndColumnToCellAddress(_StartCell, rowNumber, propertiesNamesCount);
            ExcelTable table;
            using (ExcelRange rng = wsSheet.Cells[$"{startTableCellAddress}:{endTableCellAddress}"])
            {
                table = wsSheet.Tables.Add(rng, tableName);
            }
        }

        private static void AddTableHeader(ExcelWorksheet wsSheet, string[] objectPropertyNames)
        {
            var startTableCellAddress = wsSheet.Tables.FirstOrDefault()?.Address.Start.Address;
            //Add data dictionary Headers to Table
            for (int i = 0; i < objectPropertyNames.Length; i++)
            {
                wsSheet.Cells[AddRowAndColumnToCellAddress(startTableCellAddress, 0, i)].Value = objectPropertyNames[i];
            }
        }

        private static void AddExcelTableRowData(object modelData, int rowIndex, ExcelWorksheet wsSheet, string[] dmnFields)
        {
            var startTableCellAddress = wsSheet.Tables.FirstOrDefault()?.Address.Start.Address;

            for (int i = 0; i < dmnFields.Length; i++)
            {
                var value = GetPropertyStringValue(modelData, dmnFields[i]);
                var cellAdress = AddRowAndColumnToCellAddress(startTableCellAddress, rowIndex + 1, i);
                wsSheet.Cells[cellAdress].Value = value;
            }
        }
        private static void AddVariableInfoExcelTableRowData(DmnInfo dmnInfo, VariablesInfo variableInfo, int rowIndex, ExcelWorksheet wsSheet, string[] dmnFields, string variableUseType = null)
        {
            var startTableCellAddress = wsSheet.Tables.FirstOrDefault()?.Address.Start.Address;

            for (int i = 0; i < dmnFields.Length; i++)
            {
                var value = GetPropertyStringValue(dmnInfo, dmnFields[i]) ?? GetPropertyStringValue(variableInfo, dmnFields[i]);
                if (dmnFields[i] == "VariablesUseType" && !string.IsNullOrEmpty(variableUseType))
                {
                    value = variableUseType;
                }
                var cellAdress = AddRowAndColumnToCellAddress(startTableCellAddress, rowIndex + 1, i);
                wsSheet.Cells[cellAdress].Value = value;
            }
        }
        private static void AddSummaryRowInfoToExcelTable(BpmnInfo bpmn, DmnInfo dmnInfo, VariablesInfo variableInfo, int rowIndex, ExcelWorksheet wsSheet, string[] dmnFields, string variableUseType = null)
        {
            var startTableCellAddress = wsSheet.Tables.FirstOrDefault()?.Address.Start.Address;

            for (int i = 0; i < dmnFields.Length; i++)
            {
                var propertyName = dmnFields[i];
                var value = (GetPropertyStringValue(dmnInfo, propertyName) ??
                             GetPropertyStringValue(variableInfo, propertyName)) ??
                            GetPropertyStringValue(bpmn, propertyName);
                if (dmnFields[i] == "VariablesUseType" && !string.IsNullOrEmpty(variableUseType))
                {
                    value = variableUseType;
                }
                var cellAdress = AddRowAndColumnToCellAddress(startTableCellAddress, rowIndex + 1, i);
                wsSheet.Cells[cellAdress].Value = value;
            }
        }

        private static string GetPropertyStringValue(object objectData, string propertyName)
        {
            try
            {
                foreach (String part in propertyName.Split('.'))
                {
                    if (objectData == null) { return null; }
                    Type type = objectData.GetType();
                    PropertyInfo info = type.GetProperty(part);
                    if (info == null) { return null; }
                    objectData = info.GetValue(objectData, null);
                }
                var stringValue = objectData?.ToString();
                return stringValue;
            }
            catch
            {
                return null;
            }
        }

        //------




        //-- Comun to all

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

    }
}

