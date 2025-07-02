using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Ship.Ses.Extractor.Application.DTOs;
using Ship.Ses.Extractor.Domain.Entities.DataMapping;
using Ship.Ses.Extractor.Domain.Repositories.DataMapping;
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

        // Helper method to get the connection ID from header and select it
        private async Task<IActionResult> SelectConnectionFromHeader()
        {
            if (Request.Headers.TryGetValue("X-Emr-Connection-Id", out var headerValues))
            {
                if (int.TryParse(headerValues.FirstOrDefault(), out int connectionId))
                {
                    _logger.LogInformation("Attempting to select EMR connection from header: {ConnectionId}", connectionId);
                    try
                    {
                        await _emrDatabaseService.SelectConnectionAsync(connectionId);
                        return null; // Indicates success, no error result
                    }
                    catch (ArgumentException ex) // Connection ID not found
                    {
                        _logger.LogWarning(ex, "Invalid EMR connection ID in header: {ConnectionId}", connectionId);
                        return BadRequest($"Invalid EMR connection ID provided: {connectionId}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error selecting EMR connection from header for ID: {ConnectionId}", connectionId);
                        return StatusCode(500, "Error processing EMR connection selection.");
                    }
                }
            }
            _logger.LogWarning("No EMR connection ID found in 'X-Emr-Connection-Id' header or it's invalid.");
            return BadRequest("An EMR connection must be selected. Please provide 'X-Emr-Connection-Id' header.");
        }


        /// <summary>
        /// Retrieves a list of EMR tables.
        /// </summary>
        /// <returns>A list of EMR tables.</returns>
        [HttpGet("tables")]
        [ProducesResponseType(typeof(IEnumerable<EmrTableDto>), 200)]
        [ProducesResponseType(400)] // Added for missing connection header
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetTables()
        {
            var connectionResult = await SelectConnectionFromHeader();
            if (connectionResult != null)
            {
                return connectionResult; // If connection selection failed, return the error
            }

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
            catch (InvalidOperationException ex) // Catch the "No EMR connection has been selected" if it somehow slips through
            {
                _logger.LogError(ex, "EMR connection not selected for GetTables. This should have been caught by SelectConnectionFromHeader.");
                return BadRequest("No active EMR connection. Please select one.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving EMR database tables.");
                return StatusCode(500, "Error retrieving database schema information.");
            }
        }

        /// <summary>
        /// Retrieves the schema for a specific EMR table.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <returns>The schema of the specified table.</returns>
        [HttpGet("tables/{tableName}")]
        [ProducesResponseType(typeof(EmrTableDto), 200)]
        [ProducesResponseType(400)] // Added for missing connection header
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetTableSchema(string tableName)
        {
            var connectionResult = await SelectConnectionFromHeader();
            if (connectionResult != null)
            {
                return connectionResult; // If connection selection failed, return the error
            }

            try
            {
                var schema = await _emrDatabaseService.GetTableSchemaAsync(tableName);
                var columnDtos = schema.Columns.Select(c => new EmrColumnDto
                {
                    Name = c.Name,
                    DataType = c.DataType,
                    IsNullable = c.IsNullable,
                    IsPrimaryKey = c.IsPrimaryKey
                }).ToList();

                var tableDto = new EmrTableDto
                {
                    Name = schema.TableName,
                    Columns = columnDtos
                };

                return Ok(tableDto);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "EMR connection not selected for GetTableSchema. This should have been caught by SelectConnectionFromHeader.");
                return BadRequest("No active EMR connection. Please select one.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving schema for table {TableName}", tableName);
                return StatusCode(500, $"Error retrieving schema for table {tableName}");
            }
        }
        

        /// <summary>
        /// Tests the EMR database connection.
        /// </summary>
        /// <returns>Result of the connection test.</returns>
        [HttpGet("test-connection")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)] // Added for missing connection header
        [ProducesResponseType(500)]
        public async Task<IActionResult> TestConnection()
        {
            var connectionResult = await SelectConnectionFromHeader();
            if (connectionResult != null)
            {
                return connectionResult; // If connection selection failed, return the error
            }

            try
            {
                await _emrDatabaseService.TestConnectionAsync();
                return Ok(new { message = "Connection successful" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "EMR connection not selected for TestConnection. This should have been caught by SelectConnectionFromHeader.");
                return BadRequest("No active EMR connection. Please select one.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error connecting to EMR database.");
                return StatusCode(500, "Error connecting to EMR database.");
            }
        }

        /// <summary>
        /// Retrieves a list of active EMR connections.
        /// </summary>
        /// <returns>A list of EMR connections.</returns>
        [HttpGet("connections")]
        [ProducesResponseType(typeof(IEnumerable<EmrConnectionDto>), 200)]
        public async Task<IActionResult> GetConnections()
        {
            // This endpoint does NOT require a selected connection, as it retrieves available connections
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
                    // Note: Password is intentionally not returned for security
                });

                return Ok(connectionDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving EMR connections.");
                return StatusCode(500, "Error retrieving EMR connections.");
            }
        }

        /// <summary>
        /// Selects a specific EMR connection by ID.
        /// </summary>
        /// <param name="id">The ID of the EMR connection.</param>
        /// <returns>Result of the selection operation.</returns>
        [HttpPost("connections/select/{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> SelectConnection(int id)
        {
            _logger.LogInformation("📥 Received request to select EMR connection with ID: {ConnectionId}", id);
            try
            {
                // This directly selects the connection for the current API request context.
                // In a stateless API, this "selection" means setting it for the EmrDatabaseService
                // for the duration of this specific request, rather than storing state across requests.
                await _emrDatabaseService.SelectConnectionAsync(id);
                _logger.LogInformation("✅ Successfully selected EMR connection with ID: {ConnectionId}", id);
                return Ok(new { message = $"Connection {id} selected successfully." });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "EMR connection with ID {Id} not found.", id);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error selecting EMR connection with ID {Id}.", id);
                return StatusCode(500, $"Error selecting EMR connection with ID {id}.");
            }
        }

        /// <summary>
        /// Retrieves a specific EMR connection by its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the EMR connection.</param>
        /// <returns>The EMR connection details.</returns>
        [HttpGet("connections/{id}")] // <-- NEW ENDPOINT!
        [ProducesResponseType(typeof(EmrConnectionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetConnectionById(int id)
        {
            _logger.LogInformation("📥 Received request to retrieve EMR connection with ID: {ConnectionId}", id);

            try
            {
                var connection = await _connectionRepository.GetByIdAsync(id);

                if (connection == null)
                {
                    _logger.LogWarning("⚠️ EMR connection with ID {ConnectionId} not found", id);
                    return NotFound($"EMR connection with ID {id} not found");
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
                    // Password is intentionally omitted for security
                };

                _logger.LogInformation("📤 Successfully retrieved EMR connection with ID: {ConnectionId}", id);
                return Ok(connectionDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error retrieving EMR connection with ID {ConnectionId}", id);
                return StatusCode(500, $"Error retrieving EMR connection with ID {id}");
            }
        }
        /// <summary>
        /// Creates a new EMR connection.
        /// </summary>
        /// <param name="connectionDto">The EMR connection data.</param>
        /// <returns>The ID of the created connection.</returns>
        [HttpPost("connections")]
        [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> CreateConnection([FromBody] EmrConnectionDto connectionDto)
        {
            _logger.LogInformation("📥 Received request to create a new EMR connection: {ConnectionName}", connectionDto.Name);
            if (connectionDto == null)
            {
                return BadRequest("Connection data is required.");
            }

            try
            {
                // Convert DTO to Domain Model
                var newConnection = new EmrConnection(
                    connectionDto.Name,
                    connectionDto.Description,
                    connectionDto.DatabaseType,
                    connectionDto.Server,
                    connectionDto.Port,
                    connectionDto.DatabaseName,
                    connectionDto.Username,
                    connectionDto.Password // Assuming Password is sent for creation
                );

                await _connectionRepository.AddAsync(newConnection);
                _logger.LogInformation("✅ Successfully created EMR connection with ID: {ConnectionId}", newConnection.Id);
                return CreatedAtAction(nameof(GetConnections), new { id = newConnection.Id }, newConnection.Id);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid data for EMR connection creation: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error creating EMR connection: {ConnectionName}", connectionDto.Name);
                return StatusCode(500, $"Error creating EMR connection: {ex.Message}");
            }
        }


        /// <summary>
        /// Updates an existing EMR connection.
        /// </summary>
        /// <param name="id">The ID of the EMR connection to update.</param>
        /// <param name="connectionDto">The updated EMR connection data.</param>
        /// <returns>No content if successful.</returns>
        [HttpPut("connections/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateConnection(int id, [FromBody] EmrConnectionDto connectionDto)
        {
            _logger.LogInformation("📥 Received request to update EMR connection with ID: {ConnectionId}", id);

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
                    _logger.LogWarning("⚠️ EMR connection with ID {ConnectionId} not found for update.", id);
                    return NotFound($"EMR connection with ID {id} not found.");
                }

                // Update the domain model
                existingConnection.Update(
                    connectionDto.Name,
                    connectionDto.Description,
                    connectionDto.DatabaseType,
                    connectionDto.Server,
                    connectionDto.Port,
                    connectionDto.DatabaseName,
                    connectionDto.Username,
                    connectionDto.Password // Assuming password can be updated
                );
                existingConnection.SetActive(connectionDto.IsActive); // Allow updating active status

                await _connectionRepository.UpdateAsync(existingConnection);
                _logger.LogInformation("✅ Successfully updated EMR connection with ID: {ConnectionId}", id);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid data for EMR connection update (ID: {ConnectionId}): {Message}", id, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error updating EMR connection with ID {ConnectionId}", id);
                return StatusCode(500, $"Error updating EMR connection with ID {id}: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes an EMR connection.
        /// </summary>
        /// <param name="id">The ID of the EMR connection to delete.</param>
        /// <returns>No content if successful.</returns>
        [HttpDelete("connections/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteConnection(int id)
        {
            _logger.LogInformation("📥 Received request to delete EMR connection with ID: {ConnectionId}", id);

            try
            {
                var existingConnection = await _connectionRepository.GetByIdAsync(id);
                if (existingConnection == null)
                {
                    _logger.LogWarning("⚠️ EMR connection with ID {ConnectionId} not found for deletion.", id);
                    return NotFound($"EMR connection with ID {id} not found.");
                }

                await _connectionRepository.DeleteAsync(id);
                _logger.LogInformation("🗑️ Successfully deleted EMR connection with ID: {ConnectionId}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error deleting EMR connection with ID {ConnectionId}", id);
                return StatusCode(500, $"Error deleting EMR connection with ID {id}: {ex.Message}");
            }
        }

        /// <summary>
        /// Tests a specific EMR connection by ID.
        /// </summary>
        /// <param name="id">The ID of the EMR connection.</param>
        /// <returns>Result of the connection test.</returns>
        [HttpPost("connections/test/{id}")] // Changed route from "test-connection/{id}" to "test/{id}" for clarity
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)] // Add 404 for connection not found
        [ProducesResponseType(500)]
        public async Task<IActionResult> TestConnectionById(int id)
        {
            _logger.LogInformation("📥 Received request to test EMR connection with ID: {ConnectionId}", id);
            try
            {
                // This calls SelectConnectionAsync internally, which validates the ID
                await _emrDatabaseService.SelectConnectionAsync(id);
                await _emrDatabaseService.TestConnectionAsync();
                _logger.LogInformation("✅ Connection test successful for ID: {ConnectionId}", id);
                return Ok(new { message = "Connection successful" });
            }
            catch (ArgumentException ex) // Catches if SelectConnectionAsync throws ArgumentException (not found)
            {
                _logger.LogWarning(ex, "EMR connection with ID {Id} not found for testing.", id);
                return NotFound(ex.Message); // Return 404 for not found
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error testing EMR connection with ID {Id}.", id);
                return BadRequest(new { error = ex.Message }); // Bad Request for connection failure (e.g., incorrect credentials)
            }
        }
    }
}
