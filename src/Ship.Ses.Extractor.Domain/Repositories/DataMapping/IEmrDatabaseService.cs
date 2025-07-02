using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ship.Ses.Extractor.Domain.Repositories.DataMapping
{
    using Ship.Ses.Extractor.Domain.ValueObjects;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IEmrDatabaseService
    {
        Task<IEnumerable<string>> GetTableNamesAsync();
        Task<TableSchema> GetTableSchemaAsync(string tableName);
        Task<IEnumerable<TableSchema>> GetAllTablesSchemaAsync();
        Task TestConnectionAsync();
        Task SelectConnectionAsync(int connectionId);
    }
}
