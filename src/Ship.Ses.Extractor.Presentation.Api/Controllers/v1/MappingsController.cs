using Asp.Versioning;
using Hl7.Fhir.Model;
using Microsoft.AspNetCore.Mvc;
using Ship.Ses.Extractor.Application.DTOs;
using Ship.Ses.Extractor.Application.Interfaces;
using Ship.Ses.Extractor.Domain.Repositories.DataMapping;
using Ship.Ses.Extractor.Presentation.Api.Helpers;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
namespace Ship.Ses.Extractor.Presentation.Api.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/mappings")]
    public class MappingsController : ControllerBase
    {
        private readonly IMappingService _mappingService;
        private readonly IFhirResourceService _fhirResourceService;
        private readonly ILogger<MappingsController> _logger;

        public MappingsController(
            IMappingService mappingService,
            IFhirResourceService fhirResourceService,
            ILogger<MappingsController> logger)
        {
            _mappingService = mappingService;
            _fhirResourceService = fhirResourceService;
            _logger = logger;
        }

        [HttpGet("resource-types")]
        public async Task<ActionResult<IEnumerable<FhirResourceTypeDto>>> GetResourceTypes()
        {
            _logger.LogInformation("Received request to retrieve FHIR resource types");
            try
            {
                var resourceTypes = await _fhirResourceService.GetAllResourceTypesAsync();
                var dtos = resourceTypes.Select(rt => new FhirResourceTypeDto
                {
                    Id = rt.Id,
                    Name = rt.Name,
                    Structure = rt.Structure
                });
                _logger.LogInformation("Successfully retrieved {Count} FHIR resource types", dtos.Count());
                return Ok(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving FHIR resource types");
                return StatusCode(500, "Error retrieving FHIR resource types");
            }
        }

        [HttpGet("resource-types/{resourceTypeId}/structure")]
        public async Task<ActionResult<JsonElement>> GetResourceStructure(int resourceTypeId)
        {
            var safeId = SafeMessageHelper.Sanitize(resourceTypeId);
            _logger.LogInformation("Received request to retrieve structure for FHIR resource type ID: {ResourceTypeId}", safeId);
            try
            {
                var resourceType = await _fhirResourceService.GetResourceTypeByIdAsync(resourceTypeId);
                if (resourceType == null || string.IsNullOrEmpty(resourceType.Structure))
                {
                    _logger.LogWarning("Structure not available for FHIR resource type ID: {ResourceTypeId}", safeId);
                    return NotFound($"Structure not available for FHIR resource type ID {safeId}");
                }
                using var doc = JsonDocument.Parse(resourceType.Structure);
                _logger.LogInformation("Successfully retrieved structure for FHIR resource type ID: {ResourceTypeId}", safeId);
                return Ok(doc.RootElement.Clone());
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse stored FHIR structure for ID {ResourceTypeId}", safeId);
                return StatusCode(500, "Invalid FHIR structure stored on server.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving structure for FHIR resource type ID {ResourceTypeId}", safeId);
                return StatusCode(500, $"Error retrieving structure for FHIR resource type ID {safeId}");
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MappingDefinitionDto>>> GetMappings()
        {
            _logger.LogInformation("Received request to retrieve all mappings");
            try
            {
                var mappings = await _mappingService.GetAllMappingsAsync();
                _logger.LogInformation("Successfully retrieved {Count} mappings", mappings.Count());
                return Ok(mappings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving mappings");
                return StatusCode(500, "Error retrieving mappings");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MappingDefinitionDto>> GetMapping(Guid id)
        {
            var safeId = SafeMessageHelper.Sanitize(id);
            _logger.LogInformation("Received request to retrieve mapping with ID: {MappingId}", safeId);
            try
            {
                var mapping = await _mappingService.GetMappingByIdAsync(id);
                if (mapping == null)
                {
                    _logger.LogWarning("Mapping with ID {MappingId} not found", safeId);
                    return NotFound($"Mapping with ID {safeId} not found");
                }
                _logger.LogInformation("Successfully retrieved mapping with ID: {MappingId}", safeId);
                return Ok(mapping);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving mapping with ID {MappingId}", safeId);
                return StatusCode(500, $"Error retrieving mapping with ID {safeId}");
            }
        }

        [HttpGet("resource-type/{resourceTypeId}")]
        public async Task<ActionResult<IEnumerable<MappingDefinitionDto>>> GetMappingsByResourceType(int resourceTypeId)
        {
            var safeId = SafeMessageHelper.Sanitize(resourceTypeId);
            _logger.LogInformation("Received request to retrieve mappings for resource type ID: {ResourceTypeId}", safeId);
            try
            {
                var mappings = await _mappingService.GetMappingsByResourceTypeAsync(resourceTypeId);
                _logger.LogInformation("Successfully retrieved {Count} mappings for resource type ID: {ResourceTypeId}", mappings.Count(), safeId);
                return Ok(mappings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving mappings for resource type ID {ResourceTypeId}", safeId);
                return StatusCode(500, $"Error retrieving mappings for resource type ID {safeId}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> CreateMapping([FromBody] MappingDefinitionDto mappingDto)
        {
            _logger.LogInformation("Received request to create a new mapping");
            if (mappingDto == null)
            {
                _logger.LogWarning("Mapping data is null");
                return BadRequest("Mapping data is required");
            }
            try
            {
                var id = await _mappingService.CreateMappingAsync(mappingDto);
                _logger.LogInformation("Successfully created mapping with ID: {MappingId}", SafeMessageHelper.Sanitize(id));
                return CreatedAtAction(nameof(GetMapping), new { id }, id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating mapping");
                return StatusCode(500, "Error creating mapping");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMapping(Guid id, [FromBody] MappingDefinitionDto mappingDto)
        {
            var safeId = SafeMessageHelper.Sanitize(id);
            if (id != mappingDto.Id)
            {
                _logger.LogWarning("ID mismatch: route ID {RouteId} does not match DTO ID {DtoId}", safeId, SafeMessageHelper.Sanitize(mappingDto.Id));
                return BadRequest("ID mismatch");
            }
            try
            {
                _logger.LogInformation("Successfully updated mapping with ID: {MappingId}", safeId);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Mapping with ID {MappingId} not found: {Message}", safeId, ex.Message);
                return NotFound(ex.Message);
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Mapping with ID {MappingId} not found", safeId);
                return NotFound($"Mapping with ID {safeId} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating mapping with ID {MappingId}", safeId);
                return StatusCode(500, $"Error updating mapping with ID {safeId}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMapping(Guid id)
        {
            var safeId = SafeMessageHelper.Sanitize(id);
            try
            {
                await _mappingService.DeleteMappingAsync(id);
                _logger.LogInformation("Successfully deleted mapping with ID: {MappingId}", safeId);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Mapping with ID {MappingId} not found: {Message}", safeId, ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting mapping with ID {MappingId}", safeId);
                return StatusCode(500, $"Error deleting mapping with ID {safeId}");
            }
        }
    }
}

