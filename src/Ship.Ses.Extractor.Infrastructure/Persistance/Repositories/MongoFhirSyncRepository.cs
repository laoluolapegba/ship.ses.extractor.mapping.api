using MongoDB.Driver;
using Ship.Ses.Extractor.Domain.Entities.Extractor;
using Ship.Ses.Extractor.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ship.Ses.Extractor.Infrastructure.Persistance.Repositories
{
    public class MongoFhirSyncRepository<TRecord> : IFhirSyncRepository<TRecord>
    where TRecord : FhirSyncRecord, new()
    {
        private readonly IMongoCollection<TRecord> _collection;

        public MongoFhirSyncRepository(IMongoDatabase database)
        {
            var temp = new TRecord(); // Just to get the collection name
            _collection = database.GetCollection<TRecord>(temp.CollectionName);
        }

        public async Task InsertAsync(TRecord record, CancellationToken cancellationToken = default)
        {
            await _collection.InsertOneAsync(record, cancellationToken: cancellationToken);
        }
    }


}
