using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ship.Ses.Extractor.Shared.Models
{
    public class EmrTableInfo
    {
        public string Name { get; set; }
        public List<EmrColumnInfo> Columns { get; set; } = new List<EmrColumnInfo>();
    }

    public class EmrColumnInfo
    {
        public string Name { get; set; }
        public string DataType { get; set; }
        public bool IsNullable { get; set; }
        public bool IsPrimaryKey { get; set; }
    }
}
