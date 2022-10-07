using Microsoft.Health.Fhir.Liquid.Converter;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Models.Hl7v2;
using Microsoft.Health.Fhir.Liquid.Converter.Parsers;
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
        static Dictionary<string, string> templateMapping = new Dictionary<string, string>
        {
            { "ADT^A01", "ADT_A01" },
            { "ADT^A02", "ADT_A02" },
            { "ADT^A03", "ADT_A03" },
            { "ADT^A04", "ADT_A04" },
            { "ADT^A05", "ADT_A05" },
            { "ADT^A06", "ADT_A06" },
            { "ADT^A07", "ADT_A07" },
            { "ADT^A08", "ADT_A08" },
            { "ADT^A09", "ADT_A09" },
            { "ADT^A10", "ADT_A10" },
            { "ADT^A11", "ADT_A11" },
            { "ADT^A12", "ADT_A12" },
            { "ADT^A13", "ADT_A13" },
            { "ADT^A14", "ADT_A14" },
            { "ADT^A15", "ADT_A15" },
            { "ADT^A16", "ADT_A16" },
            { "ADT^A25", "ADT_A25" },
            { "ADT^A26", "ADT_A26" },
            { "ADT^A27", "ADT_A27" },
            { "ADT^A28", "ADT_A28" },
            { "ADT^A29", "ADT_A29" },
            { "ADT^A31", "ADT_A31" },
            { "ADT^A40", "ADT_A40" },
            { "ADT^A41", "ADT_A41" },
            { "ADT^A45", "ADT_A45" },
            { "ADT^A47", "ADT_A47" },
        };

        private static readonly IDataParser _parser = new Hl7v2DataParser();

        public static string getRootTemplate(string hl7Data)
        {
            string rootTemplate = string.Empty;
            try
            {
                var hl7v2Data = _parser.Parse(hl7Data);
                if (hl7v2Data is Hl7v2Data)
                {
                    var d = hl7v2Data as Hl7v2Data;
                    var mshLine = (d.Data.FirstOrDefault(x => (x.Fields[0] as Hl7v2Field).Value == "MSH") as Hl7v2Segment);
                    if (mshLine != null && mshLine.Fields.Count > 9)
                    {
                        var msgType = (mshLine.Fields[9] as Hl7v2Field).Value;
                        if (templateMapping.ContainsKey(msgType))
                        {
                            rootTemplate = templateMapping[msgType];
                        }                        
                    }
                }
            }
            catch
            {

            }
            return rootTemplate;
        }

        //C:\Calidos\HL7\FHIR-Converter-main\bin\Hl7v2DefaultTemplates
        public static string convert(string hl7, string templateDirectory)
        {
            var templateProvider = new TemplateProvider(templateDirectory, DataType.Hl7v2);
            var traceInfo = CreateTraceInfo(DataType.Hl7v2, false);
            var dataProcessor = new Hl7v2Processor(new ProcessorSettings()
            {                
            });


            string rootTemplate = getRootTemplate(hl7);

            if (!string.IsNullOrEmpty(rootTemplate))
            {
                return dataProcessor.Convert(hl7, rootTemplate, templateProvider, traceInfo);
            }
            else
            {
                return string.Empty;
            }
        }

        private static TraceInfo CreateTraceInfo(DataType dataType, bool isTraceInfo)
        {
            return isTraceInfo ? (dataType == DataType.Hl7v2 ? new Hl7v2TraceInfo() : new TraceInfo()) : null;
        }
    }
}
