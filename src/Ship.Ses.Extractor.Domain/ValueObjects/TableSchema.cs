using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ship.Ses.Extractor.Domain.ValueObjects
{
    public class TableSchema
    {
        public string TableName { get; }
        public IReadOnlyList<ColumnSchema> Columns { get; }

        public TableSchema(string tableName, IEnumerable<ColumnSchema> columns)
        {
            TableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
            Columns = columns?.ToList() ?? throw new ArgumentNullException(nameof(columns));
        }
    }
}
