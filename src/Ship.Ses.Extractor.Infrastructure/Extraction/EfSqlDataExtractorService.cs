using DnsClient.Protocol;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ship.Ses.Extractor.Application.Services;
using Ship.Ses.Extractor.Domain.Models.Extractor;
using Ship.Ses.Extractor.Infrastructure.Persistance.Contexts;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ship.Ses.Extractor.Infrastructure.Extraction
{
    public class EfSqlDataExtractorService : IDataExtractorService
    {
        private readonly ExtractorDbContext _context;
        private readonly ILogger<EfSqlDataExtractorService> _logger;

        public EfSqlDataExtractorService(ExtractorDbContext context, ILogger<EfSqlDataExtractorService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<IDictionary<string, object>>> ExtractAsync(TableMapping mapping, CancellationToken cancellationToken = default)
        {
            var results = new List<IDictionary<string, object>>();
            var tableName = mapping.TableName;
            var resourceType = mapping.ResourceType;
            var sourceIdColumn = "patient_id"; // You can make this configurable later

            string countSql = $@"
        SELECT COUNT(*) 
        FROM {tableName} p
        LEFT JOIN ses_extract_tracking s 
            ON s.resource_type = @ResourceType AND s.source_id = p.{sourceIdColumn}
        WHERE s.source_id IS NULL AND p.extracted_flag = 'N'";

            try
            {
                await using var connection = _context.Database.GetDbConnection();
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync(cancellationToken);
                    _logger.LogDebug("🔌 Opened database connection to {DataSource}", connection.DataSource);
                }

                await using (var countCmd = connection.CreateCommand())
                {
                    countCmd.CommandText = countSql;

                    var param = countCmd.CreateParameter();
                    param.ParameterName = "@ResourceType";
                    param.Value = resourceType;
                    countCmd.Parameters.Add(param);

                    var countResult = await countCmd.ExecuteScalarAsync(cancellationToken);
                    int rowCount = Convert.ToInt32(countResult);

                    if (rowCount == 0)
                    {
                        _logger.LogInformation("⏳ No new unsynced rows found in '{TableName}' for resource '{ResourceType}'", tableName, resourceType);
                        await Task.Delay(TimeSpan.FromSeconds(180), cancellationToken);
                        return results;
                    }

                    _logger.LogInformation("📥 Found {Count} new rows in '{TableName}' for '{ResourceType}'", rowCount, tableName, resourceType);
                }

                var sql = $"SELECT * FROM {tableName} WHERE extracted_flag = 'N'";

                await using var command = connection.CreateCommand();
                command.CommandText = sql;

                await using var reader = await command.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                {
                    var row = new Dictionary<string, object>();

                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        var columnName = reader.GetName(i);

                        try
                        {
                            var value = await reader.IsDBNullAsync(i, cancellationToken)
                                ? null
                                : reader.GetValue(i);
                            row[columnName] = value;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex,
                                "❌ Error reading column '{Column}' at index {Index} in table '{TableName}'. Skipping column.",
                                columnName, i, tableName);
                            row[columnName] = null;
                        }
                    }

                    results.Add(row);
                }

                _logger.LogInformation("📦 Extracted {Count} rows from '{TableName}'", results.Count, tableName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to extract from table '{TableName}'", mapping.TableName);
                throw;
            }

            return results;
        }

    }

}
