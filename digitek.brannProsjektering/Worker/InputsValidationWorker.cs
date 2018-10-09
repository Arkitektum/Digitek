using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CamundaClient.Dto;
using CamundaClient.Worker;
using digitek.brannProsjektering.Models;

namespace digitek.brannProsjektering.Worker
{
    [ExternalTaskTopic("brannInputsValidation")]
    [ExternalTaskVariableRequirements("typeVirksomhet", "antallEtasjer", "brtArealPrEtasje", "utgangTerrengAlleBoenheter",
        "bareSporadiskPersonopphold", "alleKjennerRomningsVeiene", "beregnetForOvernatting", "liteBrannfarligAktivitet",
        "konsekvensAvBrann", "brannenergi", "bygningOffentligUnderTerreng", "arealBrannseksjonPrEtasje")]
    public class InputsValidationWorker : IExternalTaskAdapter
    {
        public void Execute(ExternalTask externalTask, ref Dictionary<string, object> resultVariables)
        {
            // just create an id for demo purposes here
            BranntekniskProsjekteringModel branntekniskProsjektering = new BranntekniskProsjekteringModel();

            try { branntekniskProsjektering.typeVirksomhet = (string)externalTask.Variables["typeVirksomhet"].Value; }catch{/*ignored*/}

            try { branntekniskProsjektering.antallEtasjer = Convert.ToInt64(externalTask.Variables["antallEtasjer"].Value); }catch{/*ignored*/}
            try { branntekniskProsjektering.brtArealPrEtasje = Convert.ToInt64(externalTask.Variables["brtArealPrEtasje"].Value); }catch{/*ignored*/}
            try { branntekniskProsjektering.utgangTerrengAlleBoenheter = (bool)externalTask.Variables["utgangTerrengAlleBoenheter"].Value; }catch{/*ignored*/}
            try { branntekniskProsjektering.bareSporadiskPersonopphold = (string)externalTask.Variables["bareSporadiskPersonopphold"].Value; }catch{/*ignored*/}
            try { branntekniskProsjektering.alleKjennerRomningsVeiene = (bool)externalTask.Variables["alleKjennerRomningsVeiene"].Value; }catch{/*ignored*/}
            try { branntekniskProsjektering.beregnetForOvernatting = (bool)externalTask.Variables["beregnetForOvernatting"].Value; }catch{/*ignored*/}
            try { branntekniskProsjektering.liteBrannfarligAktivitet = (bool)externalTask.Variables["liteBrannfarligAktivitet"].Value; }catch{/*ignored*/}
            try { branntekniskProsjektering.konsekvensAvBrann = (string)externalTask.Variables["konsekvensAvBrann"].Value; }catch{/*ignored*/}
            try { branntekniskProsjektering.brannenergi = Convert.ToInt64(externalTask.Variables["brannenergi"].Value); }catch{/*ignored*/}
            try { branntekniskProsjektering.bygningOffentligUnderTerreng = (Boolean)externalTask.Variables["bygningOffentligUnderTerreng"].Value; }catch{/*ignored*/}
            try { branntekniskProsjektering.arealBrannseksjonPrEtasje = Convert.ToInt64(externalTask.Variables["arealBrannseksjonPrEtasje"].Value); } catch {/*ignored*/}

            var newDictionary = branntekniskProsjektering.GetType()
                 .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                 .ToDictionary(prop => prop.Name, prop => prop.GetValue(branntekniskProsjektering, null));
            foreach (var variable in newDictionary)
            {
                resultVariables.Add(variable.Key, variable.Value);
            }
            resultVariables.Add("modelInputs", newDictionary);
        }
    }
}