using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ship.Ses.Extractor.Shared.Models
{
    public class FhirResourceInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ResourceJson { get; set; }
        public List<FhirPathInfo> Paths { get; set; } = new List<FhirPathInfo>();
    }

    public class FhirPathInfo
    {
        public string Path { get; set; }
        public string DisplayName { get; set; }
        public string DataType { get; set; }
    }
}
