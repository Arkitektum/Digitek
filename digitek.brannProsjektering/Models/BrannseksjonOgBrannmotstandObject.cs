using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace digitek.brannProsjektering.Models
{
    public class BrannseksjonOgBrannmotstandObject
    {
       
        public UserInfo UserInfo { get; set; }
        public BrannseksjonOgBrannmotstandModel ModelInputs { get; set; }

    }

    public class BrannseksjonOgBrannmotstandModel
    {
        public string typeVirksomhet { get; set; }
        public long? arealBrannseksjonPrEtasje { get; set; }
        public long? brannenergi { get; set; }
        public string bkl { get; set; }
        public long? avstandMellomMotstVinduerIMeter { get; set; }
    }
}
