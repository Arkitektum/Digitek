using System.Collections.Generic;

namespace digitek.brannProsjektering.Models
{
    public class bpmnInformationModel
    {
        public string BpmnName { get; set; }
        public string BpmnId { get; set; }
        public Dictionary<string,string> BpmnInputs { get; set; }
    }
}
