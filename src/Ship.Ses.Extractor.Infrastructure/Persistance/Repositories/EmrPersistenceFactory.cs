using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver.Core.Configuration;
using MySql.Data.MySqlClient;
using Npgsql;
using Ship.Ses.Extractor.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Configuration.Provider;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Ship.Ses.Extractor.Infrastructure.Persistance.Repositories
{
    public class EmrPersistenceFactory
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmrPersistenceFactory> _logger;

        public EmrPersistenceFactory(IConfiguration configuration, ILogger<EmrPersistenceFactory> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public DbConnection MakeConn()
        {
            var dbSection = _configuration.GetSection("EmrDatabase");
            if (!dbSection.Exists())
                throw new ConfigurationErrorsException("Missing EmrDatabase configuration section.");

            // Determine DB type
            var dbTypeStr = dbSection["Type"];
            if (!Enum.TryParse<DatabaseType>(dbTypeStr, true, out var dbType))
                throw new ConfigurationErrorsException($"Invalid or missing database type: {dbTypeStr}");

            // Try Vault-style (explicit credentials)
            var server = dbSection["Server"];
            var database = dbSection["Database"];
            var username = dbSection["Username"];
            var password = dbSection["Password"];
            var portStr = dbSection["Port"];
            int.TryParse(portStr, out var port);

            if (!string.IsNullOrWhiteSpace(server) && !string.IsNullOrWhiteSpace(username) &&
                !string.IsNullOrWhiteSpace(password) && !string.IsNullOrWhiteSpace(database))
            {
                _logger.LogInformation("🔐 Using explicit credentials from Vault configuration.");

                return dbType switch
                {
                    DatabaseType.MySql => new MySqlConnection(new MySqlConnectionStringBuilder
                    {
                        Server = ValidateHost(server),
                        Database = database,
                        UserID = username,
                        Password = password,
                        Port = ValidatePort(port),
                        SslMode = MySqlSslMode.Preferred
                    }.ConnectionString),

                    DatabaseType.PostgreSql => new NpgsqlConnection(new NpgsqlConnectionStringBuilder
                    {
                        Host = ValidateHost(server),
                        Database = database,
                        Username = username,
                        Password = password,
                        Port = port == 0 ? 5432 : port,
                        SslMode = SslMode.Prefer
                    }.ConnectionString),

                    DatabaseType.MsSql => new SqlConnection(new SqlConnectionStringBuilder
                    {
                        DataSource = $"{ValidateHost(server)},{ValidatePort(port)}",
                        InitialCatalog = database,
                        UserID = username,
                        Password = password,
                        Encrypt = true,
                        TrustServerCertificate = true
                    }.ConnectionString),

                    _ => throw new ConfigurationErrorsException($"Unsupported database type: {dbType}")
                };
            }

            //Try connection string mode
            var rawConnStr = dbSection["ConnectionString"];
            if (string.IsNullOrWhiteSpace(rawConnStr))
                throw new ConfigurationErrorsException("Missing connection string and no Vault credentials supplied.");

            _logger.LogInformation("🔑 Using fallback connection string from appsettings.");

            return dbType switch
            {
                DatabaseType.MySql => new MySqlConnection(new MySqlConnectionStringBuilder(rawConnStr).ConnectionString),
                DatabaseType.PostgreSql => new NpgsqlConnection(new NpgsqlConnectionStringBuilder(rawConnStr).ConnectionString),
                DatabaseType.MsSql => new SqlConnection(new SqlConnectionStringBuilder(rawConnStr).ConnectionString),
                _ => throw new ConfigurationErrorsException($"Unsupported database type: {dbType}")
            };
        }

        public DbContext CreatePersistenceContext()
        {
            var dbSection = _configuration.GetSection("EmrDatabase");
            var dbTypeStr = dbSection["Type"];
            if (!Enum.TryParse<DatabaseType>(dbTypeStr, true, out var dbType))
                throw new ConfigurationErrorsException($"Invalid database type: {dbTypeStr}");

            var conn = MakeConn();
            var optionsBuilder = new DbContextOptionsBuilder<DbContext>();

            switch (dbType)
            {
                case DatabaseType.MySql:
                    optionsBuilder.UseMySQL(conn);
                    break;
                case DatabaseType.PostgreSql:
                    optionsBuilder.UseNpgsql(conn);
                    break;
                case DatabaseType.MsSql:
                    optionsBuilder.UseSqlServer(conn);
                    break;
                default:
                    throw new ConfigurationErrorsException($"Unsupported database type: {dbType}");
            }

            return new DbContext(optionsBuilder.Options);
        }

        private string ValidateHost(string host)
        {
            if (string.IsNullOrWhiteSpace(host) || host.Contains("localhost", StringComparison.OrdinalIgnoreCase))
                throw new ConfigurationErrorsException("Untrusted or insecure host specified.");
            return host;
        }

        private uint ValidatePort(int port) =>
            port > 0 && port <= 65535 ? (uint)port : throw new ArgumentOutOfRangeException(nameof(port));
    }


}
