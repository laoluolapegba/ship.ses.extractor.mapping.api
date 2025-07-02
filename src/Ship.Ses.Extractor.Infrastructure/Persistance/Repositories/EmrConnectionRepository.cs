using Microsoft.EntityFrameworkCore;
using Ship.Ses.Extractor.Domain.Entities.DataMapping;
using Ship.Ses.Extractor.Domain.Repositories.DataMapping;
using Ship.Ses.Extractor.Infrastructure.Persistance.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ship.Ses.Extractor.Infrastructure.Persistance.Repositories
{
    public class EmrConnectionRepository : IEmrConnectionRepository
    {
        private readonly ExtractorDbContext _context;

        public EmrConnectionRepository(ExtractorDbContext context)
        {
            _context = context;
        }

        public async Task<EmrConnection> GetByIdAsync(int id)
        {
            return await _context.EmrConnections.FindAsync(id);
        }

        public async Task<IEnumerable<EmrConnection>> GetAllAsync()
        {
            return await _context.EmrConnections.ToListAsync();
        }

        public async Task<IEnumerable<EmrConnection>> GetActiveAsync()
        {
            return await _context.EmrConnections
                .Where(c => c.IsActive)
                .ToListAsync();
        }

        public async Task AddAsync(EmrConnection connection)
        {
            await _context.EmrConnections.AddAsync(connection);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(EmrConnection connection)
        {
            _context.EmrConnections.Update(connection);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var connection = await _context.EmrConnections.FindAsync(id);
            if (connection != null)
            {
                _context.EmrConnections.Remove(connection);
                await _context.SaveChangesAsync();
            }
        }
    }
}
