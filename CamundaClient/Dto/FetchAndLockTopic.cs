using System.Collections.Generic;

namespace CamundaClient.Dto
{
    public class FetchAndLockTopic
    {
        public string TopicName { get; set; }
        public long LockDuration { get; set; }
        public IEnumerable<string> Variables { get; set; }
    }
}
