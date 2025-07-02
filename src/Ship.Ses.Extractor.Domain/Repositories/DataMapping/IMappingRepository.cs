using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ship.Ses.Extractor.Domain.Entities;
using Ship.Ses.Extractor.Domain.Entities.DataMapping;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace Ship.Ses.Extractor.Domain.Repositories.DataMapping
{


    public interface IMappingRepository
    {
        Task<MappingDefinition> GetByIdAsync(Guid id);
        Task<IEnumerable<MappingDefinition>> GetAllAsync();
        Task<IEnumerable<MappingDefinition>> GetByResourceTypeAsync(int resourceTypeId);
        //Task<List<string>> GetMappingNamesAsync(string resourceType, CancellationToken cancellationToken = default);
        Task AddAsync(MappingDefinition mapping);
        Task UpdateAsync(MappingDefinition mapping);
        Task DeleteAsync(Guid id);
    }
}
