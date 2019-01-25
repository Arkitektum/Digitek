using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace digitek.brannProsjektering.Models
{
    public class KravTilBranntiltakModel
    {
        public string rkl { get; set; }
        public string typeVirksomhet { get; set; }
        public int? antallEtasjer { get; set; }
        public bool? kravOmHeis { get; set; }
        public int? brtArealBygg { get; set; }
    }
}
