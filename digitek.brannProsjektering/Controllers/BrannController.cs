using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
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
        /// <param name="Brannteknisk Prosjektering"></param>
        /// <returns></returns>
        // POST: api/DigiTek17K11
        [HttpPost, Route("BranntekniskProsjektering")]
        public IActionResult PostRkl([FromBody] BranntekniskProsjekteringModel branntekniskProsjekteringModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var key = "BranntekniskProsjekteringModel";
            var dictionary = branntekniskProsjekteringModel.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(branntekniskProsjekteringModel, null));

            var responce = _camundaClient.BpmnWorkflowService.StartProcessInstance(key, dictionary);

           
            var ResponceDictionary= new Dictionary<string,string>(){
            {
                "executionId",responce
            }};

            return Ok(ResponceDictionary);
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
    }

}
