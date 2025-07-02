using Ship.Ses.Extractor.Domain.Models.Extractor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ship.Ses.Extractor.Application.Services
{
    public interface IDataExtractorService
    {
        Task<IEnumerable<IDictionary<string, object>>> ExtractAsync(TableMapping mapping, CancellationToken cancellationToken);
    }
}
