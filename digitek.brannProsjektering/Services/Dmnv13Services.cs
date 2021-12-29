using digitek.brannProsjektering.Models.Schema.DmnV13;
using digitek.brannProsjektering.Models;
using System.Collections.Generic;
using System.Linq;

namespace digitek.brannProsjektering.services
{
    public class Dmnv13Services
    {
        //---- Data Dictionary
        public static void GetDecisionsVariablesFormDmnV13(tDecision tdecision, string fileName, ref List<DmnInfo> dataDictionaryList)
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
                    inputClause.inputExpression.typeRef, "input");
            }

            foreach (var outputClause in decisionTable.output)
            {
                // Add Output variable name
                var dictionary = AddVariablesToDictionary(ref dmnInfo, outputClause.name, outputClause.label, outputClause.typeRef, "output",outputClause.description);
            }

            dataDictionaryList.Add(dmnInfo);
        }

        public static DmnInfo AddVariablesToDictionary(ref DmnInfo dmnInfo, string variableId, string variableName, string variableType, string type, string description = null)
        {

            var list = new List<VariablesInfo>()
            {
                new VariablesInfo()
                {
                    VariabelId = variableId,
                    VariabelName = variableName,
                    VariabelType = variableType,
                    VariabelBeskrivelse = description,
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
    }
}