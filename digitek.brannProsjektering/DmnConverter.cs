using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using digitek.brannProsjektering.Models;
using digitek.brannProsjektering.Models.Schema;

namespace digitek.brannProsjektering
{
    public class DmnConverter
    {


        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileStream"></param>
        /// <returns></returns>
        public static tDefinitions DeserializeStreamDmnFile(Stream fileStream)
        {
            tDefinitions resultinMessage;
            try
            {
                var serializer = new XmlSerializer(typeof(tDefinitions));
                resultinMessage = (tDefinitions)serializer.Deserialize(new XmlTextReader(fileStream));
            }
            catch
            {
                resultinMessage = null;
            }
            return resultinMessage;
        }
        public static digitek.brannProsjektering.Models.Schema.DmnV13.tDefinitions DeserializeStreamDmnV13File(Stream fileStream)
        {
            digitek.brannProsjektering.Models.Schema.DmnV13.tDefinitions resultinMessage;
            try
            {
                var serializer = new XmlSerializer(typeof(digitek.brannProsjektering.Models.Schema.DmnV13.tDefinitions));
                resultinMessage = (digitek.brannProsjektering.Models.Schema.DmnV13.tDefinitions)serializer.Deserialize(new XmlTextReader(fileStream));
            }
            catch
            {
                resultinMessage = null;
            }
            return resultinMessage;
        }


        /// <summary>
        /// Regex to get the number from string with comparation characters = >,< 
        /// </summary>
        /// <param name="cellValue"></param>
        /// <returns></returns>
        public static string GetComparisonNumber(string cellValue)
        {
            var regex = Regex.Match(cellValue, @"^[<,>][=]?\s?(?<number>\d+[\.]?(\d+)?)$");
            return regex.Success ? regex.Groups["number"].Value : null;
        }

        /// <summary>
        /// Get the number from a range string format
        /// </summary>
        /// <param name="cellValue"></param>
        /// <returns></returns>
        public static string[] GetRangeNumber(string cellValue)
        {
            var regex = Regex.Match(cellValue, @"^[\[,\],]\s?(?<range1>\d+(\.\d+)?).{2}?(?<range2>\d+(\.\d+)?)[\[,\]]$");
            return regex.Success ? new[] { regex.Groups["range1"].Value, regex.Groups["range2"].Value } : null;
        }

        //---- Data Dictionary
        public static void GetDecisionsVariablesFormDmnV11(tDecision tdecision, string fileName, ref List<DmnInfo> dataDictionaryList)
        {
            var decisionTable = (tDecisionTable)tdecision.Item;
            var dmnInfo = new DmnInfo()
            {
                FileName = $"{fileName}.dmn",
                DmnId = tdecision.id,
                DmnName = tdecision.name,
            };


            foreach (var inputClause in decisionTable.input)
            {
                //add input variable to DMN

                //var dictionary = AddVariablesToDictionary(fileName, decisionId, decisionName, inputClause.id, inputClause.label,inputClause.inputExpression.typeRef.Name, "input");
                var dictionary = AddVariablesToDictionary(ref dmnInfo, inputClause.inputExpression.Item.ToString(), inputClause.label,
                    inputClause.inputExpression.typeRef.Name, "input");
            }

            foreach (var outputClause in decisionTable.output)
            {
                // Add Output variable name
                var dictionary = AddVariablesToDictionary(ref dmnInfo, outputClause.name, outputClause.label,
                    outputClause.typeRef.Name, "output");
            }

            dataDictionaryList.Add(dmnInfo);
        }

        public static DmnInfo AddVariablesToDictionary(ref DmnInfo dmnInfo, string variableId, string variableName, string variableType, string type)
        {

            var list = new List<VariablesInfo>()
            {
                new VariablesInfo()
                {
                    VariabelId = variableId,
                    VariabelName = variableName,
                    VariabelType = variableType,
                }
            };

            if (type == "input")
            {

                if (dmnInfo.InputVariablesInfo != null)
                {
                    var listTemp = dmnInfo.InputVariablesInfo.ToList();
                    list = listTemp.Concat(list).ToList();
                }
                dmnInfo.InputVariablesInfo = list.ToArray();
            }
            if (type == "output")
            {
                if (dmnInfo.OutputVariablesInfo != null)
                {
                    var listTemp = dmnInfo.OutputVariablesInfo.ToList();
                    list = listTemp.Concat(list).ToList();
                }
                dmnInfo.OutputVariablesInfo = list.ToArray();
            }
            return dmnInfo;
        }

        public static void GetDmnInfoFromBpmnModel(XDocument xmlBpmn, ref List<BpmnInfo> bpmnDataList)
        {
            var businessRuleTasks = xmlBpmn.Descendants()
                .Where(x => x.Name.ToString().Contains("businessRuleTask"));
            var process = xmlBpmn.Descendants()
                .Single(x => x.Name.ToString().Contains("process"));

            var ruleTasks = businessRuleTasks as XElement[] ?? businessRuleTasks.ToArray();
            if (ruleTasks.Any())
            {
                foreach (XElement element in ruleTasks)
                {
                    bpmnDataList.Add(new BpmnInfo()
                    {
                        BpmnId = process.Attribute("id")?.Value,
                        BpmnNavn = process.Attribute("name")?.Value,
                     
                        DmnResultatvariabel = element.Attributes().Single(a => a.Name.ToString().Contains("resultVariable"))?.Value
                    });
                }
            }
        }
        public static void GetDmnInfoFromBpmnModel(XDocument xmlBpmn, List<DmnInfo> dmns, ref List<BpmnInfo> bpmnDataList)
        {
            var businessRuleTasks = xmlBpmn.Descendants()
                .Where(x => x.Name.ToString().Contains("businessRuleTask"));
            var process = xmlBpmn.Descendants()
                .Single(x => x.Name.ToString().Contains("process"));

            var ruleTasks = businessRuleTasks as XElement[] ?? businessRuleTasks.ToArray();
            if (ruleTasks.Any())
            {
                foreach (XElement element in ruleTasks)
                {
                    var bpmnId = process.Attribute("id")?.Value;
                    var dmnId = element.Attributes().Single(a => a.Name.ToString().Contains("decisionRef"))?.Value;
                    var dmn = dmns.First(d => d.DmnId == dmnId);

                    BpmnInfo bpmnInfo;
                    if (bpmnDataList.Any(bp => bp.BpmnId == bpmnId))
                    {
                        bpmnInfo = bpmnDataList.First(bp => bp.BpmnId == bpmnId);
                        bpmnInfo.DmnInfos.Add(dmn);
                    }
                    else
                    {
                        bpmnInfo = new BpmnInfo()
                        {
                            BpmnId = process.Attribute("id")?.Value,
                            BpmnNavn = process.Attribute("name")?.Value,
                            DmnResultatvariabel = element.Attributes().Single(a => a.Name.ToString().Contains("resultVariable"))?.Value
                        };
                        bpmnInfo.DmnInfos = new List<DmnInfo>() { dmn };
                        bpmnDataList.Add(bpmnInfo);
                    }
                }
            }
        }

        public static List<VariablesInfo> GetVariablesFormDmns(List<DmnInfo> dmnInfoList)
        {
            List<VariablesInfo> variablesInfos = new List<VariablesInfo>();
            var dmnInputsVariables = dmnInfoList.Select(imp => imp.InputVariablesInfo).ToList();
            var dmnOutputsVariables = dmnInfoList.Select(imp => imp.OutputVariablesInfo).ToList();
            var dmnVariablesIds = dmnInputsVariables.Concat(dmnOutputsVariables).ToArray();

            List<VariablesInfo> variablesInfosFinal = new List<VariablesInfo>();

            foreach (var dmnVariablesId in dmnVariablesIds)
            {
                var ids = dmnVariablesId.GroupBy(x => x.VariabelId).Select(d => d.First()).ToList();
                foreach (var variablesInfo in dmnVariablesId)
                {
                    if (variablesInfosFinal.Any(k => k.VariabelId == variablesInfo.VariabelId))
                        continue;
                    variablesInfosFinal.Add(variablesInfo);
                }
            }
            return variablesInfosFinal;
        }
    }
}
