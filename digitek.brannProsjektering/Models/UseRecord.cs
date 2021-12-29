using System;

namespace digitek.brannProsjektering.Models
{
    public class UseRecord
    {
        public int Id { get; set; }
        public DateTime DateTime { get; set; }
        public string Model { get; set; }
        public string InputJson { get; set; }
        public int ResponseCode { get; set; }
        public string ResponseText { get; set; }
        public string Navn { get; set; }
        public string Organisasjonsnummer { get; set; }
        public string OrganisasjonsNavn { get; set; }
        public string Email { get; set; }
        public string ExecutionNr { get; set; }
        public string Kapitel { get; set; }

    }
}
