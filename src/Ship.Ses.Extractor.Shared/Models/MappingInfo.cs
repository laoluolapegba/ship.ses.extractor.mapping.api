using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ship.Ses.Extractor.Shared.Models
{
    public class MappingInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int FhirResourceTypeId { get; set; }
        public string FhirResourceName { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public List<ColumnMappingInfo> Mappings { get; set; } = new List<ColumnMappingInfo>();
    }

    public class ColumnMappingInfo
    {
        public string EmrTable { get; set; }
        public string EmrColumn { get; set; }
        public string FhirPath { get; set; }
        public string TransformExpression { get; set; }
    }
}
