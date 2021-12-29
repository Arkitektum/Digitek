﻿namespace digitek.brannProsjektering.Models
{
    public class KravTilBranntiltakObject
    {

        public UserInfo UserInfo { get; set; }
        public KravTilBranntiltakModel ModelInputs { get; set; }

    }

    public class KravTilBranntiltakModel
    {
        public string rkl { get; set; }
        public string typeVirksomhet { get; set; }
        public int? antallEtasjer { get; set; }
        public bool? kravOmHeis { get; set; }
        public long? brtArealBygg { get; set; }
    }
}
