using Microsoft.EntityFrameworkCore;
using Ship.Ses.Extractor.Domain.Models.Extractor;
using Ship.Ses.Extractor.Domain.Repositories.Extractor;
using Ship.Ses.Extractor.Infrastructure.Persistance.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ship.Ses.Extractor.Infrastructure.Persistance.Repositories
{
    public class DataSourceRepository : IDataSourceRepository
    {
        private readonly ExtractorDbContext _context;

        public DataSourceRepository(ExtractorDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DataSource>> GetActiveDataSourcesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.DataSources.Where(d => d.IsActive).ToListAsync(cancellationToken);
        }

        public async Task<DataSource?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.DataSources.FindAsync(new object[] { id }, cancellationToken);
        }
    }

}
