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
        [HttpGet, Route("GetAvailablesBrannProsjekteringsModels")]
        [Produces("application/json")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult GetAvailablesModels()
        {
            try
            {
                var bpmnModels = GetBmpnAvelabalsModelsType();
                return Ok(bpmnModels);

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
        public IActionResult ConvertJsonArrayToExcel([FromBody] JArray jsonArray, string bpmnModelName, string guid, string userName)
        {
            try
            {


                byte[] fileContents;
                using (var excelPackage = new ExcelPackage())
                {
                    var excelWorksheet = excelPackage.Workbook.Worksheets.Add(bpmnModelName);
                    ExcelConverter.AddWorksheetInfo(ref excelWorksheet, userName, guid);
                    var excelTable = ExcelConverter.AddTableToWorkSheet(ref excelWorksheet, jsonArray, "TableName");
                    ExcelConverter.AddHeadersToExcelTable(excelTable, jsonArray);
                    ExcelConverter.AddDataToTabel(ref excelWorksheet, excelTable, jsonArray);

                    // export it to byte array.
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
                    fileDownloadName: $"{bpmnModelName}_test.xlsx"
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
                    if (property.Name.Equals("typeVirksomhet", StringComparison.OrdinalIgnoreCase))
                    {
                        modelPropertiesDictionary.Add(property.Name, "CodeList");
                    }
                    else
                    {
                        modelPropertiesDictionary.Add(property.Name, propertyTypeName);
                    }
                }
            }


            return modelPropertiesDictionary;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static List<bpmnInformationModel> GetBmpnAvelabalsModelsType()
        {
            var bmpnAvelabalsModels = new List<bpmnInformationModel>();
            foreach (DigiTek17K11Controller.BpmnModels bpmnModel in Enum.GetValues(typeof(DigiTek17K11Controller.BpmnModels)))
            {
                var bpmnModelName = bpmnModel.ToString();
                bpmnInformationModel bpmnInformation = null;
                switch (bpmnModel)
                {
                    case DigiTek17K11Controller.BpmnModels.BranntekniskProsjekteringModel:
                        bpmnInformation = new bpmnInformationModel()
                        {
                            BpmnName = "Brannteknisk prosjektering",
                            BpmnId = bpmnModelName,
                            BpmnInputs = GetModelPropertiesNameAndType(new BranntekniskProsjekteringModel())
                        };
                        break;
                    case DigiTek17K11Controller.BpmnModels.BrannklasseSubModel:
                        bpmnInformation = new bpmnInformationModel()
                        {
                            BpmnName = "Brannklasse",
                            BpmnId = bpmnModelName,
                            BpmnInputs = GetModelPropertiesNameAndType(new BrannklasseModel())
                        };
                        break;
                    case DigiTek17K11Controller.BpmnModels.BrannmotstandSubModel:
                        bpmnInformation = new bpmnInformationModel()
                        {
                            BpmnName = "Brannmotstand",
                            BpmnId = bpmnModelName,
                            BpmnInputs = GetModelPropertiesNameAndType(new BrannmotstandModel())
                        };
                        break;
                    case DigiTek17K11Controller.BpmnModels.BrannseksjonOgBrannmotstandSubModel:
                        bpmnInformation = new bpmnInformationModel()
                        {
                            BpmnName = "Brannseksjon og brannmotstand",
                            BpmnId = bpmnModelName,
                            BpmnInputs = GetModelPropertiesNameAndType(new BrannseksjonOgBrannmotstandModel())
                        };
                        break;
                    case DigiTek17K11Controller.BpmnModels.KravTilBranntiltakSubModel:
                        bpmnInformation = new bpmnInformationModel()
                        {
                            BpmnName = "Krav tilBranntiltak",
                            BpmnId = bpmnModelName,
                            BpmnInputs = GetModelPropertiesNameAndType(new KravTilBranntiltakModel())
                        };
                        break;
                    case DigiTek17K11Controller.BpmnModels.RisikoklasseSubModel:
                        bpmnInformation = new bpmnInformationModel()
                        {
                            BpmnName = "Risikoklasse",
                            BpmnId = bpmnModelName,
                            BpmnInputs = GetModelPropertiesNameAndType(new RisikoklasseModel())
                        };
                        break;
                }
                bmpnAvelabalsModels.Add(bpmnInformation);
            }
            return bmpnAvelabalsModels;
        }
    }
}