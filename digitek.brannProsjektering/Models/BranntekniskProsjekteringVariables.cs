using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Routing.Constraints;

namespace digitek.brannProsjektering.Models
{
    public class BranntekniskProsjekteringVariables
    {
        //Opt1 to RKL and To Opt1 BKL
        public string typeVirksomhet { get; set; }

        //Opt1 to BKL + RKL from Opt1 
        public int? antallEtasjer { get; set; }
        public int? brtArealPrEtasje { get; set; }
        public int? arealBrannseksjonPrEtasje { get; set; }
        public int? brannenergi { get; set; }
        public bool? bygningOffentligUnderTerreng { get; set; }
        public bool? utgangTerrengAlleBoenheter { get; set; }
        public int? avstandMellomMotstVinduerIMeter { get; set; }
        public int? brtArealBygg { get; set; }
        public bool? kravOmHeis { get; set; }

        ////Opt2 to RKL
        //public bool? bareSporadiskPersonopphold { get; set; }
        //public bool? alleKjennerRomningsVeiene { get; set; }
        //public bool? beregnetForOvernatting { get; set; }
        //public bool? liteBrannfarligAktivitet { get; set; }

        ////Opt2 to BKL
        //public string konsekvensAvBrann { get; set; }

        
        //Outputs from other DMN
        //public string rkl { get; set; }
        //public string bkl { get; set; }
        //public long? brannalarmKategori { get; set; }
        //public string brannTiltakStrSeksjonBelastning { get; set; }
        //public string kravBrannmotstSeksjVegg { get; set; }
        //public bool? kravLedesystemEvakuering { get; set; }
        //public string trappeRomKlasse { get; set; }


    }
}