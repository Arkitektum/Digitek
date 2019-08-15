using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using DecisionModelNotation.Shema;
using digitek.brannProsjektering.Models;

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
        public static void GetDecisionsVariables(tDecision tdecision, string fileName, ref List<DmnInfo> dataDictionaryList)
        {

            var decisionTable = (tDecisionTable)tdecision.Item;
            var dmnInfo = new DmnInfo()
            {
                FilNavn = fileName,
                DmnId = tdecision.id,
                DmnNavn = tdecision.name,
            };


            foreach (var inputClause in decisionTable.input)
            {
                //add input variable to DMN

                //var dictionary = AddVariablesToDictionary(fileName, decisionId, decisionName, inputClause.id, inputClause.label,inputClause.inputExpression.typeRef.Name, "input");
                var dictionary = AddVariablesToDictionary(ref dmnInfo, inputClause.inputExpression.Item.ToString(), inputClause.label,
                    inputClause.inputExpression.typeRef.Name, "input");
                dataDictionaryList.Add(dictionary);
            }

            foreach (var outputClause in decisionTable.output)
            {
                // Add Output variable name
                var dictionary = AddVariablesToDictionary(ref dmnInfo, outputClause.name, outputClause.label,
                    outputClause.typeRef.Name, "output");
                dataDictionaryList.Add(dictionary);
            }
        }

        private static DmnInfo AddVariablesToDictionary(ref DmnInfo dmnInfo, string variableId, string variableName, string variableType, string type)
        {

            var list = new List<VariablesInfo>()
            {
                new VariablesInfo()
                {
                    VariabelId = variableId,
                    VariabelNavn = variableName,
                    VariabelType = variableType,
                }
            };


            if (type == "input")
            {

                if (dmnInfo.InputVariablesInfo == null || !dmnInfo.InputVariablesInfo.Any())
                {
                    dmnInfo.InputVariablesInfo = list.ToArray();
                }
                else
                {
                    var listTemp = dmnInfo.InputVariablesInfo.ToList();
                    var variablesInfos = listTemp.Concat(list);
                    dmnInfo.InputVariablesInfo = variablesInfos.ToArray();
                }
            }
            if (type == "output")
            {

                if (dmnInfo.OutputVariablesInfo == null || !dmnInfo.OutputVariablesInfo.Any())
                {
                    dmnInfo.OutputVariablesInfo = list.ToArray();
                }
                else
                {
                    var listTemp = dmnInfo.OutputVariablesInfo.ToList();
                    var variablesInfos = listTemp.Concat(list);
                    dmnInfo.OutputVariablesInfo = variablesInfos.ToArray();
                }
            }

            return dmnInfo;
        }


    }
}
