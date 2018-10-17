using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using CamundaClient;
using CamundaClient.Dto;
using digitek.brannProsjektering.Models;
using FluentAssertions;

namespace digitek.brannProsjektering.Tests
{
   public class BpmnTestServices
    {
        public static string CreateTestName(string modelName)
        {
            var name = modelName.Substring(0, modelName.IndexOf("."));
            var testName = name + "_Testcase";
            return testName;
        }
        public static void UpdateExternalTaskWithTheResult(ExternalTask externalTask, Dictionary<string, object> externalTaskDictionary)
        {
            externalTask.Variables.Add(externalTaskDictionary.FirstOrDefault().Key,
                new Variable() { Value = externalTaskDictionary.FirstOrDefault().Value });
        }
        public static Dictionary<string, object> CheckOutputAndUpdateExternalTask(Dictionary<string, object> externalTaskOutputDictionary, string key)
        {
            var dictionaryByKey = externalTaskOutputDictionary.Where(value => value.Key.Contains(key))
                .ToDictionary(value => value.Key, value => value.Value);
            return dictionaryByKey;
        }
        public static Dictionary<string, object> CreateDictionaryFromModel(BrannklasseModel brannklasseModel)
        {
            var dictionary = brannklasseModel.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(brannklasseModel, null));
            return dictionary;
        }
        public static void CheckExternalTask(CamundaEngineClient camunda, string TestName, string TopicName, string activityId, ref Dictionary<string, object> dictionary)
        {
            //Check that External Task is there
            var externalTasks = camunda.ExternalTaskService.FetchAndLockTasks(TestName, 100, TopicName, 1000, new List<string>());
            externalTasks.Count.Should().Be(1);
            externalTasks.First().ActivityId.Should().Be(activityId);
            camunda.ExternalTaskService.Complete(TestName, externalTasks.First().Id, dictionary);
        }

    }
}
