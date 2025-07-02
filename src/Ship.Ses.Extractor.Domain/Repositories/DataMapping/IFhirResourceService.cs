using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ship.Ses.Extractor.Domain.Repositories.DataMapping
{
    using Ship.Ses.Extractor.Domain.Entities;
    using Ship.Ses.Extractor.Domain.Entities.Extractor;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IFhirResourceService
    {
        Task<IEnumerable<FhirResourceType>> GetAllResourceTypesAsync();
        Task<FhirResourceType> GetResourceTypeByIdAsync(int id);
        Task<FhirResourceType> GetResourceTypeByNameAsync(string name);
        Task<string> GetResourceStructureAsync(int resourceTypeId);
    }
}
