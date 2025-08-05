using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Ship.Ses.Extractor.Application.Interfaces;
using Ship.Ses.Extractor.Application.Services.DataMapping;
using Ship.Ses.Extractor.Domain.Entities;
using Ship.Ses.Extractor.Domain.Entities.DataMapping;
using Ship.Ses.Extractor.Domain.Repositories.DataMapping;
using Ship.Ses.Extractor.Infrastructure.Installers;
using Ship.Ses.Extractor.Infrastructure.Persistance.Contexts;
using Ship.Ses.Extractor.Infrastructure.Persistance.Repositories;
using Ship.Ses.Extractor.Infrastructure.Services;
using Ship.Ses.Extractor.Infrastructure.Settings;
using Ship.Ses.Extractor.Presentation.Api.Extensions;
using Swashbuckle.AspNetCore.SwaggerGen;
using VaultSharp;
using VaultSharp.V1.AuthMethods.Kubernetes;


try
{
    // Main entry point for the application
    // Configure Serilog for structured logging
    Log.Logger = new LoggerConfiguration()
         .ReadFrom.Configuration(Host.CreateApplicationBuilder().Configuration)
         .Enrich.FromLogContext()
         //.WriteTo.Console()
         .CreateLogger();

    var builder = WebApplication.CreateBuilder(args);
    builder.Services.Configure<VaultSecrets>(builder.Configuration.GetSection("Vault"));

    if (!builder.Environment.IsDevelopment())
    {
        var vaultConfig = builder.Configuration.GetSection("Vault");

        if (vaultConfig.GetValue<bool>("Enabled"))
        {
            builder.Services.AddSingleton(provider =>
            {
                var configuration = provider.GetRequiredService<IConfiguration>();
                return new VaultSecrets
                {
                    Username = configuration["USERNAME"], 
                    Password = configuration["PASSWORD"], 
                    Server = configuration["SERVER"],
                    Database = configuration["DATABASE"],
                    Port = configuration["PORT"]
                };
            });
            //var vaultService = new VaultService(
            //    vaultUri: vaultConfig["Uri"],
            //    role: vaultConfig["Role"],
            //    mountPath: vaultConfig["Mount"],
            //    secretsPath: vaultConfig["SecretsPath"]
            //);

            //var secrets = await vaultService.GetSecretsAsync();

            //foreach (var kvp in secrets)
            //{
            //    builder.Configuration[$"EmrDatabase:{kvp.Key}"] = kvp.Value;
            //}

            Log.Information("Vault secrets injected into configuration.");
        }
    }
    else
    {
        //For Development
        builder.Configuration.AddUserSecrets<Program>();
        Log.Information("Local sensitive configurations loaded from User Secrets.");
    }

    const string AllowBlazorClient = "AllowBlazorClient";
    

    builder.Host.UseSerilog();

    builder.Services.AddOktaAuthentication(builder.Configuration);
    // Read CORS settings from configuration
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(name: AllowBlazorClient, 
            policy =>
            {
                if (builder.Environment.IsDevelopment())
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                }
                else
                {
                    var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                                      ?? Array.Empty<string>();

                    policy.WithOrigins(corsOrigins)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                }
            });
    });

    // Add services to the container
    builder.Services.AddControllers();

    // Configure API versioning
    // 1. Add API Versioning
    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0); 
        options.AssumeDefaultVersionWhenUnspecified = true; // Use the default version when no version is specified
        options.ReportApiVersions = true; 

        options.ApiVersionReader = ApiVersionReader.Combine(
            new QueryStringApiVersionReader("api-version"),
            new HeaderApiVersionReader("X-API-Version"),
            new UrlSegmentApiVersionReader()); 
    });

    
    // Configure API versioning
    builder.Services.AddApiVersioning(options =>
    {
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.ReportApiVersions = true;
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });



    builder.InstallSwagger();


    builder.Services.AddOpenApi();
    // Register DbContext
    // Add infrastructure services
    builder.Services.AddInfrastructure(builder.Configuration);
    // Add application services
    builder.Services.AddScoped<IEmrDatabaseService, EmrDatabaseService>();
    builder.Services.AddScoped<Func<EmrConnection, IEmrDatabaseReader>>(serviceProvider => connection =>
    {
        return new EmrDatabaseReader(connection);
    });
    builder.Services.AddScoped<IFhirResourceService, FhirResourceService>();
    builder.Services.AddScoped<IMappingService, MappingService>();
    builder.Services.AddScoped<IMappingRepository, MappingRepository>();
    builder.Services.AddScoped<IEmrConnectionRepository, EmrConnectionRepository>();
    builder.Services.AddSingleton<IHealthService, HealthService>();
    //builder.Services.Configure<VaultSecrets>(builder.Configuration.GetSection("Vault")); 


    var app = builder.Build();

    // Log application startup
    app.Logger.LogInformation("Application starting up");

    // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
    // specifying the Swagger JSON endpoint.
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        // Build a swagger endpoint for each discovered API version
        foreach (var description in app.Services.GetRequiredService<IApiVersionDescriptionProvider>().ApiVersionDescriptions)
        {
            options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", $"FHIR Resource Mapping API {description.GroupName.ToUpperInvariant()}");
        }
        options.RoutePrefix = "swagger"; // Sets the Swagger UI at /swagger
    });


    // Configure middleware
    if (app.Environment.IsDevelopment())
    {
        app.Logger.LogInformation("Development environment detected"); app.UseDeveloperExceptionPage();
    }
    else
    {
        app.Logger.LogInformation("Production environment detected");
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection(); 
    }
    app.UseRouting();
    app.UseCors(AllowBlazorClient);
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    var server = app.Services.GetRequiredService<Microsoft.AspNetCore.Hosting.Server.IServer>();
    var addresses = server.Features.Get<Microsoft.AspNetCore.Hosting.Server.Features.IServerAddressesFeature>();

    if (addresses != null)
    {
        foreach (var address in addresses.Addresses)
        {
            app.Logger.LogInformation("Application is listening on: {Address}", address);
        }
    }
    else
    {
        app.Logger.LogWarning("⚠️ Could not determine server addresses (no IServerAddressesFeature found).");
    }
    app.Logger.LogInformation("Mapping Api started and ready to accept requests");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "❌ Application failed to start due to an exception");
}
finally
{
    Log.CloseAndFlush();
}
