using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using MySql.Data.MySqlClient;
using Npgsql;
using Ship.Ses.Extractor.Infrastructure.Settings;
using Ship.Ses.Extractor.Shared.Enums;
using System;
using System.Configuration;
using System.Data.Common;
namespace Ship.Ses.Extractor.Infrastructure.Persistance.Repositories
{
    

    public class EmrDbContextFactory
    {
        private readonly IConfiguration _configuration;

        public EmrDbContextFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public DbConnection CreateConnection()
        {
            var dbSettings = _configuration.GetSection("EmrDatabase");

            var dbTypeStr = dbSettings["Type"];
            if (!Enum.TryParse<DatabaseType>(dbTypeStr, true, out var dbType))
                throw new ConfigurationErrorsException($"Invalid database type: {dbTypeStr}");

            var server = dbSettings["Server"];
            var database = dbSettings["DatabaseName"];
            var username = dbSettings["Username"];
            var password = dbSettings["Password"];
            var portStr = dbSettings["Port"];
            int.TryParse(portStr, out var port);

            return dbType switch
            {
                DatabaseType.MySql => new MySqlConnection(new MySqlConnectionStringBuilder
                {
                    Server = server,
                    Database = database,
                    UserID = username,
                    Password = password,
                    Port = (uint)(port == 0 ? 3306 : port),
                    SslMode = MySqlSslMode.Preferred
                }.ConnectionString),

                DatabaseType.PostgreSql => new NpgsqlConnection(new NpgsqlConnectionStringBuilder
                {
                    Host = server,
                    Database = database,
                    Username = username,
                    Password = password,
                    Port = (port == 0 ? 5432 : port),
                    SslMode = SslMode.Prefer
                }.ConnectionString),

                DatabaseType.MsSql => new SqlConnection(new SqlConnectionStringBuilder
                {
                    DataSource = $"{server},{(port == 0 ? 1433 : port)}",
                    InitialCatalog = database,
                    UserID = username,
                    Password = password,
                    Encrypt = true,
                    TrustServerCertificate = true
                }.ConnectionString),

                _ => throw new ConfigurationErrorsException($"Unsupported database type: {dbType}")
            };

        }

        public DbContext CreateDbContext()
        {
            var dbSettings = _configuration.GetSection("EmrDatabase");

            var dbTypeStr = dbSettings["Type"];
            if (!Enum.TryParse<DatabaseType>(dbTypeStr, true, out var dbType))
                throw new ConfigurationErrorsException($"Invalid database type: {dbTypeStr}");

            var conn = CreateConnection();
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

    }
}
