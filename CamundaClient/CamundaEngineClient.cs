using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CamundaClient.Service;
using CamundaClient.Worker;
using digitek.brannProsjektering.Models;

namespace CamundaClient
{
    public interface ICamundaEngineClient
    {
        BpmnWorkflowService BpmnWorkflowService { get; }
        HumanTaskService HumanTaskService { get; }
        RepositoryService RepositoryService { get; }
        ExternalTaskService ExternalTaskService { get; }
        void Startup(string assemblyName);
        void Shutdown();
        void StartWorkers(string assemblyName);
        void StopWorkers();
    }

    public class CamundaEngineClient : ICamundaEngineClient
    {
        public static string DEFAULT_URL = "http://localhost:8080/engine-rest/engine/default/";
        public static string COCKPIT_URL = "http://localhost:8080/camunda/app/cockpit/default/";

        private IList<ExternalTaskWorker> _workers = new List<ExternalTaskWorker>();
        private CamundaClientHelper _camundaClientHelper;

        public CamundaEngineClient(AppSettings settings): this(new Uri(settings.CamundaUrl), null, null) { }
        
        private CamundaEngineClient(Uri restUrl, string userName, string password)
        {
            _camundaClientHelper = new CamundaClientHelper(restUrl, userName, password);
        }

        public BpmnWorkflowService BpmnWorkflowService => new BpmnWorkflowService(_camundaClientHelper);

        public HumanTaskService HumanTaskService => new HumanTaskService(_camundaClientHelper);

        public RepositoryService RepositoryService => new RepositoryService(_camundaClientHelper);

        public ExternalTaskService ExternalTaskService => new ExternalTaskService(_camundaClientHelper);

        public void Startup(string assemblyName)
        {
            this.StartWorkers(assemblyName);
            this.RepositoryService.AutoDeploy(assemblyName);
        }

        public void Shutdown()
        {
            this.StopWorkers();
        }

        public void StartWorkers(string assemblyName)
        {
            var assemblys = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.FullName.Contains(assemblyName));
            var assembly = (Assembly)assemblys.FirstOrDefault();
            var externalTaskWorkers = RetrieveExternalTaskWorkerInfo(assembly);

            foreach (var taskWorkerInfo in externalTaskWorkers)
            {
                Console.WriteLine($"Register Task Worker for Topic '{taskWorkerInfo.TopicName}'");
                ExternalTaskWorker worker = new ExternalTaskWorker(ExternalTaskService, taskWorkerInfo);
                _workers.Add(worker);
                worker.StartWork();
            }
        }

        private static IEnumerable<Dto.ExternalTaskWorkerInfo> RetrieveExternalTaskWorkerInfo(System.Reflection.Assembly assembly)
        {
            // find all classes with CustomAttribute [ExternalTask("name")]
            var externalTaskWorkers =
                from t in assembly.GetTypes()
                let externalTaskTopicAttribute = t.GetCustomAttributes(typeof(ExternalTaskTopicAttribute), true).FirstOrDefault() as ExternalTaskTopicAttribute
                let externalTaskVariableRequirements = t.GetCustomAttributes(typeof(ExternalTaskVariableRequirementsAttribute), true).FirstOrDefault() as ExternalTaskVariableRequirementsAttribute
                where externalTaskTopicAttribute != null
                select new Dto.ExternalTaskWorkerInfo
                {
                    Type = t,
                    TopicName = externalTaskTopicAttribute.TopicName,
                    Retries = externalTaskTopicAttribute.Retries,
                    RetryTimeout = externalTaskTopicAttribute.RetryTimeout,
                    VariablesToFetch = externalTaskVariableRequirements?.VariablesToFetch,
                    TaskAdapter = t.GetConstructor(Type.EmptyTypes)?.Invoke(null) as IExternalTaskAdapter
                };
            return externalTaskWorkers;
        }

        public void StopWorkers()
        {
            foreach (ExternalTaskWorker worker in _workers)
            {
                worker.StopWork();
            }
        }

        // HELPER METHODS

    }
}
