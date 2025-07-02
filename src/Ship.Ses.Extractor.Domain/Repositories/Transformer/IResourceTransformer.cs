using Ship.Ses.Extractor.Domain.Models.Extractor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Ship.Ses.Extractor.Domain.Repositories.Transformer
{
    public interface IResourceTransformer<T>
    {
        T Transform(IDictionary<string, object> row, TableMapping mapping, List<string> errors);
        JsonObject NormalizeEnumFields(JsonObject json);

        
    }

}
