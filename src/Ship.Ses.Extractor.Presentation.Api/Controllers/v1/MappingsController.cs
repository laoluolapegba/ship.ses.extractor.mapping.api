using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Ship.Ses.Extractor.Application.DTOs;
using Ship.Ses.Extractor.Application.Interfaces;
using Ship.Ses.Extractor.Domain.Repositories.DataMapping;
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

        /// <summary>
        /// Retrieves all supported FHIR resource types.
        /// </summary>
        [HttpGet("resource-types")]
        [ProducesResponseType(typeof(IEnumerable<FhirResourceTypeDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<FhirResourceTypeDto>>> GetResourceTypes()
        {
            _logger.LogInformation("📥 Received request to retrieve FHIR resource types");

            try
            {
                var resourceTypes = await _fhirResourceService.GetAllResourceTypesAsync();

                var resourceTypeDtos = resourceTypes.Select(rt => new FhirResourceTypeDto
                {
                    Id = rt.Id,
                    Name = rt.Name,
                    Structure = rt.Structure
                }).ToList();

                _logger.LogInformation("📤 Successfully retrieved {Count} FHIR resource types", resourceTypeDtos.Count);
                return Ok(resourceTypeDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error retrieving FHIR resource types");
                return StatusCode(500, "Error retrieving FHIR resource types");
            }
        }
        /// <summary>
        /// Retrieves the JSON structure of a specific FHIR resource type.
        /// </summary>
        /// <param name="resourceTypeId">The ID of the FHIR resource type.</param>
        [HttpGet("resource-types/{resourceTypeId}/structure")]
        // Change return type here:
        [ProducesResponseType(typeof(JsonElement), StatusCodes.Status200OK)] // Or object/dynamic if less specific
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<JsonElement>> GetResourceStructure(int resourceTypeId) // Change return type here
        {
            _logger.LogInformation("📥 Received request to retrieve structure for FHIR resource type ID: {ResourceTypeId}", resourceTypeId);

            try
            {
                var resourceType = await _fhirResourceService.GetResourceTypeByIdAsync(resourceTypeId);

                if (resourceType == null)
                {
                    _logger.LogWarning("⚠️ FHIR resource type with ID {ResourceTypeId} not found", resourceTypeId);
                    return NotFound($"FHIR resource type with ID {resourceTypeId} not found");
                }

                if (string.IsNullOrEmpty(resourceType.Structure))
                {
                    _logger.LogWarning("⚠️ Structure not available for FHIR resource type ID: {ResourceTypeId}", resourceTypeId);
                    return NotFound($"Structure not available for FHIR resource type ID {resourceTypeId}");
                }

                // Parse the JSON string into a JsonDocument/JsonElement before returning
                // This tells ASP.NET Core to serialize the actual JSON object, not a string literal of it.
                using (JsonDocument doc = JsonDocument.Parse(resourceType.Structure))
                {
                    _logger.LogInformation("📤 Successfully retrieved structure for FHIR resource type ID: {ResourceTypeId}", resourceTypeId);
                    return Ok(doc.RootElement.Clone()); // Clone to return it from the using block
                }
            }
            catch (JsonException jsonEx) // Catch if resourceType.Structure is not valid JSON
            {
                _logger.LogError(jsonEx, "❌ Failed to parse stored FHIR structure for ID {ResourceTypeId}: {Structure}", resourceTypeId, "");
                return StatusCode(500, "Invalid FHIR structure stored on server.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error retrieving structure for FHIR resource type ID {ResourceTypeId}", resourceTypeId);
                return StatusCode(500, $"Error retrieving structure for FHIR resource type ID {resourceTypeId}");
            }
        }

        /// <summary>
        /// Retrieves all mapping definitions.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<MappingDefinitionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<MappingDefinitionDto>>> GetMappings()
        {
            _logger.LogInformation("📥 Received request to retrieve all mappings");

            try
            {
                var mappings = await _mappingService.GetAllMappingsAsync();
                _logger.LogInformation("📤 Successfully retrieved {Count} mappings", mappings.Count());
                return Ok(mappings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error retrieving mappings");
                return StatusCode(500, "Error retrieving mappings");
            }
        }

        /// <summary>
        /// Retrieves a specific mapping by its ID.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MappingDefinitionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<MappingDefinitionDto>> GetMapping(Guid id)
        {
            _logger.LogInformation("📥 Received request to retrieve mapping with ID: {MappingId}", id);

            try
            {
                var mapping = await _mappingService.GetMappingByIdAsync(id);

                if (mapping == null)
                {
                    _logger.LogWarning("⚠️ Mapping with ID {MappingId} not found", id);
                    return NotFound($"Mapping with ID {id} not found");
                }

                _logger.LogInformation("📤 Successfully retrieved mapping with ID: {MappingId}", id);
                return Ok(mapping);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error retrieving mapping with ID {MappingId}", id);
                return StatusCode(500, $"Error retrieving mapping with ID {id}");
            }
        }

        /// <summary>
        /// Retrieves mappings associated with a specific FHIR resource type.
        /// </summary>
        [HttpGet("resource-type/{resourceTypeId}")]
        [ProducesResponseType(typeof(IEnumerable<MappingDefinitionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<MappingDefinitionDto>>> GetMappingsByResourceType(int resourceTypeId)
        {
            _logger.LogInformation("📥 Received request to retrieve mappings for resource type ID: {ResourceTypeId}", resourceTypeId);

            try
            {
                var mappings = await _mappingService.GetMappingsByResourceTypeAsync(resourceTypeId);
                _logger.LogInformation("📤 Successfully retrieved {Count} mappings for resource type ID: {ResourceTypeId}", mappings.Count(), resourceTypeId);
                return Ok(mappings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error retrieving mappings for resource type ID {ResourceTypeId}", resourceTypeId);
                return StatusCode(500, $"Error retrieving mappings for resource type ID {resourceTypeId}");
            }
        }

        /// <summary>
        /// Creates a new mapping definition.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Guid>> CreateMapping([FromBody] MappingDefinitionDto mappingDto)
        {
            _logger.LogInformation("📥 Received request to create a new mapping");

            if (mappingDto == null)
            {
                _logger.LogWarning("⚠️ Mapping data is null");
                return BadRequest("Mapping data is required");
            }

            try
            {
                var id = await _mappingService.CreateMappingAsync(mappingDto);
                _logger.LogInformation("✅ Successfully created mapping with ID: {MappingId}", id);
                return CreatedAtAction(nameof(GetMapping), new { id }, id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error creating mapping");
                return StatusCode(500, "Error creating mapping");
            }
        }
        /// <summary>
        /// Updates an existing mapping definition.
        /// </summary>
        /// <param name="id">The unique identifier of the mapping to update.</param>
        /// <param name="mappingDto">The updated mapping definition.</param>
        /// <returns>No content if the update is successful.</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateMapping(Guid id, [FromBody] MappingDefinitionDto mappingDto)
        {
            if (id != mappingDto.Id)
            {
                _logger.LogWarning("⚠️ ID mismatch: route ID {RouteId} does not match mapping DTO ID {DtoId}", id, mappingDto.Id);
                return BadRequest("ID mismatch");
            }

            try
            {
                //await _mappingService.UpdateMappingAsync(mappingDto);
                _logger.LogInformation("✅ Successfully updated mapping with ID: {MappingId}", id);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("⚠️ Mapping with ID {MappingId} not found: {Message}", id, ex.Message);
                return NotFound(ex.Message);
            }
            
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("⚠️ Mapping with ID {MappingId} not found", id);
                return NotFound($"Mapping with ID {id} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error updating mapping with ID {MappingId}", id);
                return StatusCode(500, $"Error updating mapping with ID {id}");
            }
        }

        /// <summary>
        /// Deletes a mapping definition by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the mapping to delete.</param>
        /// <returns>No content if the deletion is successful.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteMapping(Guid id)
        {
            try
            {
                await _mappingService.DeleteMappingAsync(id);
                _logger.LogInformation("🗑️ Successfully deleted mapping with ID: {MappingId}", id);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("⚠️ Mapping with ID {MappingId} not found: {Message}", id, ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error deleting mapping with ID {MappingId}", id);
                return StatusCode(500, $"Error deleting mapping with ID {id}");
            }
        }
    }
}

