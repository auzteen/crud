using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Citadel.Services;
using Newtonsoft.Json.Linq;
using Citadel.Model.Asset;
using System.Text.Json;
using Newtonsoft.Json;
using Citadel.Model.Root;
using System.Dynamic;

namespace Citadel
{
    public class CitadelUpdateAssets
    {

        // set up dependency injection
        // make sure the object is registered in the startup
        private readonly ILogger<CitadelUpdateAssets> _logger;
        private readonly IAssetService _asset;

        public CitadelUpdateAssets(ILogger<CitadelUpdateAssets> log, IAssetService asset)
        {
            _logger = log;
            _asset = asset;
        }

        [FunctionName(Constant.UPDATE_ASSET)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = Constant.ASSETS_UPDATE_ROUTE)] HttpRequest req, ILogger _logger, string id, string asset_id)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            try
            {
                string companyShortName = id;

                string token = req.Headers["Authorization"];
                if (token == null)
                    throw new UnauthorizedAccessException("Unauthorized access");
                else
                    await Task.Run(() => Common.ValidateBearertokenAsync(token));

                if (asset_id == null)
                    throw new BadRequestException("Bad Request - Missing asset id");

                if (companyShortName == null || companyShortName != id)
                    throw new BadRequestException("Bad Request - Missing company short");

                // Parse assetId
                if (!int.TryParse(asset_id, out int assetId))
                    throw new BadRequestException("Bad Request - Asset id must be convertable to int");

                string requestBody = String.Empty;
                using (StreamReader streamReader = new StreamReader(req.Body))
                {
                    requestBody = await streamReader.ReadToEndAsync();
                }

                // Tokenize, Deserialize
                JObject bodyJson = JObject.Parse(requestBody);
                var assetType = bodyJson.SelectToken("data[0].attributes.asset_type").ToString();
                Asset assetToUpdate = System.Text.Json.JsonSerializer.Deserialize<Asset>(bodyJson.SelectToken("data[0].attributes.assetList[0]").ToString());

                // Response
                DataMessage<AssetData, List<ExpandoObject>> dataMessage = new DataMessage<AssetData, List<ExpandoObject>>();
                dataMessage.meta = new Meta<List<ExpandoObject>>();

                if (_asset.NeedToUpdate(asset_id, assetToUpdate, assetType, out dynamic fields))
                {
                    // Update
                    if (await Task.Run(() => _asset.UpdateAsset(assetToUpdate, companyShortName, assetType, assetId)))
                    {
                        var fieldList = new List<ExpandoObject>();
                        fieldList.Add(fields);
                        dataMessage.meta.result = fieldList;
                        return new OkObjectResult(System.Text.Json.JsonSerializer.Serialize(dataMessage, new JsonSerializerOptions { WriteIndented = true }))
                        {
                            StatusCode = StatusCodes.Status202Accepted
                        };
                    }
                    else
                    {
                        throw new AssetUpdateException("Asset could not be update. Please try again later.");
                    }
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
            catch (AssetUpdateException e)
            {
                _logger.LogError(e.Message);
                return Common.ReturnErrorResponse(e.Message, "500");
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.Message);
                return Common.ReturnErrorResponse(e.Message, "400");
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
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return Common.ReturnErrorResponse(e.Message, "500");
            }

        }
    }
}
