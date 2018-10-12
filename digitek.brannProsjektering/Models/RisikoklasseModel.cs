using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace digitek.brannProsjektering.Models
{
    public class RisikoklasseModel
    {
        //Opt1
        public string typeVirksomhet { get; set; }
        //Opt2
        public bool? bareSporadiskPersonopphold { get; set; }
        public bool? alleKjennerRomningsVeiene { get; set; }
        public bool? beregnetForOvernatting { get; set; }
        public bool? liteBrannfarligAktivitet { get; set; }
    }
}
