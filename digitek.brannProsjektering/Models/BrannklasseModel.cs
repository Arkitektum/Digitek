using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace digitek.brannProsjektering.Models
{
    public class BrannklasseModel
    {
        //Op1
        public string typeVirksomhet { get; set; }
        public long? antallEtasjer { get; set; }
        public string rkl { get; set; }
        public long? brtArealPrEtasje { get; set; }
        public bool? utgangTerrengAlleBoenheter { get; set; }
        //Opt2
        public string konsekvensAvBrann { get; set; }
    }
}
