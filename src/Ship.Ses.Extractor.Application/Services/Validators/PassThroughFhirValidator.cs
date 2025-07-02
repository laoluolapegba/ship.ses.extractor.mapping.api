using Ship.Ses.Extractor.Domain.Repositories.Validator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Ship.Ses.Extractor.Application.Services.Validators
{
    public class PassThroughFhirValidator 
    {
        public Task<bool> IsValidAsync(JsonObject fhirResource, CancellationToken cancellationToken = default)
        {
            //if (fhirResource == null) return Task.FromResult(false);

            //if (!fhirResource.TryGetPropertyValue("resourceType", out var resourceType) || string.IsNullOrWhiteSpace(resourceType?.ToString()))
            //    return Task.FromResult(false);

            //if (!fhirResource.TryGetPropertyValue("id", out var id) || string.IsNullOrWhiteSpace(id?.ToString()))
            //    return Task.FromResult(false);

            //if (resourceType.ToString() == "Patient")
            //{
            //    if (!fhirResource.TryGetPropertyValue("identifier", out var identifiers) || identifiers is not JsonArray idArray || idArray.Count == 0)
            //        return Task.FromResult(false);

            //    // Optional: check if required patient demographics exist (e.g. name, gender, birthDate)
            //    if (!fhirResource.ContainsKey("name") || !fhirResource.ContainsKey("gender") || !fhirResource.ContainsKey("birthDate"))
            //        return Task.FromResult(false);
            //}

            return Task.FromResult(true);
        }

    }

}
