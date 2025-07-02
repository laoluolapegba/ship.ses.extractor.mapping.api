using Ship.Ses.Extractor.Shared.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ship.Ses.Extractor.Domain.Entities.DataMapping
{
    [Table("ses_emr_connections")]
    public class EmrConnection
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        [Key, Column("Id", Order = 0)]
        public int Id { get; private set; }

        [Column("name")]
        public string Name { get; private set; }

        [Column("description")]
        public string Description { get; private set; }

        [Column("database_type")]
        public DatabaseType DatabaseType { get; private set; }

        [Column("server")]
        public string Server { get; private set; }

        [Column("port")]
        public int Port { get; private set; }

        [Column("database_name")]
        public string DatabaseName { get; private set; }

        [Column("username")]
        public string Username { get; private set; }

        [Column("password")]
        public string Password { get; private set; }

        [Column("is_active")]
        public bool IsActive { get; private set; }

        [Column("created_date")]
        public DateTime CreatedDate { get; private set; }

        [Column("last_modified_date")]
        public DateTime LastModifiedDate { get; private set; }

        private EmrConnection() { } // For EF Core

        public EmrConnection(
            string name,
            string description,
            DatabaseType databaseType,
            string server,
            int port,
            string databaseName,
            string username,
            string password)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty", nameof(name));

            if (string.IsNullOrWhiteSpace(server))
                throw new ArgumentException("Server cannot be empty", nameof(server));

            if (string.IsNullOrWhiteSpace(databaseName))
                throw new ArgumentException("Database name cannot be empty", nameof(databaseName));

            Name = name;
            Description = description;
            DatabaseType = databaseType;
            Server = server;
            Port = port;
            DatabaseName = databaseName;
            Username = username;
            Password = password;
            IsActive = true;
            CreatedDate = DateTime.UtcNow;
            LastModifiedDate = CreatedDate;
        }

        public void Update(
            string name,
            string description,
            DatabaseType databaseType,
            string server,
            int port,
            string databaseName,
            string username,
            string password)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty", nameof(name));

            if (string.IsNullOrWhiteSpace(server))
                throw new ArgumentException("Server cannot be empty", nameof(server));

            if (string.IsNullOrWhiteSpace(databaseName))
                throw new ArgumentException("Database name cannot be empty", nameof(databaseName));

            Name = name;
            Description = description;
            DatabaseType = databaseType;
            Server = server;
            Port = port;
            DatabaseName = databaseName;
            Username = username;
            Password = password;
            LastModifiedDate = DateTime.UtcNow;
        }

        public void SetActive(bool isActive)
        {
            IsActive = isActive;
            LastModifiedDate = DateTime.UtcNow;
        }

        public string GetConnectionString()
        {
            return DatabaseType switch
            {
                DatabaseType.MySql => $"Server={Server};Port={Port};Database={DatabaseName};Uid={Username};Pwd={Password};",
                DatabaseType.PostgreSql => $"Host={Server};Port={Port};Database={DatabaseName};Username={Username};Password={Password};",
                DatabaseType.MsSql => $"Server={Server},{Port};Database={DatabaseName};User Id={Username};Password={Password};TrustServerCertificate=True;",
                _ => throw new NotSupportedException($"Database type {DatabaseType} is not supported")
            };
        }
    }
}
