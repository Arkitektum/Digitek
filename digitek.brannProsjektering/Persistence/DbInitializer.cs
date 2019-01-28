using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

            BranntekniskProsjekteringModel branntekniskProsjekteringModel = new BranntekniskProsjekteringModel()
            {
                ModelInputs = new ModelInputs(){
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
                    Organisasjonsnummer = "1212131551",
                    Email = "Noko@emial.no",
                }
            };

            var useRecords = new UseRecord()
            {
                DateTime = DateTime.Now,
                Model = "BranntekniskProsjekteringModel",
                InputJson = JsonConvert.SerializeObject(branntekniskProsjekteringModel.ModelInputs),
                ResponseCode = 200,
                ResponseText = "Json",
                Navn = branntekniskProsjekteringModel.UserInfo.Navn,
                OrganisasjonsNavn = branntekniskProsjekteringModel.UserInfo.OrganisasjonsNavn,
                Organisasjonsnummer = branntekniskProsjekteringModel.UserInfo.Organisasjonsnummer,
                Email = branntekniskProsjekteringModel.UserInfo.Email,
                ExecutionNr = Guid.NewGuid().ToString(),
                Kapitel = "12"
            };

            context.UseRecords.Add(useRecords);
            context.SaveChanges();
        }
    }
}
