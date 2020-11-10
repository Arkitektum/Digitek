using CamundaClient.Dto;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace CamundaClient
{

    public class CamundaClientHelper
    {
        public Uri RestUrl { get; }
        public const string CONTENT_TYPE_JSON = "application/json";
        public string RestUsername { get; }
        public string RestPassword { get; }

        private static HttpClient httpClient;

        public CamundaClientHelper(Uri restUrl, string username, string password)
        {
            this.RestUrl = restUrl;
            this.RestUsername = username;
            this.RestPassword = password;
        }

        public HttpClient HttpClient()
        {
            if (httpClient == null)
            {
                httpClient = new HttpClient();
                
                https://stackoverflow.com/a/33091871
                //specify to use TLS 1.2 as default connection
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                if (RestUsername != null)
                {
                    var byteArray = Encoding.ASCII.GetBytes($"{RestUsername}:{RestPassword}");
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                }

                // Add an Accept header for JSON format.
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(CONTENT_TYPE_JSON));
                httpClient.BaseAddress = RestUrl;
            }

            return httpClient;
        }

        public static Dictionary<string, Variable> ConvertVariables(Dictionary<string, object> variables)
        {
            // report successful execution
            var result = new Dictionary<string, Variable>();
            if (variables == null)
            {
                return result;
            }
            foreach (var variable in variables)
            {
                Variable camundaVariable = new Variable
                {
                    Value = variable.Value
                };
                result.Add(variable.Key, camundaVariable);
            }
            return result;
        }
    }
}
