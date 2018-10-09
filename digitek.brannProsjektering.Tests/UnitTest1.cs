using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CamundaClient;
using CamundaClient.Dto;
using CamundaClient.Service;
using FluentAssertions;
using Xunit;
using  digitek.brannProsjektering.Models;

namespace digitek.brannProsjektering.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var branntekniskProsjektering = new BranntekniskProsjekteringModel()
            {
                typeVirksomhet = "Sykehus",
            };

            var dictionary = branntekniskProsjektering.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(branntekniskProsjektering, null));
            var camunda = new CamundaEngineClient(new System.Uri("http://localhost:8080/engine-rest/engine/default/"), null, null);

            // Deploy the models under test
            string deploymentId = camunda.RepositoryService.Deploy("testcase", new List<object> {
                FileParameter.FromManifestResource(Assembly.GetExecutingAssembly(), "digitek.brannProsjektering.Tests.TestData.RisikoklassenModelDotNet.bpmn")});
            try
            {
                // Start Instance
                string processInstanceId =
                    camunda.BpmnWorkflowService.StartProcessInstance("RisikoklassenModel.Net", dictionary);
                //var processInstanceId = new BpmnWorkflowService(camunda).StartProcessInstance("RisikoklassenModel.Net", dictionary);

                processInstanceId.Should().NotBeNullOrEmpty();

                // Check that External Task for Inputs Validation is there
                var externalTasks = camunda.ExternalTaskService.FetchAndLockTasks("testcase", 100, "brannInputsValidation", 1000, new List<string>());
                externalTasks.Count.Should().Be(1);
                externalTasks.First().ActivityId.Should().Be("branninputsVariablesValidation");
                camunda.ExternalTaskService.Complete("testcase", externalTasks.First().Id, dictionary);

                // Check that External Task for output Consolidation is there
                externalTasks = camunda.ExternalTaskService.FetchAndLockTasks("testcase", 100, "outputConsolidation", 1000, new List<string>());
                externalTasks.Count.Should().Be(1);
                externalTasks.First().ActivityId.Should().Be("VariablesOutputConsolidation");
                camunda.ExternalTaskService.Complete("testcase", externalTasks.First().Id, new Dictionary<string, object>());

                // Check that External Task for data Dictionary is there
                externalTasks = camunda.ExternalTaskService.FetchAndLockTasks("testcase", 100, "modelOutputDataDictionary", 1000, new List<string>());
                externalTasks.Count.Should().Be(1);
                externalTasks.First().ActivityId.Should().Be("dataDictionary");
                camunda.ExternalTaskService.Complete("testcase", externalTasks.First().Id, new Dictionary<string, object>());
            }
            finally
            {
                // cleanup after test case
                camunda.RepositoryService.DeleteDeployment(deploymentId);
            }
        }
    }
}
