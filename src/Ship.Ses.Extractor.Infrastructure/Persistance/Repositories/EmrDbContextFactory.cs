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
            if (!Enum.TryParse<DatabaseType>(dbTypeStr, ignoreCase: true, out var dbType))
                throw new ConfigurationErrorsException($"Invalid database type: {dbTypeStr}");

            var connectionString = dbSettings["ConnectionString"];
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ConfigurationErrorsException("Missing or empty connection string.");
            DbConnection connection;
            switch (dbType)
            {
                case DatabaseType.MySql:
                    var mySqlBuilder = new MySqlConnectionStringBuilder(connectionString);
                    connection = new MySqlConnection(mySqlBuilder.ConnectionString);
                    break;

                case DatabaseType.PostgreSql:
                    var npgsqlBuilder = new NpgsqlConnectionStringBuilder(connectionString);
                    connection = new NpgsqlConnection(npgsqlBuilder.ConnectionString);
                    break;

                case DatabaseType.MsSql:
                    var sqlBuilder = new SqlConnectionStringBuilder(connectionString);
                    connection = new SqlConnection(sqlBuilder.ConnectionString);
                    break;

                default:
                    throw new ArgumentException($"Unsupported database type: {dbType}");
            }
            return connection;

        }

        public DbContext CreateDbContext()
        {
            var dbSettings = _configuration.GetSection("EmrDatabase");

            // Validate and parse the database type
            var dbTypeStr = dbSettings["Type"];
            if (!Enum.TryParse<DatabaseType>(dbTypeStr, ignoreCase: true, out var dbType))
            {
                throw new ConfigurationErrorsException($"Invalid database type: {dbTypeStr}");
            }

            // Validate connection string
            var connectionString = dbSettings["ConnectionString"];
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ConfigurationErrorsException("Missing or empty database connection string.");
            }

            var optionsBuilder = new DbContextOptionsBuilder<DbContext>();

            switch (dbType)
            {
                case DatabaseType.MySql:
                    optionsBuilder.UseMySQL(connectionString);
                    break;

                case DatabaseType.PostgreSql:
                    optionsBuilder.UseNpgsql(connectionString);
                    break;

                case DatabaseType.MsSql:
                    optionsBuilder.UseSqlServer(connectionString);
                    break;

                default:
                    throw new ConfigurationErrorsException($"Unsupported database type: {dbType}");
            }

            return new DbContext(optionsBuilder.Options);
        }

    }
}
