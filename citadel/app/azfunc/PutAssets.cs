using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Citadel.Model.Root;
using Citadel.Model.Asset;
using Citadel.Services;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Dynamic;
using System.Text.Json;

namespace Citadel
{
    public class CitadelPutAssets
    {

        // set up dependency injection
        // make sure the object is registered in the startup
        private readonly ILogger<CitadelPutAssets> _logger;
        private readonly IAssetService _asset;

        public CitadelPutAssets(ILogger<CitadelPutAssets> log, IAssetService asset)
        {
            _logger = log;
            _asset = asset;
        }

        [FunctionName(Constant.PUT_ASSET)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = Constant.ASSETS_PUT_ROUTE)] HttpRequest req, ILogger _logger, string id)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            try
            {
                string companyShortName = req.Headers["X-Company-Short"];

                string token = req.Headers["Authorization"];
                if (token == null)
                    throw new UnauthorizedAccessException("Unauthorized access");
                else
                    await Task.Run(() => Common.ValidateBearertokenAsync(token));

                if (companyShortName == null || companyShortName != id)
                    throw new BadRequestException("Bad Request - Missing id (company short)");

                string requestBody = String.Empty;
                using (StreamReader streamReader = new StreamReader(req.Body))
                {
                    requestBody = await streamReader.ReadToEndAsync();
                }
                DataMessage<AssetData, List<ExpandoObject>> dataMessage = System.Text.Json.JsonSerializer.Deserialize<DataMessage<AssetData, List<ExpandoObject>>>(requestBody);
                var results = await Task.Run(() => _asset.PutCustomerAssets(dataMessage.data, companyShortName));

                // Build Response
                dataMessage.data = null;
                dataMessage.links = null;
                dataMessage.jsonApi = null;

                dataMessage.meta = new Meta<List<ExpandoObject>>();
                dataMessage.meta.result = new List<ExpandoObject>();
                dataMessage.meta.result = results;

                var response = System.Text.Json.JsonSerializer.Serialize(dataMessage, new JsonSerializerOptions { WriteIndented = true });

                bool _202Accepted = false;
                foreach (var result in results)
                {
                    foreach (var pair in result)
                    {
                        if (pair.Key == "addedAssetCount" && (int)pair.Value > 0)
                            _202Accepted = true;
                        if (pair.Key == "updatedAssetCount" && (int)pair.Value > 0)
                            _202Accepted = true;
                    }
                }

                if (_202Accepted)
                {
                    return new OkObjectResult(response)
                    {
                        StatusCode = StatusCodes.Status202Accepted
                    };
                }
                else
                {
                    dataMessage.meta.result = Common.GetNothingToDoResponse();
                    return new OkObjectResult(System.Text.Json.JsonSerializer.Serialize(dataMessage, new JsonSerializerOptions { WriteIndented = true }))
                    {
                        StatusCode = StatusCodes.Status200OK
                    };
                }
            }
            catch (System.Text.Json.JsonException e)
            {
                _logger.LogError(e.Message);
                return Common.ReturnErrorResponse("Invalid JSON request", "400");
            }
            catch (BadRequestException e)
            {
                _logger.LogError(e.Message);
                return Common.ReturnErrorResponse(e.Message, "400");
            }
            catch (InvalidTokenException e)
            {
                _logger.LogError(e.Message);
                return Common.ReturnErrorResponse(e.Message, "401");
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
    }

}
