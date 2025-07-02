
using MongoDB.Bson;
using Ship.Ses.Extractor.Application.Services;
using Ship.Ses.Extractor.Domain.Entities.Extractor;
using Ship.Ses.Extractor.Domain.Entities.Patients;
using Ship.Ses.Extractor.Domain.Repositories.Extractor;
using Ship.Ses.Extractor.Domain.Repositories.Transformer;
using Ship.Ses.Extractor.Domain.Repositories.Validator;
using Ship.Ses.Extractor.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Serilog.Context;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;
using Fhir.Metrics;

namespace Ship.Ses.Extractor.Application.Services.Extractors
{
    public class PatientResourceExtractor
    {
        private readonly ITableMappingService _mappingService;
        private readonly IDataExtractorService _dataExtractor;
        private readonly IResourceTransformer<JsonObject> _transformer;
        private readonly IFhirResourceValidator _validator;
        private readonly IFhirSyncRepository<PatientSyncRecord> _repository;
        private readonly ISyncTrackingRepository _syncTrackingRepository;
        private readonly ILogger<PatientResourceExtractor> _logger;
        private readonly string _facilityId;
        const string prefix = "Organization/";
        public PatientResourceExtractor(
            ITableMappingService mappingService,
            IDataExtractorService dataExtractor,
            IResourceTransformer<JsonObject> transformer,
            IFhirResourceValidator validator,
            IFhirSyncRepository<PatientSyncRecord> repository,
            ISyncTrackingRepository syncTrackingRepository,
            ILogger<PatientResourceExtractor> logger,
            IConfiguration configuration)
        {
            _mappingService = mappingService;
            _dataExtractor = dataExtractor;
            _transformer = transformer;
            _validator = validator;
            _repository = repository;
            _syncTrackingRepository = syncTrackingRepository;
            _logger = logger;
            //_facilityId = configuration.GetSection("EnvironmentDefaults")["FacilityId"];
            var envDefaults = configuration.GetSection("EnvironmentDefaults").Get<EnvironmentDefaults>();

            const string prefix = "Organization/";
            string? rawReference = envDefaults?.ManagingOrganization?.Reference;

            // Check if the reference exists and starts with the expected prefix
            if (string.IsNullOrWhiteSpace(rawReference) || !rawReference.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                var errorMessage = $"Extraction startup failed: 'EnvironmentDefaults:ManagingOrganization.Reference' is missing, empty, or does not start with '{prefix}'. Cannot determine Facility ID.";
                logger.LogError(errorMessage);
                throw new ApplicationException(errorMessage);
            }
            string potentialFacilityId = rawReference.Substring(prefix.Length);
            if (!Guid.TryParse(potentialFacilityId, out _))
            {
                var errorMessage = $"Extraction startup failed: Extracted Facility ID '{potentialFacilityId}' from '{rawReference}' is not a valid GUID format. If a GUID is strictly required, please correct the configuration.";
                logger.LogError(errorMessage);
            }

            _facilityId = potentialFacilityId;
        }

        public async Task ExtractAndPersistAsync(CancellationToken cancellationToken = default)
        {
            var correlationId = Guid.NewGuid().ToString();
            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                _logger.LogInformation("Started extraction with CorrelationId {CorrelationId}", correlationId);

                var mapping = await _mappingService.GetMappingForResourceAsync("Patient", cancellationToken);
                _logger.LogInformation("Starting extraction for resource {Resource}", mapping.ResourceType);

                var rawRows = await _dataExtractor.ExtractAsync(mapping, cancellationToken);
                _logger.LogInformation("Extracted {Count} rows from source table {Table}", rawRows.Count(), mapping.TableName);

                foreach (var row in rawRows)
                {
                    var sourceId = row["patient_id"]?.ToString();
                    var lastUpdated = row.TryGetValue("created_at", out var u) ? DateTime.Parse(u?.ToString()!) : (DateTime?)null;
                    var rowHash = ComputeRowHash(row);

                    if (string.IsNullOrWhiteSpace(sourceId))
                    {
                        _logger.LogWarning("Skipping row with null or empty ID");
                        continue;
                    }

                    if (await _syncTrackingRepository.ExistsAsync("Patient", sourceId, cancellationToken))
                    {
                        _logger.LogInformation("Skipping already tracked record {SourceId}", sourceId);
                        continue;
                    }

                    var tracking = new SyncTracking
                    {
                        ResourceType = "Patient",
                        SourceId = sourceId,
                        SourceHash = rowHash,
                        LastUpdated = lastUpdated,
                        CreatedAt = DateTime.UtcNow,
                        RetryCount = 0
                    };

                    try
                    {
                        var errors = new List<string>();
                        var json = _transformer.Transform(row, mapping, errors);
                        var normalizedjson = _transformer.NormalizeEnumFields(json); // Apply safe enum casing fix

                        if (errors.Any())
                        {
                            var errorMessage = string.Join("; ", errors);
                            _logger.LogWarning("Skipping record {SourceId} due to missing required fields: {Errors}", sourceId, errorMessage);

                            await _syncTrackingRepository.AddOrUpdateAsync(new SyncTracking
                            {
                                ResourceType = "Patient",
                                SourceId = sourceId,
                                SourceHash = rowHash,
                                LastUpdated = lastUpdated,
                                ExtractStatus = "Failed",
                                RetryCount = 0,
                                ErrorMessage = errorMessage,
                                CreatedAt = DateTime.UtcNow,
                                LastAttemptAt = DateTime.UtcNow,
                               
                            }, cancellationToken);

                            continue; // Skip persistence
                        }

                        var record = new PatientSyncRecord
                        {
                            ResourceId = sourceId,
                            FhirJson = BsonDocument.Parse(normalizedjson.ToJsonString()),
                            CreatedDate = DateTime.UtcNow,
                            Status = "Pending",
                            LastAttemptAt = DateTime.UtcNow,
                            ExtractSource = "extractor",
                            RetryCount = 0,
                            TransactionId = null,
                            ApiResponsePayload = null, // Initially null, will be updated after sync
                            SyncedResourceId = null, // Initially null, will be updated after sync
                            FacilityId = _facilityId,
                        };

                        string formattedJson = JsonSerializer.Serialize(normalizedjson, new JsonSerializerOptions { WriteIndented = true });
                        _logger.LogInformation("Transformed FHIR JSON:\n{FormattedJson}", formattedJson);

                        var validationResult = await _validator.ValidateAsync(normalizedjson);
                        _logger.LogInformation("Validation result for record {SourceId}: {IsValid}", sourceId, validationResult.IsValid);
                        if (!validationResult.IsValid)
                        {
                            _logger.LogWarning("❌ Validation failed. Errors: {Errors}", string.Join("; ", validationResult.Errors));

                            var errorMessage = string.Join("; ", validationResult.Errors);
                            _logger.LogWarning("Validation failed for record {SourceId}: {Errors}", sourceId, errorMessage);
                            tracking.ExtractStatus = "Failed";
                            tracking.ErrorMessage = errorMessage;
                            await _syncTrackingRepository.AddOrUpdateAsync(tracking, cancellationToken);
                            continue; // Skip persistence
                        }
                        else
                        {
                            _logger.LogInformation("✅ Validation succeeded for record {SourceId}", sourceId);
                            await _repository.InsertAsync(record, cancellationToken);
                            tracking.ExtractStatus = "Success";
                            _logger.LogInformation("Successfully persisted record {SourceId}", sourceId);
                        }

                       

                        await _syncTrackingRepository.AddOrUpdateAsync(tracking, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Unhandled error processing record {SourceId}", sourceId);

                        await _syncTrackingRepository.AddOrUpdateAsync(new SyncTracking
                        {
                            ResourceType = "Patient",
                            SourceId = sourceId,
                            SourceHash = null,
                            LastUpdated = null,
                            ExtractStatus = "Failed",
                            RetryCount = 0,
                            ErrorMessage = ex.Message,
                            CreatedAt = DateTime.UtcNow,
                            LastAttemptAt = DateTime.UtcNow
                        }, cancellationToken);
                    }
                }

                _logger.LogInformation("Extraction and persistence completed for resource {Resource}", mapping.ResourceType);
            }
        }

        private string ComputeRowHash(IDictionary<string, object> row)
        {
            using var sha256 = SHA256.Create();
            var raw = string.Join("|", row.OrderBy(kvp => kvp.Key).Select(kvp => $"{kvp.Key}:{kvp.Value}"));
            var bytes = Encoding.UTF8.GetBytes(raw);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToHexString(hash); // .NET 5+
        }
    }



}
