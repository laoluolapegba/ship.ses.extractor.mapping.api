using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using MySql.Data.MySqlClient;
using Ship.Ses.Extractor.Infrastructure.Settings;
using Ship.Ses.Extractor.Shared.Enums;
using System;
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
            var dbType = Enum.Parse<DatabaseType>(dbSettings["Type"]);
            var connectionString = dbSettings["ConnectionString"];

            return dbType switch
            {
                DatabaseType.MySql => new MySqlConnection(connectionString),
                DatabaseType.PostgreSql => new Npgsql.NpgsqlConnection(connectionString),
                DatabaseType.MsSql => new Microsoft.Data.SqlClient.SqlConnection(connectionString),
                _ => throw new ArgumentException($"Unsupported database type: {dbType}")
            };
        }

        public DbContext CreateDbContext()
        {
            var dbSettings = _configuration.GetSection("EmrDatabase");

            var dbType = Enum.Parse<DatabaseType>(dbSettings["Type"]);
            var connectionString = dbSettings["ConnectionString"];

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
                    throw new ArgumentException($"Unsupported database type: {dbType}");
            }

            return new DbContext(optionsBuilder.Options);
        }
    }
}
