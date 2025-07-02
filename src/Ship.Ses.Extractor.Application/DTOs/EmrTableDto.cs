using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ship.Ses.Extractor.Application.DTOs
{
    using System.Collections.Generic;

    public class EmrTableDto
    {
        public string Name { get; set; }
        public List<EmrColumnDto> Columns { get; set; } = new List<EmrColumnDto>();
    }

    public class EmrColumnDto
    {
        public string Name { get; set; }
        public string DataType { get; set; }
        public bool IsNullable { get; set; }
        public bool IsPrimaryKey { get; set; }
    }
}
