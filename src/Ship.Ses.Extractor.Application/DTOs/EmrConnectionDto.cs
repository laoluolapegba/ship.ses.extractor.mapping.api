using Ship.Ses.Extractor.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ship.Ses.Extractor.Application.DTOs
{
    using System.ComponentModel.DataAnnotations;

    public class EmrConnectionDto
    {
        public int Id { get; set; }

        [Required, StringLength(100, MinimumLength = 3)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public DatabaseType DatabaseType { get; set; }

        [Required, StringLength(200)]
        public string Server { get; set; }

        [Range(1, 65535)]
        public int Port { get; set; }

        [Required, StringLength(100)]
        public string DatabaseName { get; set; }

        [Required, StringLength(100)]
        public string Username { get; set; }

        // Password is only used during create/update, not returned in responses
        [StringLength(100)]
        public string Password { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime LastModifiedDate { get; set; }
    }

}
