using System.Collections.Generic;
using CamundaClient.Dto;
using CamundaClient.Worker;
using Newtonsoft.Json;

namespace digitek.brannProsjektering.Worker
{
    [ExternalTaskTopic("outputConsolidation")]
    //[ExternalTaskVariableRequirements("rklOutputs")]
    public class OutputConsolidation : IExternalTaskAdapter
    {
        public void Execute(ExternalTask externalTask, ref Dictionary<string, object> resultVariables)
        {
            var dmnsDictionary = new Dictionary<string, object>();
            var variables = externalTask.Variables;
            foreach (var variable in variables)
            {
                var type = variable.Value.ValueInfo.ToString();
                if (!type.Contains("HashMap") || variable.Key.Contains("modelInputs")) continue;
                var variableValue =variable.Value.Value;
                var json = JsonConvert.DeserializeObject(variableValue.ToString());
                dmnsDictionary.Add(variable.Key, json);
            }
            resultVariables.Add("modelOutputs", dmnsDictionary);
        }
    }
}