using System.Collections.Generic;

namespace digitek.brannProsjektering.Models
{
    public class BrannDictionaryModel
    {
        public Dictionary<string, TableInfo> BranntekniskProsjekteringDictionary { get; set; }
    }

    public class TableInfo
    {
        public string DmnId { get; set; }
        public string DmnNavn { get; set; }
        public string TekKapitel { get; set; }
        public string TekLedd { get; set; }
        public string TekTabell { get; set; }
        public string TekForskriften { get; set; }
        public string TekWebLink { get; set; }
        public VariablesInfo[] InputVariablesInfo { get; set; }
        public VariablesInfo[] OutputVariablesInfo { get; set; }
    }

    public class VariablesInfo
    {
        public string VariabelId { get; set; }
        public string VariabelNavn { get; set; }
        public string VariabelBeskrivelse { get; set; }
    }
}