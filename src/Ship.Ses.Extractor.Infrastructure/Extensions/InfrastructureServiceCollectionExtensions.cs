using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ship.Ses.Extractor.Application.Services;
using Ship.Ses.Extractor.Application.Services.DataMapping;
using Ship.Ses.Extractor.Domain.Repositories.DataMapping;
using Ship.Ses.Extractor.Domain.Shared;
using Ship.Ses.Extractor.Infrastructure.Configuration;
using Ship.Ses.Extractor.Infrastructure.Persistance.Repositories;
using Ship.Ses.Extractor.Infrastructure.Services;
using Ship.Ses.Extractor.Infrastructure.Settings;
using Ship.Ses.Extractor.Infrastructure.Shared;

namespace Ship.Ses.Extractor.Infrastructure.Extensions
{
    

    public static class InfrastructureServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration config)
        {
           

            // Table mapping loader
            services.AddSingleton<ITableMappingService, JsonTableMappingService>();


            // Infra-level services
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            


            // Register UI EMR database services
            services.AddSingleton<EmrPersistenceFactory>();
            services.AddScoped<IEmrDatabaseReader, EmrDatabaseReader>();

            // Register repositories
            services.AddScoped<IMappingRepository, MappingRepository>();


            return services;
        }
    }

}
