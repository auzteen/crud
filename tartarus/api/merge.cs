//###############################
//Defender Score
//################################

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using tartarus.Model.Vulnerability;
using Tartarus;
using Tartarus.Services;

namespace Tarta
{
    public class DeviceScore
    {
        private static HttpClient httpClient = new HttpClient();
        private static string GET_URL = "https://api-eu.securitycenter.microsoft.com/api/configurationScore"; // Change these uri for GET
        private static string GET_DEFENDER_SCORE_URI = "https://api.securitycenter.microsoft.com/api/configurationScore";
        //private static string POST_URL = "https://dummyjson.com/products/add";
        private readonly IVulnerabilityService _vulnerabilityService;
        public DeviceScore(IVulnerabilityService vulnerabilityService)
        {
            _vulnerabilityService=vulnerabilityService;
        }
        [FunctionName("DeviceScore")]
        public async Task Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/scores/get-score")]
        HttpRequest req, ILogger logger)
        {

            try
            {
                #region Defender Score Get Request
                var request = new HttpRequestMessage(HttpMethod.Get, GET_DEFENDER_SCORE_URI);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _vulnerabilityService.GetToken());
                var response = httpClient.SendAsync(request).GetAwaiter().GetResult();
                var result = response.Content.ReadAsStringAsync();
                var responseData = JsonConvert.DeserializeObject<ScoreData>(result.Result);
                #endregion

                if (responseData != null) // && responseData.Score > 0)
                {
                    var jsonObject = JsonConvert.SerializeObject(responseData, new JsonSerializerSettings
                    {
                        Formatting = Formatting.Indented
                    });

                    // var jwt_token = "xxxxxxxxxxxxx";
                    // httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt_token);
                    // var httpContent = new StringContent(jsonObject, System.Text.Encoding.UTF8, "application/json");
                    //  httpContent.Headers.Add("X-Company-Short", "open");
                    //  var respMessage = await httpClient.PostAsync(POST_URL, httpContent);
                    //  var postResp = await respMessage.Content.ReadAsStringAsync();
                    //  logger.LogInformation($"Post function executed at: {DateTime.Now}{postResp}");
                 logger.LogInformation($"{responseData.Score}");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new System.UnauthorizedAccessException("Invalid bearer token for Defender");
                }
                else
                    throw new Exception($"Somethings went wrong: {response.StatusCode}");
            }
            catch (BadRequestException e)
            {
                logger.LogError(e.Message);
                //return Common.ReturnErrorResponse(e.Message, "400");
            }
            catch (KeyNotFoundException e)
            {
                logger.LogError(e.Message);
                //return Common.ReturnErrorResponse(e.Message, "400");
            }
            catch (Newtonsoft.Json.JsonException e)
            {
                logger.LogError(e.Message);
                //return Common.ReturnErrorResponse("Invalid JSON request", "400");
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
                //return Common.ReturnErrorResponse(e.Message, "500");
            }
        }



        //public static string GetToken()
        //{
        //    string tenantId = Environment.GetEnvironmentVariable("tenantId");
        //    string appId = Environment.GetEnvironmentVariable("appId");
        //    string appSecret = Environment.GetEnvironmentVariable("appSecret");
        //    const string authority = "https://login.microsoftonline.com";
        //    const string audience = "https://api.securitycenter.microsoft.com";

        //    IConfidentialClientApplication myApp = ConfidentialClientApplicationBuilder.Create(appId).WithClientSecret(appSecret).WithAuthority($"{authority}/{tenantId}").Build();

        //    List<string> scopes = new List<string>() { $"{audience}/.default" };

        //    AuthenticationResult authResult = myApp.AcquireTokenForClient(scopes).ExecuteAsync().GetAwaiter().GetResult();

        //    return authResult.AccessToken;

        //}


    }
}



//##############################
//VulnerabilityDiscovery
//##############################
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
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Tarta;
using tartarus.Services;
using Tartarus.Model.Vulnerability;
using Tartarus.Services;

namespace Tarta
{
    public class DiscoverVulnerabilities
    {
        private static HttpClient httpClient = new HttpClient();
        private static string GET_VULNERABILITY_URL = "https://api.securitycenter.microsoft.com/api/vulnerabilities/machinesVulnerabilities"; // Change these uri for GET
        private static string POST_URL = "https://fa-cita-sbx-cac-01.azurewebsites.net/api/v1/customers/open/vulnerabilities/"; // Change these uri for POST
        private static string GET_MACHINES_URL = "https://api.securitycenter.microsoft.com/api/machines";
        private readonly IVulnerabilityService _vulnerabilityService;
        public DiscoverVulnerabilities(IVulnerabilityService vulnerabilityService)
        {
            _vulnerabilityService= vulnerabilityService;
        }
        [FunctionName("DiscoverVulnerabilities")]
        public async Task Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/vulnerabilities/discover")]
        HttpRequest req, ILogger log )
        {
            #region Machine Lists
            var serviceMachineData = new ResponseService<JsonResponseDataMachine>();
            var responseMachineModel = serviceMachineData.GetResponseData(GET_MACHINES_URL, _vulnerabilityService);
            #endregion

            #region Machine Vulnerabilty List
            var serviceVulnerability = new ResponseService<JsonResponseDataVulnerabilityMachines>();
            var responseVulnerabilityModel = serviceVulnerability.GetResponseData(GET_VULNERABILITY_URL, _vulnerabilityService);
            #endregion

            //POST DATA
            if ((responseMachineModel.Response != null && responseMachineModel.Response.Value?.Length > 0)
                && (responseVulnerabilityModel.Response != null && responseVulnerabilityModel.Response.Value?.Length > 0))
            // Check if the responseModel.Response object is not null and the Value lenght is greater than 0 then execute below statement.
            {
                DefaultContractResolver contractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                };

                // Serilaize Object using CamelCase Notations.
                var jsonObject = JsonConvert.SerializeObject(ConvertToAssetData(responseMachineModel.Response, responseVulnerabilityModel.Response), new JsonSerializerSettings
                {
                    //ContractResolver = contractResolver,
                    Formatting = Formatting.Indented,
                     
                });
                log.LogInformation($"C# View: {DateTime.Now}{jsonObject}"); // Print to screen

                // Post data
                var httpContent = new StringContent(jsonObject, System.Text.Encoding.UTF8, "application/json"); // Create HttpPost content as type of json and enable UTF-8 Content
                httpContent.Headers.Add("X-Company-Short", "open"); // Add Headers
                var respMessage = await httpClient.PostAsync(POST_URL, httpContent); // POST Request
                var postResp = await respMessage.Content.ReadAsStringAsync(); // Read content as string 
                log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}{postResp}"); // Print to screen
                new ObjectResult(postResp);
            }
            else if (responseMachineModel.ResponseMessage.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("Invalid bearer token for Defender");
            }
            else
                throw new Exception($"Somethings went wrong: {responseMachineModel.ResponseMessage.StatusCode}");
        }
 
        private object ConvertToAssetData(JsonResponseDataMachine responseMachines, JsonResponseDataVulnerabilityMachines responseVulnerabilities)
        {
            return _vulnerabilityService.TransformVulnaribiltyData(responseMachines, responseVulnerabilities);
        }
 
    }

}


//######################
//VulnerabilityService.cs
//#######################
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using Tartarus.Model.Vulnerability;
using Tartarus.Services.Data;
namespace Tartarus.Services
{
    public class VulnerabilityService : IVulnerabilityService
    {
        private readonly ILogger<IVulnerabilityService> _logger;
        private readonly IDatabaseService _dbservice;

        public VulnerabilityService(ILogger<IVulnerabilityService> log, IDatabaseService dbservice)
        {
            _logger = log;
            _dbservice = dbservice;
        }
        public List<VulernabilityRootData> TransformVulnaribiltyData(JsonResponseDataMachine responseMachines, JsonResponseDataVulnerabilityMachines responseVulnerabilities)
        {
            #region Transformation Work - Machines -> Vulnerability By Machine 
            List<VulernabilityRootData> rootData = new List<VulernabilityRootData>();

            List<TransformVulnerabilityMachineModel> vulnerabilities = new List<TransformVulnerabilityMachineModel>();
            for (int i = 0; i < responseVulnerabilities.Value.Length; i++)
            {
                for (int j = 0; j < responseMachines.Value.Length; j++)
                {
                    if (responseMachines.Value[j].Id == responseVulnerabilities.Value[i].MachineId)
                    {
                        var vulnerabilityData = responseVulnerabilities.Value[i];
                        var machineData = responseMachines.Value[j];
                        var transformData = new TransformVulnerabilityMachineModel()
                        {
                           // ID = vulnerabilityData.ID,
                            CVEID = vulnerabilityData.CVEID,
                            SourceId = vulnerabilityData.MachineId,
                            VenderReference = vulnerabilityData.FixingKbId,
                            ProductName = vulnerabilityData.ProductName,
                            VenderName = vulnerabilityData.ProductVendor,
                            ProductVersion = vulnerabilityData.ProductVersion,
                            Severity = vulnerabilityData.Severity,
                            AssetName = machineData.ComputerDnsName,
                            IPAddress = machineData.LastIpAddress
                        };
                        vulnerabilities.Add(transformData);
                    }
                }
            }
            rootData.Add(new VulernabilityRootData
            {
                data = new List<VulernabilityData> {
                new VulernabilityData {
                Attributes = new VulnerabilityAttributes
                {
                     VulnerabilityList = vulnerabilities
                },
                 Type = "MDE",
                 Id = "1",
                }
                }
            });
            return rootData;
            #endregion
            /*List<VulnerabilityData> vulnerabilities = new List<VulnerabilityData>();
            if (responseData != null)
            {
                foreach (var data in responseData.Value)
                {
                    var vulnerabiltyAttributes = new Vulnerability
                    {
                        AssetName = data.ComputerDnsName,
                        AssetType = "",
                        CompanyShortName = "",
                        Source = "MS Defender",
                        SourceId = data.Id,
                        OS = "",
                        VendorName = "",
                        VendorReference = "",
                        ProductName = "",
                        ProductVersion = "",
                        CVEID = "",
                        IPAddress = "",
                        FQDN = "",
                        Severity = ""
                    };
                    var vulnerabilityData = new VulnerabilityData
                    {
                        Type = "Vulnerabilities",
                        Attributes = vulnerabiltyAttributes
                    };
                    vulnerabilities.Add(vulnerabilityData);
                }
            }
            return vulnerabilities;*/
        }
        public string GetToken()
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


//#####################
//VulnerabilityData.cs
//#####################
using System.Text.Json.Serialization;
using System.Collections.Generic;
using Tartarus.Model.Root;
using System;
using Newtonsoft.Json;

namespace Tartarus.Model.Vulnerability
{
    public class VulnerabilityData
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("attributes")]
        public Vulnerability Attributes { get; set; }
    }

    public class Vulnerabilities
    {
        public List<Vulnerability> vulnerabilityList { get; set; }
    }

    public class Vulnerability
    {
        [JsonPropertyName("VID")]
        public int VID { get; set; }

        [JsonPropertyName("AssetName")]
        public string AssetName { get; set; }

        [JsonPropertyName("AssetType")]
        public string AssetType { get; set; }

        [JsonPropertyName("CompanyShortName")]
        public string CompanyShortName { get; set; }

        [JsonPropertyName("Source")]
        public string Source { get; set; }

        [JsonPropertyName("SourceId")]
        public string SourceId { get; set; }

        [JsonPropertyName("OS")]
        public string OS { get; set; }

        [JsonPropertyName("VendorName")]
        public string VendorName { get; set; }

        [JsonPropertyName("VenderReference")]
        public string VendorReference { get; set; }

        [JsonPropertyName("ProductName")]
        public string ProductName { get; set; }

        [JsonPropertyName("ProductVersion")]
        public string ProductVersion { get; set; }

        [JsonPropertyName("CVEID")]
        public string CVEID { get; set; }

        [JsonPropertyName("IPAddress")]
        public string IPAddress { get; set; }

        [JsonPropertyName("FQDN")]
        public string FQDN { get; set; }

        [JsonPropertyName("Severity")]
        public string Severity { get; set; }

    }
    #region Machine Date Model
    public class JsonResponseDataMachine
    {
        public Machine[]? Value { get; set; }
    }
    public class Machine
    {
        public string Id { get; set; }
        public string ComputerDnsName { get; set; }
        public string FirstSeen { get; set; }
        public string LastSeen { get; set; }
        public string OsPlatform { get; set; }
        public string LastIpAddress { get; set; }
        public string LastExternalIpAddress { get; set; }
        public int OsBuild { get; set; }
        public string HealthStatus { get; set; }
        public int RbacGroupId { get; set; }
        public string RbacGroupName { get; set; }
        public string RiskScore { get; set; }
        public string ExposureLevel { get; set; }
        public bool IsAadJoined { get; set; }
        public string AadDeviceId { get; set; }
        public string[] MachineTags { get; set; }
    }
    #endregion

    #region Vulneribility Machine Data
    public class JsonResponseDataVulnerabilityMachines
    {
        [JsonPropertyName("value")]
        public VulnerabilityMachine[]? Value { get; set; }
    }
    public class VulnerabilityMachine
    {
        [JsonPropertyName("id")]
        public string ID { get; set; }
        [JsonPropertyName("cveId")]
        public string CVEID { get; set; }
        [JsonPropertyName("machineId")]
        public string MachineId { get; set; }
        [JsonPropertyName("fixingKbId")]
        public string FixingKbId { get; set; }
        [JsonPropertyName("productName")]
        public string ProductName { get; set; }
        [JsonPropertyName("productVendor")]
        public string ProductVendor { get; set; }
        [JsonPropertyName("productVersion")]
        public string ProductVersion { get; set; }
        [JsonPropertyName("severity")]
        public string Severity { get; set; }
    }
    public class VulernabilityRootData
    {
        [JsonPropertyName("data")]
        public List<VulernabilityData> data { get; set; }
    }
    public class VulernabilityData
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("attributes")]
        public VulnerabilityAttributes Attributes { get; set; }
    }
    public class VulnerabilityAttributes
    {
    [JsonPropertyName("vulnerabilityList")]
     public List<TransformVulnerabilityMachineModel> VulnerabilityList { get; set; }
    }
    public class TransformVulnerabilityMachineModel
    {
       // [JsonPropertyName("id")]
       // public string ID { get; set; }
        [JsonPropertyName("CVEID")]
        public string CVEID { get; set; }
        [JsonPropertyName("SourceId")]
        public string SourceId { get; set; }
        [JsonPropertyName("VenderReference")]
        public string VenderReference { get; set; }
        [JsonPropertyName("ProductName")]
        public string ProductName { get; set; }
        [JsonPropertyName("VenderName")]
        public string VenderName { get; set; }
        [JsonPropertyName("ProductVersion")]
        public string ProductVersion { get; set; }
        [JsonPropertyName("Severity")]
        public string Severity { get; set; }
        [JsonPropertyName("AssetName")]
        public string AssetName { get; set; }
        [JsonPropertyName("IPAddress")]
        public string IPAddress { get; set; }
        [JsonPropertyName("Source")]
        public string Source { get; set; } = "MDE";
        [JsonPropertyName("AssetType")]
        public string AssetType { get; set; } = "Audit/Scan Servers";
        [JsonPropertyName("OS")]
        public string OS { get; set; } = "Microsoft";
        [JsonPropertyName("CompanyShortName")]
        public string CompanyShortName { get; set; } = "demo";
        [JsonPropertyName("FQDN")]
        public string FQDN { get; set; } = "domain.com";
    }
    #endregion
}


//########################
//ScoreData.cs
//#######################
using Newtonsoft.Json;

namespace tartarus.Model.Vulnerability
{
    public class ScoreData
    {
        [JsonProperty("time")]
        public string Time { get; set; }
        [JsonProperty("score")]
        public string Score { get; set; }
    }
}



//#######################
//local.services.json
//########################
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "DefaultEndpointsProtocol=https;AccountName=holdcsv;AccountKey=syVsYahQl2sfJQzisrOj0taO+vBpm4I4NWx3puxQUg7PvLhnkc37PQTeE5e2opqTeVKes2Tpkx7Z+AStnpa+3w==;EndpointSuffix=core.windows.net",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "ContainerName": "bucket",
    "CONNECTION_STRING": "Server=tcp:rodsbx01sql.database.windows.net,1433;Initial Catalog=CustomerAssetsDB;Persist Security Info=False;User ID=cadb_login_rw;Password=ur5ecret12#;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;",
    "tenantId": "584059b8-96b7-4f25-aff3-aad0264a1d51",
    "appId": "8e4fcfa6-90fc-4d2b-9036-a11e859abf16",
    "appSecret": "Iw~8Q~tYZRULT8VdVf6Q3NCNXcwtwkVIKWD0Cce."
  }
}


//######################
//host.json
//#####################
{
  "version": "2.0",
  "logging": {
    "applicationInsights": {
      "samplingSettings": {
        "isEnabled": true,
        "excludedTypes": "Request"
      }
    }
  }
}
