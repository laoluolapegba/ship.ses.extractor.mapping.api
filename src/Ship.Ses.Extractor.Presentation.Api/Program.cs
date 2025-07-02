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

try
{
    var builder = WebApplication.CreateBuilder(args);

    const string AllowBlazorClient = "AllowBlazorClient";
    // Configure Serilog
    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
        .CreateLogger();

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

        // You can choose how to read the API version (e.g., from query string, header, or URL segment)
        // options.ApiVersionReader = new QueryStringApiVersionReader("api-version"); // e.g., ?api-version=1.0
        // options.ApiVersionReader = new HeaderApiVersionReader("X-API-Version"); // e.g., X-API-Version: 1.0
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
