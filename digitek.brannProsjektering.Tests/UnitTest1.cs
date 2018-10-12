using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CamundaClient;
using CamundaClient.Dto;
using CamundaClient.Service;
using FluentAssertions;
using Xunit;
using digitek.brannProsjektering.Models;
using digitek.brannProsjektering.Worker;

namespace digitek.brannProsjektering.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var branntekniskProsjektering = new BranntekniskProsjekteringVariables()
            {
                typeVirksomhet = "Bolig",
            };

            var dictionary = branntekniskProsjektering.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(branntekniskProsjektering, null));

            var camunda = new CamundaEngineClient(new System.Uri("http://localhost:8080/engine-rest/engine/default/"), null, null);

            // Deploy the models under test
            string deploymentId = camunda.RepositoryService.Deploy("testcase", new List<object> {
                FileParameter.FromManifestResource(Assembly.GetExecutingAssembly(), "digitek.brannProsjektering.Tests.TestData.RisikoklassenModelDotNet.bpmn")});
            var assemblys = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.FullName.Contains("digitek.brannProsjektering"));
            var assembly = (Assembly)assemblys.FirstOrDefault();
            try
            {

                // Start Instance
                string processInstanceId =
                    camunda.BpmnWorkflowService.StartProcessInstance("RisikoklassenModel.Net", dictionary);
                //var processInstanceId = new BpmnWorkflowService(camunda).StartProcessInstance("RisikoklassenModel.Net", dictionary);

                processInstanceId.Should().NotBeNullOrEmpty();
                
                // Get process variables and set to ExternalTask variables
                var externalTask = new ExternalTask();
                var modelVariables = new Dictionary<string,object>();

                //Check that External Task for Inputs Validation is there
                var externalTasks = camunda.ExternalTaskService.FetchAndLockTasks("testcase", 100, "brannInputsValidation", 1000, new List<string>());
                externalTasks.Count.Should().Be(1);
                externalTasks.First().ActivityId.Should().Be("branninputsVariablesValidation");
                camunda.ExternalTaskService.Complete("testcase", externalTasks.First().Id, dictionary);

                var processVariables = camunda.BpmnWorkflowService.GetProcessVariables(processInstanceId);
                externalTask.Variables = CamundaClientHelper.ConvertVariables(processVariables);

                //validate Input method
                new InputsValidationWorker().Execute(externalTask, ref modelVariables);
                if (modelVariables.TryGetValue("modelInputs", out var modelInputs))
                    externalTask.Variables.Add("modelInputs", new Variable() { Value = modelInputs });

                // Check that External Task for output Consolidation is there
                externalTasks = camunda.ExternalTaskService.FetchAndLockTasks("testcase", 100, "outputConsolidation", 1000, new List<string>());
                externalTasks.Count.Should().Be(1);
                externalTasks.First().ActivityId.Should().Be("VariablesOutputConsolidation");
                camunda.ExternalTaskService.Complete("testcase", externalTasks.First().Id, new Dictionary<string, object>());
                
                // Consolidate Outputs method
                new OutputConsolidation().Execute(externalTask, ref modelVariables);
                if (modelVariables.TryGetValue("modelOutputs", out var modelOutputs))
                    externalTask.Variables.Add("modeloOutputs",new Variable(){Value = modelOutputs });



                // Check that External Task for data Dictionary is there
                externalTasks = camunda.ExternalTaskService.FetchAndLockTasks("testcase", 100, "modelOutputDataDictionary", 1000, new List<string>());
                externalTasks.Count.Should().Be(1);
                externalTasks.First().ActivityId.Should().Be("dataDictionary");
                camunda.ExternalTaskService.Complete("testcase", externalTasks.First().Id, new Dictionary<string, object>());

                //Creat Dictionary to model method
                new ModelOutputsDataDictionary().Execute(externalTask, ref modelVariables);
                if (modelVariables.TryGetValue("modelDataDictionary", out var modelDataDictionary))
                    externalTask.Variables.Add("modelDataDictionary", new Variable() { Value = modelDataDictionary });

            }
            finally
            {
                // cleanup after test case
                camunda.RepositoryService.DeleteDeployment(deploymentId);
            }
        }

        [Fact]
        public void RisikoklassenModelTest()
        {
            var branntekniskProsjektering = new BranntekniskProsjekteringVariables()
            {
                typeVirksomhet = "Sykehus",
            };
            var key = "RisikoklassenModel.Net";

            var dictionary = branntekniskProsjektering.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(branntekniskProsjektering, null));
            var camunda = new CamundaEngineClient();
            var id = camunda.BpmnWorkflowService.StartProcessInstance(key, dictionary);
            var responce = camunda.BpmnWorkflowService.GetProcessVariables(id);
            var newDict = new Dictionary<string,object>();
            if (responce != null && responce.Any())
            {
                 newDict = responce.Where(value => value.Key.Contains("modelOutputs") || value.Key.Contains("modelDataDictionary"))
                    .ToDictionary(value => value.Key, value => value.Value);
            }

            if (newDict.Any())
            {
                var values = newDict["modelOutputs"];
            }


        }
    }
}
