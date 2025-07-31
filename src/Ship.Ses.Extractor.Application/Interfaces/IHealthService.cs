using Microsoft.Extensions.Diagnostics.HealthChecks;
using Ship.Ses.Extractor.Domain.Entities.DataMapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ship.Ses.Extractor.Application.Interfaces
{
    public interface IHealthService
    {
        Task<HealthResult> CheckHealthAsync();
    }
}
