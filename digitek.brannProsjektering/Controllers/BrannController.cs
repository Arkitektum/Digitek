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
    public class DigiTekK11Controller : ControllerBase
    {
        private readonly ICamundaEngineClient _camundaClient;

        public DigiTekK11Controller(ICamundaEngineClient camundaClient)
        {
            _camundaClient = camundaClient;
        }
        
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="brannteknisk"></param>
        /// <returns></returns>
        // POST: api/DigiTek17K11
        [HttpPost, Route("RisikoklassenModel")]
        public IActionResult PostRkl([FromBody] RisikoklasseModel brannteknisk)
        {
            var key = "RisikoklassenModel.Net";

            var dictionary = brannteknisk.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(brannteknisk, null));
            var responce = _camundaClient.BpmnWorkflowService.StartProcessInstance(key, dictionary);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return Ok(responce);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="brannteknisk"></param>
        /// <returns></returns>
        // POST: api/DigiTek17K11
        [HttpPost, Route("BrannklasseModel")]
        public IActionResult PostBKL([FromBody] BrannklasseModel brannteknisk)
        {
            var key = "BrannklasseModel.Net";

            var dictionary = brannteknisk.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(brannteknisk, null));
            var responce = _camundaClient.BpmnWorkflowService.StartProcessInstance(key, dictionary);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return Ok(responce);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="brannteknisk"></param>
        /// <returns></returns>
        [HttpPost, Route("BrannmotstandModel")]
        public IActionResult PostBM([FromBody] BrannmotstandModel brannteknisk)
        {
            var key = "BrannmotstandModel.Net";

            var dictionary = brannteknisk.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(brannteknisk, null));
            var responce = _camundaClient.BpmnWorkflowService.StartProcessInstance(key, dictionary);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return Ok(responce);
        }

        [HttpPost, Route("BrannseksjonOgBrannmotstand")]
        public IActionResult PostBB([FromBody] BrannseksjonOgBrannmotstandModel brannteknisk)
        {
            var key = "BrannseksjonOgBrannmotstand.Net";

            var dictionary = brannteknisk.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(brannteknisk, null));
            var responce = _camundaClient.BpmnWorkflowService.StartProcessInstance(key, dictionary);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return Ok(responce);
        }
        /// <summary>
        /// Krav til branntiltak
        /// </summary>
        /// <param name="brannteknisk"></param>
        /// <returns></returns>
        [HttpPost, Route("KravTilBranntiltaktModel")]
        public IActionResult PostKB([FromBody] KravTilBranntiltakModel brannteknisk)
        {
            var key = "KravTilBranntiltaktModel.Net";

            var dictionary = brannteknisk.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(brannteknisk, null));
            var responce = _camundaClient.BpmnWorkflowService.StartProcessInstance(key, dictionary);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return Ok(responce);
        }
        /// <summary>
        /// Ledesystem
        /// </summary>
        /// <param name="brannteknisk"></param>
        /// <returns></returns>
        [HttpPost, Route("LedesystemModel")]
        public IActionResult PostL([FromBody] LedesystemModel brannteknisk)
        {
            var key = "LedesystemModel.Net";

            var dictionary = brannteknisk.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(brannteknisk, null));
            var responce = _camundaClient.BpmnWorkflowService.StartProcessInstance(key, dictionary);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return Ok(responce);
        }
        /// <summary>
        /// Get model output
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("GetResult/{id}", Name = "Get")]
        public IActionResult Get(string id)
        {
            var responce = _camundaClient.BpmnWorkflowService.GetProcessVariables(id);
            if (responce != null && responce.Any())
            {

                var newDict = responce.Where(value => value.Key.Contains("modelOutputs") || value.Key.Contains("modelDataDictionary"))
                    .ToDictionary(value => value.Key, value => value.Value);
                return Ok(newDict);
            }
            return BadRequest(responce);
        }
    }

}
