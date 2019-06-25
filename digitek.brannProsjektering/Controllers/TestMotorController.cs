using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using digitek.brannProsjektering.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace digitek.brannProsjektering.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestMotorController : ControllerBase
    {
        /// <returns></returns>
        [HttpGet, Route("GetAvailablesBrannProsjekteringModels2")]
        [Produces("application/json")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult GetAvailablesModels()
        {
            try
            {
                var bpmnModels = GetBmpnAvelabalsModelsType();

                var bpmnInformationList = new List<bpmnInformationModel>();
                foreach (var bpmnModel in bpmnModels)
                {
                    Dictionary<string, string> modelProperties = GetModelPropertiesNameAndType(bpmnModel.Value);
                    bpmnInformationList.Add(new bpmnInformationModel()
                    {
                        BpmnName = bpmnModel.Key,
                        BpmnInpust = modelProperties
                    });

                }
                return Ok(bpmnInformationList);

            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }

        }
        /// <returns></returns>
        [HttpPost, Route("ConverJsonArrayToExcel")]
        [Consumes("application/Json")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult ConvertJsonArrayToExcel([FromBody] JArray jsonArray, string bpmnModelName, string guid)
        {
            try
            {

   
                byte[] fileContents;
                using (var excelPackage = new ExcelPackage())
                {
                    var ExcelWorksheet = excelPackage.Workbook.Worksheets.Add("brannProsjektering");
                    ExcelConverter.AddWorksheetInfo(ref ExcelWorksheet, "Test", "GUID");
                    var excelTable = ExcelConverter.AddTableToWorkSheet(ref ExcelWorksheet, jsonArray, "TableName");
                    ExcelConverter.AddHeadersToExcelTable(excelTable, jsonArray);
                    ExcelConverter.AddDataToTabel(ref ExcelWorksheet, excelTable, jsonArray);
                    // So many things you can try but you got the idea.

                    // Finally when you're done, export it to byte array.
                    fileContents = excelPackage.GetAsByteArray();
                }

                if (fileContents == null || fileContents.Length == 0)
                {
                    return NotFound();
                }

                //https://stackoverflow.com/a/50259742
                return File(
                    fileContents: fileContents,
                    contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileDownloadName: "test.xlsx"
                );

            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetModelPropertiesNameAndType(object model)
        {
            var modelPropertiesDictionary = new Dictionary<string, string>();
            var modelProperties = model.GetType().GetProperties();

            if (modelProperties != null)
            {
                foreach (var property in modelProperties)
                {
                    //GET general type for nullable variables
                    var modelGenericType = property.PropertyType.GenericTypeArguments;

                    string propertyTypeName = modelGenericType.Any() ? modelGenericType?.First().Name : property.PropertyType.Name;
                    modelPropertiesDictionary.Add(property.Name, propertyTypeName);
                }
            }


            return modelPropertiesDictionary;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, object> GetBmpnAvelabalsModelsType()
        {
            var bmpnAvelabalsModels = new Dictionary<string, object>();
            foreach (DigiTek17K11Controller.BpmnModels bpmnModel in Enum.GetValues(typeof(DigiTek17K11Controller.BpmnModels)))
            {
                var bpmnModelName = bpmnModel.ToString();
                switch (bpmnModel)
                {
                    case DigiTek17K11Controller.BpmnModels.BranntekniskProsjekteringModel:
                        bmpnAvelabalsModels.Add(bpmnModelName, new BranntekniskProsjekteringModel());
                        break;
                    case DigiTek17K11Controller.BpmnModels.BrannklasseSubModel:
                        bmpnAvelabalsModels.Add(bpmnModelName, new BrannklasseModel());
                        break;
                    case DigiTek17K11Controller.BpmnModels.BrannmotstandSubModel:
                        bmpnAvelabalsModels.Add(bpmnModelName, new BrannmotstandModel());
                        break;
                    case DigiTek17K11Controller.BpmnModels.BrannseksjonOgBrannmotstandSubModel:
                        bmpnAvelabalsModels.Add(bpmnModelName, new BrannseksjonOgBrannmotstandModel());

                        break;
                    case DigiTek17K11Controller.BpmnModels.KravTilBranntiltakSubModel:
                        bmpnAvelabalsModels.Add(bpmnModelName, new KravTilBranntiltakModel());
                        break;
                    case DigiTek17K11Controller.BpmnModels.RisikoklasseSubModel:
                        bmpnAvelabalsModels.Add(bpmnModelName, new RisikoklasseModel());

                        break;
                }
            }

            return bmpnAvelabalsModels;
        }
    }
}