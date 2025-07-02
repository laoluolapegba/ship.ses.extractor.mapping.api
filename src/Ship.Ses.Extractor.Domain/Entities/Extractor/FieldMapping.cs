using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Ship.Ses.Extractor.Domain.Models.Extractor
{
    public class FieldMapping
    {
        [JsonPropertyName("fhirPath")]
        public string FhirPath { get; set; } = default!;

        [JsonPropertyName("emrField")]
        public string? EmrField { get; set; }

        [JsonPropertyName("emrFieldMap")]
        public Dictionary<string, string>? EmrFieldMap { get; set; }

        [JsonPropertyName("emrFieldPriority")]
        public Dictionary<string, string>? EmrFieldPriorityMap { get; set; }

        [JsonPropertyName("identifierTypeMap")]
        public Dictionary<string, Dictionary<string, object>>? IdentifierTypeMap { get; set; }

        [JsonPropertyName("template")]
        public string? Template { get; set; }

        [JsonPropertyName("dataType")]
        public string? DataType { get; set; }

        [JsonPropertyName("format")]
        public string? Format { get; set; }

        [JsonPropertyName("default")]
        public string? Default { get; set; }

        [JsonPropertyName("defaults")]
        public Dictionary<string, object>? Defaults { get; set; }

        [JsonPropertyName("valueSet")]
        public Dictionary<string, object>? ValueSet { get; set; }

        [JsonPropertyName("required")]
        public bool Required { get; set; } = false;
    }


}
