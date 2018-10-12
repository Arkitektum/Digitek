using System;
using System.Collections.Generic;
using System.Linq;
using CamundaClient.Dto;
using CamundaClient.Worker;
using Newtonsoft.Json;

namespace digitek.brannProsjektering.Worker
{
    [ExternalTaskTopic("outputConsolidation")]
    public class OutputConsolidation : IExternalTaskAdapter
    {
        public void Execute(ExternalTask externalTask, ref Dictionary<string, object> resultVariables)
        {
            var dmnsDictionary = new Dictionary<string, object>();
            var variables = externalTask.Variables;
            foreach (var variable in variables)
            {
                var value = variable.Value.Value;
                if (value != null)
                {
                    try
                    {
                        if (variable.Key.Contains("modelInputs")) continue;
                        var dictionaryTemp = JsonConvert.DeserializeObject<Dictionary<string, object>>(value.ToString());
                        if (dictionaryTemp.Any())
                            dmnsDictionary.Add(variable.Key, new Variable(){Value = value});
                    }
                    catch
                    {
                        //nada
                    }
                }
            }
            resultVariables.Add("modelOutputs", dmnsDictionary);
        }
    }
}