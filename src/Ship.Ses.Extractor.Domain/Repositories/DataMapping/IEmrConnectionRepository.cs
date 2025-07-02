using Ship.Ses.Extractor.Domain.Entities.DataMapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ship.Ses.Extractor.Domain.Repositories.DataMapping
{
    public interface IEmrConnectionRepository
    {
        Task<EmrConnection> GetByIdAsync(int id);
        Task<IEnumerable<EmrConnection>> GetAllAsync();
        Task<IEnumerable<EmrConnection>> GetActiveAsync();
        Task AddAsync(EmrConnection connection);
        Task UpdateAsync(EmrConnection connection);
        Task DeleteAsync(int id);
    }
}
