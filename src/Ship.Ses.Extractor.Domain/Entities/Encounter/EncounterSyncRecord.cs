using Ship.Ses.Extractor.Domain.Entities.Extractor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace Ship.Ses.Extractor.Domain.Entities.Encounter
{
    public class EncounterSyncRecord : FhirSyncRecord
    {
        public override string CollectionName => "transformed_pool_encounters";

        public EncounterSyncRecord()
        {
            ResourceType = "Encounter";
        }
    }

}
