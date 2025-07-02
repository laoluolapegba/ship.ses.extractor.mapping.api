using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ship.Ses.Extractor.Infrastructure.Settings
{
    public class TableMappingSettings
    {
        [Required]
        public string RootPath { get; set; }
    }
}
