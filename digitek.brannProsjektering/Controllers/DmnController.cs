﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using digitek.brannProsjektering.Builder;
using digitek.brannProsjektering.Models;
using digitek.brannProsjektering.Models.Schema;
using digitek.brannProsjektering.services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Table;


namespace digitek.brannProsjektering.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class DmnController : ControllerBase
    {

        private IHostingEnvironment _hostingEnvironment;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        public DmnController(IHostingEnvironment environment)
        {
            _hostingEnvironment = environment;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputs"></param>
        /// <param name="outputs"></param>
        /// <param name="haveId"></param>
        /// <returns></returns>
        [HttpPost, Route("excelToDmn/{inputs}/{outputs}/{haveId}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult PostExcelToDmn(string inputs, string outputs, bool haveId)
        {


            var httpRequest = HttpContext.Request;
            if (httpRequest.Form.Files.Count != 1)
                return BadRequest(new Dictionary<string, string>() { { "Error:", "Not file fount" } });
            var file = httpRequest.Form.Files[0];
            if (file != null)
            {
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

                string dmnName;
                string dmnId;

                Dictionary<int, Dictionary<string, object>> inputsRulesFromExcel;
                Dictionary<int, Dictionary<string, object>> outputsRulesFromExcel;
                Dictionary<int, string> annotationsRulesDictionary;

                Dictionary<int, string> outputsRulesTypes;
                Dictionary<int, string> inputsRulesTypes;
                Dictionary<string, Dictionary<string, string>> inputsDictionary;
                Dictionary<string, Dictionary<string, string>> outputsDictionary;

                using (ExcelWorksheet worksheet = ep.Workbook.Worksheets.FirstOrDefault())
                {
                    if (worksheet == null)
                        return BadRequest(new Dictionary<string, string>() { { file.FileName, "Can't find Excelsheet" } });

                    var table = worksheet.Tables.FirstOrDefault();

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

                // Create DMN Stream response
                try
                {


                    MemoryStream stream = new MemoryStream();
                    StreamWriter sw = new StreamWriter(stream, Encoding.UTF8);

                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(tDefinitions));
                    xmlSerializer.Serialize(sw, newDmn);

                    stream.Position = 0;
                    return File(stream, "aplication/dmn", $"{filename}.dmn");
                }
                catch (Exception e)
                {
                    return BadRequest(new Dictionary<string, string>() { { filename + ".dmn", "Unable Stream dmn file" } });

                }
            }
            else
            {
                return BadRequest(new Dictionary<string, string>() { { "Error", "Can't find any file" } });

            }
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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost, Route("dmnToExcel")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult PosDmnToExcel()
        {
            var httpRequest = HttpContext.Request;

            var httpFiles = httpRequest.Form.Files;
            var errorDictionary = new Dictionary<string, string>();

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
                    if (file != null) return BadRequest(new Dictionary<string, string>() { { file.FileName + ".dmn", "Can't Deserialize DMN file" } });
                }

                // check if DMN have desicion table
                var items = dmn.Items;
                var decision = items.Where(t => t.GetType() == typeof(tDecision));
                var tDrgElements = decision as tDRGElement[] ?? decision.ToArray();
                if (!tDrgElements.Any())
                {
                    if (file != null) return BadRequest(new Dictionary<string, string>() { { file.FileName + ".dmn", "Dmn file have non Decision tables" } });
                }

                // create Excel Package
                var excelPkg = new ExcelPackage();
                foreach (var tdecision in tDrgElements)
                {
                    try
                    {
                        var dt = ((tDecision)tdecision).Item;
                        var decisionTable = (tDecisionTable)Convert.ChangeType(dt, typeof(tDecisionTable));
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
                        if (file != null) return BadRequest(new Dictionary<string, string>() { { file.FileName + ".dmn", "Can't be create ExcelPackage" } });
                    }
                }

                // Create Excel Stream response
                try
                {
                    if (file != null)
                    {
                        var filename = Path.GetFileNameWithoutExtension(file.FileName);
                        excelPkg.Save();
                        var fileStream = excelPkg.Stream;
                        fileStream.Flush();
                        fileStream.Position = 0;

                        return File(fileStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{filename}.xlsx");
                    }
                }
                catch
                {
                    if (file != null) errorDictionary.Add(file.FileName, "Can't create excel Stream response");
                }


            }
            else
            {
                return BadRequest(new Dictionary<string, string>() { { "Error", "Can't convert more than one file." } });
            }

            return Ok(new Dictionary<string, string>() { { "Error", "No data to process." } });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost, Route("GetBPMNDataDictionaryToExcel")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult PostModelsToExcelDataDictionary()
        {
            var httpRequest = HttpContext.Request;

            var httpFiles = httpRequest.Form.Files;
            var errorDictionary = new Dictionary<string, string>();
            var dmnInfoList = new List<DmnInfo>();
            var bpmnDataDictionaryModels = new List<BpmnInfo>();
            //var dataDictionaryModels = new List<DataDictionaryModel>();
            var dataDictionaryModels = new Dictionary<BpmnInfo, List<DmnInfo>>();

            if (httpFiles == null && !httpFiles.Any())
                return NotFound("Can't find any file");
            var dmnFiles = httpFiles.Where(f => Path.GetExtension(f.FileName) == ".dmn");
            var bpmFiles = httpFiles.Where(f => Path.GetExtension(f.FileName) == ".bpmn");


            var dmnFormsFiles = dmnFiles as IFormFile[] ?? dmnFiles.ToArray();
            var bpmnFormFiles = bpmFiles as IFormFile[] ?? bpmFiles.ToArray();
            if (!dmnFormsFiles.Any() || !bpmnFormFiles.Any())
                return BadRequest("They must be one BPMN and at least one DMN to create the data dictionary");


            //get information drom DMN files
            foreach (var dmnFile in dmnFormsFiles)
            {
                string stringXml = string.Empty;
                using (Stream dmnStream = dmnFile.OpenReadStream())
                {
                    // convert stream to string
                    StreamReader reader = new StreamReader(dmnStream);
                    dmnStream.Seek(0, SeekOrigin.Begin);
                    stringXml = reader.ReadToEnd();
                }


                //Deserialize DMN 1.2 file
                try
                {
                    var dmn = new SerializeUtil().DeserializeByggesakFromString<tDefinitions>(stringXml);
                    if (dmn != null)
                    {
                        // check if DMN have desicion table
                        var items = dmn.Items;
                        var decision = items.Where(t => t.GetType() == typeof(tDecision));

                        var tdecisions = decision as tDRGElement[] ?? decision.ToArray();
                        if (!tdecisions.Any())
                        {
                            errorDictionary.Add(dmnFile.FileName, "Dmn file have non decision");
                            continue;
                        }
                        //Add Dmn info
                        foreach (var tDrgElement in tdecisions)
                        {
                            var tdecision = (tDecision)tDrgElement;
                            try
                            {
                                DmnConverter.GetDecisionsVariablesFormDmnV11(tdecision, Path.GetFileNameWithoutExtension(dmnFile.FileName), ref dmnInfoList);
                            }
                            catch
                            {
                                errorDictionary.Add(dmnFile.FileName, "Can't add variables info from DMN");
                            }
                        }
                    }
                }
                catch 
                {
                    //errorDictionary.Add(dmnFile.FileName, "DMN Can't be Deserialize. DMN Version 1.2 read more: https://www.omg.org/spec/DMN/1.2");
                    //continue;
                }

                //Deserialize DMN 1.3 file
                try
                {
                var dmnV13 = new SerializeUtil().DeserializeByggesakFromString<digitek.brannProsjektering.Models.Schema.DmnV13.tDefinitions>(stringXml);
                    if (dmnV13 != null)
                    {
                        // check if DMN have desicion table
                        var items = dmnV13.Items;
                        var decision = items.Where(t => t.GetType() == typeof(digitek.brannProsjektering.Models.Schema.DmnV13.tDecision));

                        var tdecisions = decision as digitek.brannProsjektering.Models.Schema.DmnV13.tDRGElement[] ?? decision.ToArray();
                        if (!tdecisions.Any())
                        {
                            errorDictionary.Add(dmnFile.FileName, "Dmn file have non decision");
                            continue;
                        }
                        //Add Dmn info
                        foreach (var tDrgElement in tdecisions)
                        {
                            var tdecision = (digitek.brannProsjektering.Models.Schema.DmnV13.tDecision)tDrgElement;
                            try
                            {
                                Dmnv13Services.GetDecisionsVariablesFormDmnV13(tdecision, Path.GetFileNameWithoutExtension(dmnFile.FileName), ref dmnInfoList);
                            }
                            catch
                            {
                                errorDictionary.Add(dmnFile.FileName, "Can't add variables info from DMN");
                            }
                        }
                    }
                }
                catch 
                {
                    //errorDictionary.Add(dmnFile.FileName, "DMN Can't be Deserialize. DMN Version 1.2 read more: https://www.omg.org/spec/DMN/1.2");
                    //continue;
                }


                
            }

            foreach (var bpmFile in bpmnFormFiles)
            {
                try
                {
                    XDocument bpmnXml;
                    using (Stream dmnfile = bpmFile.OpenReadStream())
                    {
                        bpmnXml = XDocument.Load(dmnfile);
                    }
                    if (bpmnXml != null)
                    {
                        try
                        {
                            DmnConverter.GetDmnInfoFromBpmnModel(bpmnXml, dmnInfoList, ref bpmnDataDictionaryModels);
                        }
                        catch
                        {
                            errorDictionary.Add(bpmFile.FileName, "Can't add serialize bpmn to Data Model Dictionary");
                        }
                    }
                }
                catch
                {
                    errorDictionary.Add(bpmFile.FileName, "BPMN Can't be Deserialize. BPMN Version 2.0.2 read more: https://www.omg.org/spec/BPMN/2.0.2");
                }
            }

            // create Excel Package
            ExcelPackage excelPkg = null;
            try
            {
                excelPkg = new ExcelPackage();
                ExcelWorksheet wsSheet = excelPkg.Workbook.Worksheets.Add("DmnTEK");
                var dmnIds = dmnInfoList.GroupBy(x => x.DmnId).Select(y => y.First());
                var objectPropertyNames = new[] { "DmnId", "DmnName", "TekKapitel", "TekLedd", "TekTabell", "TekForskriften", "TekWebLink" };
                var dmnInfos = dmnIds as DmnInfo[] ?? dmnIds.ToArray();
                ExcelConverter.CreateDmnExcelTableDataDictionary(dmnInfos, wsSheet, "dmnTek", objectPropertyNames);

                ExcelWorksheet wsSheet1 = excelPkg.Workbook.Worksheets.Add("Variables");
                var dmnVariablesIds = DmnConverter.GetVariablesFormDmns(dmnInfoList);
                var dmnVariablesIdstPropertyNames = new[] { "VariabelId", "VariabelName", "VariabelBeskrivelse", "IFC4", "IfcURL" };
                ExcelConverter.CreateVariablesExcelTableDataDictionary(dmnVariablesIds, wsSheet1, "Variables", dmnVariablesIdstPropertyNames);


                ExcelWorksheet wsSheet2 = excelPkg.Workbook.Worksheets.Add("Dmn+Variables");
                var objectPropertyNames1 = new[] { "DmnId", "VariabelId", "VariabelType" };
                ExcelConverter.CreateDMNAndVariablesExcelTableDataDictionary(dmnInfos, wsSheet2, "Dmn+Variables", objectPropertyNames1);



                ExcelWorksheet wsSheet3 = excelPkg.Workbook.Worksheets.Add("summary");
                var summaryPropertyNames = new[] { "FileName", "BpmnId", "DmnId", "VariabelId", "VariabelType", "VariablesUseType" };

                ExcelConverter.CreateSummaryExcelTableDataDictionary(bpmnDataDictionaryModels, wsSheet3, "summary", summaryPropertyNames);
            }
            catch
            {
                errorDictionary.Add("Error", "Can't create Excel file");
            }

            // Create Excel Stream response
            var filename = Path.GetFileNameWithoutExtension("Bpmn&Dmn Data Dictionary");
            Stream fileStream = null;
            try
            {

                excelPkg.Save();
                fileStream = excelPkg.Stream;

            }
            catch
            {

                errorDictionary.Add(filename, "Can't create excel Stream response");
            }

            if (errorDictionary.Any())
                return BadRequest(errorDictionary);

            fileStream.Flush();
            fileStream.Position = 0;
            return File(fileStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{filename}.xlsx");

        }


    }
}