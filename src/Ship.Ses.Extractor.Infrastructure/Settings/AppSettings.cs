namespace Ship.Ses.Extractor.Infrastructure.Settings
{
    public record AppSettings
    {
        public required string OriginDbType { get; init; }
        public TableMappingSettings TableMappings { get; set; }

    }
}
