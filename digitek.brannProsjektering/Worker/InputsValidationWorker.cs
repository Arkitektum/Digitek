using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CamundaClient.Dto;
using CamundaClient.Worker;
using digitek.brannProsjektering.Models;
using Newtonsoft.Json;

namespace digitek.brannProsjektering.Worker
{
    [ExternalTaskTopic("brannInputsValidation")]
    //[ExternalTaskVariableRequirements("typeVirksomhet", "antallEtasjer", "brtArealPrEtasje", "utgangTerrengAlleBoenheter",
    //    "bareSporadiskPersonopphold", "alleKjennerRomningsVeiene", "beregnetForOvernatting", "liteBrannfarligAktivitet",
    //    "konsekvensAvBrann", "brannenergi", "bygningOffentligUnderTerreng", "arealBrannseksjonPrEtasje")]
    public class InputsValidationWorker : IExternalTaskAdapter
    {
        public void Execute(ExternalTask externalTask, ref Dictionary<string, object> resultVariables)
        {
            var inputsDictionary = new Dictionary<string, object>();
            // just create an id for demo purposes here
            BranntekniskProsjekteringVariables branntekniskProsjektering = new BranntekniskProsjekteringVariables();

            try { branntekniskProsjektering.typeVirksomhet = (string)externalTask.Variables["typeVirksomhet"].Value; } catch {/*ignored*/}
            try { branntekniskProsjektering.antallEtasjer = Convert.ToInt64(externalTask.Variables["antallEtasjer"].Value); } catch {/*ignored*/}
            try { branntekniskProsjektering.brtArealPrEtasje = Convert.ToInt64(externalTask.Variables["brtArealPrEtasje"].Value); } catch {/*ignored*/}
            try { branntekniskProsjektering.utgangTerrengAlleBoenheter = (bool)externalTask.Variables["utgangTerrengAlleBoenheter"].Value; } catch {/*ignored*/}
            try { branntekniskProsjektering.bareSporadiskPersonopphold = (string)externalTask.Variables["bareSporadiskPersonopphold"].Value; } catch {/*ignored*/}
            try { branntekniskProsjektering.alleKjennerRomningsVeiene = (bool)externalTask.Variables["alleKjennerRomningsVeiene"].Value; } catch {/*ignored*/}
            try { branntekniskProsjektering.beregnetForOvernatting = (bool)externalTask.Variables["beregnetForOvernatting"].Value; } catch {/*ignored*/}
            try { branntekniskProsjektering.liteBrannfarligAktivitet = (bool)externalTask.Variables["liteBrannfarligAktivitet"].Value; } catch {/*ignored*/}
            try { branntekniskProsjektering.konsekvensAvBrann = (string)externalTask.Variables["konsekvensAvBrann"].Value; } catch {/*ignored*/}
            try { branntekniskProsjektering.brannenergi = Convert.ToInt64(externalTask.Variables["brannenergi"].Value); } catch {/*ignored*/}
            try { branntekniskProsjektering.bygningOffentligUnderTerreng = (Boolean)externalTask.Variables["bygningOffentligUnderTerreng"].Value; } catch {/*ignored*/}
            try { branntekniskProsjektering.arealBrannseksjonPrEtasje = Convert.ToInt64(externalTask.Variables["arealBrannseksjonPrEtasje"].Value); } catch {/*ignored*/}
            try { branntekniskProsjektering.avstandMellomMotstVinduerIMeter = Convert.ToInt64(externalTask.Variables["avstandMellomMotstVinduerIMeter"].Value); } catch {/*ignored*/}

            //Outputs from other DMN
            try { branntekniskProsjektering.rkl = (string)externalTask.Variables["rkl"].Value; } catch {/*ignored*/}
            try { branntekniskProsjektering.bkl = (string)externalTask.Variables["bkl"].Value; } catch {/*ignored*/}
            try { branntekniskProsjektering.brannalarmKategori = Convert.ToInt64(externalTask.Variables["brannalarmKategori"].Value); } catch {/*ignored*/}
            try { branntekniskProsjektering.brannTiltakStrSeksjonBelastning = (string)externalTask.Variables["brannTiltakStrSeksjonBelastning"].Value; } catch {/*ignored*/}
            try { branntekniskProsjektering.kravBrannmotstSeksjVegg = (string)externalTask.Variables["kravBrannmotstSeksjVegg"].Value; } catch {/*ignored*/}
            try { branntekniskProsjektering.kravLedesystemEvakuering = (Boolean)externalTask.Variables["kravLedesystemEvakuering"].Value; } catch {/*ignored*/}
            try { branntekniskProsjektering.trappeRomKlasse = (string)externalTask.Variables["trappeRomKlasse"].Value; } catch {/*ignored*/}


            // Convert class model to Dictionary
            var newDictionary = branntekniskProsjektering.GetType()
                 .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                 .ToDictionary(prop => prop.Name, prop => prop.GetValue(branntekniskProsjektering, null));
            foreach (var variable in newDictionary)
            {
                resultVariables.Add(variable.Key, variable.Value);
                if (variable.Value != null)
                    inputsDictionary.Add(variable.Key, new Variable() { Value = variable.Value });
            }
            resultVariables.Add("modelInputs", JsonConvert.SerializeObject(inputsDictionary));
        }
    }
}