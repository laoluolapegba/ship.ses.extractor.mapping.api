using Ship.Ses.Extractor.Domain.Repositories.Extractor;
using Ship.Ses.Extractor.Domain.Shared;
using Ship.Ses.Extractor.Infrastructure.Persistance.Contexts;
using Ship.Ses.Extractor.Worker;
using Microsoft.EntityFrameworkCore;
using Ship.Ses.Extractor.Infrastructure.Settings;

using Ship.Ses.Extractor.Worker.Extensions;
using Serilog;
using Ship.Ses.Extractor.Application.Services.Transformers;
using Ship.Ses.Extractor.Domain.Repositories.Transformer; // Fix for CS1061: Ensure the correct namespace for EF Core is included.


Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(Host.CreateApplicationBuilder().Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();
try
{
    Log.Information("Starting Ship Extractor Worker...");

    var builder = Host.CreateApplicationBuilder(args);
    // Load strongly-typed settings
    // ✅ Bind app settings
    builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
    var appSettings = builder.Configuration.GetSection("AppSettings").Get<AppSettings>()
        ?? throw new Exception("AppSettings section not found.");

    builder.Services.Configure<AppSettings>(builder.Configuration.GetSection(nameof(AppSettings)));
    //builder.Services.Configure<EnvironmentDefaults>(builder.Configuration.GetSection("EnvironmentDefaults"));
    var envDefaults = builder.Configuration.GetSection("EnvironmentDefaults").Get<EnvironmentDefaults>();
    builder.Services.AddSingleton(envDefaults);
   

    // Register DbContext
    builder.Services.AddDbContext<ExtractorDbContext>(options =>
    {
        options.UseMySQL(appSettings.OriginDb.ConnectionString);
    });

    // Register Infra + Application services via extension method
    builder.Services.AddExtractorDependencies(builder.Configuration);

    // Hosted Service (Runner)
    builder.Services.AddHostedService<PatientExtractorWorker>();
    TemplateBuilders.ConfigureDefaults(envDefaults);
    Log.Information($"environment defaults: {envDefaults.ManagingOrganization}");

    builder.Build().Run();
    TemplateBuilders.ConfigureDefaults(envDefaults);
    Log.Information("Ship Extractor Worker stopped cleanly.");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Ship Extractor Worker terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}