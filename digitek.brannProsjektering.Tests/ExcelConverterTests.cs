using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.RefAndLookup;
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
            var columnName = ExcelConverter.GetColumnName(28);
            columnName.Should().Be("AB");
        }

        [Fact(DisplayName = "GetRowAndColumIndex Test")]
        public void GetRowAndColumIndexTest()
        {
            var columnName = ExcelConverter.GetRowAndColumIndex("A5");

            columnName.Should().NotBeEmpty();
        }
    }
}
