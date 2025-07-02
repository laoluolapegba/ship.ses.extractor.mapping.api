using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ship.Ses.Extractor.Domain.Entities.DataMapping
{
    public class Mapping
    {
        public int Id { get; set; }
        public string EmrName { get; set; } // e.g., "OpenMRS"
        public string ResourceType { get; set; } // e.g., "Patient"
        public string TableName { get; set; } // e.g., "patients"
        public List<FieldMapping> Fields { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class FieldMapping
    {
        public string EmrField { get; set; }
        public string FhirPath { get; set; }
        public string DataType { get; set; }
        public string? Format { get; set; }
        public string? Default { get; set; }
    }

}
