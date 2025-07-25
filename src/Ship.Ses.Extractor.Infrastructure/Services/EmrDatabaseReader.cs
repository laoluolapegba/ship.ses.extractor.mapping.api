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
    using System.Configuration;
    using System.Data;
    using System.Data.Common;
    using System.Linq;
    using System.Threading.Tasks;

    public class EmrDatabaseReader : IEmrDatabaseReader
    {
        private readonly EmrPersistenceFactory _dbContextFactory;
        private readonly ILogger<EmrDatabaseReader> _logger;
        private readonly EmrConnection _connection;
        public EmrDatabaseReader(EmrPersistenceFactory dbContextFactory, ILogger<EmrDatabaseReader> logger)
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

            using var connection = MakeConn();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();

            if (_connection.DatabaseType == DatabaseType.MySql)
            {
                command.CommandText = @"SELECT TABLE_NAME 
                                FROM INFORMATION_SCHEMA.TABLES 
                                WHERE TABLE_SCHEMA = @schema 
                                  AND TABLE_TYPE = 'BASE TABLE'";
                var param = command.CreateParameter();
                param.ParameterName = "@schema";
                param.Value = _connection.DatabaseName;
                command.Parameters.Add(param);
            }
            else if (_connection.DatabaseType == DatabaseType.PostgreSql)
            {
                command.CommandText = "SELECT tablename FROM pg_catalog.pg_tables WHERE schemaname = 'public'";
            }
            else if (_connection.DatabaseType == DatabaseType.MsSql)
            {
                command.CommandText = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'";
            }
            else
            {
                throw new NotSupportedException($"Database type {_connection.DatabaseType} is not supported");
            }

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

            using var connection = MakeConn();
            await connection.OpenAsync();

            var primaryKeyColumns = await GetPrimaryKeyColumnsAsync(connection, tableName);

            using var command = connection.CreateCommand();

            if (_connection.DatabaseType == DatabaseType.MySql)
            {
                command.CommandText = @"
            SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE 
            FROM INFORMATION_SCHEMA.COLUMNS 
            WHERE TABLE_SCHEMA = @schema 
              AND TABLE_NAME = @table";

                command.Parameters.Add(CreateParam(command, "@schema", _connection.DatabaseName));
                command.Parameters.Add(CreateParam(command, "@table", tableName));
            }
            else if (_connection.DatabaseType == DatabaseType.PostgreSql)
            {
                command.CommandText = @"
            SELECT column_name as COLUMN_NAME, data_type as DATA_TYPE, is_nullable as IS_NULLABLE 
            FROM information_schema.columns 
            WHERE table_schema = 'public' 
              AND table_name = @table";
                command.Parameters.Add(CreateParam(command, "@table", tableName));
            }
            else if (_connection.DatabaseType == DatabaseType.MsSql)
            {
                command.CommandText = @"
            SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE 
            FROM INFORMATION_SCHEMA.COLUMNS 
            WHERE TABLE_NAME = @table";
                command.Parameters.Add(CreateParam(command, "@table", tableName));
            }

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

        private string GetTableNamesQuery(DbCommand command)
        {
            switch (_connection.DatabaseType)
            {
                case DatabaseType.MySql:
                    command.CommandText = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = @schema AND TABLE_TYPE = 'BASE TABLE'";
                    command.Parameters.Add(CreateParam(command, "@schema", _connection.DatabaseName));
                    break;

                case DatabaseType.PostgreSql:
                    command.CommandText = "SELECT tablename FROM pg_catalog.pg_tables WHERE schemaname = 'public'";
                    break;

                case DatabaseType.MsSql:
                    command.CommandText = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'";
                    break;

                default:
                    throw new NotSupportedException($"Database type {_connection.DatabaseType} is not supported");
            }

            return command.CommandText;
        }

        private string GetColumnSchemaQuery(string tableName, DbCommand command)
        {
            switch (_connection.DatabaseType)
            {
                case DatabaseType.MySql:
                    command.CommandText = @"
                SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE 
                FROM INFORMATION_SCHEMA.COLUMNS 
                WHERE TABLE_SCHEMA = @schema 
                  AND TABLE_NAME = @table";
                    command.Parameters.Add(CreateParam(command, "@schema", _connection.DatabaseName));
                    command.Parameters.Add(CreateParam(command, "@table", tableName));
                    break;

                case DatabaseType.PostgreSql:
                    command.CommandText = @"
                SELECT column_name as COLUMN_NAME, data_type as DATA_TYPE, is_nullable as IS_NULLABLE 
                FROM information_schema.columns 
                WHERE table_schema = 'public' 
                  AND table_name = @table";
                    command.Parameters.Add(CreateParam(command, "@table", tableName));
                    break;

                case DatabaseType.MsSql:
                    command.CommandText = @"
                SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE 
                FROM INFORMATION_SCHEMA.COLUMNS 
                WHERE TABLE_NAME = @table";
                    command.Parameters.Add(CreateParam(command, "@table", tableName));
                    break;

                default:
                    throw new NotSupportedException($"Database type {_connection.DatabaseType} is not supported");
            }

            return command.CommandText;
        }


        private async Task<HashSet<string>> GetPrimaryKeyColumnsAsync(DbConnection connection, string tableName)
        {
            var primaryKeys = new HashSet<string>();
            using var command = connection.CreateCommand();

            if (_connection.DatabaseType == DatabaseType.MySql)
            {
                command.CommandText = @"
            SELECT COLUMN_NAME 
            FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE 
            WHERE TABLE_SCHEMA = @schema 
              AND TABLE_NAME = @table 
              AND CONSTRAINT_NAME = 'PRIMARY'";
                command.Parameters.Add(CreateParam(command, "@schema", _connection.DatabaseName));
                command.Parameters.Add(CreateParam(command, "@table", tableName));
            }
            else if (_connection.DatabaseType == DatabaseType.PostgreSql)
            {
                command.CommandText = @"
            SELECT a.attname as COLUMN_NAME
            FROM pg_index i
            JOIN pg_attribute a ON a.attrelid = i.indrelid AND a.attnum = ANY(i.indkey)
            WHERE i.indrelid = @table::regclass AND i.indisprimary";
                command.Parameters.Add(CreateParam(command, "@table", tableName));
            }
            else if (_connection.DatabaseType == DatabaseType.MsSql)
            {
                command.CommandText = @"
            SELECT c.name as COLUMN_NAME
            FROM sys.indexes i
            INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
            INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
            INNER JOIN sys.tables t ON i.object_id = t.object_id
            WHERE i.is_primary_key = 1 AND t.name = @table";
                command.Parameters.Add(CreateParam(command, "@table", tableName));
            }

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                primaryKeys.Add(reader.GetString(0));
            }

            return primaryKeys;
        }

        public async Task TestConnectionAsync()
        {
            using var connection = MakeConn();
            await connection.OpenAsync();
            // If we get here, the connection is successful
        }

        private DbConnection MakeConn()
        {
            if (_connection == null)
                throw new InvalidOperationException("EMR connection settings not initialized.");

            var connectionString = _connection.GetConnectionString();
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ConfigurationErrorsException("Connection string is null or empty.");

            return _connection.DatabaseType switch
            {
                DatabaseType.MySql => new MySqlConnection(new MySqlConnectionStringBuilder(connectionString)
                {
                    SslMode = MySqlSslMode.Preferred
                }.ConnectionString),

                DatabaseType.PostgreSql => new NpgsqlConnection(new NpgsqlConnectionStringBuilder(connectionString)
                {
                    SslMode = SslMode.Prefer
                }.ConnectionString),

                DatabaseType.MsSql => new SqlConnection(new SqlConnectionStringBuilder(connectionString)
                {
                    Encrypt = true,
                    TrustServerCertificate = false // only true if validated TLS cert
                }.ConnectionString),

                _ => throw new NotSupportedException($"Unsupported database type: {_connection.DatabaseType}")
            };
        }

        public async Task<TableSchema> GetTableSchemaAsync1(string tableName)
        {
            try
            {
                using var connection = _dbContextFactory.MakeConn();
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
                using var connection = _dbContextFactory.MakeConn();
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
                using var connection = _dbContextFactory.MakeConn();
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
        private static DbParameter CreateParam(DbCommand command, string name, object value)
        {
            var param = command.CreateParameter();
            param.ParameterName = name;
            param.Value = value ?? DBNull.Value;
            return param;
        }
    }
}
