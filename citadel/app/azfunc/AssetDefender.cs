using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Net;
using System.Text;
using System.Text.Json;
using Citadel.Model;
using Citadel.Services;
using Citadel.Model.Root;
using Citadel.Model.Asset;
using Citadel.Model.Defender;
using Microsoft.AspNetCore.Routing;


namespace Citadel
{
    public class CitadelAssetDefender
    {
        private static HttpClient httpClient = new HttpClient();
        private static string GET_URL = "https://api-us.securitycenter.microsoft.com/api/machines"; // Change these uri for GET //86842942-00cd-4d85-b907-152dbd277a88.mock.pstmn.io
        private static string POST_URL = "https://fa-cita-sbx-cac-01.azurewebsites.net/v1/customers/open/assets/"; // Change these uri for PUT //fa-cita-sbx-cac-01.azurewebsites.net/v1/customers/open/assets/

        private readonly ILogger<CitadelAssetDefender> _logger;
        private readonly IAssetService _asset;

        public CitadelAssetDefender(ILogger<CitadelAssetDefender> log, IAssetService asset)
        {
            _logger = log;
            _asset = asset;
        }

        // function runs daily at 1:30 am
        [FunctionName("TriggerDefenderAssets")]
        public void Run([TimerTrigger("0 30 1 * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        }

        [FunctionName("GetDefenderAssets")]
        public async Task<IActionResult> GetDefenderAssets(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = Constant.GET_DEFENDER_HOSTS_ROUTE)] HttpRequest req,
            ILogger _logger)
        {
            try
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetToken());
                var response = await httpClient.GetAsync(GET_URL); // Get Request
                var result = response.Content.ReadAsStringAsync(); // Read content as string
                _logger.LogInformation("C# HTTP trigger function processed a GET request");

                var responseData = JsonConvert.DeserializeObject<JsonResponseData>(result.Result); // Convert JSON response to an object

                if (responseData != null && responseData.Value?.Length > 0) // Check if the responseData object is not null and the Value lenght is greater than 0 then execute below statement.
                {
                    DefaultContractResolver contractResolver = new DefaultContractResolver // Setting default casing for JSON data as CamelCase for example convert FirstName to firstName
                    {
                        NamingStrategy = new CamelCaseNamingStrategy()
                    };
                    var jsonObject = JsonConvert.SerializeObject(ConvertToAssetData(responseData), new JsonSerializerSettings // Serilaize Object using CamelCase Notations.
                    {
                        ContractResolver = contractResolver,
                        Formatting = Formatting.Indented
                    });

                    // Put data
                    var httpContent = new StringContent(jsonObject, System.Text.Encoding.UTF8, "application/json"); // Create HttpPost content as type of json and enable UTF-8 Content
                    httpContent.Headers.Add("X-Company-Short", "open"); // Add Headers
                    var respMessage = await httpClient.PutAsync(POST_URL, httpContent); // PUT Request
                    var postResp = await respMessage.Content.ReadAsStringAsync(); // Read content as string 
                    _logger.LogInformation($"C# Timer trigger PUT function executed at: {DateTime.Now}{postResp}"); // Print to screen
                    //_logger.LogInformation($"json: {jsonObject}");
                    return new ObjectResult(postResp);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedAccessException("Invalid bearer token for Defender");
                }
                else
                    throw new Exception($"Somethings went wrong: {response.StatusCode}");
            }
            catch (BadRequestException e)
            {
                _logger.LogError(e.Message);
                return Common.ReturnErrorResponse(e.Message, "400");
            }
            catch (UnauthorizedAccessException e)
            {
                _logger.LogError(e.Message);
                return Common.ReturnErrorResponse(e.Message, "401");
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.Message);
                return Common.ReturnErrorResponse(e.Message, "400");
            }
            catch (Newtonsoft.Json.JsonException e)
            {
                _logger.LogError(e.Message);
                return Common.ReturnErrorResponse("Invalid JSON request", "400");
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return Common.ReturnErrorResponse(e.Message, "500");
            }
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

        private object ConvertToAssetData(JsonResponseData responseData)
        {
            return _asset.ConvertToAssetData(responseData);
        }

    }

}