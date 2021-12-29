using System;
using System.Linq;
using digitek.brannProsjektering.Models;
using Newtonsoft.Json;

namespace digitek.brannProsjektering.Persistence
{
    public class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();
            if (context.UseRecords.Any())
            {
                return;
            }

            BranntekniskProsjekteringObject branntekniskProsjekteringObject = new BranntekniskProsjekteringObject()
            {
                ModelInputs = new BranntekniskProsjekteringModel(){
                typeVirksomhet = "Bolig",
                antallEtasjer = 3,
                brtArealPrEtasje = 300,
                arealBrannseksjonPrEtasje = 300,
                brannenergi = 300,
                bygningOffentligUnderTerreng = false,
                utgangTerrengAlleBoenheter = true,
                avstandMellomMotstVinduerIMeter = 4,
                brtArealBygg = 900,
                kravOmHeis = false
                }, UserInfo =new UserInfo()
                {
                    Navn = "Matias Gonzalez",
                    OrganisasjonsNavn = "Arkitektum AS",
                    Organisasjonsnummer = "1234567879",
                    Email = "Noko@emial.no",
                }
            };

            var useRecords = new UseRecord()
            {
                DateTime = DateTime.Now,
                Model = "BranntekniskProsjekteringModel",
                InputJson = JsonConvert.SerializeObject(branntekniskProsjekteringObject.ModelInputs),
                ResponseCode = 200,
                ResponseText = "Json",
                Navn = branntekniskProsjekteringObject.UserInfo.Navn,
                OrganisasjonsNavn = branntekniskProsjekteringObject.UserInfo.OrganisasjonsNavn,
                Organisasjonsnummer = branntekniskProsjekteringObject.UserInfo.Organisasjonsnummer,
                Email = branntekniskProsjekteringObject.UserInfo.Email,
                ExecutionNr = Guid.NewGuid().ToString(),
                Kapitel = "12"
            };

            context.UseRecords.Add(useRecords);
            context.SaveChanges();
        }
    }
}
