using tartarus.Model.Vulnerability;
using Tartarus.Services;

namespace tartarus.Services.Interface
{
    public interface IResponseService<T> where T : class
    {
        public ResponseData<T> GetResponseData(string URL, IVulnerabilityService _vulnerabilityService);
    }
}
