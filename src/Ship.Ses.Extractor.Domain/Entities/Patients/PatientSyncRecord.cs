using Ship.Ses.Extractor.Domain.Entities.Extractor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ship.Ses.Extractor.Domain.Entities.Patients
{
    public class PatientSyncRecord : FhirSyncRecord
    {
        public override string CollectionName => "transformed_pool_patients";

        public PatientSyncRecord()
        {
            ResourceType = "Patient";
        }
    }


}
