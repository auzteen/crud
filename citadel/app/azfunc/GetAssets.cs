using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Text.Json;
using Citadel.Model.Asset;
using Citadel.Model.Root;
using Citadel.Services;

namespace Citadel
{
    public class CitadelGetAssets
    {

        // set up dependency injection
        // make sure the object is registered in the startup
        private readonly ILogger<CitadelGetAssets> _logger;
        private readonly IAssetService _asset;

        public CitadelGetAssets(ILogger<CitadelGetAssets> log, IAssetService asset)
        {
            _logger = log;
            _asset = asset;
        }

        [FunctionName(Constant.GET_ASSET)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = Constant.ASSETS_ROUTE)] HttpRequest req,
            ILogger _logger, string id)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            //req.HttpContext.Response.Headers.Add("Content-Type", "application/vnd.api+json");
            string response;
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

                string pageLimit = req.Query["page[limit]"];
                string pageOffset = req.Query["page[offset]"];
                string url = req.Path;

                pageLimit = pageLimit == null ? Constant.DEFAULT_ASSET_PAGE_SIZE : pageLimit;
                //pageOffset = pageOffset == null ? Constant.DEFAULT_ASSET_PAGE_OFFSET : pageOffset;
                if ((pageOffset == null) || (Int32.Parse(pageOffset) < Int32.Parse(pageLimit)))
                    pageOffset = Constant.DEFAULT_ASSET_PAGE_OFFSET;

                List<AssetData> data = await Task.Run(() => _asset.GetCustomerAssets(companyShortName, pageLimit, pageOffset));
                Dictionary<string, int> counts = new Dictionary<string, int>();
                DataMessage<AssetData, Dictionary<string, int>> dataMessage = new DataMessage<AssetData, Dictionary<string, int>>();
                dataMessage.data = data;

                AssetData lastData = data[data.Count - 1];
                Links links = BuildAssetLinks(req, Int32.Parse(pageLimit), Int32.Parse(pageOffset), data[0].PageCount, data[0].TotalRecords);
                Citadel.Model.Root.Meta<Dictionary<string, int>> meta = new Citadel.Model.Root.Meta<Dictionary<string, int>>();
                counts["Page Count"] = data[0].PageCount;
                counts["Total Records"] = data[0].TotalRecords;

                meta.result = new Dictionary<string, int>();
                meta.result = counts;
                dataMessage.meta = meta;
                dataMessage.links = links;

                response = JsonSerializer.Serialize(dataMessage, new JsonSerializerOptions { WriteIndented = true });
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
            catch (RangePaginationException e)
            {
                _logger.LogError(e.Message);
                return Common.ReturnErrorResponse(e.Message, "400");
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

        private Links BuildAssetLinks(HttpRequest req, int pageLimit, int pageOffset, int pageCount, int totalRecords)
        {
            string url = req.Path;
            string displayUrl = req.GetDisplayUrl();
            string[] urlArray = displayUrl.Split(Constant.VERSION_PATH);

            Links links = new Links();
            links.self = $"/{Constant.VERSION_PATH}{urlArray[1]}";
            links.first = $"{url}?page[limit]={pageLimit}&page[offset]=0";
            links.prev = pageOffset < pageLimit ? null : $"{url}?page[limit]={pageLimit}&page[offset]={pageOffset - pageLimit}";
            links.next = (pageOffset + pageLimit) >= totalRecords ? null : $"{url}?page[limit]={pageLimit}&page[offset]={pageOffset + pageLimit}";
            links.last = $"{url}?page[limit]={pageLimit}&page[offset]={(pageCount * pageLimit) - pageLimit}";
            return links;
        }

    }

}
