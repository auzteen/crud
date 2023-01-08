using System.Text.Json.Serialization;
using System.Collections.Generic;
using Citadel.Model.Root;
using Citadel.Model.Asset;
using System;

namespace Citadel.Model.Csv
{
    public class CsvData
    {

        public string AssetType { get; set; }
        public string AssetTier { get; set; }
        public string AssetName { get; set; }
        public string Domain { get; set; }
        public string IPAddress { get; set; }
        public string IPLocation { get; set; }
        public string Manufacturer { get; set; }
        public string OS { get; set; }
        public string Comment { get; set; }
        public string CompanyShortName { get; set; }
    }

    public class CsvAsset
    {

        [JsonPropertyName("asset_type")]
        public string AssetType { get; set; }

        public Asset.Asset asset { get; set; }

    }
}