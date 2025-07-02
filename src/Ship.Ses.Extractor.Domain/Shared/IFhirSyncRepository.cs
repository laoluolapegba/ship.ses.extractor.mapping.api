using Ship.Ses.Extractor.Domain.Entities.Extractor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ship.Ses.Extractor.Domain.Shared
{ 
    public interface IFhirSyncRepository<TRecord>
    where TRecord : FhirSyncRecord
    {
        Task InsertAsync(TRecord record, CancellationToken cancellationToken = default);
    }

}
