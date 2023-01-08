using System.Text.Json.Serialization;
using System.Collections.Generic;
using Citadel.Model.Root;
using System;

namespace Citadel.Model.Asset
{
    public class AssetData
    {
        public string type { get; set; }
        public string id { get; set; }
        public Assets attributes { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Links links { get; set; }

        [JsonIgnore]
        public int PageCount { get; set; }

        [JsonIgnore]
        public int TotalRecords { get; set; }
    }

    public class Assets
    {
        [JsonPropertyName("asset_type")]
        public string assetType { get; set; }

        public List<Asset> assetList { get; set; }
    }

    public class Asset
    {
        [JsonPropertyName("asset_id")]
        public int AssetId { get; set; }

        [JsonPropertyName("asset_name")]
        public string AssetName { get; set; }

        [JsonPropertyName("asset_tier")]
        public string AssetTier { get; set; }

        [JsonPropertyName("domain")]
        public string Domain { get; set; }

        [JsonPropertyName("ip_address")]
        public string IPAddress { get; set; }

        [JsonPropertyName("ip_location")]
        public string IPLocation { get; set; }

        [JsonPropertyName("manufacturer")]
        public string Manufacturer { get; set; }

        [JsonPropertyName("os")]
        public string OS { get; set; }

        [JsonPropertyName("comment")]
        public string Comment { get; set; }

        [JsonPropertyName("asset_criticality")]
        public AssetCriticality assetCriticality { get; set; }

        [JsonPropertyName("source_id")]
        public string SourceId { get; set; }

        public static readonly Dictionary<string, string> nameMap = new Dictionary<string, string>
        {
            {"asset_name", "AssetName"},
            {"asset_tier", "AssetTier"},
            {"domain", "Domain"},
            {"ip_address", "IPAddress"},
            {"ip_location", "IPLocation"},
            {"manufacturer", "Manufacturer"},
            {"os", "OS"},
            {"comment", "Comment"},
            {"asset_criticality", "assetCriticality"},
            {"source_id", "SourceId"}
        };
    }

    public class AssetCriticality
    {
        public string status { get; set; }
        public int? score { get; set; }
    }
}