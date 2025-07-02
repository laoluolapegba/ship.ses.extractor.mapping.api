using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ship.Ses.Extractor.Application.Interfaces
{
    using Ship.Ses.Extractor.Application.DTOs;
    using Ship.Ses.Extractor.Domain.Entities.DataMapping;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IMappingService
    {
        Task<IEnumerable<MappingDefinitionDto>> GetAllMappingsAsync();
        Task<MappingDefinitionDto> GetMappingByIdAsync(Guid id);
        Task<IEnumerable<MappingDefinitionDto>> GetMappingsByResourceTypeAsync(int resourceTypeId);
        Task<Guid> CreateMappingAsync(MappingDefinitionDto mappingDto);
        //Task UpdateMappingAsync(MappingDefinitionDto mappingDto);
        Task DeleteMappingAsync(Guid id);



        //Task SaveMappingAsync(MappingDefinition mapping, CancellationToken cancellationToken = default);
        //Task<MappingDefinition?> GetMappingAsync(string resourceType, string mappingName, CancellationToken cancellationToken = default);
        //Task<List<string>> GetMappingNamesAsync(string resourceType, CancellationToken cancellationToken = default);
    }
}
