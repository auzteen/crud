
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
        private static string POST_URL = "https://dummyjson.com/products/add";
        private readonly IVulnerabilityService _vulnerabilityService;
        public DeviceScore(IVulnerabilityService vulnerabilityService)
        {
            _vulnerabilityService=vulnerabilityService;
        }
        [FunctionName("DeviceScore")]
        public async Task<OkObjectResult> Run(
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

                if (responseData != null && responseData.Score > 0)
                {
                    var jsonObject = JsonConvert.SerializeObject(responseData, new JsonSerializerSettings
                    {
                        Formatting = Formatting.Indented
                    });

                    var jwt_token = "xxxxxxxxxxxxx";
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt_token);
                    var httpContent = new StringContent(jsonObject, System.Text.Encoding.UTF8, "application/json");
                    httpContent.Headers.Add("X-Company-Short", "open");
                    var respMessage = await httpClient.PostAsync(POST_URL, httpContent);
                    var postResp = await respMessage.Content.ReadAsStringAsync();
                    logger.LogInformation($"Post function executed at: {DateTime.Now}{postResp}");
                    return new OkObjectResult(postResp);
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

