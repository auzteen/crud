using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using tartarus.Model.Vulnerability;
using tartarus.Services.Interface;
using Tartarus.Services;

namespace tartarus.Services
{
    public class ResponseService<T> : IResponseService<T> where T : class
    {
        private static HttpClient httpClient = new HttpClient();
        public ResponseData<T> GetResponseData(string URL, IVulnerabilityService _vulnerabilityService)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, URL);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _vulnerabilityService.GetToken());
            var response = httpClient.SendAsync(request).GetAwaiter().GetResult();
            var result = response.Content.ReadAsStringAsync();
            var responseData = JsonConvert.DeserializeObject<T>(result.Result);
            var returnResponse = new ResponseData<T>()
            {
                Response = responseData,
                ResponseMessage = response
            };
            return returnResponse;
        }
    }
}
