using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.Extensions.Logging;
using Ship.Ses.Extractor.Application.DTOs;
using Ship.Ses.Extractor.Domain.Repositories.Validator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Ship.Ses.Extractor.Domain.Shared;

namespace Ship.Ses.Extractor.Infrastructure.Validator
{
    public class FirelyResourceValidator : IFhirResourceValidator
    {
        private readonly FhirJsonParser _parser;
        private readonly ILogger<FirelyResourceValidator> _logger;

        public FirelyResourceValidator(ILogger<FirelyResourceValidator> logger)
        {
            _parser = new FhirJsonParser();
            _logger = logger;
        }
        
        public async Task<FhirValidationResult> ValidateAsync(JsonObject fhirResource, CancellationToken cancellationToken = default)
        {
            var result = new FhirValidationResult();

            if (fhirResource == null)
            {
                result.Errors.Add("Resource is null.");
                return result;
            }

            if (!fhirResource.TryGetPropertyValue("resourceType", out var resourceType) || string.IsNullOrWhiteSpace(resourceType?.ToString()))
            {
                result.Errors.Add("Missing or invalid 'resourceType'.");
                return result;
            }

            if (!fhirResource.TryGetPropertyValue("id", out var id) || string.IsNullOrWhiteSpace(id?.ToString()))
            {
                result.Errors.Add("Missing or invalid 'id'.");
                return result;
            }

            if (resourceType.ToString() == "Patient")
            {
                if (!fhirResource.TryGetPropertyValue("identifier", out var identifiers) || identifiers is not JsonArray idArray || idArray.Count == 0)
                    result.Errors.Add("Patient resource is missing 'identifier' array or it is empty.");

                if (!fhirResource.ContainsKey("name")) result.Errors.Add("Missing 'name' field.");
                if (!fhirResource.ContainsKey("gender")) result.Errors.Add("Missing 'gender' field.");
                if (!fhirResource.ContainsKey("birthDate")) result.Errors.Add("Missing 'birthDate' field.");
            }

            try
            {
                var resource = _parser.Parse<Resource>(fhirResource.ToJsonString());

                if (resource is DomainResource domainResource && domainResource.TryDeriveResourceType != null)
                {
                    result.IsValid = result.Errors.Count == 0;
                    return result;
                }
                else if (resource is Hl7.Fhir.Model.Resource)
                {
                    result.IsValid = result.Errors.Count == 0;
                    return result;
                }
                else
                {
                    result.Errors.Add("Parsed resource is not a valid FHIR DomainResource.");
                    return result;
                }
            }
            catch (FormatException fe)
            {
                _logger.LogWarning(fe, "FHIR resource parsing format error.");
                result.Errors.Add("FHIR resource format error: " + fe.Message);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected exception during FHIR resource validation.");
                result.Errors.Add("Unhandled exception: " + ex.Message);
                return result;
            }
        }

        

        //Task<Domain.Shared.FhirValidationResult> ValidateAsync(JsonObject fhirResource, CancellationToken cancellationToken)
        //{
        //    throw new NotImplementedException();
        //}
    }

}
