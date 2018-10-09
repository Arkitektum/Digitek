using System;
using System.Reflection;
using CamundaClient;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace digitek.brannProsjektering
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CamundaEngineClient camunda = new CamundaEngineClient();
            try
            {
                var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
                camunda.Startup(assemblyName); // Deploys all models to Camunda and Start all found ExternalTask-Workers
            }
            catch (Exception exception)
            {
                Console.WriteLine("\n\n" + "Can't deploy to Server.\n\n");
            }

            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
