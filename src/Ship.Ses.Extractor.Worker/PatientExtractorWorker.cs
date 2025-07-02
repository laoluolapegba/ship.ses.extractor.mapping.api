using Ship.Ses.Extractor.Application.Services.Extractors;
using Ship.Ses.Extractor.Infrastructure.Persistance.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Ship.Ses.Extractor.Worker
{
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using System.Threading;
    using System.Threading.Tasks;

    public class PatientExtractorWorker : BackgroundService
    {
        private readonly ILogger<PatientExtractorWorker> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public PatientExtractorWorker(
            ILogger<PatientExtractorWorker> logger,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }
        private Task _executingTask;
        private readonly CancellationTokenSource _stoppingCts =
                                                       new CancellationTokenSource();

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🚀 Starting Patient Extractor Worker...");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var extractor = scope.ServiceProvider.GetRequiredService<PatientResourceExtractor>();
                    await extractor.ExtractAndPersistAsync(stoppingToken);
                    _logger.LogInformation("✅ Patient extraction completed");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Unhandled exception in PatientExtractorWorker");

                    // Optional: delay restart or retry loop
                    await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
                }
            }
        }
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Patient Extractor Service is stopping.");
            // Stop called without start
            if (_executingTask == null)
            {
                return;
            }

            try
            {
                // Signal cancellation to the executing method
                _stoppingCts.Cancel();
            }
            finally
            {
                // Wait until the task completes or the stop token triggers
                await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite,
                                                              cancellationToken));
            }

        }
    }


}
