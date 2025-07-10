using Asp.Versioning.ApiExplorer;
using Asp.Versioning;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Ship.Ses.Extractor.Application.Interfaces;
using Ship.Ses.Extractor.Application.Services.DataMapping;
using Ship.Ses.Extractor.Domain.Repositories.DataMapping;
using Ship.Ses.Extractor.Infrastructure.Installers;
using Ship.Ses.Extractor.Infrastructure.Persistance.Repositories;
using Ship.Ses.Extractor.Domain.Entities.DataMapping;
using Ship.Ses.Extractor.Infrastructure.Services;
using Ship.Ses.Extractor.Presentation.Api.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


try
{
    // Main entry point for the application
    // Configure Serilog for structured logging
    Log.Logger = new LoggerConfiguration()
         .ReadFrom.Configuration(Host.CreateApplicationBuilder().Configuration)
         .Enrich.FromLogContext()
         .WriteTo.Console()
         .CreateLogger();

    var builder = WebApplication.CreateBuilder(args);
    if (!builder.Environment.IsDevelopment())
    {
        
        // Make sure your appsettings.json or environment variables configure KeyVault:Uri
        // and appropriate access policies are set in Azure.
        // var keyVaultUri = builder.Configuration["KeyVault:Uri"];
        // if (!string.IsNullOrEmpty(keyVaultUri))
        // {
        //     builder.Configuration.AddAzureKeyVault(
        //         new Uri(keyVaultUri),
        //         new DefaultAzureCredential()); // Use appropriate credential based on your setup
        //     app.Logger.LogInformation("🔐 Sensitive configurations loaded from Azure Key Vault.");
        // }
        // else
        // {
        //     app.Logger.LogWarning("KeyVault:Uri not configured for production environment. Sensitive data might be sourced from less secure locations.");
        // }
    }
    else
    {
        // 2. For Development: Use User Secrets for sensitive data
        // This is a secure way to store secrets during development outside of your source control.
        // It will automatically load secrets from %APPDATA%\Microsoft\UserSecrets\<UserSecretsId>\secrets.json
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
        options.DefaultApiVersion = new ApiVersion(1, 0); // Default version if not specified
        options.AssumeDefaultVersionWhenUnspecified = true; // Use the default version when no version is specified
        options.ReportApiVersions = true; // Report API versions in the response headers

        options.ApiVersionReader = ApiVersionReader.Combine(
            new QueryStringApiVersionReader("api-version"),
            new HeaderApiVersionReader("X-API-Version"),
            new UrlSegmentApiVersionReader()); // Enables versioning in URL path (e.g., /v1/resource)
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
    // Add infrastructure services
    builder.Services.AddInfrastructure(builder.Configuration);
    var app = builder.Build();

    // Log application startup
    app.Logger.LogInformation("🚀 Application starting up");

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
        app.Logger.LogInformation("🛠️ Development environment detected"); app.UseDeveloperExceptionPage();
    }
    else
    {
        app.Logger.LogInformation("🏭 Production environment detected");
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    app.UseSerilogRequestLogging();

    app.UseHttpsRedirection();
    app.UseRouting();
    app.UseCors(AllowBlazorClient);
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    // Log application ready
    app.Logger.LogInformation("✅ Application started and ready to accept requests");

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
