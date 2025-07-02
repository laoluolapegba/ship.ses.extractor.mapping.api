using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ship.Ses.Extractor.Infrastructure.Persistance.Repositories
{
    using Microsoft.EntityFrameworkCore;
    using Ship.Ses.Extractor.Domain.Entities;
    using Ship.Ses.Extractor.Domain.Entities.DataMapping;
    using Ship.Ses.Extractor.Domain.Repositories.DataMapping;
    using Ship.Ses.Extractor.Domain.ValueObjects;
    using Ship.Ses.Extractor.Infrastructure.Persistance.Contexts;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;

    public class MappingRepository : IMappingRepository
    {
        private readonly ExtractorDbContext _dbContext;

        public MappingRepository(ExtractorDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<MappingDefinition> GetByIdAsync(Guid id)
        {
            var mapping = await _dbContext.Mappings
                .Include(m => m.FhirResourceType)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (mapping != null)
            {
                LoadColumnMappings(mapping);
            }

            return mapping;
        }

        public async Task<IEnumerable<MappingDefinition>> GetAllAsync()
        {
            var mappings = await _dbContext.Mappings
                .Include(m => m.FhirResourceType)
                .ToListAsync();

            foreach (var mapping in mappings)
            {
                LoadColumnMappings(mapping);
            }

            return mappings;
        }

        public async Task<IEnumerable<MappingDefinition>> GetByResourceTypeAsync(int resourceTypeId)
        {
            var mappings = await _dbContext.Mappings
                .Include(m => m.FhirResourceType)
                .Where(m => m.FhirResourceTypeId == resourceTypeId)
                .ToListAsync();

            foreach (var mapping in mappings)
            {
                LoadColumnMappings(mapping);
            }

            return mappings;
        }

        public async Task AddAsync(MappingDefinition mapping)
        {
            _dbContext.Mappings.Add(mapping);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(MappingDefinition mapping)
        {
            _dbContext.Mappings.Update(mapping);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var mapping = await _dbContext.Mappings.FindAsync(id);
            if (mapping != null)
            {
                _dbContext.Mappings.Remove(mapping);
                await _dbContext.SaveChangesAsync();
            }
        }

        private void LoadColumnMappings(MappingDefinition mapping)
        {
            var json = _dbContext.Entry(mapping).Property<string>("ColumnMappingsJson").CurrentValue;
            if (!string.IsNullOrEmpty(json))
            {
                var columnMappings = JsonSerializer.Deserialize<List<ColumnMapping>>(json);
                var method = mapping.GetType().GetMethod("SetMappings", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                method?.Invoke(mapping, new object[] { columnMappings });
            }
        }
    }
}
