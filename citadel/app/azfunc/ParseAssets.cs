using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Citadel.Services;
using System.Collections.Generic;
using System.Text.Json;
using System;
using Citadel.Model.Root;
using Citadel.Model.Asset;
using Newtonsoft.Json.Linq;
using System.Dynamic;

namespace Citadel
{
    public class CitadelParseAssets
    {
        private readonly ILogger<CitadelParseAssets> _logger;

        private readonly IAssetParseService _csvParser;

        private readonly IAssetService _asset;

        private readonly IAssetParseService _csv;

        public CitadelParseAssets(ILogger<CitadelParseAssets> log, IAssetParseService csvParser, IAssetService asset, IAssetParseService csv)
        {
            _logger = log;
            _csvParser = csvParser;
            _asset = asset;
            _csv = csv;
        }

        [FunctionName(Constant.PARSE_ASSET)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = Constant.ASSETS_PARSE_ROUTE)] HttpRequest req, ILogger _logger, string id)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            string companyShortName = req.Headers["X-Company-Short"];

            string token = req.Headers["Authorization"];
            if (token == null)
                throw new UnauthorizedAccessException("Unauthorized access");
            else
                await Task.Run(() => Common.ValidateBearertokenAsync(token));

            if (companyShortName == null || companyShortName != id)
                throw new BadRequestException("Bad Request - Missing company short");

            try
            {
                // IFormat File
                var formdata = await req.ReadFormAsync();
                var file = req.Form.Files["file"];

                // Data
                Dictionary<string, List<Asset>> assetDictionary = new Dictionary<string, List<Asset>>();

                // CSV Records
                var records = await Task.Run(() => _csvParser.Parse(file, companyShortName));
                foreach (var record in records)
                {
                    string recordsSerialized = JsonConvert.SerializeObject(record);
                    Assets assetsRecord = JsonConvert.DeserializeObject<Assets>(recordsSerialized);
                    Asset assetRecord = JsonConvert.DeserializeObject<Asset>(recordsSerialized);

                    // Validate Csv Company Short Name
                    if (!_csvParser.ValidCompanyShortName(record, companyShortName))
                        throw new BadRequestException("Company short name in csv file must match both the url id and the header: X-Company-Short");

                    // Store Asset Data
                    if (!assetDictionary.ContainsKey(assetsRecord.assetType))
                    {
                        var list = new List<Asset>();
                        list.Add(assetRecord);
                        assetDictionary.Add(assetsRecord.assetType, list);
                    }
                    else
                    {
                        var list = assetDictionary[assetsRecord.assetType];
                        list.Add(assetRecord);
                        assetDictionary[assetsRecord.assetType] = list;
                    }
                }

                // The Asset Type
                AssetData assetData = new AssetData();

                // The Message
                DataMessage<AssetData, List<ExpandoObject>> dataMessage = new DataMessage<AssetData, List<ExpandoObject>>();
                dataMessage.data = new List<AssetData>();

                foreach (var type in assetDictionary)
                {
                    // Asset Data
                    assetData.type = Constant.TYPE_ASSETS;
                    assetData.id = Guid.NewGuid().ToString();
                    assetData.attributes = new Assets();
                    assetData.attributes.assetType = type.Key;
                    assetData.attributes.assetList = new List<Asset>();

                    // Add Assets to Asset Data
                    foreach (var asset in assetDictionary[type.Key])
                    {
                        var sJson = JsonConvert.SerializeObject(asset);
                        JObject keyValuePairs = JObject.Parse(sJson);
                        var a = JsonConvert.DeserializeObject<Asset>(keyValuePairs.ToString());
                        assetData.attributes.assetList.Add(a);

                    }
                    // Add Asset Data to Message Data
                    dataMessage.data.Add(assetData);

                    // Reset Asset Data
                    assetData = new AssetData();
                }

                // Process Message Data
                dynamic results = await Task.Run(() => _asset.PutCustomerAssets(dataMessage.data, companyShortName));

                // Build Response
                dataMessage.data = null;
                dataMessage.links = null;
                dataMessage.jsonApi = null;

                dataMessage.meta = new Meta<List<ExpandoObject>>();
                dataMessage.meta.result = results;

                dynamic response = System.Text.Json.JsonSerializer.Serialize(dataMessage, new JsonSerializerOptions { WriteIndented = true });

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
                    return new OkObjectResult(JsonConvert.SerializeObject(Common.GetNothingToDoResponse(), Formatting.Indented))
                    {
                        StatusCode = StatusCodes.Status200OK
                    };
                }

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
    }
}
