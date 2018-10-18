using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CamundaClient;
using CamundaClient.Dto;
using FluentAssertions;
using Xunit;
using digitek.brannProsjektering.Models;
using digitek.brannProsjektering.Tests.Models;
using digitek.brannProsjektering.Worker;

namespace digitek.brannProsjektering.Tests
{
    public class BrannklasseModelTests
    {
        private readonly string _resourcePath1 = "digitek.brannProsjektering.CamundaModels.";

        [Fact(DisplayName = "Brannklasse Model Core Test Opt1")]
        public void BrannklasseModelTest()
        {
            BpmnTestModel bpmnTestModel = new BpmnTestModel();

            //Model to Test
            var modelName = "BrannklasseModelDotNet.bpmn";
            var TestName = BpmnTestServices.CreateTestName(modelName);

            //Model inputs
            var brannklasseModel = new BrannklasseModel()
            {
                typeVirksomhet = "Bolig",
                antallEtasjer = 2,
                brtArealPrEtasje = 400,
                konsekvensAvBrann = null,
                rkl = "RKL3",
                utgangTerrengAlleBoenheter = true
            };
            var dictionary = BpmnTestServices.CreateDictionaryFromModel(brannklasseModel);

            //Start Camunda in debug mode
            var camunda = BpmnTestServices.CamundaEngineClient();
            camunda.Should().NotBeNull();

            // Get brann project Assembly
            var assembly = BpmnTestServices.GetAssembly("digitek.brannProsjektering,");

            // Deploy the models under test
            bpmnTestModel.DeploymentId = BpmnTestServices.DeployModelToCamunda(camunda, string.Concat(_resourcePath1, modelName), assembly, TestName);
            bpmnTestModel.DeploymentId.Should().NotBeNullOrEmpty();

            try
            {
                var externalTask = new ExternalTask();

                // Start Instance
                bpmnTestModel.ProcessInstanceId = BpmnTestServices.StartInstance(camunda, "BrannklasseModel.Net", dictionary);
                bpmnTestModel.ProcessInstanceId.Should().NotBeNullOrEmpty();

                //Check that external task for Inputs validation is there and can runs
                var inputsExternalTask = BpmnTestServices.CheckExternalTask(camunda, TestName, "brannInputsValidation", "branninputsVariablesValidation", ref dictionary);
                inputsExternalTask.Should().BeTrue();

                //Execute external task model inputs
                bpmnTestModel.BrannInputsValidationExternalTasks = BpmnTestServices.GetModelInputsExternalTaskOutput(externalTask);
                bpmnTestModel.BrannInputsValidationExternalTasks.Count().Should().Be(1);

                // Get process variables and set to ExternalTask variables
                externalTask.Variables = BpmnTestServices.GetProcessVariables(camunda, bpmnTestModel.ProcessInstanceId);
                externalTask.Variables.Should().NotBeEmpty();

                //Update ExternalTask
                BpmnTestServices.UpdateExternalTaskWithTheResult(externalTask, bpmnTestModel.BrannInputsValidationExternalTasks);

                // Check that External Task for output Consolidation is there
                var outputExternalTask = BpmnTestServices.CheckExternalTask(camunda, TestName, "outputConsolidation", "VariablesOutputConsolidation", ref dictionary);
                outputExternalTask.Should().BeTrue();

                // Execute Consolidate Outputs method
                bpmnTestModel.OutputConsolidationExternalTasks = BpmnTestServices.GetOutputConsolidationExternalTaskOutput(externalTask);
                bpmnTestModel.OutputConsolidationExternalTasks.Count().Should().Be(1);

                // Check that External Task for data Dictionary is there
               var outputDataDictionaryExternalTask= BpmnTestServices.CheckExternalTask(camunda, TestName, "modelOutputDataDictionary", "dataDictionary", ref dictionary);
                outputDataDictionaryExternalTask.Should().BeTrue();

                //Creat Dictionary to model method
                bpmnTestModel.ModelOutputDataDictionaryExternalTasks = BpmnTestServices.GetModelOutputsDataDictionaryExternalTaskOutput(externalTask);
                bpmnTestModel.ModelOutputDataDictionaryExternalTasks.Count().Should().Be(1);

                //Check that all the DMN tables were executed
                var ModelValue = (Dictionary<string, object>)bpmnTestModel.OutputConsolidationExternalTasks.FirstOrDefault().Value;
                ModelValue.Count().Should().Be(2);
            }
            finally
            {
                // cleanup after test case
                camunda.RepositoryService.DeleteDeployment(bpmnTestModel.DeploymentId);
            }
        }

        [Fact(DisplayName = "Brannklasse Model Core Test Opt2")]
        public void BrannklasseModelTestOpt02()
        {
            BpmnTestModel bpmnTestModel = new BpmnTestModel();

            //Model to Test
            var modelName = "BrannklasseModelDotNet.bpmn";
            var TestName = BpmnTestServices.CreateTestName(modelName);

            //Model inputs
            var brannklasseModel = new BrannklasseModel()
            {
                konsekvensAvBrann = "Middels konsekvens"
            };
            var dictionary = BpmnTestServices.CreateDictionaryFromModel(brannklasseModel);

            //Start Camunda in debug mode
            var camunda = BpmnTestServices.CamundaEngineClient();

            // Get brann project Assembly
            var assembly = BpmnTestServices.GetAssembly("digitek.brannProsjektering,");

            // Deploy the models under test
            bpmnTestModel.DeploymentId = BpmnTestServices.DeployModelToCamunda(camunda, string.Concat(_resourcePath1, modelName), assembly, TestName);

            try
            {
                var externalTask = new ExternalTask();

                // Start Instance
                bpmnTestModel.ProcessInstanceId = BpmnTestServices.StartInstance(camunda, "BrannklasseModel.Net", dictionary);

                //Check that external task for Inputs validation is there and can runs
                BpmnTestServices.CheckExternalTask(camunda, TestName, "brannInputsValidation", "branninputsVariablesValidation", ref dictionary);

                //Execute external task model inputs
                bpmnTestModel.BrannInputsValidationExternalTasks = BpmnTestServices.GetModelInputsExternalTaskOutput(externalTask);

                // Get process variables and set to ExternalTask variables
                externalTask.Variables = BpmnTestServices.GetProcessVariables(camunda, bpmnTestModel.ProcessInstanceId);

                //Update ExternalTask
                BpmnTestServices.UpdateExternalTaskWithTheResult(externalTask, bpmnTestModel.BrannInputsValidationExternalTasks);

                // Check that External Task for output Consolidation is there
                BpmnTestServices.CheckExternalTask(camunda, TestName, "outputConsolidation", "VariablesOutputConsolidation", ref dictionary);

                // Execute Consolidate Outputs method
                bpmnTestModel.OutputConsolidationExternalTasks = BpmnTestServices.GetOutputConsolidationExternalTaskOutput(externalTask);

                // Check that External Task for data Dictionary is there
                BpmnTestServices.CheckExternalTask(camunda, TestName, "modelOutputDataDictionary", "dataDictionary", ref dictionary);

                //Creat Dictionary to model method
                bpmnTestModel.ModelOutputDataDictionaryExternalTasks = BpmnTestServices.GetModelOutputsDataDictionaryExternalTaskOutput(externalTask);

                //Check that all the DMN tables were executed
                var ModelValue = (Dictionary<string, object>)bpmnTestModel.OutputConsolidationExternalTasks.FirstOrDefault().Value;
                ModelValue.Count().Should().Be(1);
            }
            finally
            {
                // cleanup after test case
                camunda.RepositoryService.DeleteDeployment(bpmnTestModel.DeploymentId);
            }
        }
        //TODO test all DMN i model positive and negative Tests
    }
}
