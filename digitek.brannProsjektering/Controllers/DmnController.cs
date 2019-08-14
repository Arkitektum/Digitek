using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Serialization;
using digitek.brannProsjektering.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Table;

namespace digitek.brannProsjektering.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DmnController : ControllerBase
    {

        [HttpPost, Route("excelToDmn/{inputs}/{outputs}/{haveId}")]
        [Produces("application/json")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public ActionResult<string> PostExcelToDmn(string inputs, string outputs, bool haveId)
        {


            var httpRequest = HttpContext.Request;
            var responsDictionary = new Dictionary<string, string>();
            HttpResponseMessage response = null;

            if (httpRequest.Form.Files.Count != 1)
                return BadRequest(new Dictionary<string, string>() { { "Error:", "Not file fount" } });
            var file = httpRequest.Form.Files[0];
            var file1 = httpRequest.Form.Files.FirstOrDefault();
            if (file != null)
            {
                ExcelWorksheet workSheet;
                ExcelPackage ep;

                try
                {
                    //Open Excel file
                    using (Stream excelFile = file.OpenReadStream())
                    {
                        ep = new ExcelPackage(excelFile);
                    }
                }
                catch (Exception e)
                {
                    return BadRequest(new Dictionary<string, string>() { { "Error:", "Can't Open Excel File" } });
                }

                ExcelTable table;
                string dmnName;
                string dmnId;

                Dictionary<int, Dictionary<string, object>> inputsRulesFromExcel = null;
                Dictionary<int, Dictionary<string, object>> outputsRulesFromExcel = null;
                Dictionary<int, string> annotationsRulesDictionary = null;

                Dictionary<int, string> outputsRulesTypes = null;
                Dictionary<int, string> inputsRulesTypes = null;
                Dictionary<string, Dictionary<string, string>> inputsDictionary = null;
                Dictionary<string, Dictionary<string, string>> outputsDictionary = null;

                using (ExcelWorksheet worksheet = ep.Workbook.Worksheets.FirstOrDefault())
                {
                    if (worksheet == null)
                        return BadRequest(new Dictionary<string, string>() { { file.FileName, "Can't find Excelsheet" } });

                    table = worksheet.Tables.FirstOrDefault();

                    if (table == null)
                        return BadRequest(new Dictionary<string, string>() { { file.FileName, "Excel file don't have a table" } });

                    //Fix cell were to set the information for the DMN table
                    dmnName = (string)(worksheet.Cells["C1"].Value ?? "DMN Table Name");
                    dmnId = (string)(worksheet.Cells["C2"].Value ?? "dmnId");



                    var columnsDictionary = GetTablesIndex(table, inputs, outputs);
                    string[] outputsIndex = null;
                    string[] inputsIndex = null;
                    if (columnsDictionary != null)
                    {
                        columnsDictionary.TryGetValue("outputsIndex", out outputsIndex);
                        columnsDictionary.TryGetValue("inputsIndex", out inputsIndex);
                    }

                    if (inputsIndex == null && outputsIndex == null)
                    {
                        return BadRequest(new Dictionary<string, string>() { { "Error", "Can't get inputs/output rows" } });
                    }

                    try
                    {
                        inputsRulesFromExcel = ExcelConverter.GetTableCellsAdressAndValue(worksheet, inputsIndex, haveId);
                        outputsRulesFromExcel = ExcelConverter.GetTableCellsAdressAndValue(worksheet, outputsIndex, haveId);
                        annotationsRulesDictionary = ExcelConverter.GetTableAnnotationsCellsValue(worksheet, inputsIndex, outputsIndex, haveId);

                        inputsRulesTypes = ExcelConverter.GetTableColumnsType(worksheet, inputsIndex, haveId);
                        outputsRulesTypes = ExcelConverter.GetTableColumnsType(worksheet, outputsIndex, haveId);

                        inputsDictionary = ExcelConverter.GetTableHeader(worksheet, inputsIndex, haveId);
                        outputsDictionary = ExcelConverter.GetTableHeader(worksheet, outputsIndex, haveId);

                        if (!outputsRulesFromExcel.Any() || !inputsRulesFromExcel.Any() ||
                            !outputsRulesTypes.Any() || !inputsRulesTypes.Any() || !inputsDictionary.Any()
                            || !outputsDictionary.Any())
                        {
                            return BadRequest(new Dictionary<string, string>() { { "Error:", "Wrong information to create DMN from Excel" } });
                        }
                    }
                    catch (Exception e)
                    {
                        return BadRequest(new Dictionary<string, string>() { { "Error:", "Can't Get Excel info" } });
                    }
                }



                var filename = Path.GetFileNameWithoutExtension(file.FileName);
                var newDmn = new DmnV1Builder()
                    .AddDefinitionsInfo("Excel2Dmn_" + DateTime.Now.ToString("dd-mm-yy"), filename)
                    .AddDecision(dmnId, dmnName, "decisionTable")
                    .AddInputsToDecisionTable(inputsDictionary, inputsRulesTypes)
                    .AddOutputsToDecisionTable(outputsDictionary, outputsRulesTypes)
                    .AddDecisionRules(inputsRulesFromExcel, outputsRulesFromExcel, annotationsRulesDictionary)
                    .Build();
                // Save DMN 
                try
                {

                    var path = Path.Combine(@"C:\", "ExcelToDmn");

                    Directory.CreateDirectory(path);


                    //var dmnFile = string.Concat(path, filename, "_Exc2Dmn", ".dmn");
                    XmlSerializer xs = new XmlSerializer(typeof(DecisionModelNotation.Shema.tDefinitions));
                    var combine = Path.Combine(path, string.Concat(filename, ".dmn"));
                    using (TextWriter tw = new StreamWriter(combine))
                    {
                        xs.Serialize(tw, newDmn);
                    }

                    return Ok(new Dictionary<string, string>() { { filename + ".dmn", "Created" }, { "Path", combine } });
                }
                catch (Exception e)
                {
                    return BadRequest(new Dictionary<string, string>() { { filename + ".dmn", "Can't be safe" } });

                }
            }
            return Ok(responsDictionary);
        }

        private static Dictionary<string, string[]> GetTablesIndex(ExcelTable table, string inputs, string outputs)
        {
            var inputsIsNumber = int.TryParse(inputs, out var inputsColumnsCount);
            var outputsIsNumber = int.TryParse(outputs, out var outputsColumnsCount);
            Dictionary<string, string[]> dictionary = null;

            if (inputsIsNumber && outputsIsNumber)
            {
                dictionary = ExcelConverter.GetColumnRagngeInLeters(table, inputsColumnsCount, outputsColumnsCount);
            }
            else
            {
                var inputsIndex = inputs.Split(',').ToArray();
                var outputsIndex = outputs.Split(',').ToArray();
                if (inputsIndex.Any() && outputsIndex.Any())
                {
                    dictionary = new Dictionary<string, string[]>()
                    {
                        {"inputsIndex",inputsIndex },
                        {"outputsIndex",outputsIndex },
                        {"anotations",new []{ExcelConverter.GetColumnLetter(1)} }
                    };
                }

            }
            return dictionary;
        }
    }
}