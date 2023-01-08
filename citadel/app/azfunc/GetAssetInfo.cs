using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Citadel.Model.Root;
using Citadel.Model.AssetInfo;
using Citadel.Services;
using System.Collections.Generic;

namespace Citadel
{
    public class CitadelGetAssetInfo
    {

        // set up dependency injection
        // make sure the object is registered in the startup
        private readonly ILogger<CitadelGetAssetInfo> _logger;
        private readonly IAssetInfoService _assetInfoService;

        public CitadelGetAssetInfo(ILogger<CitadelGetAssetInfo> log, IAssetInfoService assetInfoService)
        {
            _logger = log;
            _assetInfoService = assetInfoService;
        }

        [FunctionName(Constant.GET_ASSET_INFO)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = Constant.ASSETS_INFO_ROUTE)] HttpRequest req,
            ILogger _logger, string id)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            //req.HttpContext.Response.Headers.Add("Content-Type", "application/vnd.api+json");
            try
            {
                string token = req.Headers["Authorization"];
                if (token == null)
                    throw new UnauthorizedAccessException("Unauthorized access");
                else
                    await Task.Run(() => Common.ValidateBearertokenAsync(token));

                string companyShortName = id;
                if (companyShortName == null)
                    throw new BadRequestException("Bad Request - company short is missing");

                DataMessage<AssetInfoData, Dictionary<string, int>> dataMessage = new DataMessage<AssetInfoData, Dictionary<string, int>>();
                dataMessage.data = await Task.Run(() => _assetInfoService.GetAssetInformation(companyShortName));
                dataMessage.links = new Citadel.Model.Root.Links();
                dataMessage.meta = new Citadel.Model.Root.Meta<Dictionary<string, int>>();

                string response = JsonSerializer.Serialize(dataMessage, new JsonSerializerOptions { WriteIndented = true });
                return new OkObjectResult(response);
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
            catch (NoRecordsFoundException e)
            {
                _logger.LogError(e.Message);
                return Common.ReturnErrorResponse(e.Message, "404");
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return Common.ReturnErrorResponse(e.Message, "500");
            }

        }
    }
}
