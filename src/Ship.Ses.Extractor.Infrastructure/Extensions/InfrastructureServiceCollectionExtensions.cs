using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Ship.Ses.Extractor.Application.Services;
using Ship.Ses.Extractor.Application.Services.DataMapping;
using Ship.Ses.Extractor.Application.Services.Extractors;
using Ship.Ses.Extractor.Application.Services.Transformers;
using Ship.Ses.Extractor.Application.Services.Validators;
using Ship.Ses.Extractor.Domain.Entities.Patients;
using Ship.Ses.Extractor.Domain.Repositories.DataMapping;
using Ship.Ses.Extractor.Domain.Repositories.Extractor;
using Ship.Ses.Extractor.Domain.Repositories.Transformer;
using Ship.Ses.Extractor.Domain.Repositories.Validator;
using Ship.Ses.Extractor.Domain.Shared;
using Ship.Ses.Extractor.Infrastructure.Configuration;
using Ship.Ses.Extractor.Infrastructure.Extraction;
using Ship.Ses.Extractor.Infrastructure.Persistance.Repositories;
using Ship.Ses.Extractor.Infrastructure.Services;
using Ship.Ses.Extractor.Infrastructure.Settings;
using Ship.Ses.Extractor.Infrastructure.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ship.Ses.Extractor.Infrastructure.Extensions
{
    

    public static class InfrastructureServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration config)
        {
            // MongoDB
            // ✅ Bind LandingZone settings separately
            services.Configure<LandingZoneDbSettings>(config.GetSection("AppSettings:LandingZoneDbSettings"));

            // ✅ Register MongoDB
            services.AddSingleton<IMongoClient>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<LandingZoneDbSettings>>().Value;
                return new MongoClient(settings.ConnectionString);
            });

            services.AddSingleton(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<LandingZoneDbSettings>>().Value;
                var client = sp.GetRequiredService<IMongoClient>();
                return client.GetDatabase(settings.DatabaseName);
            });


            // Table mapping loader
            services.AddSingleton<ITableMappingService, JsonTableMappingService>();


            // Infra-level services
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            // Core extractor, transformer, validator, and repository
            services.AddScoped<IDataExtractorService, EfSqlDataExtractorService>();
            services.AddScoped<ISyncTrackingRepository, SyncTrackingRepository>();
            services.AddSingleton<IResourceTransformer<System.Text.Json.Nodes.JsonObject>, PatientTransformer>();
            services.AddScoped<IFhirSyncRepository<PatientSyncRecord>, MongoFhirSyncRepository<PatientSyncRecord>>();


            // Register UI EMR database services
            services.AddSingleton<EmrDbContextFactory>();
            services.AddScoped<IEmrDatabaseReader, EmrDatabaseReader>();

            // Register repositories
            services.AddScoped<IMappingRepository, MappingRepository>();


            return services;
        }
    }

}
