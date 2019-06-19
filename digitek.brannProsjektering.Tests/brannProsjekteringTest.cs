using System;
using System.Collections.Generic;
using System.Text;
using digitek.brannProsjektering.Controllers;
using digitek.brannProsjektering.Models;
using Xunit;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace digitek.brannProsjektering.Tests
{
    public class brannProsjekteringTest
    {
        [Fact(DisplayName = "Integration Test")]
        public void Test1()
        {
            BranntekniskProsjekteringModel branntekniskProsjektering = new BranntekniskProsjekteringModel();
            //Dictionary<string, string> modelProperties = DigiTek17K11Controller.GetModelPropertiesNameAndType(branntekniskProsjektering);
            var bpmnModels = DigiTek17K11Controller.GetBmpnAvelabalsModelsType();

            var bpmnInformationList = new List<bpmnInformationModel>();
            foreach (var bpmnModel in bpmnModels)
            {
                Dictionary<string, string> modelProperties = DigiTek17K11Controller.GetModelPropertiesNameAndType(bpmnModel.Value);
                bpmnInformationList.Add(new bpmnInformationModel()
                {
                    BpmnName = bpmnModel.Key,
                    BpmnInpust = modelProperties
                });

            }

            var bpmnModelsJson = JsonConvert.SerializeObject(bpmnInformationList);

            var jsonArray = JArray.Parse(bpmnModelsJson);

            bpmnModels.Should().NotBeEmpty();

        }
    }
}
