using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Ship.Ses.Extractor.Presentation.Api.Extensions
{
    public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _provider;
        public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) => _provider = provider;

        public void Configure(SwaggerGenOptions options)
        {
            // Add a swagger document for each discovered API version
            // NOTE: "ApiVersion" is not set in the OpenApiInfo if there's only one version.
            foreach (var description in _provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));
            }
        }

        private static OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
        {
            var info = new OpenApiInfo()
            {
                Title = $"Ship.Ses.Extractor API {description.ApiVersion}",
                Version = description.ApiVersion.ToString(),
                Description = "AAPI for mapping EMR fields to FHIR resources.",
                Contact = new OpenApiContact { Name = "Interswitch", Email = "contact@interswitchgroup.com" },
                //License = new OpenApiLicense { Name = "MIT License", Url = new Uri("https://opensource.org/licenses/MIT") } // Example license
            };

            if (description.IsDeprecated)
            {
                info.Description += " This API version has been deprecated.";
            }

            return info;
        }

    }
}
