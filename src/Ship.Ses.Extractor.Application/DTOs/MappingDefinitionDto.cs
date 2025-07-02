using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ship.Ses.Extractor.Application.DTOs
{
    using System;
    using System.Collections.Generic;

    public class MappingDefinitionDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int FhirResourceTypeId { get; set; }
        public string FhirResourceTypeName { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public List<FieldMappingConfigurationModel> Mappings { get; set; } = new List<FieldMappingConfigurationModel>();
    }

    public class ColumnMappingDto
    {
        public string EmrTable { get; set; }
        public string EmrColumn { get; set; }
        public string FhirPath { get; set; }
        public string TransformationExpression { get; set; }
    }
}
