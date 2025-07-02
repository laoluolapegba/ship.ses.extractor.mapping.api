using Ship.Ses.Extractor.Domain.Entities.DataMapping;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ship.Ses.Extractor.Domain.ValueObjects
{
    [Table("ses_column_mappings")]
    public class ColumnMapping
    {
        [Key]
        [Column("column_mapping_id")]
        public int ColumnMappingId { get; set; }

        [Column("emr_table")]
        public string EmrTable { get; set; }

        [Column("emr_column")]
        public string EmrColumn { get; set; }

        [Column("fhir_path")]
        public string FhirPath { get; set; }

        [Column("data_type")]
        public string DataType { get; set; }

        [Column("transformation_expression")]
        public string TransformationExpression { get; set; }  //ublic string? Format { get; set; }

        // Foreign key to MappingDefinition
        [Column("mapping_definition_id")]
        public Guid MappingDefinitionId { get; set; }

        // Navigation property
        //[ForeignKey("MappingDefinitionId")]
        public MappingDefinition MappingDefinition { get; set; }

        // Add a parameterless constructor for EF Core
        public ColumnMapping() { }

        public ColumnMapping(string emrTable, string emrColumn, string fhirPath, string transformationExpression = null)
        {
            EmrTable = emrTable ?? throw new ArgumentNullException(nameof(emrTable));
            EmrColumn = emrColumn ?? throw new ArgumentNullException(nameof(emrColumn));
            FhirPath = fhirPath ?? throw new ArgumentNullException(nameof(fhirPath));
            TransformationExpression = transformationExpression;
        }
    }
}
