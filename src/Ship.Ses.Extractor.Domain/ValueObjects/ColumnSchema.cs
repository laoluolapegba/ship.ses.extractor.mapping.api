using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ship.Ses.Extractor.Domain.ValueObjects
{
    public class ColumnSchema
    {
        public string Name { get; }
        public string DataType { get; }
        public bool IsNullable { get; }
        public bool IsPrimaryKey { get; }

        public ColumnSchema(string name, string dataType, bool isNullable, bool isPrimaryKey)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            DataType = dataType ?? throw new ArgumentNullException(nameof(dataType));
            IsNullable = isNullable;
            IsPrimaryKey = isPrimaryKey;
        }
    }
}
