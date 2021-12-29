namespace digitek.brannProsjektering.Models
{
    public class UserInfo
    {
        public string Navn { get; set; }
        public string Organisasjonsnummer { get; set; }
        public string OrganisasjonsNavn { get; set; }
        //[DataType(DataType.EmailAddress)]
        //[EmailAddress]
        public string Email { get; set; }
    }
}
