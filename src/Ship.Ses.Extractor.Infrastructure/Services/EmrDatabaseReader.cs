using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ship.Ses.Extractor.Infrastructure.Services
{
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Logging;
    using MySql.Data.MySqlClient;
    using Npgsql;
    using Ship.Ses.Extractor.Application.Services;
    using Ship.Ses.Extractor.Application.Services.DataMapping;
    using Ship.Ses.Extractor.Domain.Entities.DataMapping;
    using Ship.Ses.Extractor.Domain.ValueObjects;
    using Ship.Ses.Extractor.Infrastructure.Persistance.Repositories;
    using Ship.Ses.Extractor.Shared.Enums;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Linq;
    using System.Threading.Tasks;

    public class EmrDatabaseReader : IEmrDatabaseReader
    {
        private readonly EmrDbContextFactory _dbContextFactory;
        private readonly ILogger<EmrDatabaseReader> _logger;
        private readonly EmrConnection _connection;
        public EmrDatabaseReader(EmrDbContextFactory dbContextFactory, ILogger<EmrDatabaseReader> logger)
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;
        }
        public EmrDatabaseReader(EmrConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        
        public async Task<IEnumerable<string>> GetTableNamesAsync()
        {
            var tables = new List<string>();

            using var connection = CreateConnection();
            await connection.OpenAsync();

            // Query depends on the database type
            string query = GetTableNamesQuery();

            using var command = connection.CreateCommand();
            command.CommandText = query;

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                tables.Add(reader.GetString(0));
            }

            return tables;
        }

        public async Task<TableSchema> GetTableSchemaAsync(string tableName)
        {
            var columns = new List<ColumnSchema>();

            using var connection = CreateConnection();
            await connection.OpenAsync();

            // Get primary key columns
            var primaryKeyColumns = await GetPrimaryKeyColumnsAsync(connection, tableName);

            // Query depends on the database type
            string query = GetColumnSchemaQuery(tableName);

            using var command = connection.CreateCommand();
            command.CommandText = query;

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var columnName = reader.GetString(reader.GetOrdinal("COLUMN_NAME"));
                var dataType = reader.GetString(reader.GetOrdinal("DATA_TYPE"));
                var isNullable = reader.GetString(reader.GetOrdinal("IS_NULLABLE")) == "YES";
                var isPrimaryKey = primaryKeyColumns.Contains(columnName);

                columns.Add(new ColumnSchema(columnName, dataType, isNullable, isPrimaryKey));
            }

            return new TableSchema(tableName, columns);
        }
        private string GetTableNamesQuery()
        {
            return _connection.DatabaseType switch
            {
                DatabaseType.MySql =>
                    $"SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '{_connection.DatabaseName}' AND TABLE_TYPE = 'BASE TABLE'",

                DatabaseType.PostgreSql =>
                    "SELECT tablename FROM pg_catalog.pg_tables WHERE schemaname = 'public'",

                DatabaseType.MsSql =>
                    "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'",

                _ => throw new NotSupportedException($"Database type {_connection.DatabaseType} is not supported")
            };
        }
        private string GetColumnSchemaQuery(string tableName)
        {
            return _connection.DatabaseType switch
            {
                DatabaseType.MySql =>
                    $"SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = '{_connection.DatabaseName}' AND TABLE_NAME = '{tableName}'",

                DatabaseType.PostgreSql =>
                    $"SELECT column_name as COLUMN_NAME, data_type as DATA_TYPE, is_nullable as IS_NULLABLE FROM information_schema.columns WHERE table_schema = 'public' AND table_name = '{tableName}'",

                DatabaseType.MsSql =>
                    $"SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{tableName}'",

                _ => throw new NotSupportedException($"Database type {_connection.DatabaseType} is not supported")
            };
        }

        private async Task<HashSet<string>> GetPrimaryKeyColumnsAsync(DbConnection connection, string tableName)
        {
            var primaryKeys = new HashSet<string>();

            string query = _connection.DatabaseType switch
            {
                DatabaseType.MySql =>
                    $"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE TABLE_SCHEMA = '{_connection.DatabaseName}' AND TABLE_NAME = '{tableName}' AND CONSTRAINT_NAME = 'PRIMARY'",

                DatabaseType.PostgreSql =>
                    $@"SELECT a.attname as COLUMN_NAME
                       FROM pg_index i
                       JOIN pg_attribute a ON a.attrelid = i.indrelid AND a.attnum = ANY(i.indkey)
                       WHERE i.indrelid = '{tableName}'::regclass AND i.indisprimary",

                DatabaseType.MsSql =>
                    $@"SELECT c.name as COLUMN_NAME
                       FROM sys.indexes i
                       INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
                       INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
                       INNER JOIN sys.tables t ON i.object_id = t.object_id
                       WHERE i.is_primary_key = 1 AND t.name = '{tableName}'",

                _ => throw new NotSupportedException($"Database type {_connection.DatabaseType} is not supported")
            };

            using var command = connection.CreateCommand();
            command.CommandText = query;

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                primaryKeys.Add(reader.GetString(0));
            }

            return primaryKeys;
        }
        public async Task TestConnectionAsync()
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();
            // If we get here, the connection is successful
        }

        private DbConnection CreateConnection()
        {
            return _connection.DatabaseType switch
            {
                DatabaseType.MySql => new MySqlConnection(_connection.GetConnectionString()),
                DatabaseType.PostgreSql => new NpgsqlConnection(_connection.GetConnectionString()),
                DatabaseType.MsSql => new SqlConnection(_connection.GetConnectionString()),
                _ => throw new NotSupportedException($"Database type {_connection.DatabaseType} is not supported")
            };
        }
        public async Task<TableSchema> GetTableSchemaAsync1(string tableName)
        {
            try
            {
                using var connection = _dbContextFactory.CreateConnection();
                await connection.OpenAsync();

                var columns = await GetColumnsForTable(connection, tableName);
                return new TableSchema(tableName, columns);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving schema for table {TableName}", tableName);
                throw;
            }
        }

        public async Task TestConnectionAsync1()
        {
            try
            {
                using var connection = _dbContextFactory.CreateConnection();
                await connection.OpenAsync();
                _logger.LogInformation("Successfully connected to EMR database");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to EMR database");
                throw;
            }
        }
        
        private async Task<List<string>> GetTableNamesForConnectionType(DbConnection connection)
        {
            var tables = new List<string>();

            if (connection is MySqlConnection)
            {
                var schema = connection.Database;
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT table_name FROM information_schema.tables WHERE table_schema = @schema AND table_type = 'BASE TABLE'";
                var parameter = command.CreateParameter();
                parameter.ParameterName = "@schema";
                parameter.Value = schema;
                command.Parameters.Add(parameter);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    tables.Add(reader.GetString(0));
                }
            }
            else if (connection is Npgsql.NpgsqlConnection)
            {
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' AND table_type = 'BASE TABLE'";

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    tables.Add(reader.GetString(0));
                }
            }
            else if (connection is Microsoft.Data.SqlClient.SqlConnection)
            {
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT table_name FROM information_schema.tables WHERE table_type = 'BASE TABLE'";

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    tables.Add(reader.GetString(0));
                }
            }

            return tables;
        }
        public async Task<IEnumerable<string>> GetTableNamesAsync1()
        {
            var tables = new List<string>();

            try
            {
                using var connection = _dbContextFactory.CreateConnection();
                await connection.OpenAsync();

                tables = await GetTableNamesForConnectionType(connection);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving database tables");
                throw;
            }

            return tables;
        }
        private async Task<List<ColumnSchema>> GetColumnsForTable(DbConnection connection, string tableName)
        {
            var columns = new List<ColumnSchema>();

            if (connection is MySqlConnection)
            {
                var schema = connection.Database;
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT 
                        column_name, 
                        data_type,
                        is_nullable,
                        CASE WHEN column_key = 'PRI' THEN 1 ELSE 0 END as is_primary_key
                    FROM 
                        information_schema.columns 
                    WHERE 
                        table_schema = @schema 
                        AND table_name = @tableName
                    ORDER BY 
                        ordinal_position";

                var schemaParam = command.CreateParameter();
                schemaParam.ParameterName = "@schema";
                schemaParam.Value = schema;
                command.Parameters.Add(schemaParam);

                var tableParam = command.CreateParameter();
                tableParam.ParameterName = "@tableName";
                tableParam.Value = tableName;
                command.Parameters.Add(tableParam);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    columns.Add(new ColumnSchema(
                        reader.GetString(0),
                        reader.GetString(1),
                        reader.GetString(2).Equals("YES", StringComparison.OrdinalIgnoreCase),
                        reader.GetInt32(3) == 1));
                }
            }
            else if (connection is Npgsql.NpgsqlConnection)
            {
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT 
                        column_name, 
                        data_type,
                        is_nullable,
                        CASE WHEN pk.column_name IS NOT NULL THEN true ELSE false END as is_primary_key
                    FROM 
                        information_schema.columns c
                    LEFT JOIN (
                        SELECT kcu.column_name 
                        FROM information_schema.table_constraints tc
                        JOIN information_schema.key_column_usage kcu 
                        ON tc.constraint_name = kcu.constraint_name
                        WHERE tc.constraint_type = 'PRIMARY KEY' 
                        AND tc.table_name = @tableName
                    ) pk ON c.column_name = pk.column_name
                    WHERE 
                        c.table_schema = 'public' 
                        AND c.table_name = @tableName
                    ORDER BY 
                        c.ordinal_position";

                var parameter = command.CreateParameter();
                parameter.ParameterName = "@tableName";
                parameter.Value = tableName;
                command.Parameters.Add(parameter);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    columns.Add(new ColumnSchema(
                        reader.GetString(0),
                        reader.GetString(1),
                        reader.GetString(2).Equals("YES", StringComparison.OrdinalIgnoreCase),
                        reader.GetBoolean(3)));
                }
            }
            else if (connection is Microsoft.Data.SqlClient.SqlConnection)
            {
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT 
                        c.COLUMN_NAME, 
                        c.DATA_TYPE,
                        c.IS_NULLABLE,
                        CASE WHEN pk.COLUMN_NAME IS NOT NULL THEN 1 ELSE 0 END as is_primary_key
                    FROM 
                        INFORMATION_SCHEMA.COLUMNS c
                    LEFT JOIN (
                        SELECT ku.COLUMN_NAME
                        FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS tc
                        INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS ku
                        ON tc.CONSTRAINT_NAME = ku.CONSTRAINT_NAME
                        WHERE tc.CONSTRAINT_TYPE = 'PRIMARY KEY' 
                        AND ku.TABLE_NAME = @tableName
                    ) pk ON c.COLUMN_NAME = pk.COLUMN_NAME
                    WHERE 
                        c.TABLE_NAME = @tableName
                    ORDER BY 
                        c.ORDINAL_POSITION";

                var parameter = command.CreateParameter();
                parameter.ParameterName = "@tableName";
                parameter.Value = tableName;
                command.Parameters.Add(parameter);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    columns.Add(new ColumnSchema(
                        reader.GetString(0),
                        reader.GetString(1),
                        reader.GetString(2).Equals("YES", StringComparison.OrdinalIgnoreCase),
                        reader.GetInt32(3) == 1));
                }
            }

            return columns;
        }
    }
}
