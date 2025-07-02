using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ship.Ses.Extractor.Application.Services.DataMapping
{
    using Ship.Ses.Extractor.Domain.Entities.DataMapping;
    using Ship.Ses.Extractor.Domain.Repositories.DataMapping;
    using Ship.Ses.Extractor.Domain.ValueObjects;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class EmrDatabaseService : IEmrDatabaseService
    {
        private readonly IEmrConnectionRepository _connectionRepository;
        private readonly Func<EmrConnection, IEmrDatabaseReader> _databaseReaderFactory;
        private EmrConnection _currentConnection;

        public EmrDatabaseService(
            IEmrConnectionRepository connectionRepository,
            Func<EmrConnection, IEmrDatabaseReader> databaseReaderFactory)
        {
            _connectionRepository = connectionRepository;
            _databaseReaderFactory = databaseReaderFactory;
        }

        public async Task SelectConnectionAsync(int connectionId)
        {
            _currentConnection = await _connectionRepository.GetByIdAsync(connectionId);
            if (_currentConnection == null)
                throw new ArgumentException($"EMR connection with ID {connectionId} not found.");
        }

        public async Task<IEnumerable<EmrConnection>> GetAvailableConnectionsAsync()
        {
            return await _connectionRepository.GetActiveAsync();
        }

        public Task<IEnumerable<string>> GetTableNamesAsync()
        {
            EnsureConnectionSelected();
            var reader = _databaseReaderFactory(_currentConnection);
            return reader.GetTableNamesAsync();
        }

        public Task<TableSchema> GetTableSchemaAsync(string tableName)
        {
            EnsureConnectionSelected();
            var reader = _databaseReaderFactory(_currentConnection);
            return reader.GetTableSchemaAsync(tableName);
        }

        public async Task<IEnumerable<TableSchema>> GetAllTablesSchemaAsync()
        {
            EnsureConnectionSelected();
            var reader = _databaseReaderFactory(_currentConnection);
            var tableNames = await reader.GetTableNamesAsync();
            var tables = new List<TableSchema>();

            foreach (var tableName in tableNames)
            {
                var schema = await reader.GetTableSchemaAsync(tableName);
                tables.Add(schema);
            }

            return tables;
        }

        
        public Task TestConnectionAsync()
        {
            EnsureConnectionSelected();
            var reader = _databaseReaderFactory(_currentConnection);
            return reader.TestConnectionAsync();
        }

        private void EnsureConnectionSelected()
        {
            if (_currentConnection == null)
                throw new InvalidOperationException("No EMR connection has been selected. Call SelectConnectionAsync first.");
        }
    }

    // Internal interface used by EmrDatabaseService
    public interface IEmrDatabaseReader
    {
        Task<IEnumerable<string>> GetTableNamesAsync();
        Task<TableSchema> GetTableSchemaAsync(string tableName);
        Task TestConnectionAsync();
    }
}
