using System;
using System.IO;
using Tartarus.Model.Root;
using Tartarus.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Tartarus
{
    public class PostVulnerabilities
    {
        private readonly ILogger<PostVulnerabilities> _logger;
        private readonly IVulnerabilityService _vulnerability;
        public PostVulnerabilities(ILogger<PostVulnerabilities> log, IVulnerabilityService vulnerability)
        {
            _logger = log;
            _vulnerability = vulnerability;
        }

        [FunctionName(Constant.POST_ASSET_VULNERABILITY)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "patch", Route = Constant.ASSETS_VOLUNERABILITY_ROUTE)] HttpRequest req,
            ILogger log, string id)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string companyShortName = req.Headers["X-Company-Short"];

            string token = req.Headers["Authorization"];
            if (token == null)
                throw new UnauthorizedAccessException("Unauthorized access");
            else
                await Task.Run(() => Common.ValidateBearertokenAsync(token));

            if (companyShortName == null || companyShortName != id)
                throw new BadRequestException("Bad Request - Missing id (company short)");

            try
            {
                switch(req.Method)
                {
                    case "GET":
                        break;
                    case "POST":
                        break;
                    case "PATCH":
                        break;
                    default:
                        break;
                }
            }
            catch(BadRequestException e)
            {
                _logger.LogError(e.Message);
                return Common.ReturnErrorResponse(e.Message, "400");
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return Common.ReturnErrorResponse(e.Message, "500");
            }

            return new OkObjectResult("We done do good.");
        }
    }
}
