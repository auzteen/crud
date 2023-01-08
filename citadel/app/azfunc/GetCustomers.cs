using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Text.Json;
using Citadel.Model;
using Citadel.Model.Root;
using Citadel.Model.Customer;
using Citadel.Services;


namespace Citadel
{
    public class CitadelGetCustomers
    {

        // set up dependency injection
        // make sure the object is registered in the startup
        private readonly ILogger<CitadelGetCustomers> _logger;
        private readonly ICustomerService _customer;

        public CitadelGetCustomers(ILogger<CitadelGetCustomers> log, ICustomerService customer)
        {
            _logger = log;
            _customer = customer;
        }

        [FunctionName(Constant.GET_CUSTOMER)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = Constant.CUSTOMERS_ROUTE)] HttpRequest req,
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

                Dictionary<string, string> count = new Dictionary<string, string>();

                DataMessage<CustomerData, Dictionary<string, string>> dataMessage = new DataMessage<CustomerData, Dictionary<string, string>>();
                if (string.IsNullOrEmpty(companyShortName))
                    dataMessage.data = await Task.Run(() => _customer.GetAllCustomers());
                else
                    dataMessage.data = await Task.Run(() => _customer.GetCustomer(companyShortName));

                dataMessage.links = new Citadel.Model.Root.Links();
                dataMessage.meta = new Citadel.Model.Root.Meta<Dictionary<string, string>>();

                string response = JsonSerializer.Serialize(dataMessage, new JsonSerializerOptions { WriteIndented = true });
                return new OkObjectResult(response);
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
