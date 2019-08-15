using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Serialization;
using DecisionModelNotation.Shema;
using digitek.brannProsjektering.Builder;
using Microsoft.AspNetCore.Hosting;
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

        private IHostingEnvironment _hostingEnvironment;

        public DmnController(IHostingEnvironment environment)
        {
            _hostingEnvironment = environment;
        }



        [HttpPost, Route("excelToDmn/{inputs}/{outputs}/{haveId}")]
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
                FileStream streamContent;
                string fullPath;

                // Save DMN 
                try
                {
                    // https://mariusschulz.com/blog/getting-the-web-root-path-and-the-content-root-path-in-asp-net-core#asp-net-core
                    string contentRootPath = _hostingEnvironment.ContentRootPath;
                    fullPath = Path.Combine(contentRootPath, "DataTemp", $"{filename}.dmn");

                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(DecisionModelNotation.Shema.tDefinitions));
                    using (TextWriter textWriter = new StreamWriter(fullPath))
                    {
                        xmlSerializer.Serialize(textWriter, newDmn);
                    }

                    //return Ok(new Dictionary<string, string>() { { filename + ".dmn", "Created" }, { "Path", fullPath } });
                }
                catch (Exception e)
                {
                    return BadRequest(new Dictionary<string, string>() { { filename + ".dmn", "Unable to save file" } });

                }

                try
                {
                    //https://stackoverflow.com/a/14763098
                    var bytes = System.IO.File.ReadAllBytes(fullPath);
                    System.IO.File.Delete(fullPath);
                    return File(bytes, "application/dmn", $"{filename}.dmn");
                }
                catch (Exception e)
                {
                    return BadRequest(new Dictionary<string, string>() { { filename + ".dmn", "Unable get file from server" } });
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

        [HttpPost, Route("dmnToExcel")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult PosDmnToExcel()
        {
            var httpRequest = HttpContext.Request;
            HttpResponseMessage response = null;

            string okResponsText = null;
            var httpFiles = httpRequest.Form.Files;
            var okDictionary = new Dictionary<string, string>();
            var ErrorDictionary = new Dictionary<string, string>();

            if (httpFiles == null && !httpFiles.Any())
                return NotFound("Can't find any file");

            if (httpFiles.Count == 1)
            {

                var file = httpFiles.FirstOrDefault();

                tDefinitions dmn = null;

                //Deserialize DMN file
                if (file != null)
                {
                    using (Stream dmnfile = file.OpenReadStream())
                    {
                        dmn = DmnConverter.DeserializeStreamDmnFile(dmnfile);
                    }
                }
                if (dmn == null)
                {
                    return BadRequest(new Dictionary<string, string>() { { file.FileName + ".dmn", "Can't Deserialize DMN file" } });
                }

                // check if DMN have desicion table
                var items = dmn.Items;
                var decision = items.Where(t => t.GetType() == typeof(tDecision));
                var tDrgElements = decision as tDRGElement[] ?? decision.ToArray();
                if (!tDrgElements.Any())
                {
                    return BadRequest(new Dictionary<string, string>() { { file.FileName + ".dmn", "Dmn file have non Decision tables" } });
                }

                // create Excel Package
                ExcelPackage excelPkg = null;
                excelPkg = new ExcelPackage();
                foreach (var tdecision in tDrgElements)
                {
                    tDecisionTable decisionTable = null;
                    try
                    {
                        var dt = ((tDecision)tdecision).Item;
                        decisionTable = (tDecisionTable)Convert.ChangeType(dt, typeof(tDecisionTable));
                        ExcelWorksheet wsSheet = excelPkg.Workbook.Worksheets.Add(tdecision.id);
                        //Add Table Title
                        ExcelConverter.AddTableTitle(tdecision.name, wsSheet, decisionTable.hitPolicy.ToString(), tdecision.id);
                        // Add "input" and "output" headet to Excel table
                        ExcelConverter.AddTableInputOutputTitle(wsSheet, decisionTable);
                        //Add DMN Table to excel Sheet
                        ExcelConverter.CreateExcelTableFromDecisionTable(decisionTable, wsSheet, tdecision.id);
                        wsSheet.Protection.IsProtected = true;

                    }
                    catch
                    {
                        return BadRequest(new Dictionary<string, string>() { { file.FileName + ".dmn", "Can't be create ExcelPackage" } });
                    }
                }

                // Save Excel Package
                string fullPath = null;
                var filename = Path.GetFileNameWithoutExtension(file.FileName);

                try
                {

                    string contentRootPath = _hostingEnvironment.ContentRootPath;

                    fullPath = Path.Combine(contentRootPath, "DataTemp", $"{filename}.xlsx");
                    excelPkg.SaveAs(new FileInfo(fullPath));

                }
                catch
                {

                    ErrorDictionary.Add(file.FileName, "Can't be saved excel file in server");
                }

                try
                {
                    //https://stackoverflow.com/a/14763098
                    var bytes = System.IO.File.ReadAllBytes(fullPath);
                    System.IO.File.Delete(fullPath);
                    return File(bytes, "application/dmn", $"{filename}.xlsx");
                }
                catch (Exception e)
                {
                    return BadRequest(new Dictionary<string, string>() { { filename + ".xlsx", "Unable get file from server." } });
                }

            }
            else
            {
                return BadRequest(new Dictionary<string, string>() { { "Error", "Can't convert more than one file." } });
            }

        }

    }
}