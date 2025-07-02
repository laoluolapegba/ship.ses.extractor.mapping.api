using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ship.Ses.Extractor.Domain.Entities.Extractor
{
    [Table("ses_extract_tracking")]
    public class SyncTracking
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        [Key, Column("Id", Order = 0)]
        public int Id { get; set; }
        [Column("resource_type")]
        [Required]
        public string ResourceType { get; set; }
        [Column("source_id")]
        [Required]
        public string SourceId { get; set; }
        [Column("source_hash")]
        public string SourceHash { get; set; }
        [Column("last_updated")]
        public DateTime? LastUpdated { get; set; }
        [Column("sync_status")]
        public string ExtractStatus { get; set; } = "Pending"; // Pending | Success | Failed
        [Column("retry_count")]
        public int RetryCount { get; set; }
        [Column("error_message")]
        public string ErrorMessage { get; set; }
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [Column("last_attempt_at")]
        public DateTime? LastAttemptAt { get; set; }
    }

}
