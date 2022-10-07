using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Health.HL7.Receiver
{
    public sealed class Settings
    {
        public int Port { get; set; }
        public bool ConvertToFhir { get; set; }
        public bool SendToServer { get; set; }

        public string FhirServer { get; set; }
        public string TemplatePath { get; set; }
        public string OutputPath { get; set; }
        
    }
}
