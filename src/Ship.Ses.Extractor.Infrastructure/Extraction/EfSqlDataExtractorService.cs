using DnsClient.Protocol;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ship.Ses.Extractor.Application.Services;
using Ship.Ses.Extractor.Domain.Models.Extractor;
using Ship.Ses.Extractor.Infrastructure.Persistance.Contexts;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ship.Ses.Extractor.Infrastructure.Extraction
{
    public class EfSqlDataExtractorService : IDataExtractorService
    {
        private readonly ExtractorDbContext _context;
        private readonly ILogger<EfSqlDataExtractorService> _logger;

        public EfSqlDataExtractorService(ExtractorDbContext context, ILogger<EfSqlDataExtractorService> logger)
        {
            _context = context;
            _logger = logger;
        }
        /// <summary>
        /// unused , but can be used to extract data from a table based on the provided mapping.
        /// </summary>
        /// <param name="mapping"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IEnumerable<IDictionary<string, object>>> ExtractAsync(TableMapping mapping, CancellationToken cancellationToken = default)
        {
            var results = new List<IDictionary<string, object>>();
            var tableName = mapping.TableName;
            var resourceType = mapping.ResourceType;
            var sourceIdColumn = "patient_id"; // You can make this configurable later
            return results; // Return empty for now, as this is a placeholder implementation
        }

    }

}
