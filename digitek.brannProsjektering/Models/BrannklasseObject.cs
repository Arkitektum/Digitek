﻿namespace digitek.brannProsjektering.Models
{
    public class BrannklasseObject
    {
        public UserInfo UserInfo { get; set; }
        public BrannklasseModel ModelInputs { get; set; }
       
    }

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
