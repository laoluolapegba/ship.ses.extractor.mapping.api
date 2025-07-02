
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ship.Ses.Extractor.Application.Services.DataMapping;
using Ship.Ses.Extractor.Domain.Repositories.DataMapping;
using Ship.Ses.Extractor.Infrastructure.Persistance.Contexts;
using Ship.Ses.Extractor.Infrastructure.Persistance.Repositories;
using Ship.Ses.Extractor.Infrastructure.Services;
using Ship.Ses.Extractor.Infrastructure.Settings;

namespace Ship.Ses.Extractor.Infrastructure.Installers
{
    public static class DependencyInjectionInstaller
    {
        public static void InstallDependencyInjectionRegistrations(this WebApplicationBuilder builder)
        {
            
        }
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Register DbContext
            services.AddDbContext<ExtractorDbContext>(options =>
                options.UseMySQL(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(ExtractorDbContext).Assembly.FullName)));

            // Register EMR database services
            services.AddSingleton<EmrDbContextFactory>();
            services.AddScoped<IEmrDatabaseReader, EmrDatabaseReader>();

            // Register repositories
            services.AddScoped<IMappingRepository, MappingRepository>();

            return services;
        }
    }
}
