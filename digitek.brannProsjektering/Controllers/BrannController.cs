using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CamundaClient;
using digitek.brannProsjektering.Models;
using CamundaClient.Requests;
using CamundaClient.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;

namespace digitek.brannProsjektering.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DigiTek17K11Controller : ControllerBase
    {
        private readonly ICamundaEngineClient _camundaClient;

        public DigiTek17K11Controller(ICamundaEngineClient camundaClient)
        {
            _camundaClient = camundaClient;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="branntekniskProsjekteringModel"></param>
        /// <param name="justValues"></param>
        /// <returns></returns>
        [HttpPost, Route("BranntekniskProsjekteringModel")]
        [Produces("application/json", Type = typeof(BranntekniskProsjekteringModel))]
        [Consumes("application/Json")]
        public IActionResult Postbrannpro([FromBody] BranntekniskProsjekteringModel branntekniskProsjekteringModel, bool? justValues)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var key = "BranntekniskProsjekteringModel";
            var dictionary = ModelToDictionary(branntekniskProsjekteringModel);

            var responce = _camundaClient.BpmnWorkflowService.StartProcessInstance(key, dictionary);

            var processVariables = GetVariables(responce, justValues);

            if (processVariables == null)
                return BadRequest(responce);

            return Ok(processVariables);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="risikoklasseModel"></param>
        /// <param name="justValues"></param>
        /// <returns></returns>
        [HttpPost, Route("RisikoklasseSubModel")]
        [Produces("application/json", Type = typeof(RisikoklasseModel))]
        [Consumes("application/Json")]
        public IActionResult PostRisikoklasseSubModel([FromBody] RisikoklasseModel risikoklasseModel, bool? justValues)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var key = "RisikoklasseSubModel";

            var dictionary = ModelToDictionary(risikoklasseModel);

            var responce = _camundaClient.BpmnWorkflowService.StartProcessInstance(key, dictionary);

            var processVariables = GetVariables(responce, justValues);

            if (processVariables == null)
                return BadRequest(responce);

            return Ok(processVariables);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="brannklasseModel"></param>
        /// <param name="justValues"></param>
        /// <returns></returns>
        [HttpPost, Route("BrannklasseSubModel")]
        [Produces("application/json", Type = typeof(BrannklasseModel))]
        [Consumes("application/Json")]
        public IActionResult PostBrannklasseModel([FromBody] BrannklasseModel brannklasseModel, bool? justValues)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var key = "BrannklasseSubModel";

            var dictionary = ModelToDictionary(brannklasseModel);

            var responce = _camundaClient.BpmnWorkflowService.StartProcessInstance(key, dictionary);

            var processVariables = GetVariables(responce, justValues);

            if (processVariables == null)
                return BadRequest(responce);

            return Ok(processVariables);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="kravTilBranntiltakModel"></param>
        /// <param name="justValues"></param>
        /// <returns></returns>
        [HttpPost, Route("KravTilBranntiltakSubModel")]
        [Produces("application/json", Type = typeof(KravTilBranntiltakModel))]
        [Consumes("application/Json")]
        public IActionResult PostKravTilBranntiltakSubModel([FromBody] KravTilBranntiltakModel kravTilBranntiltakModel, bool? justValues)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var key = "KravTilBranntiltakSubModel";

            var dictionary = ModelToDictionary(kravTilBranntiltakModel);

            var responce = _camundaClient.BpmnWorkflowService.StartProcessInstance(key, dictionary);

            var processVariables = GetVariables(responce, justValues);

            if (processVariables == null)
                return BadRequest(responce);

            return Ok(processVariables);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="brannseksjonOgBrannmotstandModel"></param>
        /// <param name="justValues"></param>
        /// <returns></returns>
        [HttpPost, Route("BrannseksjonOgBrannmotstandSubModel")]
        [Produces("application/json", Type = typeof(BrannseksjonOgBrannmotstandModel))]
        [Consumes("application/Json")]
        public IActionResult PostBrannseksjonOgBrannmotstandSubModel([FromBody] BrannseksjonOgBrannmotstandModel brannseksjonOgBrannmotstandModel, bool? justValues)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var key = "BrannseksjonOgBrannmotstandSubModel";

            var dictionary = ModelToDictionary(brannseksjonOgBrannmotstandModel);

            var responce = _camundaClient.BpmnWorkflowService.StartProcessInstance(key, dictionary);

            var processVariables = GetVariables(responce, justValues);

            if (processVariables == null)
                return BadRequest(responce);

            return Ok(processVariables);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="brannmotstandModel"></param>
        /// <param name="justValues"></param>
        /// <returns></returns>
        [HttpPost, Route("BrannmotstandSubModel")]
        [Produces("application/json", Type = typeof(BrannmotstandModel))]
        [Consumes("application/Json")]
        public IActionResult PostBrannmotstandSubModel([FromBody] BrannmotstandModel brannmotstandModel, bool? justValues)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var key = "BrannmotstandSubModel";

            var dictionary = ModelToDictionary(brannmotstandModel);

            var responce = _camundaClient.BpmnWorkflowService.StartProcessInstance(key, dictionary);

            var processVariables = GetVariables(responce, justValues);

            if (processVariables == null)
                return BadRequest(responce);

            return Ok(processVariables);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="justValues"></param>
        /// <returns></returns>
        [HttpGet("GetResult/{id}", Name = "Get")]
        public IActionResult Get(string id, bool? justValues)
        {
            var responce = _camundaClient.BpmnWorkflowService.GetProcessVariables(id);
            if (responce != null && responce.Any())
            {
                if (justValues.HasValue && justValues.Value)
                {
                    var newDict = responce.Where(value => value.Key.Contains("modelOutputs"))
                        .ToDictionary(value => value.Key, value => value.Value);
                    return Ok(newDict);
                }

                return Ok(responce);
            }
            return BadRequest(responce);
        }

        private Dictionary<string, object> ModelToDictionary(object model)
        {
            Dictionary<string, object> modelDictionary = model.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(model, null)); ;
            return modelDictionary;
        }

        private Dictionary<string, object> GetVariables(string executionId, bool? justValues)
        {
            Dictionary<string, object> processVariables = null;

            for (int i = 0; i < 3; i++)
            {
                processVariables = _camundaClient.BpmnWorkflowService.GetProcessVariables(executionId);
                processVariables.Add("executionId", executionId);
                if (processVariables != null && processVariables.Any())
                {
                    if (processVariables.ContainsKey("Advarsel"))
                    {

                        return processVariables;
                    }
                    if (processVariables.ContainsKey("modelOutputs"))
                    {
                        if (justValues.HasValue && justValues.Value)
                        {
                            var newDict = processVariables.Where(value => value.Key.Contains("modelOutputs"))
                                .ToDictionary(value => value.Key, value => value.Value);
                            newDict.Add("executionId", executionId);
                            return newDict;
                        }
                        return processVariables;
                    }
                }
                Task.Delay(500);
            }

            return processVariables;
        }

    }

}
