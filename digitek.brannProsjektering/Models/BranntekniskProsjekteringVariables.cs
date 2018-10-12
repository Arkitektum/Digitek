using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Routing.Constraints;

namespace digitek.brannProsjektering.Models
{
    public class BranntekniskProsjekteringModel
    {
        //Opt1 to RKL and To Opt1 BKL
        public string typeVirksomhet { get; set; }

        //Opt1 to BKL + RKL from Opt1 
        public long? antallEtasjer { get; set; }
        public long? brtArealPrEtasje { get; set; }
        public bool? utgangTerrengAlleBoenheter { get; set; }

        //Opt2 to RKL

        public string bareSporadiskPersonopphold { get; set; }
        public bool? alleKjennerRomningsVeiene { get; set; }
        public bool? beregnetForOvernatting { get; set; }
        public bool? liteBrannfarligAktivitet { get; set; }

        //Opt2 to BKL
        public string konsekvensAvBrann { get; set; }

        // Commun to all
        public long? arealBrannseksjonPrEtasje { get; set; }
        public long? brannenergi { get; set; }
        public bool? bygningOffentligUnderTerreng { get; set; }

        //Outputs from other DMN
        public string rkl { get; set; }
        public string bkl { get; set; }
        public long? brannalarmKategori { get; set; }
        public string brannTiltakStrSeksjonBelastning { get; set; }
        public string kravBrannmotstSeksjVegg { get; set; }
        public bool? kravLedesystemEvakuering { get; set; }
        public string trappeRomKlasse { get; set; }


    }
}