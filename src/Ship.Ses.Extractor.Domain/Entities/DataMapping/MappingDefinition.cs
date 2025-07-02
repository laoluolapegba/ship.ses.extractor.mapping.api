using Ship.Ses.Extractor.Domain.Entities.Extractor;
using Ship.Ses.Extractor.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ship.Ses.Extractor.Domain.Entities.DataMapping
{
    [Table("ses_mapping_definitions")]
    public class MappingDefinition
    {
        [Key]
        [Column("id")]
        public Guid Id { get; private set; }

        [Column("name")]
        public string Name { get; private set; } // e.g., "OpenMRS"

        [Column("description")]
        public string Description { get; private set; }

        [Column("fhir_resource_type_id")]
        public int FhirResourceTypeId { get; private set; } // e.g., "Patient"

        [ForeignKey("FhirResourceTypeId")]
        public FhirResourceType FhirResourceType { get; private set; }

        [Column("created_date")]
        public DateTime CreatedDate { get; private set; }

        [Column("last_modified_date")]
        public DateTime LastModifiedDate { get; private set; }

        [Column("is_active")]
        public bool IsActive { get; private set; }

        // Navigation property for related ColumnMappings
        public List<ColumnMapping> _columnMappings = new();
        //public ICollection<ColumnMapping> ColumnMappings { get;  } = new List<ColumnMapping>();

        
        public IReadOnlyCollection<ColumnMapping> ColumnMappings => _columnMappings.AsReadOnly();

        private MappingDefinition() { } // For EF Core

        public MappingDefinition(string name, string description, FhirResourceType fhirResourceType)
        {
            Id = Guid.NewGuid();
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description;
            FhirResourceType = fhirResourceType ?? throw new ArgumentNullException(nameof(fhirResourceType));
            FhirResourceTypeId = fhirResourceType.Id;
            CreatedDate = DateTime.UtcNow;
            LastModifiedDate = CreatedDate;
            IsActive = true;
        }

        public void AddMapping(ColumnMapping mapping)
        {
            if (mapping == null)
                throw new ArgumentNullException(nameof(mapping));

            _columnMappings.Add(mapping);
            LastModifiedDate = DateTime.UtcNow;
        }

        public void RemoveMapping(string emrTable, string emrColumn)
        {
            var mapping = _columnMappings.Find(m => m.EmrTable == emrTable && m.EmrColumn == emrColumn);
            if (mapping != null)
            {
                _columnMappings.Remove(mapping);
                LastModifiedDate = DateTime.UtcNow;
            }
        }

        public void ClearMappings()
        {
            _columnMappings.Clear();
            LastModifiedDate = DateTime.UtcNow;
        }

        public void SetMappings(IEnumerable<ColumnMapping> mappings)
        {
            _columnMappings.Clear();
            _columnMappings.AddRange(mappings);
            LastModifiedDate = DateTime.UtcNow;
        }

        public void Update(string name, string description)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description;
            LastModifiedDate = DateTime.UtcNow;
        }

        public void SetActive(bool isActive)
        {
            IsActive = isActive;
            LastModifiedDate = DateTime.UtcNow;
        }
    }
}
