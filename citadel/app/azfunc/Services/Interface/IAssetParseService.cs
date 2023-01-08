using Citadel.Model;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Http;
using Citadel.Model.Root;
using Citadel.Model.Csv;

namespace Citadel.Services
{
    public interface IAssetParseService
    {
        public IEnumerable<CsvData> Parse(IFormFile file, string companyShortName);
        public bool ValidCompanyShortName(CsvData record, string companyShortName);
    }
}