using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System.Collections.Generic;
using System.Text.Json;
using Citadel.Model;
using Citadel.Model.AssetInfo;
using Citadel.Model.Root;
using Citadel.Services;

namespace Citadel
{
    public class CitadelPostAssetInfo
    {

        // set up dependency injection
        // make sure the object is registered in the startup
        private readonly ILogger<CitadelPostAssetInfo> _logger;
        private readonly IAssetInfoService _assetInfo;

        public CitadelPostAssetInfo(ILogger<CitadelPostAssetInfo> log, IAssetInfoService assetInfo)
        {
            _logger = log;
            _assetInfo = assetInfo;
        }

        [FunctionName(Constant.POST_ASSET_INFO)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = Constant.ASSETS_INFO_POST_ROUTE)] HttpRequest req, ILogger _logger, string id)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            //req.HttpContext.Response.Headers.Add("Content-Type", "application/vnd.api+json");
            try
            {
                string companyShortName = req.Headers["X-Company-Short"];

                string token = req.Headers["Authorization"];
                if (token == null)
                    throw new UnauthorizedAccessException("Unauthorized access");
                else
                    await Task.Run(() => Common.ValidateBearertokenAsync(token));

                if (companyShortName == null || id == null)
                    throw new BadRequestException("Bad Request - Missing company short");

                if (companyShortName != id)
                    throw new BadRequestException("Bad Request - Invalid company short");

                string requestBody = String.Empty;
                using (StreamReader streamReader = new StreamReader(req.Body))
                {
                    requestBody = await streamReader.ReadToEndAsync();
                }
                DataMessage<AssetInfoData, Dictionary<string, int>> dataMessage = JsonSerializer.Deserialize<DataMessage<AssetInfoData, Dictionary<string, int>>>(requestBody);
                await Task.Run(() => _assetInfo.UpdateAssetInformation(dataMessage.data, companyShortName));
                return new OkObjectResult(requestBody)
                {
                    StatusCode = StatusCodes.Status201Created
                };
            }
            catch (UnauthorizedAccessException e)
            {
                _logger.LogError(e.Message);
                return Common.ReturnErrorResponse(e.Message, "401");
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
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return Common.ReturnErrorResponse(e.Message, "500");
            }

        }

    }

}
