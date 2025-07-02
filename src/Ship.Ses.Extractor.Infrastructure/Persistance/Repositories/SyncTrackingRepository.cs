using Microsoft.EntityFrameworkCore;
using Ship.Ses.Extractor.Domain.Entities.Extractor;
using Ship.Ses.Extractor.Domain.Repositories.Extractor;
using Ship.Ses.Extractor.Infrastructure.Persistance.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ship.Ses.Extractor.Infrastructure.Persistance.Repositories
{
    public class SyncTrackingRepository : ISyncTrackingRepository
    {
        private readonly ExtractorDbContext _context;

        public SyncTrackingRepository(ExtractorDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ExistsAsync(string resourceType, string sourceId, CancellationToken cancellationToken = default)
        {
            return await _context.SyncTracking.AnyAsync(t => t.ResourceType == resourceType && t.SourceId == sourceId, cancellationToken);
        }

        public async Task AddOrUpdateAsync(SyncTracking entry, CancellationToken cancellationToken = default)
        {
            var existing = await _context.SyncTracking
                .FirstOrDefaultAsync(t => t.ResourceType == entry.ResourceType && t.SourceId == entry.SourceId, cancellationToken);

            if (existing == null)
            {
                _context.SyncTracking.Add(entry);
            }
            else
            {
                existing.SourceHash = entry.SourceHash;
                existing.LastUpdated = entry.LastUpdated;
                existing.ExtractStatus = entry.ExtractStatus;
                existing.RetryCount = entry.RetryCount;
                existing.ErrorMessage = entry.ErrorMessage;
                existing.LastAttemptAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<IEnumerable<SyncTracking>> GetFailedOrPendingAsync(string resourceType, int maxRetries, CancellationToken cancellationToken = default)
        {
            return await _context.SyncTracking
                .Where(t => t.ResourceType == resourceType &&
                            t.ExtractStatus != "Success" &&
                            t.RetryCount <= maxRetries)
                .ToListAsync(cancellationToken);
        }
    }

}
