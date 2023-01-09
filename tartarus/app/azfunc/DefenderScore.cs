
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Tarta;

namespace Tarta
{
    public class DeviceScore
    {
        private static HttpClient httpClient = new HttpClient();
        private static string GET_URL = "https://api-eu.securitycenter.microsoft.com/api/configurationScore"; // Change these uri for GET

        [FunctionName("DeviceScore")]
        public static async Task Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/scores/get-score")]
        HttpRequest req, ILogger log)
        {

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetToken()); // Add Bearer token to the request.
            var response = await httpClient.GetAsync(GET_URL); // Get Request
            var result = response.Content.ReadAsStringAsync(); // Read content as string 
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}{result.Result}");

            //var responseData = JsonConvert.DeserializeObject<JsonResponseData>(result.Result); // Convert JSON response to an object
            new ObjectResult(result.Result);
            //log.LogInformation($"View result: { responseData}");
        }



        public static string GetToken()
        {
            string tenantId = Environment.GetEnvironmentVariable("tenantId");
            string appId = Environment.GetEnvironmentVariable("appId");
            string appSecret = Environment.GetEnvironmentVariable("appSecret");
            const string authority = "https://login.microsoftonline.com";
            const string audience = "https://api.securitycenter.microsoft.com";

            IConfidentialClientApplication myApp = ConfidentialClientApplicationBuilder.Create(appId).WithClientSecret(appSecret).WithAuthority($"{authority}/{tenantId}").Build();

            List<string> scopes = new List<string>() { $"{audience}/.default" };

            AuthenticationResult authResult = myApp.AcquireTokenForClient(scopes).ExecuteAsync().GetAwaiter().GetResult();

            return authResult.AccessToken;

        }


    }
}
 
