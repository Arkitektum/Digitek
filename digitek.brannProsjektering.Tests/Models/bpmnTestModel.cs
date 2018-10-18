using System;
using System.Collections.Generic;
using System.Text;
using CamundaClient.Dto;

namespace digitek.brannProsjektering.Tests.Models
{
   public class BpmnTestModel
    {
        public string DeploymentId { get; set; }
        public string ProcessInstanceId { get; set; }
        public Dictionary<string,object> BrannInputsValidationExternalTasks { get; set; } 
        public Dictionary<string, object> OutputConsolidationExternalTasks { get; set; } 
        public Dictionary<string, object> ModelOutputDataDictionaryExternalTasks { get; set; } 
    }
}
