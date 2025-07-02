using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Ship.Ses.Extractor.Domain.Repositories.Transformer
{
    public class EnvironmentDefaults
    {
        //public JsonObject ManagingOrganization { get; set; } = new();
        public OrganizationReference ManagingOrganization { get; set; } = new();
    }

    public class OrganizationReference
    {
        public string Reference { get; set; } = "";
        public string Display { get; set; } = "";
    }
}
