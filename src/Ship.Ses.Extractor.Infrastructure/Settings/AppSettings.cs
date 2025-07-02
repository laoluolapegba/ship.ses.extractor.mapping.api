namespace Ship.Ses.Extractor.Infrastructure.Settings
{
    public record AppSettings
    {
        public required OriginDbSettings OriginDb { get; init; }
        public required string OriginDbType { get; init; }
        public LandingZoneDbSettings LandingZoneDbSettings { get; set; }
        public TableMappingSettings TableMappings { get; set; }

    }
}
