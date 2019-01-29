using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace digitek.brannProsjektering.Models
{
    public class BrannmotstandObject
    {
       
        public UserInfo UserInfo { get; set; }
        public BrannmotstandModel ModelInputs { get; set; }

    }

    public class BrannmotstandModel
    {
        public string rkl { get; set; }
        public string bkl { get; set; }
        public long? antallEtasjer { get; set; }
        public long? arealBrannseksjonPrEtasje { get; set; }
    }
}
