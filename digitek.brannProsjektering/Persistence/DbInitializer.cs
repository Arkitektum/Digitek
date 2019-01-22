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
                ModelInputs = {
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
                }
            };

            var useRecords = new UseRecord()
            {
                DateTime = DateTime.Now,
                Email = "Noko@emial.no",
                InputJson = JsonConvert.SerializeObject(branntekniskProsjekteringModel.ModelInputs),
                Model = "BranntekniskProsjekteringModel",
                Name = "Matias Gonzalez",
                ResponseCode = 200,
                ResponseText = "Json",
                ExecutionNr = Guid.NewGuid().ToString()
            };

            context.UseRecords.Add(useRecords);
            context.SaveChanges();
        }
    }
}
