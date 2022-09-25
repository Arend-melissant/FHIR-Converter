using Microsoft.Health.Fhir.Liquid.Converter;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Models.Hl7v2;
using Microsoft.Health.Fhir.Liquid.Converter.Processors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Health.HL7.Receiver
{
    public static class Hl7TemplateConverter
    {


        //C:\Calidos\HL7\FHIR-Converter-main\bin\Hl7v2DefaultTemplates
        public static string convert(string hl7, string templateDirectory)
        {
            var templateProvider = new TemplateProvider(templateDirectory, DataType.Hl7v2);
            var traceInfo = CreateTraceInfo(DataType.Hl7v2, false);
            var dataProcessor = new Hl7v2Processor();
            return dataProcessor.Convert(hl7, null, templateProvider, traceInfo);
        }

        private static TraceInfo CreateTraceInfo(DataType dataType, bool isTraceInfo)
        {
            return isTraceInfo ? (dataType == DataType.Hl7v2 ? new Hl7v2TraceInfo() : new TraceInfo()) : null;
        }
    }
}
