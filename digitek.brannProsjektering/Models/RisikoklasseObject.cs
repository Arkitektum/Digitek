namespace digitek.brannProsjektering.Models
{
    public class RisikoklasseObject
    {

        public UserInfo UserInfo { get; set; }
        public RisikoklasseModel ModelInputs { get; set; }

    }

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
