using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using Citadel.Model.Qualys;
using Citadel.Model.Asset;
using Citadel.Model.Root;
using Citadel.Services;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Dynamic;

namespace Citadel
{
    public class CitadelAssetDiscovery
    {
        // set up dependency injection
        // make sure the object is registered in the startup
        private readonly ILogger<CitadelAssetDiscovery> _logger;
        private readonly IAssetService _asset;

        public CitadelAssetDiscovery(ILogger<CitadelAssetDiscovery> log, IAssetService asset)
        {
            _logger = log;
            _asset = asset;
        }

        // function runs daily at 1:30 am
        [FunctionName("TriggerQualysAssetDiscovery")]
        public void Run([TimerTrigger("0 30 1 * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        }

        [FunctionName("GetQualysAssets")]
        public async Task<IActionResult> GetQualysAssets(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = Constant.GET_QUALYS_HOSTS_ROUTE)] HttpRequest req,
            ILogger _logger, string id)
        {
            try
            {
                _logger.LogInformation("C# HTTP trigger function processed a request.");

                string token = req.Headers["Authorization"];
                if (token == null)
                    throw new UnauthorizedAccessException("Unauthorized access");
                else
                    await Task.Run(() => Common.ValidateBearertokenAsync(token));

                QualysData data = await GetQualysData("https://qualysapi.qualys.eu:443/api/2.0/fo/asset/host/", "?action=list", "application/xml");

                List<AssetData> assetData = await Task.Run(() => _asset.MapQualysToAssetData(data));
                DataMessage<AssetData, List<ExpandoObject>> dataMessage = new DataMessage<AssetData, List<ExpandoObject>>();
                dataMessage.data = assetData;
                dataMessage.meta = new Meta<List<ExpandoObject>>();
                dataMessage.links = new Links();

                // add qualys data
                string companyShortName = id;

                //_asset.AddCustomerAssets(assetData, companyShortName);
                var result = _asset.PutCustomerAssets(assetData, companyShortName);

                string response = JsonSerializer.Serialize(dataMessage, new JsonSerializerOptions { WriteIndented = true });
                /*
                switch (result.StatusCode)
                {
                    case 201:
                        return new ObjectResult(result.Value)
                        {
                            StatusCode = StatusCodes.Status201Created
                        };
                    default:
                        return new ObjectResult(result.Value)
                        {
                            StatusCode = StatusCodes.Status200OK
                        };
                }
                */

                return new OkObjectResult("This needs to be addressed") { StatusCode = StatusCodes.Status200OK };

            }
            catch (JsonException e)
            {
                _logger.LogError(e.Message);
                return Common.ReturnErrorResponse("Invalid JSON request", "400");
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
            catch (ExistingAssetsException e)
            {
                _logger.LogInformation(e.Message);
                return Common.ReturnErrorResponse(e.Message, "409");
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return Common.ReturnErrorResponse(e.Message, "500");
            }
        }

        private async Task<QualysData> GetQualysData(string path, string parameters, string contentType)
        {
            try
            {
                // initiate the request from the client
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(path);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(contentType));

                // generate basic authorization token
                string username = Environment.GetEnvironmentVariable("QualysUsername", EnvironmentVariableTarget.Process);
                string password = Environment.GetEnvironmentVariable("QualysPassword", EnvironmentVariableTarget.Process);

                var byteArray = Encoding.ASCII.GetBytes($"{username}:{password}");
                string basicAuthToken = $"Basic {Convert.ToBase64String(byteArray)}";
                client.DefaultRequestHeaders.Add("Authorization", basicAuthToken);
                client.DefaultRequestHeaders.Add("X-Requested-With", "function");

                HttpResponseMessage response = await client.GetAsync(parameters);
                if (response.IsSuccessStatusCode)
                {
                    Stream xmlStream = await response.Content.ReadAsStreamAsync();
                    string xml = await response.Content.ReadAsStringAsync();
                    // deserialize
                    XmlReaderSettings settings = new XmlReaderSettings();
                    settings.DtdProcessing = DtdProcessing.Parse;
                    settings.MaxCharactersFromEntities = 1024;
                    XmlReader xmlReader = XmlReader.Create(xmlStream, settings);
                    XmlSerializer reader = new XmlSerializer(typeof(QualysData), new XmlRootAttribute("HOST_LIST_OUTPUT"));
                    QualysData data = (QualysData)reader.Deserialize(xmlReader);
                    return data;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedAccessException("Invalid credentials for Qualys");
                }
                else
                    throw new Exception($"Somethings went wrong: {response.StatusCode}");
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw;
            }
        }
    }
}