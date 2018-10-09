using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CamundaClient.Dto;
using CamundaClient.Worker;
using digitek.brannProsjektering.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace digitek.brannProsjektering.Worker
{
    [ExternalTaskTopic("modelOutputDataDictionary")]
    //[ExternalTaskVariableRequirements("modelOutputs")]
    public class ModelOutputsDataDictionary : IExternalTaskAdapter
    {
        public void Execute(ExternalTask externalTask, ref Dictionary<string, object> resultVariables)
        {
            var dmnDictionary = new Dictionary<string, object>();

            var jsonDmn2Tek = GetJsonArrayFromFile("JsonDmn2TEK.json");
            var jsonDmnVariablesInfo = GetJsonArrayFromFile("JsonDmnVariablesNames.json");
            var jsonTable2Variables = GetJsonArrayFromFile("JsonTable2Variables.json");


            if (externalTask.Variables.TryGetValue("modelOutputs", out var modelVariables))
            {
                var jsonObjects = (JObject)JsonConvert.DeserializeObject(modelVariables.Value.ToString());
                foreach (var jsonObject in jsonObjects)
                {
                    var tableInfo = new TableInfo();
                    var dmnId = jsonObject.Key;
                    var dmnInfo = jsonDmn2Tek.FirstOrDefault(obj => Extensions.Value<string>(obj["DmnId"]) == dmnId);

                    if (dmnInfo != null)
                    {
                        // Add DMN info from JsonDmn2TEK.json
                        tableInfo.DmnId = dmnInfo.SelectToken("DmnId").ToString();
                        tableInfo.DmnNavn = dmnInfo.SelectToken("DmnNavn").ToString();
                        tableInfo.TekKapitel = dmnInfo.SelectToken("TekKapitel").ToString();
                        tableInfo.TekLedd = dmnInfo.SelectToken("TekLedd").ToString();
                        tableInfo.TekTabell = dmnInfo.SelectToken("TekTabell").ToString();
                        tableInfo.TekForskriften = dmnInfo.SelectToken("TekForskriften").ToString();
                        tableInfo.TekWebLink = dmnInfo.SelectToken("TekWebLink").ToString();
                    }

                    var dmnOutputs = (JObject)jsonObject.Value;

                    //get output variables info from JsonDmnVariablesNames.json file to all Dmn variables result
                    List<VariablesInfo> outputList = new List<VariablesInfo>();
                    foreach (var dmnOutput in dmnOutputs)
                    {
                        var dmnVariableInfo = jsonDmnVariablesInfo.FirstOrDefault(obj => Extensions.Value<string>(obj["VariabelId"]) == dmnOutput.Key);
                        GetVariableInfo(dmnVariableInfo, outputList);
                    }
                    tableInfo.OutputVariablesInfo = outputList.ToArray();

                    // get all DMN inputs from JsonTable2Variables.json by dmnID
                    var dmnInputs = jsonTable2Variables.Where(obj => Extensions.Value<string>(obj["DmnId"]) == dmnId && Extensions.Value<string>(obj["VariabelType"]) == "input");

                    //Create input lists from JsonDmnVariablesNames.json file
                    List<VariablesInfo> inputsList = new List<VariablesInfo>();
                    foreach (var dmnInputVariable in dmnInputs)
                    {
                        var dmnVariableInfo = jsonDmnVariablesInfo.FirstOrDefault(obj => Extensions.Value<string>(obj["VariabelId"]) == dmnInputVariable.SelectToken("VariabelId").ToString());
                        GetVariableInfo(dmnVariableInfo, inputsList);
                    }

                    tableInfo.InputVariablesInfo = inputsList.ToArray();
                    dmnDictionary.Add(dmnId, tableInfo);
                }
            }
            resultVariables.Add("modelDataDictionary", dmnDictionary);
        }

        private static void GetVariableInfo(JToken dmnVariableInfo, List<VariablesInfo> outputList)
        {
            if (dmnVariableInfo != null)
            {
                outputList.Add(new VariablesInfo()
                {
                    VariabelId = dmnVariableInfo.SelectToken("VariabelId").ToString(),
                    VariabelNavn = dmnVariableInfo.SelectToken("VariabelNavn").ToString(),
                    VariabelBeskrivelse = dmnVariableInfo.SelectToken("VariabelBeskrivelse").ToString()
                });
            }
        }

        private static JArray GetJsonArrayFromFile(string filename)
        {
            JArray jsonArray;
            try
            {
                var basePath = AppDomain.CurrentDomain.BaseDirectory;
                var table2Variables = Path.Combine(basePath, "Data", filename);

                var jsonText = File.ReadAllText(table2Variables);
                jsonArray = (JArray)JsonConvert.DeserializeObject<object>(jsonText);
            }
            catch
            {
                jsonArray = new JArray("Error, not possible to Get Json from: " + filename);
            }

            return jsonArray;
        }
    }
}