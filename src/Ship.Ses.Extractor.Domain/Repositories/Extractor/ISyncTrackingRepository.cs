using Ship.Ses.Extractor.Domain.Entities.Extractor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ship.Ses.Extractor.Domain.Repositories.Extractor
{
    public interface ISyncTrackingRepository
    {
        Task<bool> ExistsAsync(string resourceType, string sourceId, CancellationToken cancellationToken = default);
        Task AddOrUpdateAsync(SyncTracking entry, CancellationToken cancellationToken = default);
        Task<IEnumerable<SyncTracking>> GetFailedOrPendingAsync(string resourceType, int maxRetries, CancellationToken cancellationToken = default);
    }

}
