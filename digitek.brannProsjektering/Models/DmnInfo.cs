using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace digitek.brannProsjektering.Models
{
    public class DmnInfo
    {
        public string DmnId { get; set; }
        public string DmnNavn { get; set; }
        public string FilNavn { get; set; }
        public string TekKapitel { get; set; }
        public string TekLedd { get; set; }
        public string TekTabell { get; set; }
        public string TekForskriften { get; set; }
        public string TekWebLink { get; set; }
        public VariablesInfo[] InputVariablesInfo { get; set; }
        public VariablesInfo[] OutputVariablesInfo { get; set; }
    }
}
