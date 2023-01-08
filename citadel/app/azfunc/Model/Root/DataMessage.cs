using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Collections;
using System.Dynamic;
using System;

namespace Citadel.Model.Root
{
    public class DataMessage<D, M>
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<D> data { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Meta<M> meta { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Links links { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public JsonApi<D> jsonApi { get; set; }

    }

    public class Meta<M>
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("result")]
        public M result { get; set; }

    }

    public class Links
    {
        public string self { get; set; }
        public string prev { get; set; }
        public string next { get; set; }
        public string first { get; set; }
        public string last { get; set; }
    }

    public class JsonApi<T>
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string version { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Meta<T> meta { get; set; }
    }

}