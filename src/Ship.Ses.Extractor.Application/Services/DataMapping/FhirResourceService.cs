﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ship.Ses.Extractor.Application.Services.DataMapping
{
    using Ship.Ses.Extractor.Domain.Entities;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Linq;
    using Ship.Ses.Extractor.Domain.Entities.Extractor;
    using Ship.Ses.Extractor.Domain.Repositories.DataMapping;

    public class FhirResourceService : IFhirResourceService
    {
        private readonly List<FhirResourceType> _resourceTypes;

        public FhirResourceService()
        {
            // Initialize with a set of common FHIR resource types
            _resourceTypes = new List<FhirResourceType>
            {
                new FhirResourceType("Patient", GetPatientStructure()) { Id = 1 },
                new FhirResourceType("Encounter", GetEncounterStructure()) { Id = 2 },
                new FhirResourceType("Observation", GetObservationStructure()) { Id = 3 },
                new FhirResourceType("Condition", GetConditionStructure()) { Id = 4 },
                new FhirResourceType("Medication", GetMedicationStructure()) { Id = 5 },
                new FhirResourceType("MedicationRequest", GetMedicationRequestStructure()) { Id = 6 },
                new FhirResourceType("Procedure", GetProcedureStructure()) { Id = 7 },
                new FhirResourceType("Immunization", GetImmunizationStructure()) { Id = 8 },
                new FhirResourceType("AllergyIntolerance", GetAllergyIntoleranceStructure()) { Id = 9 },
                new FhirResourceType("DiagnosticReport", GetDiagnosticReportStructure()) { Id = 10 },
            };
        }

        public Task<IEnumerable<FhirResourceType>> GetAllResourceTypesAsync()
        {
            return Task.FromResult(_resourceTypes.AsEnumerable());
        }

        public Task<FhirResourceType> GetResourceTypeByIdAsync(int id)
        {
            return Task.FromResult(_resourceTypes.FirstOrDefault(r => r.Id == id));
        }

        public Task<FhirResourceType> GetResourceTypeByNameAsync(string name)
        {
            return Task.FromResult(_resourceTypes.FirstOrDefault(r => r.Name == name));
        }

        public async Task<string> GetResourceStructureAsync(int resourceTypeId)
        {
            var resourceType = await GetResourceTypeByIdAsync(resourceTypeId);
            return resourceType?.Structure;
        }

        private string GetPatientStructure()
        {
            return @"
            {
              ""resourceType"": ""Patient"",
              ""tableName"": """",
              ""fields"": [
                {
                  ""fhirPath"": ""id"",
                  ""emrField"": """",
                  ""dataType"": ""string"",
                  ""required"": true
                },
                {
                  ""fhirPath"": ""active"",
                  ""emrField"": """",
                  ""dataType"": ""bool"",
                  ""required"": true
                },
                {
                  ""fhirPath"": ""maritalStatus"",
                  ""required"": true,
                  ""template"": ""codeableConcept"",
                  ""emrField"": """",
                  ""valueSet"": {
                    ""system"": ""http://terminology.hl7.org/CodeSystem/v3-MaritalStatus"",
                    ""displayMap"": {
                      ""S"": ""Single"",
                      ""M"": ""Married"",
                      ""D"": ""Divorced"",
                      ""W"": ""Widowed""
                    }
                  }
                },
                {
                  ""fhirPath"": ""name[0]"",
                  ""required"": true,
                  ""template"": ""humanName"",
                  ""emrFieldMap"": {
                    ""given"": """",
                    ""family"": """",
                    ""prefix"": """"
                  },
                  ""defaults"": {
                    ""use"": ""official""
                  }
                },
                {
                  ""fhirPath"": ""gender"",
                  ""emrField"": """",
                  ""dataType"": ""string"",
                  ""required"": true
                },
                {
                  ""fhirPath"": ""birthDate"",
                  ""emrField"": """",
                  ""dataType"": ""date"",
                  ""format"": ""yyyy-MM-dd"",
                  ""required"": true
                },
                {
                  ""fhirPath"": ""telecom[0]"",
                  ""required"": true,
                  ""template"": ""contactPoint"",
                  ""emrFieldMap"": {
                    ""value"": """"
                  },
                  ""defaults"": {
                    ""system"": ""phone"",
                    ""use"": ""home""
                  }
                },
                {
                  ""fhirPath"": ""telecom[1]"",
                  ""template"": ""contactPoint"",
                  ""emrFieldMap"": {
                    ""value"": """"
                  },
                  ""defaults"": {
                    ""system"": ""email"",
                    ""use"": ""work""
                  }
                },
                {
                  ""fhirPath"": ""address[0]"",
                  ""required"": true,
                  ""template"": ""address"",
                  ""emrFieldMap"": {
                    ""line[0]"": """",
                    ""text"": """",
                    ""city"": """",
                    ""state"": """",
                    ""country"": """"
                  },
                  ""defaults"": {
                    ""use"": ""home"",
                    ""type"": ""physical""
                  }
                },
                {
                  ""fhirPath"": ""contact[0]"",
                  ""required"": true,
                  ""template"": ""contact"",
                  ""emrFieldMap"": {
                    ""name.family"": """",
                    ""name.given[0]"": """",
                    ""name.prefix"": """",
                    ""telecom[0].value"": """",
                    ""telecom[1].value"": """",
                    ""address.line"": """",
                    ""address.text"": """",
                    ""address.city"": """",
                    ""address.state"": """",
                    ""address.country"": """",
                    ""address.use"": ""__default__"",
                    ""address.type"": ""__default__"",
                    ""organization.reference"": """",
                    ""organization.display"": """",
                    ""gender"": """"
                  },
                  ""defaults"": {
                    ""name.use"": ""official"",
                    ""address.use"": ""home"",
                    ""address.type"": ""physical"",
                    ""telecom[0].system"": ""phone"",
                    ""telecom[0].use"": ""home"",
                    ""telecom[1].system"": ""email"",
                    ""telecom[1].use"": ""work"",
                    ""relationship"": [
                      {
                        ""coding"": [
                          {
                            ""system"": ""https://terminology.hl7.org/CodeSystem/v2-0131"",
                            ""code"": ""N"",
                            ""display"": ""Next of Kin""
                          }
                        ],
                        ""text"": ""Next of Kin""
                      },
                      {
                        ""coding"": [
                          {
                            ""system"": ""https://terminology.hl7.org/CodeSystem/v2-0131"",
                            ""code"": ""C"",
                            ""display"": ""Emergency Contact""
                          }
                        ],
                        ""text"": ""Emergency Contact""
                      }
                    ],
                    ""period"": {
                      ""start"": ""2020-01-01T00:00:00.000Z"",
                      ""end"": ""2025-01-01T00:00:00.000Z""
                    }
                  }
                },
                {
                  ""template"": ""identifier"",
                  ""fhirPath"": ""identifier[0]"",
                  ""required"": true,
                  ""emrFieldPriority"": {
                    ""national_id"": """",
                    ""bank_id"": """",
                    ""passport_id"": """",
                    ""driver_license"": """",
                    ""emr_patient_identifier"": """"
                  },
                  ""identifierTypeMap"": {
                    ""nin"": {
                      ""code"": ""NIN"",
                      ""display"": ""National Identification Number"",
                      ""system"": ""http://terminology.hl7.org/CodeSystem/v2-0203"",
                      ""text"": ""National Identification Number""
                    },
                    ""bvn"": {
                      ""code"": ""BVN"",
                      ""display"": ""Bank Verification Number"",
                      ""system"": ""http://terminology.hl7.org/CodeSystem/v2-0203"",
                      ""text"": ""Bank Verification Number""
                    },
                    ""passport_number"": {
                      ""code"": ""PPN"",
                      ""display"": ""Passport Number"",
                      ""system"": ""http://terminology.hl7.org/CodeSystem/v2-0203"",
                      ""text"": ""Passport Number""
                    },
                    ""driver_license"": {
                      ""code"": ""DL"",
                      ""display"": ""Drivers License"",
                      ""system"": ""http://terminology.hl7.org/CodeSystem/v2-0203"",
                      ""text"": ""Drivers License""
                    },
                    ""upi"": {
                      ""code"": ""UPI"",
                      ""display"": ""EMR Patient Identifier"",
                      ""system"": ""http://terminology.hl7.org/CodeSystem/v2-0203"",
                      ""text"": ""EMR Patient Identifier""
                    }
                  },
                  ""defaults"": {
                    ""use"": ""official"",
                    ""system"": ""http://hospital.smarthealth.org/patient-ids""
                  }
                },
                {
                  ""fhirPath"": ""managingOrganization"",
                  ""required"": true,
                  ""template"": ""reference"",
                  ""source"": ""environment""
                }
              ]
            }";
        }

        private string GetPatientStructure2()
        {
                        return @"
            {
              ""resourceType"": ""Patient"",
              ""id"": ""string"",
              ""identifier"": [
                {
                  ""use"": ""usual | official | temp | secondary | old"",
                  ""type"": {
                    ""coding"": [
                      {
                        ""system"": ""uri"",
                        ""code"": ""string"",
                        ""display"": ""string""
                      }
                    ],
                    ""text"": ""string""
                  },
                  ""system"": ""uri"",
                  ""value"": ""string"",
                  ""period"": {
                    ""start"": ""dateTime"",
                    ""end"": ""dateTime""
                  },
                  ""assigner"": {
                    ""display"": ""string""
                  }
                }
              ],
              ""active"": ""boolean"",
              ""name"": [
                {
                  ""use"": ""usual | official | temp | nickname | anonymous | old | maiden"",
                  ""text"": ""string"",
                  ""family"": ""string"",
                  ""given"": [""string""],
                  ""prefix"": [""string""],
                  ""suffix"": [""string""],
                  ""period"": {
                    ""start"": ""dateTime"",
                    ""end"": ""dateTime""
                  }
                }
              ],
              ""telecom"": [
                {
                  ""system"": ""phone | fax | email | pager | url | sms | other"",
                  ""value"": ""string"",
                  ""use"": ""home | work | temp | old | mobile"",
                  ""rank"": ""positiveInt"",
                  ""period"": {
                    ""start"": ""dateTime"",
                    ""end"": ""dateTime""
                  }
                }
              ],
              ""gender"": ""male | female | other | unknown"",
              ""birthDate"": ""date"",
              ""deceasedBoolean"": ""boolean"",
              ""deceasedDateTime"": ""dateTime"",
              ""address"": [
                {
                  ""use"": ""home | work | temp | old | billing"",
                  ""type"": ""postal | physical | both"",
                  ""text"": ""string"",
                  ""line"": [""string""],
                  ""city"": ""string"",
                  ""district"": ""string"",
                  ""state"": ""string"",
                  ""postalCode"": ""string"",
                  ""country"": ""string"",
                  ""period"": {
                    ""start"": ""dateTime"",
                    ""end"": ""dateTime""
                  }
                }
              ],
              ""maritalStatus"": {
                ""coding"": [
                  {
                    ""system"": ""http://terminology.hl7.org/CodeSystem/v3-MaritalStatus"",
                    ""code"": ""string"",
                    ""display"": ""string""
                  }
                ],
                ""text"": ""string""
              },
              ""multipleBirthBoolean"": ""boolean"",
              ""multipleBirthInteger"": ""integer"",
              ""photo"": [
                {
                  ""contentType"": ""string"",
                  ""language"": ""string"",
                  ""data"": ""base64Binary"",
                  ""url"": ""uri"",
                  ""size"": ""integer"",
                  ""hash"": ""base64Binary"",
                  ""title"": ""string"",
                  ""creation"": ""dateTime""
                }
              ],
              ""contact"": [
                {
                  ""relationship"": [
                    {
                      ""coding"": [
                        {
                          ""system"": ""uri"",
                          ""code"": ""string"",
                          ""display"": ""string""
                        }
                      ],
                      ""text"": ""string""
                    }
                  ],
                  ""name"": {
                    ""use"": ""official"",
                    ""text"": ""string"",
                    ""family"": ""string"",
                    ""given"": [""string""],
                    ""prefix"": [""string""]
                  },
                  ""telecom"": [
                    {
                      ""system"": ""phone | email"",
                      ""value"": ""string"",
                      ""use"": ""home | work"",
                      ""rank"": 1
                    }
                  ],
                  ""address"": {
                    ""line"": [""string""],
                    ""city"": ""string"",
                    ""state"": ""string"",
                    ""country"": ""string""
                  },
                  ""gender"": ""male | female | other | unknown"",
                  ""organization"": {
                    ""reference"": ""Organization/id"",
                    ""display"": ""string""
                  },
                  ""period"": {
                    ""start"": ""dateTime"",
                    ""end"": ""dateTime""
                  }
                }
              ],
              ""managingOrganization"": {
                ""reference"": ""Organization/id"",
                ""display"": ""string""
              }
            }";
                    }

        private string GetPatientStructure1()
        {
            return @"{
                ""resourceType"": ""Patient"",
                ""id"": """",
                ""identifier"": [],
                ""active"": true,
                ""name"": [],
                ""telecom"": [],
                ""gender"": """",
                ""birthDate"": """",
                ""address"": [],
                ""maritalStatus"": {},
                ""contact"": []
            }";
        }

        private string GetEncounterStructure()
        {
            return @"{
                ""resourceType"": ""Encounter"",
                ""id"": """",
                ""status"": """",
                ""class"": {},
                ""type"": [],
                ""subject"": {},
                ""participant"": [],
                ""period"": {},
                ""reasonCode"": [],
                ""location"": []
            }";
        }

        private string GetObservationStructure()
        {
            return @"{
                ""resourceType"": ""Observation"",
                ""id"": """",
                ""status"": ""final"",
                ""category"": [],
                ""code"": {},
                ""subject"": {},
                ""effectiveDateTime"": """",
                ""valueQuantity"": {},
                ""interpretation"": []
            }";
        }

        private string GetConditionStructure()
        {
            return @"{
                ""resourceType"": ""Condition"",
                ""id"": """",
                ""clinicalStatus"": {},
                ""verificationStatus"": {},
                ""category"": [],
                ""code"": {},
                ""subject"": {},
                ""onsetDateTime"": """"
            }";
        }

        private string GetMedicationStructure()
        {
            return @"{
                ""resourceType"": ""Medication"",
                ""id"": """",
                ""code"": {},
                ""status"": """",
                ""manufacturer"": {},
                ""form"": {},
                ""amount"": {},
                ""ingredient"": []
            }";
        }

        private string GetMedicationRequestStructure()
        {
            return @"{
                ""resourceType"": ""MedicationRequest"",
                ""id"": """",
                ""status"": """",
                ""intent"": """",
                ""medicationCodeableConcept"": {},
                ""subject"": {},
                ""authoredOn"": """",
                ""requester"": {},
                ""dosageInstruction"": []
            }";
        }

        private string GetProcedureStructure()
        {
            return @"{
                ""resourceType"": ""Procedure"",
                ""id"": """",
                ""status"": """",
                ""code"": {},
                ""subject"": {},
                ""performedDateTime"": """",
                ""performer"": [],
                ""location"": {}
            }";
        }

        private string GetImmunizationStructure()
        {
            return @"{
                ""resourceType"": ""Immunization"",
                ""id"": """",
                ""status"": """",
                ""vaccineCode"": {},
                ""patient"": {},
                ""occurrenceDateTime"": """",
                ""site"": {},
                ""route"": {}
            }";
        }

        private string GetAllergyIntoleranceStructure()
        {
            return @"{
                ""resourceType"": ""AllergyIntolerance"",
                ""id"": """",
                ""clinicalStatus"": {},
                ""type"": """",
                ""category"": [],
                ""criticality"": """",
                ""code"": {},
                ""patient"": {},
                ""onsetDateTime"": """"
            }";
        }

        private string GetDiagnosticReportStructure()
        {
            return @"{
                ""resourceType"": ""DiagnosticReport"",
                ""id"": """",
                ""status"": """",
                ""category"": [],
                ""code"": {},
                ""subject"": {},
                ""effectiveDateTime"": """",
                ""issued"": """",
                ""result"": []
            }";
        }
    }
}
