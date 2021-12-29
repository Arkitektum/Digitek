using System;
using System.IO;
using System.Linq;
using digitek.brannProsjektering.Models.Schema;
using DecisionModelNotation.Shema;
using FluentAssertions;
using OfficeOpenXml;
using Xunit;

namespace digitek.brannProsjektering.Tests
{
    public class ExcelConverterTests
    {
        [Fact(DisplayName = "Get excel index Test")]
        public void GetExcelIndex()
        {
            var index = ExcelConverter.GetColumnIndex("AA");
            index.Should().Be(27);
        }
        [Fact(DisplayName = "Get column name Test")]
        public void GetExcelCellName()
        {
            var columnName = ExcelConverter.GetColumnLetter(28);
            columnName.Should().Be("AB");
        }

        [Fact(DisplayName = "GetRowAndColumIndex Test")]
        public void GetRowAndColumIndexTest()
        {
            var columnName = ExcelConverter.GetRowAndColumIndex("A5");

            columnName.Should().NotBeEmpty();
        }
        [Fact(DisplayName = "Integration Test", Skip = "integration test")]
        public void Test1()
        {
            var file = "dmnTest1.dmn";
            string ifcDataFile = Path.Combine(Directory.GetCurrentDirectory() + @"..\..\..\..\Data\", file);
            tDefinitions dmn;
            using (Stream dmnStream = File.Open(ifcDataFile, FileMode.Open))
            {
                dmn = DmnConverter.DeserializeStreamDmnFile(dmnStream);
            }

            var Items = dmn.Items;
            var decision = Items.Where(t => t.GetType() == typeof(tDecision));

            var excelPkg = new ExcelPackage();
            foreach (var tdecision in decision)
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

                }
                catch
                {
                    //
                }
            }

            var filename = Path.GetFileNameWithoutExtension(ifcDataFile);
            var path = string.Concat(@"c:\temp\");
            Directory.CreateDirectory(path);
            var filePath = Path.Combine(path, string.Concat(filename,"new1", ".xlsx"));
            excelPkg?.SaveAs(new FileInfo(filePath));

            File.Exists(filePath).Should().BeTrue();

        }
    }
}
