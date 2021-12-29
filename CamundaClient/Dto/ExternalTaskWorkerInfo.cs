using System;
using System.Collections.Generic;
using CamundaClient.Worker;

namespace CamundaClient.Dto
{
    internal class ExternalTaskWorkerInfo
    {
        public int Retries { get; internal set; }
        public long RetryTimeout { get; internal set; }
        public Type Type { get; internal set; }
        public string TopicName { get; internal set; }
        public List<string> VariablesToFetch { get; internal set; }
        public IExternalTaskAdapter TaskAdapter { get; internal set; }
    }
}
