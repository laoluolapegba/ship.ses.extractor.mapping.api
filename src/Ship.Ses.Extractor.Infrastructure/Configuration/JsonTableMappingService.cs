using DnsClient.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ship.Ses.Extractor.Application.Services;
using Ship.Ses.Extractor.Domain.Models.Extractor;
using Ship.Ses.Extractor.Infrastructure.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Ship.Ses.Extractor.Infrastructure.Configuration
{
    public class JsonTableMappingService : ITableMappingService
    {
        private readonly string _rootPath;
        private readonly ILogger<JsonTableMappingService> _logger;
        private readonly Dictionary<string, TableMapping> _mappings;

        public JsonTableMappingService(IOptions<AppSettings> appSettings, IHostEnvironment env, ILogger<JsonTableMappingService> logger)
        {
            var configuredPath = appSettings.Value.TableMappings?.RootPath
                ?? throw new InvalidOperationException("TableMappings:RootPath not configured in AppSettings");

            // Combine relative path with content root (project directory)
            _rootPath = Path.Combine(env.ContentRootPath, configuredPath);
            _logger = logger;
            if (!Directory.Exists(_rootPath))
                throw new DirectoryNotFoundException($"Mapping root directory does not exist: {_rootPath}");

            _mappings = new Dictionary<string, TableMapping>(StringComparer.OrdinalIgnoreCase);

            foreach (var file in Directory.GetFiles(_rootPath, "*.mapping.json"))
            {
                var json = File.ReadAllText(file);
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var mapping = JsonSerializer.Deserialize<TableMapping>(json, options)
                              ?? throw new InvalidOperationException($"Failed to deserialize mapping: {file}");

                _logger.LogInformation("Loaded mapping for resource: {ResourceType}, table: {Table}, fields: {Count}",
    mapping.ResourceType, mapping.TableName, mapping.Fields.Count);

                ValidateMapping(mapping, file);
                _mappings[mapping.ResourceType] = mapping;
            }
        }


        public Task<TableMapping> GetMappingForResourceAsync(string resourceType, CancellationToken cancellationToken = default)
        {
            if (_mappings.TryGetValue(resourceType, out var mapping))
            {
                return Task.FromResult(mapping);
            }

            throw new FileNotFoundException($"Mapping for resource '{resourceType}' not found in {_rootPath}");
        }
        private void ValidateMapping(TableMapping mapping, string fileName)
        {
            if (string.IsNullOrWhiteSpace(mapping.ResourceType))
                throw new InvalidOperationException($"Missing 'resourceType' in mapping file: {fileName}");

            if (string.IsNullOrWhiteSpace(mapping.TableName))
                throw new InvalidOperationException($"Missing 'tableName' in mapping file: {fileName}");

            if (mapping.Fields == null || mapping.Fields.Count == 0)
                throw new InvalidOperationException($"Mapping file '{fileName}' must contain at least one field mapping");

            foreach (var field in mapping.Fields)
            {
                if (string.IsNullOrWhiteSpace(field.FhirPath))
                    throw new InvalidOperationException($"A field in '{fileName}' has an empty 'fhirPath'");

                // ✅ Skip further checks for template-based fields
                if (!string.IsNullOrWhiteSpace(field.Template))
                    continue;

                var isEmptyStringMarker = field.EmrField?.Trim() == "__empty__";
                var isConstant = mapping.Constants.ContainsKey(field.FhirPath);

                if (string.IsNullOrWhiteSpace(field.EmrField) && !isConstant && !isEmptyStringMarker)
                {
                    throw new InvalidOperationException(
                        $"❌ The mapping for FHIR path '{field.FhirPath}' in '{fileName}' has no 'emrField', is not a constant, and is not marked for empty string.");
                }
            }
        }
        private void ValidateMappingold(TableMapping mapping, string fileName)
        {
            if (string.IsNullOrWhiteSpace(mapping.ResourceType))
                throw new InvalidOperationException($"Missing 'resourceType' in mapping file: {fileName}");

            if (string.IsNullOrWhiteSpace(mapping.TableName))
                throw new InvalidOperationException($"Missing 'tableName' in mapping file: {fileName}");

            if (mapping.Fields == null || mapping.Fields.Count == 0)
                throw new InvalidOperationException($"Mapping file '{fileName}' must contain at least one field mapping");

            foreach (var field in mapping.Fields)
            {
                var isEmptyStringMarker = field.EmrField?.Trim() == "__empty__";
                var isConstant = mapping.Constants.ContainsKey(field.FhirPath);

                if (string.IsNullOrWhiteSpace(field.EmrField) && !isConstant && !isEmptyStringMarker)
                {
                    throw new InvalidOperationException(
                        $"❌ The mapping for FHIR path '{field.FhirPath}' in '{fileName}' has no 'emrField', is not a constant, and is not marked for empty string.");
                }
                

                if (string.IsNullOrWhiteSpace(field.FhirPath))
                    throw new InvalidOperationException($"A field in '{fileName}' has an empty 'fhirPath'");
            }
        }
        public async Task<TableMapping> GetMappingForResourceAsync1(string resourceType, CancellationToken cancellationToken = default)
        {
            var filePath = Path.Combine(_rootPath, $"{resourceType.ToLowerInvariant()}.mapping.json");

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Mapping file not found: {filePath}");

            var json = await File.ReadAllTextAsync(filePath, cancellationToken);
            return JsonSerializer.Deserialize<TableMapping>(json)
                   ?? throw new InvalidOperationException($"Invalid mapping JSON: {filePath}");
        }
    }


}
