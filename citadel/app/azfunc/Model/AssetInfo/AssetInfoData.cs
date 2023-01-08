using System;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using Citadel.Model.Root;

namespace Citadel.Model.AssetInfo
{
    public class AssetInfoData
    {
        public string type { get; set; }
        public string id { get; set; }

        public AssetInfoCategory attributes { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Links links { get; set; }

    }

    public class AssetInfoCategory
    {
        [JsonPropertyName("category_name")]
        public string CategoryName { get; set; }

        [JsonPropertyName("category_info")]
        public List<Information> informationList { get; set; }

    }

    public class Information
    {
        [JsonPropertyName("request_info")]
        public string RequestInfoQuestion { get; set; }

        [JsonPropertyName("request_info_answer")]
        public string RequestInfoAnswer { get; set; }

        [JsonPropertyName("additional_info")]
        public string AdditionalInfo { get; set; }

        [JsonPropertyName("additional_info_example")]
        public string AdditionalInfoExample { get; set; }
    }
}
