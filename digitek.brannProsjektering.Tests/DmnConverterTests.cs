using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using DecisionModelNotation.Shema;
using digitek.brannProsjektering.Controllers;
using digitek.brannProsjektering.Models;
using FluentAssertions;
using OfficeOpenXml;
using Xunit;

namespace digitek.brannProsjektering.Tests
{
   public class DmnConverterTests
    {
        [Fact(DisplayName = "Add dmn Variable info - Test")]
        public void GetRowAndColumIndexTest()
        {
            DmnInfo dmnInfo = new DmnInfo()
            {
                DmnId = "id_01",
                DmnName = "Dmn_Test",
                FileName = "Dmn_test.dmn"
            };
            DmnConverter.AddVariablesToDictionary(ref dmnInfo, "col_01", "Column 01", "String", "input");
            DmnConverter.AddVariablesToDictionary(ref dmnInfo, "col_02", "Column 02", "String", "input");
            DmnConverter.AddVariablesToDictionary(ref dmnInfo, "col_03", "Column 03", "String", "input");

            dmnInfo.InputVariablesInfo.Length.Should().Be(3);
        }
        [Fact(DisplayName = "INTEGRATION test - Create Data Dictionary From DMN & BPMN models", Skip = "integration test")]
        public void Test1()
        {

            var file = "dmnTest1.dmn";
            var file2 = "dmnTest2.dmn";
            var file3 = "BpmnTest01.bpmn";
            var dmns = new List<tDefinitions>();
            var filePath1 = Path.Combine(Directory.GetCurrentDirectory() + @"..\..\..\..\Data\", file);
            var filePath2 = Path.Combine(Directory.GetCurrentDirectory() + @"..\..\..\..\Data\", file2);
            var bpmn = Path.Combine(Directory.GetCurrentDirectory() + @"..\..\..\..\Data\", file3);

            XDocument bpmnXml = XDocument.Load(bpmn);

            using (Stream dmnStream = File.Open(filePath1, FileMode.Open))
            {
                dmns.Add(DmnConverter.DeserializeStreamDmnFile(dmnStream));
            }
            using (Stream dmnStream = File.Open(filePath2, FileMode.Open))
            {
                dmns.Add(DmnConverter.DeserializeStreamDmnFile(dmnStream));
            }

            var dmnDataDictionaryModels = new List<DmnInfo>();


            var excelPkg = new ExcelPackage();
            foreach (var tdefinitions in dmns)
            {
                var Items = tdefinitions.Items;
                var decision = Items.Where(t => t.GetType() == typeof(tDecision));

                foreach (tDecision tdecision in decision)
                {
                    tDecisionTable decisionTable = null;
                    try
                    {
                        DmnConverter.GetDecisionsVariables(tdecision, Path.GetFileNameWithoutExtension(filePath1),
                            ref dmnDataDictionaryModels);
                    }
                    catch
                    {
                        //
                    }
                }

            }

            var bpmnDataDictionary = new List<BpmnInfo>();
            DmnConverter.GetDmnInfoFromBpmnModel(bpmnXml, dmnDataDictionaryModels, ref bpmnDataDictionary);

            //List<DataDictionaryModel> dataDictionaryModels = new List<DataDictionaryModel>();
            foreach (var dmnData in dmnDataDictionaryModels)
            {
                var submodel = new BpmnInfo();
                try
                {

                    var value = dmnData.GetType();
                    var property = value.GetProperty("DmnId");
                    String name = (String)(property.GetValue(dmnData, null));

                    submodel = bpmnDataDictionary.Single(b => b.DmnId == "sdsds");

                }
                catch
                {
                }

                //dataDictionaryModels.Add(new DataDictionaryModel()
                //{
                //    BpmnData = submodel,
                //    DmnData = dmnData
                //});

            }

            ExcelWorksheet wsSheet = excelPkg.Workbook.Worksheets.Add("DmnTEK");
            //Filter all dmn by Id
            var dmnIds = dmnDataDictionaryModels.GroupBy(x => x.DmnId).Select(y => y.First());
            var objectPropertyNames = new[] { "DmnId", "DmnName", "TekKapitel", "TekLedd", "TekTabell", "TekForskriften", "TekWebLink" };
            ExcelConverter.CreateDmnExcelTableDataDictionary(dmnIds, wsSheet, "dmnTek", objectPropertyNames);

            ExcelWorksheet wsSheet1 = excelPkg.Workbook.Worksheets.Add("Variables");
            var dmnVariablesIds = DmnConverter.GetVariablesFormDmns(dmnDataDictionaryModels);
            var dmnVariablesIdstPropertyNames = new[] { "VariabelId", "VariabelNavn", "VariabelBeskrivelse","IFC4","IfcUrl" };
            ExcelConverter.CreateVariablesExcelTableDataDictionary(dmnVariablesIds, wsSheet1, "Variables", dmnVariablesIdstPropertyNames);

            ExcelWorksheet wsSheet2 = excelPkg.Workbook.Worksheets.Add("Dmn+Variables");
            var objectPropertyNames1 = new[] { "DmnId", "VariabelId", "VariabelType" };
            ExcelConverter.CreateDMNAndVariablesExcelTableDataDictionary(dmnIds, wsSheet2, "Dmn+Variables", objectPropertyNames1);



            ExcelWorksheet wsSheet3 = excelPkg.Workbook.Worksheets.Add("summary");
            var summaryPropertyNames = new[] { "FileName", "BpmnId", "DmnId", "VariabelId", "VariabelType", "VariablesUseType", "Kilde" };

            ExcelConverter.CreateSummaryExcelTableDataDictionary(bpmnDataDictionary,wsSheet3, "summary", summaryPropertyNames);

            var path = string.Concat(@"c:\temp\");
            Directory.CreateDirectory(path);
            var filePath = Path.Combine(path, string.Concat("dataDictionary", ".xlsx"));
            excelPkg?.SaveAs(new FileInfo(filePath));

            File.Exists(filePath).Should().BeTrue();

        }


    }
}
