using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ship.Ses.Extractor.Domain.Entities
{
    public class VaultSecrets
    {
        [Required(ErrorMessage = "Vault 'Username' is required.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Vault 'Password' is required.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Vault 'Server' is required.")]
        public string Server { get; set; }

        [Required(ErrorMessage = "Vault 'Database' is required.")]
        public string Database { get; set; }

        [Required(ErrorMessage = "Vault 'Port' is required.")]
        public string Port { get; set; }
    }
}
