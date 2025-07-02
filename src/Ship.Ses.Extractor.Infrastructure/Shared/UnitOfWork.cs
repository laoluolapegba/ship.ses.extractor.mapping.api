using Ship.Ses.Extractor.Domain.Shared;
using Ship.Ses.Extractor.Infrastructure.Persistance.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ship.Ses.Extractor.Infrastructure.Shared
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ExtractorDbContext _context;

        public UnitOfWork(ExtractorDbContext context)
        {
            _context = context;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public void Dispose() => _context.Dispose();
    }

}
