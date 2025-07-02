
using Ship.Ses.Extractor.Application.Services;
using Ship.Ses.Extractor.Domain.Repositories.Extractor;
using Ship.Ses.Extractor.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Ship.Ses.Extractor.Application.Handlers
{
    public class ExtractDataSourcesHandler
    {
        private readonly IDataSourceRepository _dataSourceRepository;
        private readonly IDataExtractorService _extractorService;
        private readonly IUnitOfWork _unitOfWork;

        public ExtractDataSourcesHandler(
            IDataSourceRepository dataSourceRepository,
            IDataExtractorService extractorService,
            IUnitOfWork unitOfWork)
        {
            _dataSourceRepository = dataSourceRepository;
            _extractorService = extractorService;
            _unitOfWork = unitOfWork;
        }

        public async Task HandleAsync(CancellationToken cancellationToken = default)
        {
            var dataSources = await _dataSourceRepository.GetActiveDataSourcesAsync(cancellationToken);

            foreach (var source in dataSources)
            {
                //Console.WriteLine($"Extracting from: {source.Name}");

                //var rows = await _extractorService.ExtractAsync(source, cancellationToken);

                //foreach (var row in rows)
                //{
                //    // Placeholder: Pass to transformation/validation layer
                //    Console.WriteLine(JsonSerializer.Serialize(row));
                //}
            }
        }
    }

}
