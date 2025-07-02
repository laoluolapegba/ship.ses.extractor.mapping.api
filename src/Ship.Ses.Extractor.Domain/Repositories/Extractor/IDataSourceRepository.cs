using Ship.Ses.Extractor.Domain.Models.Extractor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ship.Ses.Extractor.Domain.Repositories.Extractor
{
    public interface IDataSourceRepository
    {
        Task<IEnumerable<DataSource>> GetActiveDataSourcesAsync(CancellationToken cancellationToken = default);
        Task<DataSource?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
