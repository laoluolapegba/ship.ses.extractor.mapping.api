using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ship.Ses.Extractor.Domain.Repositories.Extractor
{
    public interface IResourceExtractor<TResource>
    {
        Task<IEnumerable<TResource>> ExtractAsync(CancellationToken cancellationToken = default);
    }

}
