using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ship.Ses.Extractor.Application.Services.DataMapping
{
    using Ship.Ses.Extractor.Application.DTOs;
    using Ship.Ses.Extractor.Application.Interfaces;
    using Ship.Ses.Extractor.Domain.Entities;
    using Ship.Ses.Extractor.Domain.Entities.DataMapping;
    using Ship.Ses.Extractor.Domain.Repositories.DataMapping;
    using Ship.Ses.Extractor.Domain.ValueObjects;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class MappingService : IMappingService
    {
        private readonly IMappingRepository _mappingRepository;
        private readonly IFhirResourceService _fhirResourceService;

        public MappingService(IMappingRepository mappingRepository, IFhirResourceService fhirResourceService)
        {
            _mappingRepository = mappingRepository;
            _fhirResourceService = fhirResourceService;
        }

        public async Task<IEnumerable<MappingDefinitionDto>> GetAllMappingsAsync()
        {
            var mappings = await _mappingRepository.GetAllAsync();
            return mappings.Select(MapToDto);
        }

        public async Task<MappingDefinitionDto> GetMappingByIdAsync(Guid id)
        {
            var mapping = await _mappingRepository.GetByIdAsync(id);
            return mapping != null ? MapToDto(mapping) : null;
        }

        public async Task<IEnumerable<MappingDefinitionDto>> GetMappingsByResourceTypeAsync(int resourceTypeId)
        {
            var mappings = await _mappingRepository.GetByResourceTypeAsync(resourceTypeId);
            return mappings.Select(MapToDto);
        }

        public async Task<Guid> CreateMappingAsync(MappingDefinitionDto mappingDto)
        {
            var resourceType = await _fhirResourceService.GetResourceTypeByIdAsync(mappingDto.FhirResourceTypeId);

            if (resourceType == null)
                throw new ArgumentException($"FHIR resource type with ID {mappingDto.FhirResourceTypeId} not found.");

            var mapping = new MappingDefinition(
                mappingDto.Name,
                mappingDto.Description,
                resourceType);

            foreach (var columnMapping in mappingDto.Mappings)
            {
                mapping.AddMapping(new ColumnMapping(
                    columnMapping.EmrTable,
                    columnMapping.EmrField,
                    columnMapping.FhirPath,
                    columnMapping.TransformationExpression));
            }

            await _mappingRepository.AddAsync(mapping);
            return mapping.Id;
        }

        public async Task UpdateMappingAsync(MappingDefinitionDto mappingDto)
        {
            var mapping = await _mappingRepository.GetByIdAsync(mappingDto.Id);

            if (mapping == null)
                throw new ArgumentException($"Mapping with ID {mappingDto.Id} not found.");

            mapping.Update(mappingDto.Name, mappingDto.Description);

            var columnMappings = mappingDto.Mappings.Select(m => new ColumnMapping(
                m.EmrTable,
                m.EmrField,
                m.FhirPath,
                m.TransformationExpression)).ToList();

            mapping.SetMappings(columnMappings);

            await _mappingRepository.UpdateAsync(mapping);
        }

        public async Task DeleteMappingAsync(Guid id)
        {
            await _mappingRepository.DeleteAsync(id);
        }

        private MappingDefinitionDto MapToDto(MappingDefinition mapping)
        {
            return new MappingDefinitionDto
            {
                Id = mapping.Id,
                Name = mapping.Name,
                Description = mapping.Description,
                FhirResourceTypeId = mapping.FhirResourceTypeId,
                FhirResourceTypeName = mapping.FhirResourceType?.Name,
                CreatedDate = mapping.CreatedDate,
                LastModifiedDate = mapping.LastModifiedDate,
                Mappings = mapping.ColumnMappings 
            .Select(cm => new Ship.Ses.Extractor.Application.DTOs.FieldMappingConfigurationModel
            {
                // Ensure property names on 'cm' (ColumnMapping domain model) match
                // what's available and what you want to map to FieldMappingConfigurationModel
                EmrField = cm.EmrColumn,
                TransformationExpression = cm.TransformationExpression
            })
            .ToList()
            };
        }
    }
}
