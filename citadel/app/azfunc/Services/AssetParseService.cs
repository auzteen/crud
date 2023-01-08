using System;
using Citadel.Model;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.IO;
using Newtonsoft.Json;
using Citadel.Model.Csv;
using CsvHelper;
using System.Globalization;
using CsvHelper.Configuration;

namespace Citadel.Services
{
    public class AssetParseService : IAssetParseService
    {
        private readonly ILogger<IAssetService> _logger;

        public AssetParseService(ILogger<IAssetService> log)
        {
            _logger = log;
        }

        public IEnumerable<CsvData> Parse(IFormFile file, string companyShortName)
        {
            /*
            * Unfortunately there is no way to map these to the Asset Model
            * automatically.  Non of the tools out there handle our case :(
            */

            try
            {
                var reader = new StreamReader(file.OpenReadStream());
                var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                csv.Context.RegisterClassMap<CsvAssetMap>();
                var records = csv.GetRecords<CsvData>();
                return csv.GetRecords<CsvData>();
            }
            catch (JsonException e)
            {
                _logger.LogError(e.Message);
                throw;
            }
            catch (BadRequestException e)
            {
                _logger.LogError(e.Message);
                throw;
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogError(e.Message);
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw;
            }

        }

        public sealed class CsvAssetMap : ClassMap<CsvData>
        {
            public CsvAssetMap()
            {
                AutoMap(CultureInfo.InvariantCulture);

            }
        }
        public bool ValidCompanyShortName(CsvData record, string companyShortName)
        {
            var recorsSerialize = JsonConvert.SerializeObject(record);
            if (recorsSerialize.Contains(companyShortName))
                return true;
            else
                return false;
        }
    }
}