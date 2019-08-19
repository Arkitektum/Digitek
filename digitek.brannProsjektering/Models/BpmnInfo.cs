using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace digitek.brannProsjektering.Models
{
    public class BpmnInfo
    {
        public string BpmnId { get; set; }
        public string BpmnNavn { get; set; }
        
        ///// <summary>
        ///// The Id of the DMN that is use in the process
        ///// </summary>
        //public string DmnId { get; set; }
        ///// <summary>
        ///// Given Name of the DMN
        ///// </summary>
        //public string DmnNavn { get; set; }
        ///// <summary>
        ///// Id for the outputs variables from the DMN process
        ///// </summary>
        public string DmnResultatvariabel { get; set; }

        public List<DmnInfo> DmnInfos { get; set; }
    }
}
