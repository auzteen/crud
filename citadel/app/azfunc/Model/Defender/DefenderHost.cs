using System.Text.Json.Serialization;
using System.Collections.Generic;
using Citadel.Model.Root;
using Citadel.Model.Asset;
using System;
using Newtonsoft.Json;

namespace Citadel.Model.Defender
{
    #region POST Request Data for Assets API
    public class RootData
    {
        [JsonProperty("data")]
        public DefenderData[] DefenderData { get; set; }
    }
    public class DefenderData
    {
        [JsonProperty("type")]
        public string AssetType { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("attributes")]
        public DefenderAttributes AssetAttributes { get; set; }
    }
    public class DefenderAttributes
    {
        [JsonProperty("asset_type")]
        public string AssetType { get; set; }
        [JsonProperty("assetList")]
        public DefenderAsset[] Assets { get; set; }
    }
    public class DefenderAsset
    {
        [JsonProperty("asset_name")]
        public string AssetName { get; set; }
        [JsonProperty("asset_tier")]
        public string AssetTier { get; set; }
        [JsonProperty("domain")]
        public string Domain { get; set; }
        [JsonProperty("ip_address")]
        public string IpAddress { get; set; }
        [JsonProperty("ip_location")]
        public string IpLocation { get; set; }
        [JsonProperty("manufacturer")]
        public string Manufacturer { get; set; }
        [JsonProperty("os")]
        public string OS { get; set; }
        [JsonProperty("comment")]
        public string Comment { get; set; }
        [JsonProperty("asset_criticality")]
        public DefenderCriticality AssetCriticality { get; set; }
    }
    public class DefenderCriticality
    {
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("score")]
        public int Score { get; set; }

    }
    #endregion
    public class JsonResponseData
    {
#nullable enable
        public Machine[]? Value { get; set; }
#nullable disable
    }
    public class Machine
    {
        public string id { get; set; }
        public string computerDnsName { get; set; }
        public string lastIpAddress { get; set; }
        public string LastExternalIpAddress { get; set; }
        public string riskScore { get; set; }
        public string exposureLevel { get; set; }
        public string osPlatform { get; set; }
    }

}