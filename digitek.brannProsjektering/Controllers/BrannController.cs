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
using digitek.brannProsjektering.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace digitek.brannProsjektering.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DigiTek17K11Controller : ControllerBase
    {
        private readonly ICamundaEngineClient _camundaClient;
        private readonly IDbServices _dbServices;
        private Dictionary<string, object> _processVariables;

        public DigiTek17K11Controller(ICamundaEngineClient camundaClient, IDbServices dbServices)
        {
            _camundaClient = camundaClient;
            _dbServices = dbServices;

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

            var modelInputs = branntekniskProsjekteringModel.ModelInputs;
            var dictionary = ModelToDictionary(modelInputs);

            var responce = _camundaClient.BpmnWorkflowService.StartProcessInstance(key, dictionary);

            var actionResponse = ActionResultResponse(justValues, responce);
            //_dbServices.AddUseRecord();

            var useRecord = CreateUseRedordModel(branntekniskProsjekteringModel, responce, key);
            _dbServices.AddUseRecord(useRecord);


            return actionResponse;
        }
        private UseRecord CreateUseRedordModel(object branntekniskProsjekteringModel, string responce, string model)
        {
            var useRecord = new UseRecord();
            //add Model 
            useRecord.Model = model;

            // Add ExecutionId
            useRecord.ExecutionNr = ResponseIsExecutionId(responce) ? responce : null;

            //serialize Object
            var json = JsonConvert.SerializeObject(branntekniskProsjekteringModel);

            //replace String text from swagger to null
            var newJson = json.Replace("string", null);
            var jsonObj = JObject.Parse(newJson);

            //Add User Info
            var userInfo = jsonObj["UserInfo"];
            AddUserInfo(userInfo, ref useRecord);

            //Add date&Time
            useRecord.DateTime = DateTime.Now;

            // add Json input
            var modelInputs = JsonConvert.SerializeObject(jsonObj["ModelInputs"]);
            if (!string.IsNullOrEmpty(modelInputs))
                useRecord.InputJson = modelInputs;

            if (_processVariables != null)
            {
                useRecord.ResponseCode = 200;
                useRecord.ResponseText = JsonConvert.SerializeObject(_processVariables.ContainsKey("modelOutputs") ? _processVariables["modelOutputs"] : _processVariables);

            }
            else
            {
                var responceArrey = responce.Split("-", 2);
                if (int.TryParse(responceArrey[0], out var code))
                {
                    useRecord.ResponseCode = code;
                    useRecord.ResponseText = responceArrey[1];
                }
                else
                {
                    useRecord.ResponseText = responce;
                }
            }

            return useRecord;
        }

        private void AddUserInfo(JToken userInfo, ref UseRecord useRecord)
        {
            var user = userInfo.ToObject<UserInfo>();
            if (!ObjectPropertiesIsNullOrEmpty(user))
            {
                if (!string.IsNullOrEmpty(userInfo["Name"].ToString()))
                    useRecord.Name = userInfo["Name"].ToString();
                if (!string.IsNullOrEmpty(userInfo["Company"].ToString()))
                    useRecord.Company = userInfo["Company"].ToString();
                if (!string.IsNullOrEmpty(userInfo["Email"].ToString()))
                    useRecord.Email = userInfo["Email"].ToString();
            }
        }
        private static bool ObjectPropertiesIsNullOrEmpty(object obj)
        {
            var propertiesValues = obj.GetType().GetProperties()
                .Select(prop => prop.GetValue(obj, null))
                .Where(val => val != null)
                .Select(val => val.ToString())
                .Where(str => str.Length > 0)
                .Where(v => v.ToLower() != "string" && v != "0")
                .ToList();

            return !propertiesValues.Any();
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

            return ActionResultResponse(justValues, responce);
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

            return ActionResultResponse(justValues, responce);
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

            return ActionResultResponse(justValues, responce);
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

            return ActionResultResponse(justValues, responce);
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
            return ActionResultResponse(justValues, responce);
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
        private IActionResult ActionResultResponse(bool? justValues, string responce)
        {

            if (!ResponseIsExecutionId(responce))
                return BadRequest("Bad request");

            _processVariables = GetVariables(responce, justValues);

            return Ok(_processVariables);
        }
        private Dictionary<string, object> ModelToDictionary(object model)
        {
            Dictionary<string, object> modelDictionary = model.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(model, null)); ;
            return modelDictionary;
        }

        private bool ResponseIsExecutionId(string responce)
        {
            return Guid.TryParse(responce, out var guid);
        }

        private Dictionary<string, object> GetVariables(string executionId, bool? justValues)
        {
            Dictionary<string, object> processVariables = null;

            for (int i = 0; i < 3; i++)
            {
                processVariables = _camundaClient.BpmnWorkflowService.GetProcessVariables(executionId);


                if (processVariables.ContainsKey("Advarsel"))
                    return processVariables;

                if (processVariables.ContainsKey("Error"))
                {
                    processVariables.Add("Error", "Could not load result variables");
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
                Task.Delay(500);
            }
            processVariables.Add("executionId", executionId);
            return processVariables;
        }
    }

}
