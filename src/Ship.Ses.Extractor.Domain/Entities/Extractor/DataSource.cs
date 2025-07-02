using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ship.Ses.Extractor.Domain.Models.Extractor
{
    [Table("ses_datasources")]
    public class DataSource
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        [Key, Column("Id", Order = 0)]
        public Guid Id { get; set; }
        [Column("dsn")]
        public string Name { get; set; }
        [Column("connectionstring")]
        public string ConnectionString { get; set; }
        public string DbType { get; set; }
        public string TableName { get; set; }
        public string ResourceType { get; set; } // e.g., "Patient", "Encounter"
        public bool IsActive { get; set; }
    }
}
