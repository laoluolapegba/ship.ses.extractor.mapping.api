using Hl7.Fhir.Serialization;
using Ship.Ses.Extractor.Domain.Repositories.Validator;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using System.Text.Json;

namespace Ship.Ses.Extractor.Application.Services.Validators
{
    public class FhirValidatorService
    {

        private readonly IFhirResourceValidator _fhirResourceValidator;

        public FhirValidatorService(IFhirResourceValidator fhirResourceValidator)
        {
            _fhirResourceValidator = fhirResourceValidator;
        }

        public async Task<bool> ProcessFhirResourceAsync(string jsonPayload, CancellationToken cancellationToken = default)
        {
            try
            {
                return false;
                //JsonNode rootNode = JsonNode.Parse(jsonPayload);
                //if (rootNode is JsonObject fhirResourceJsonObject)
                //{
                //    bool isValid = await _fhirResourceValidator.IsValidAsync(fhirResourceJsonObject, cancellationToken);
                //    return isValid;
                //}
                //else
                //{
                //    // The root of the JSON is not a JsonObject (e.g., it's a JsonArray or a primitive)
                //    return false;
                //}
            }
            catch (JsonException ex)
            {
                return false;
            }
        }
    }
    //    private readonly Validator _validator;

    //    public FhirValidatorService(Validator validator)
    //    {
    //        _validator = validator;
    //    }

    //    public Task<bool> IsValidAsync(JsonObject fhirResource, CancellationToken cancellationToken = default)
    //    {
    //        try
    //        {
    //            // Deserialize JSON to POCO
    //            var xml = fhirResource.ToJsonString();
    //            var parsed = FhirJsonNode.Parse(xml);
    //            var typedResource = new Hl7.Fhir.ElementModel.PocoBuilder().Build(parsed);

    //            var outcome = _validator.Validate(typedResource);

    //            return Task.FromResult(outcome.Success);
    //        }
    //        catch (Exception ex)
    //        {
    //            // Log or capture errors if needed
    //            return Task.FromResult(false);
    //        }
    //    }
    //}

}
