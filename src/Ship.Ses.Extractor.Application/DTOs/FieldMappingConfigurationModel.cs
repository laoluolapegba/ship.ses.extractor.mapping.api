using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Ship.Ses.Extractor.Application.DTOs
{
    public class FieldMappingConfigurationModel
    {
        [JsonPropertyName("fhirPath")]
        public string FhirPath { get; set; }

        [JsonPropertyName("emrTable")] 
        public string EmrTable { get; set; }

        [JsonPropertyName("emrField")]
        public string EmrField { get; set; }

        [JsonPropertyName("dataType")]
        public string DataType { get; set; }

        [JsonPropertyName("required")]
        public bool Required { get; set; }

        [JsonPropertyName("format")]
        public string Format { get; set; }

        [JsonPropertyName("template")]
        public string Template { get; set; }

        [JsonPropertyName("emrFieldMap")]
        public Dictionary<string, string> EmrFieldMap { get; set; }

        [JsonPropertyName("valueSet")]
        public ValueSetMap ValueSet { get; set; }

        [JsonPropertyName("emrFieldPriority")]
        public Dictionary<string, string> EmrFieldPriority { get; set; }

        [JsonPropertyName("identifierTypeMap")]
        public Dictionary<string, IdentifierTypeMapEntry> IdentifierTypeMap { get; set; }

        [JsonPropertyName("defaults")]
        public Dictionary<string, object> Defaults { get; set; }

        [JsonPropertyName("source")]
        public string Source { get; set; }
        public string TransformationExpression { get; set; }

        public FieldMappingConfigurationModel()
        {
            EmrFieldMap = new Dictionary<string, string>();
            ValueSet = new ValueSetMap(); // Ensure ValueSet is initialized here too
            ValueSet.DisplayMap = new Dictionary<string, string>();
            EmrFieldPriority = new Dictionary<string, string>();
            IdentifierTypeMap = new Dictionary<string, IdentifierTypeMapEntry>();
            Defaults = new Dictionary<string, object>();
        }
    }

    public class ValueSetMap
    {
        [JsonPropertyName("system")]
        public string System { get; set; }

        [JsonPropertyName("displayMap")]
        public Dictionary<string, string> DisplayMap { get; set; } = new Dictionary<string, string>();
    }

    public class IdentifierTypeMapEntry
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("display")]
        public string Display { get; set; }

        [JsonPropertyName("system")]
        public string System { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }
    }
}
