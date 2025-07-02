using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Ship.Ses.Extractor.Domain.Models.Extractor
{
    //public class TableMapping
    //{
    //    public string ResourceType { get; set; } = default!;
    //    public string TableName { get; set; } = default!;
    //    public Dictionary<string, string> FieldMappings { get; set; } = new(); // e.g. EMR_column -> FHIR_property
    //}
    public class TableMapping
    {
        [JsonPropertyName("resourceType")]
        public string ResourceType { get; set; } = default!;
        [JsonPropertyName("tableName")]
        public string TableName { get; set; } = default!;
        [JsonPropertyName("fields")]
        public List<FieldMapping> Fields { get; set; } = new();
        [JsonPropertyName("constants")]
        public Dictionary<string, JsonNode> Constants { get; set; } = new();
    }
}
