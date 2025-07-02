using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Ship.Ses.Extractor.Domain.Shared;

namespace Ship.Ses.Extractor.Domain.Repositories.Validator
{
    public interface IFhirResourceValidator
    {
        //Task<bool> IsValidAsync(JsonObject fhirResource, CancellationToken cancellationToken = default);
        //Task<FhirValidationResult> ValidateAsync(JsonObject fhirResource, CancellationToken cancellationToken = default);
        Task<FhirValidationResult> ValidateAsync(JsonObject fhirResource, CancellationToken cancellationToken = default);
    }

}
