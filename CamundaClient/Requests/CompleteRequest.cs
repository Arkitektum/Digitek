using System.Collections.Generic;
using CamundaClient.Dto;

namespace CamundaClient.Requests
{
    class CompleteRequest
    {
        public string BusinessKey { get; set; }
        public Dictionary<string, Variable> Variables { get; set; }
        public string WorkerId { get; set; }
    }
}
