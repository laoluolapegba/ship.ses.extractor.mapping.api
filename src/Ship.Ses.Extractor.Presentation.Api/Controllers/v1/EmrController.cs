using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using Ship.Ses.Extractor.Application.DTOs;
using Ship.Ses.Extractor.Domain.Entities.DataMapping;
using Ship.Ses.Extractor.Domain.Repositories.DataMapping;
using Ship.Ses.Extractor.Presentation.Api.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace Ship.Ses.Extractor.Presentation.Api.Controllers.v1
{

    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/emr")]
    public class EmrController : ControllerBase
    {
        private readonly IEmrDatabaseService _emrDatabaseService;
        private readonly ILogger<EmrController> _logger;
        private readonly IEmrConnectionRepository _connectionRepository;

        public EmrController(
            IEmrDatabaseService emrDatabaseService,
            IEmrConnectionRepository connectionRepository,
            ILogger<EmrController> logger)
        {
            _emrDatabaseService = emrDatabaseService;
            _connectionRepository = connectionRepository;
            _logger = logger;
        }

        private async Task<IActionResult> SelectConnectionFromHeader()
        {
            if (Request.Headers.TryGetValue("X-Emr-Connection-Id", out var headerValues))
            {
                if (int.TryParse(headerValues.FirstOrDefault(), out int connectionId))
                {
                    var safeId = SafeMessageHelper.Sanitize(connectionId);
                    _logger.LogInformation("Attempting to select EMR connection from header: {ConnectionId}", safeId);
                    try
                    {
                        await _emrDatabaseService.SelectConnectionAsync(connectionId);
                        return null;
                    }
                    catch (ArgumentException ex)
                    {
                        _logger.LogWarning(SafeMessageHelper.Sanitize(ex), "Invalid EMR connection ID in header: {ConnectionId}", safeId);
                        return BadRequest($"Invalid EMR connection ID provided: {safeId}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(SafeMessageHelper.Sanitize(ex), "Error selecting EMR connection from header for ID: {ConnectionId}", safeId);
                        return StatusCode(500, "Error processing EMR connection selection.");
                    }
                }
            }
            _logger.LogWarning("No EMR connection ID found in 'X-Emr-Connection-Id' header or it's invalid.");
            return BadRequest("An EMR connection must be selected. Please provide 'X-Emr-Connection-Id' header.");
        }

        [HttpGet("tables")]
        public async Task<IActionResult> GetTables()
        {
            var connectionResult = await SelectConnectionFromHeader();
            if (connectionResult != null) return connectionResult;

            try
            {
                var allTableSchemas = await _emrDatabaseService.GetAllTablesSchemaAsync();
                var tableDtos = allTableSchemas.Select(ts => new EmrTableDto
                {
                    Name = ts.TableName,
                    Columns = ts.Columns.Select(c => new EmrColumnDto
                    {
                        Name = c.Name,
                        DataType = c.DataType,
                        IsNullable = c.IsNullable,
                        IsPrimaryKey = c.IsPrimaryKey
                    }).ToList()
                }).ToList();

                return Ok(tableDtos);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(SafeMessageHelper.Sanitize(ex), "EMR connection not selected for GetTables.");
                return BadRequest("No active EMR connection. Please select one.");
            }
            catch (Exception ex)
            {
                _logger.LogError(SafeMessageHelper.Sanitize(ex), "Error retrieving EMR database tables.");
                return StatusCode(500, "Error retrieving database schema information.");
            }
        }

        [HttpGet("tables/{tableName}")]
        public async Task<IActionResult> GetTableSchema(string tableName)
        {
            var connectionResult = await SelectConnectionFromHeader();
            if (connectionResult != null) return connectionResult;

            var safeTable = SafeMessageHelper.Sanitize(tableName);
            try
            {
                var schema = await _emrDatabaseService.GetTableSchemaAsync(tableName);
                var tableDto = new EmrTableDto
                {
                    Name = schema.TableName,
                    Columns = schema.Columns.Select(c => new EmrColumnDto
                    {
                        Name = c.Name,
                        DataType = c.DataType,
                        IsNullable = c.IsNullable,
                        IsPrimaryKey = c.IsPrimaryKey
                    }).ToList()
                };
                return Ok(tableDto);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(SafeMessageHelper.Sanitize(ex), "EMR connection not selected for GetTableSchema.");
                return BadRequest("No active EMR connection. Please select one.");
            }
            catch (Exception ex)
            {
                _logger.LogError(SafeMessageHelper.Sanitize(ex), "Error retrieving schema for table {TableName}", safeTable);
                return StatusCode(500, $"Error retrieving schema for table {safeTable}");
            }
        }

        [HttpGet("test-connection")]
        public async Task<IActionResult> TestConnection()
        {
            var connectionResult = await SelectConnectionFromHeader();
            if (connectionResult != null) return connectionResult;

            try
            {
                await _emrDatabaseService.TestConnectionAsync();
                return Ok(new { message = "Connection successful" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(SafeMessageHelper.Sanitize(ex), "EMR connection not selected for TestConnection.");
                return BadRequest("No active EMR connection. Please select one.");
            }
            catch (Exception ex)
            {
                _logger.LogError(SafeMessageHelper.Sanitize(ex), "Error connecting to EMR database.");
                return StatusCode(500, "Error connecting to EMR database.");
            }
        }

        [HttpGet("connections")]
        public async Task<IActionResult> GetConnections()
        {
            try
            {
                var connections = await _connectionRepository.GetActiveAsync();
                var connectionDtos = connections.Select(c => new EmrConnectionDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    DatabaseType = c.DatabaseType,
                    Server = c.Server,
                    Port = c.Port,
                    DatabaseName = c.DatabaseName,
                    Username = c.Username,
                    IsActive = c.IsActive,
                    CreatedDate = c.CreatedDate,
                    LastModifiedDate = c.LastModifiedDate
                });
                return Ok(connectionDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(SafeMessageHelper.Sanitize(ex), "Error retrieving EMR connections.");
                return StatusCode(500, "Error retrieving EMR connections.");
            }
        }

        /// <summary>
        /// Selects a specific EMR connection by ID.
        /// </summary>
        [HttpPost("connections/select/{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> SelectConnection(int id)
        {
            var safeId = SafeMessageHelper.Sanitize(id);
            _logger.LogInformation("📥 Received request to select EMR connection with ID: {ConnectionId}", safeId);
            try
            {
                await _emrDatabaseService.SelectConnectionAsync(id);
                _logger.LogInformation("✅ Successfully selected EMR connection with ID: {ConnectionId}", safeId);
                return Ok(new { message = $"Connection {safeId} selected successfully." });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(SafeMessageHelper.Sanitize(ex), "EMR connection with ID {Id} not found.", safeId);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(SafeMessageHelper.Sanitize(ex), "Error selecting EMR connection with ID {Id}.", safeId);
                return StatusCode(500, $"Error selecting EMR connection with ID {safeId}.");
            }
        }

        /// <summary>
        /// Retrieves a specific EMR connection by its ID.
        /// </summary>
        [HttpGet("connections/{id}")]
        [ProducesResponseType(typeof(EmrConnectionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetConnectionById(int id)
        {
            var safeId = SafeMessageHelper.Sanitize(id);
            _logger.LogInformation("📥 Received request to retrieve EMR connection with ID: {ConnectionId}", safeId);
            try
            {
                var connection = await _connectionRepository.GetByIdAsync(id);
                if (connection == null)
                {
                    _logger.LogWarning("⚠️ EMR connection with ID {ConnectionId} not found", safeId);
                    return NotFound($"EMR connection with ID {safeId} not found");
                }
                var connectionDto = new EmrConnectionDto
                {
                    Id = connection.Id,
                    Name = connection.Name,
                    Description = connection.Description,
                    DatabaseType = connection.DatabaseType,
                    Server = connection.Server,
                    Port = connection.Port,
                    DatabaseName = connection.DatabaseName,
                    Username = connection.Username,
                    IsActive = connection.IsActive,
                    CreatedDate = connection.CreatedDate,
                    LastModifiedDate = connection.LastModifiedDate
                };
                _logger.LogInformation("📤 Successfully retrieved EMR connection with ID: {ConnectionId}", safeId);
                return Ok(connectionDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(SafeMessageHelper.Sanitize(ex), "❌ Error retrieving EMR connection with ID {ConnectionId}", safeId);
                return StatusCode(500, $"Error retrieving EMR connection with ID {safeId}");
            }
        }

        /// <summary>
        /// Creates a new EMR connection.
        /// </summary>
        [HttpPost("connections")]
        [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> CreateConnection([FromBody] EmrConnectionDto connectionDto)
        {
            var safeName = SafeMessageHelper.Sanitize(connectionDto?.Name);
            _logger.LogInformation("📥 Received request to create a new EMR connection: {ConnectionName}", safeName);
            if (connectionDto == null) return BadRequest("Connection data is required.");
            try
            {
                var newConnection = new EmrConnection(
                    connectionDto.Name,
                    connectionDto.Description,
                    connectionDto.DatabaseType,
                    connectionDto.Server,
                    connectionDto.Port,
                    connectionDto.DatabaseName,
                    connectionDto.Username,
                    connectionDto.Password);
                await _connectionRepository.AddAsync(newConnection);
                _logger.LogInformation("✅ Successfully created EMR connection with ID: {ConnectionId}", newConnection.Id);
                return CreatedAtAction(nameof(GetConnections), new { id = newConnection.Id }, newConnection.Id);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(SafeMessageHelper.Sanitize(ex), "Invalid data for EMR connection creation: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(SafeMessageHelper.Sanitize(ex), "❌ Error creating EMR connection: {ConnectionName}", safeName);
                return StatusCode(500, $"Error creating EMR connection: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates an existing EMR connection.
        /// </summary>
        [HttpPut("connections/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateConnection(int id, [FromBody] EmrConnectionDto connectionDto)
        {
            var safeId = SafeMessageHelper.Sanitize(id);
            _logger.LogInformation("📥 Received request to update EMR connection with ID: {ConnectionId}", safeId);
            if (connectionDto == null || id != connectionDto.Id)
            {
                _logger.LogWarning("⚠️ Invalid update request for EMR connection: ID mismatch or null DTO.");
                return BadRequest("Invalid connection data or ID mismatch.");
            }
            try
            {
                var existingConnection = await _connectionRepository.GetByIdAsync(id);
                if (existingConnection == null)
                {
                    _logger.LogWarning("⚠️ EMR connection with ID {ConnectionId} not found for update.", safeId);
                    return NotFound($"EMR connection with ID {safeId} not found.");
                }
                existingConnection.Update(
                    connectionDto.Name,
                    connectionDto.Description,
                    connectionDto.DatabaseType,
                    connectionDto.Server,
                    connectionDto.Port,
                    connectionDto.DatabaseName,
                    connectionDto.Username,
                    connectionDto.Password);
                existingConnection.SetActive(connectionDto.IsActive);
                await _connectionRepository.UpdateAsync(existingConnection);
                _logger.LogInformation("✅ Successfully updated EMR connection with ID: {ConnectionId}", safeId);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(SafeMessageHelper.Sanitize(ex), "Invalid data for EMR connection update (ID: {ConnectionId}): {Message}", safeId, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(SafeMessageHelper.Sanitize(ex), "❌ Error updating EMR connection with ID {ConnectionId}", safeId);
                return StatusCode(500, $"Error updating EMR connection with ID {safeId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes an EMR connection.
        /// </summary>
        [HttpDelete("connections/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteConnection(int id)
        {
            var safeId = SafeMessageHelper.Sanitize(id);
            _logger.LogInformation("📥 Received request to delete EMR connection with ID: {ConnectionId}", safeId);
            try
            {
                var existingConnection = await _connectionRepository.GetByIdAsync(id);
                if (existingConnection == null)
                {
                    _logger.LogWarning("⚠️ EMR connection with ID {ConnectionId} not found for deletion.", safeId);
                    return NotFound($"EMR connection with ID {safeId} not found.");
                }
                await _connectionRepository.DeleteAsync(id);
                _logger.LogInformation("🗑️ Successfully deleted EMR connection with ID: {ConnectionId}", safeId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(SafeMessageHelper.Sanitize(ex), "❌ Error deleting EMR connection with ID {ConnectionId}", safeId);
                return StatusCode(500, $"Error deleting EMR connection with ID {safeId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Tests a specific EMR connection by ID.
        /// </summary>
        [HttpPost("connections/test/{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> TestConnectionById(int id)
        {
            var safeId = SafeMessageHelper.Sanitize(id);
            _logger.LogInformation("📥 Received request to test EMR connection with ID: {ConnectionId}", safeId);
            try
            {
                await _emrDatabaseService.SelectConnectionAsync(id);
                await _emrDatabaseService.TestConnectionAsync();
                _logger.LogInformation("✅ Connection test successful for ID: {ConnectionId}", safeId);
                return Ok(new { message = "Connection successful" });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(SafeMessageHelper.Sanitize(ex), "EMR connection with ID {Id} not found for testing.", safeId);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(SafeMessageHelper.Sanitize(ex), "❌ Error testing EMR connection with ID {Id}.", safeId);
                return BadRequest(new { error = ex.Message });
            }
        }
    }

}
