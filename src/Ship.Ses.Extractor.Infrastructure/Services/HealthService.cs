using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging;
using Ship.Ses.Extractor.Application.Interfaces;
using Ship.Ses.Extractor.Domain.Entities.DataMapping;
using Ship.Ses.Extractor.Infrastructure.Persistance.Repositories;
using System.Data.Common;
using System.Threading.Tasks;

namespace Ship.Ses.Extractor.Infrastructure.Services
{


    public class HealthService : IHealthService
    {
        private readonly ILogger<HealthService> _logger;
        private readonly EmrPersistenceFactory _dbContextFactory;
        private readonly string _connectionString;
        private readonly string _dbType;

        public HealthService(IConfiguration configuration, ILogger<HealthService> logger, EmrPersistenceFactory dbContextFactory)
        {
            _logger = logger;
            //_connectionString = configuration["EmrDatabase:ConnectionString"];
            //_dbType = configuration["EmrDatabase:Type"] ?? "Unknown";
            _dbContextFactory = dbContextFactory;
        }

        public async Task<HealthResult> CheckHealthAsync()
        {
            try
            {
                //using var connection = CreateDbConnection();
                using var connection = _dbContextFactory.MakeConn();
                await connection.OpenAsync();
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT 1";
                await command.ExecuteScalarAsync();

                return HealthResult.Healthy();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Health check failed for DB");
                return HealthResult.Unhealthy($"DB connection failed: {ex.Message}");
            }
        }

        private DbConnection CreateDbConnection()
        {
            return _dbType.ToLowerInvariant() switch
            {
                "mysql" => new MySql.Data.MySqlClient.MySqlConnection(_connectionString),
                "postgresql" => new Npgsql.NpgsqlConnection(_connectionString),
                "mssql" => new System.Data.SqlClient.SqlConnection(_connectionString),
                _ => throw new NotSupportedException($"Unsupported DB type: {_dbType}")
            };
        }
    }




}
