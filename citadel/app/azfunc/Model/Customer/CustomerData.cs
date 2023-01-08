using System.Text.Json.Serialization;
using Citadel.Model.Root;

namespace Citadel.Model.Customer
{
    public class CustomerData
    {
        public string type { get; set; }
        public string id { get; set; }
        public Customer attributes { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Links links { get; set; }
    }

    public class Customer
    {
        [JsonPropertyName("company_short")]
        public string CompanyShort { get; set; }

        [JsonPropertyName("customer_name")]
        public string CustomerName { get; set; }
    }
}