using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ship.Ses.Extractor.Application.DTOs
{
    using System;
    using System.Collections.Generic;

    using System.ComponentModel.DataAnnotations;

    public class MappingDefinitionDto
    {
        public Guid Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public int FhirResourceTypeId { get; set; }

        [Required, StringLength(100)]
        public string FhirResourceTypeName { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime LastModifiedDate { get; set; }

        [Required, MinLength(1)]
        public List<FieldMappingConfigurationModel> Mappings { get; set; } = new();
    }


    public class ColumnMappingDto
    {
        public string EmrTable { get; set; }
        public string EmrColumn { get; set; }
        public string FhirPath { get; set; }
        public string TransformationExpression { get; set; }
    }
}
