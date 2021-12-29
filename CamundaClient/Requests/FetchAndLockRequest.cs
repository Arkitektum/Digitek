using System.Collections.Generic;
using CamundaClient.Dto;

namespace CamundaClient.Requests
{
    public class FetchAndLockRequest
    {
        public string WorkerId { get; set; }
        public int MaxTasks { get; set; }
        public bool UsePriority { get; set; }
        public List<FetchAndLockTopic> Topics { get; set; } = new List<FetchAndLockTopic>();
        public long AsyncResponseTimeout { get; set;  }
    }
}
