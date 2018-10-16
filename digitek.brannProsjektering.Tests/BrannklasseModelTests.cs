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
    public class BrannklasseModelTests
    {
        private readonly string _camundaUrl = "http://localhost:8080/engine-rest/engine/default/";
        private readonly string _resourcePath1 = "digitek.brannProsjektering.CamundaModels.";

        [Fact(DisplayName = "Brannklasse Model Core Test Opt1")]
        public void BrannklasseModelTest()
        {
            var brannklasseModel = new BrannklasseModel()
            {
                typeVirksomhet = "Bolig",
                antallEtasjer = 2,
                brtArealPrEtasje = 400,
                konsekvensAvBrann = null,
                rkl = "RKL3",
                utgangTerrengAlleBoenheter = true
            };

            var dictionary = brannklasseModel.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(brannklasseModel, null));

            var camunda = new CamundaEngineClient(new System.Uri(_camundaUrl), null, null);

            var modelName = "BrannklasseModelDotNet.bpmn";
            var name = modelName.Substring(0, modelName.IndexOf("."));
            string TestName = name + "_Testcase";

            // Deploy the models under test
            var modelPath = string.Concat(_resourcePath1, modelName);
            var assemblys = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.FullName.Contains("digitek.brannProsjektering")).ToArray();
            var assembly = (Assembly)assemblys[1];

            //check that can start the sub model
            string deploymentId = camunda.RepositoryService.Deploy(TestName,
                new List<object>
                {
                    FileParameter.FromManifestResource(assembly, modelPath)
                });

            deploymentId.Should().NotBeNullOrEmpty();

            try
            {
                // Start Instance
                string processInstanceId = camunda.BpmnWorkflowService.StartProcessInstance("BrannklasseModel.Net", dictionary);
                processInstanceId.Should().NotBeNullOrEmpty();

                var externalTask = new ExternalTask();
                var modelVariables = new Dictionary<string, object>();

                //Check that External Task for Inputs Validation is there
                var externalTasks = camunda.ExternalTaskService.FetchAndLockTasks(TestName, 100, "brannInputsValidation", 1000, new List<string>());
                externalTasks.Count.Should().Be(1);
                externalTasks.First().ActivityId.Should().Be("branninputsVariablesValidation");
                camunda.ExternalTaskService.Complete(TestName, externalTasks.First().Id, dictionary);

                // Get process variables and set to ExternalTask variables
                var processVariables = camunda.BpmnWorkflowService.GetProcessVariables(processInstanceId);
                externalTask.Variables = CamundaClientHelper.ConvertVariables(processVariables);

                //validate Input method
                new InputsValidationWorker().Execute(externalTask, ref modelVariables);
                if (modelVariables.TryGetValue("modelInputs", out var modelInputs))
                    externalTask.Variables.Add("modelInputs", new Variable() { Value = modelInputs });

                // Check that External Task for output Consolidation is there
                externalTasks = camunda.ExternalTaskService.FetchAndLockTasks(TestName, 100, "outputConsolidation", 1000, new List<string>());
                externalTasks.Count.Should().Be(1);
                externalTasks.First().ActivityId.Should().Be("VariablesOutputConsolidation");
                camunda.ExternalTaskService.Complete(TestName, externalTasks.First().Id, new Dictionary<string, object>());

                // Consolidate Outputs method
                new OutputConsolidation().Execute(externalTask, ref modelVariables);
                var ModelOutputsDictionary = new Dictionary<string, object>();
                if (modelVariables.TryGetValue("modelOutputs", out var modelOutputs))
                {
                    externalTask.Variables.Add("modeloOutputs", new Variable() { Value = modelOutputs });
                    ModelOutputsDictionary = (Dictionary<string, object>)modelOutputs;
                }

                // Check that External Task for data Dictionary is there
                externalTasks = camunda.ExternalTaskService.FetchAndLockTasks(TestName, 100, "modelOutputDataDictionary", 1000, new List<string>());
                externalTasks.Count.Should().Be(1);
                externalTasks.First().ActivityId.Should().Be("dataDictionary");
                camunda.ExternalTaskService.Complete(TestName, externalTasks.First().Id, new Dictionary<string, object>());

                //Creat Dictionary to model method
                new ModelOutputsDataDictionary().Execute(externalTask, ref modelVariables);
                if (modelVariables.TryGetValue("modelDataDictionary", out var modelDataDictionary)) { }
                externalTask.Variables.Add("modelDataDictionary", new Variable() { Value = modelDataDictionary });

                
                //Check that all the DMN ara runn and
                ModelOutputsDictionary.Count.Should().Be(2);

            }
            finally
            {
                // cleanup after test case
                camunda.RepositoryService.DeleteDeployment(deploymentId);
            }
        }
        [Fact(DisplayName = "Brannklasse Model Core Test Opt2")]
        public void BrannklasseModelTest02()
        {
            var brannklasseModel = new BrannklasseModel()
            {
                konsekvensAvBrann = "Stor konsekvens",
            };

            var dictionary = brannklasseModel.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(brannklasseModel, null));

            var camunda = new CamundaEngineClient(new System.Uri(_camundaUrl), null, null);

            var modelName = "BrannklasseModelDotNet.bpmn";
            var name = modelName.Substring(0, modelName.IndexOf("."));
            string TestName = name + "_Testcase";

            // Deploy the models under test
            var modelPath = string.Concat(_resourcePath1, modelName);
            var assemblys = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.FullName.Contains("digitek.brannProsjektering")).ToArray();
            var assembly = (Assembly)assemblys[1];

            //check that can start the sub model
            string deploymentId = camunda.RepositoryService.Deploy(TestName,
                new List<object>
                {
                    FileParameter.FromManifestResource(assembly, modelPath)
                });

            deploymentId.Should().NotBeNullOrEmpty();

            try
            {
                // Start Instance
                string processInstanceId = camunda.BpmnWorkflowService.StartProcessInstance("BrannklasseModel.Net", dictionary);
                processInstanceId.Should().NotBeNullOrEmpty();

                var externalTask = new ExternalTask();
                var modelVariables = new Dictionary<string, object>();

                //Check that External Task for Inputs Validation is there
                var externalTasks = camunda.ExternalTaskService.FetchAndLockTasks(TestName, 100, "brannInputsValidation", 1000, new List<string>());
                externalTasks.Count.Should().Be(1);
                externalTasks.First().ActivityId.Should().Be("branninputsVariablesValidation");
                camunda.ExternalTaskService.Complete(TestName, externalTasks.First().Id, dictionary);

                // Get process variables and set to ExternalTask variables
                var processVariables = camunda.BpmnWorkflowService.GetProcessVariables(processInstanceId);
                externalTask.Variables = CamundaClientHelper.ConvertVariables(processVariables);

                //validate Input method
                new InputsValidationWorker().Execute(externalTask, ref modelVariables);
                if (modelVariables.TryGetValue("modelInputs", out var modelInputs))
                    externalTask.Variables.Add("modelInputs", new Variable() { Value = modelInputs });

                // Check that External Task for output Consolidation is there
                externalTasks = camunda.ExternalTaskService.FetchAndLockTasks(TestName, 100, "outputConsolidation", 1000, new List<string>());
                externalTasks.Count.Should().Be(1);
                externalTasks.First().ActivityId.Should().Be("VariablesOutputConsolidation");
                camunda.ExternalTaskService.Complete(TestName, externalTasks.First().Id, new Dictionary<string, object>());

                // Consolidate Outputs method
                new OutputConsolidation().Execute(externalTask, ref modelVariables);
                var ModelOutputsDictionary = new Dictionary<string, object>();
                if (modelVariables.TryGetValue("modelOutputs", out var modelOutputs))
                {
                    externalTask.Variables.Add("modeloOutputs", new Variable() { Value = modelOutputs });
                    ModelOutputsDictionary = (Dictionary<string, object>)modelOutputs;
                }

                // Check that External Task for data Dictionary is there
                externalTasks = camunda.ExternalTaskService.FetchAndLockTasks(TestName, 100, "modelOutputDataDictionary", 1000, new List<string>());
                externalTasks.Count.Should().Be(1);
                externalTasks.First().ActivityId.Should().Be("dataDictionary");
                camunda.ExternalTaskService.Complete(TestName, externalTasks.First().Id, new Dictionary<string, object>());

                //Creat Dictionary to model method
                new ModelOutputsDataDictionary().Execute(externalTask, ref modelVariables);
                if (modelVariables.TryGetValue("modelDataDictionary", out var modelDataDictionary)) { }
                externalTask.Variables.Add("modelDataDictionary", new Variable() { Value = modelDataDictionary });

                
                //Check that all the DMN ara runn and
                ModelOutputsDictionary.Count.Should().Be(1);

            }
            finally
            {
                // cleanup after test case
                camunda.RepositoryService.DeleteDeployment(deploymentId);
            }
        }
    }
}
