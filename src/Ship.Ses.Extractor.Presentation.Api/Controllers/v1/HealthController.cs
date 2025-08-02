using Hl7.Fhir.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Driver.Search;
using Ship.Ses.Extractor.Application.Interfaces;
using Ship.Ses.Extractor.Domain.Entities.DataMapping;
using Ship.Ses.Extractor.Infrastructure.Services;
using System.Threading.Tasks;
using static MassTransit.Util.ChartTable;

namespace Ship.Ses.Mapping.Presentation.Api.Controllers.v1
{

    namespace Ship.Ses.Extractor.Presentation.Api.Controllers
    {
        [ApiController]
        [Route("/health")]
        public class HealthController : ControllerBase
        {
            private readonly IHealthService _healthService;
            private readonly ILogger<HealthController> _logger;

            public HealthController(IHealthService healthService, ILogger<HealthController> logger)
            {
                _healthService = healthService;
                _logger = logger;
            }

            [HttpGet]
            public async Task<IActionResult> Get()
            {
                var health = await _healthService.CheckHealthAsync();

                return health.Status switch
                {
                    HealthStatus.Healthy => Ok(new { status = "healthy" }),
                    HealthStatus.Degraded => StatusCode(206, new { status = "degraded", reason = health.Reason }),
                    HealthStatus.Unhealthy => StatusCode(503, new { status = "unhealthy", reason = health.Reason }),
                    _ => StatusCode(500, new { status = "unknown", reason = "Unexpected status" })
                };
            }
        }
    }
}
