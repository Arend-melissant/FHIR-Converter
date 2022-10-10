using Hl7.Fhir.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Health.HL7.Receiver
{
    public static class FhirExtensions
    {
        public static bool IsValid(this FhirDateTime value)
        {
            return !string.IsNullOrEmpty(value.Value);
        }
    }
}
