// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using DotLiquid;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Models.Hl7v2;
using Microsoft.Health.Fhir.Liquid.Converter.Parsers;

namespace Microsoft.Health.Fhir.Liquid.Converter.Processors
{
    public class Hl7v2Processor : BaseProcessor
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
            { "ADT^A12", "ADT_A02" },
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

        private readonly IDataParser _parser = new Hl7v2DataParser();

        public Hl7v2Processor(ProcessorSettings processorSettings = null)
            : base(processorSettings)
        {
        }

        protected override string DataKey { get; set; } = "hl7v2Data";

        public override string Convert(string data, string rootTemplate, ITemplateProvider templateProvider, TraceInfo traceInfo = null)
        {
            try
            {
                var hl7v2Data = _parser.Parse(data);
                if (hl7v2Data is Hl7v2Data)
                {
                    var d = hl7v2Data as Hl7v2Data;
                    var mshLine = (d.Data.FirstOrDefault(x => (x.Fields[0] as Hl7v2Field).Value == "MSH") as Hl7v2Segment);
                    if (mshLine != null && mshLine.Fields.Count > 9)
                    {
                        var msgType = (mshLine.Fields[9] as Hl7v2Field).Value;
                        if (rootTemplate == null && templateMapping.ContainsKey(msgType))
                        {
                            rootTemplate = templateMapping[msgType];
                        }
                        if (rootTemplate != null)
                        {
                            return Convert(hl7v2Data, rootTemplate, templateProvider, traceInfo);
                        }
                    }
                }
            }
            catch
            {

            }
            return null;
        }

        protected override Context CreateContext(ITemplateProvider templateProvider, IDictionary<string, object> data)
        {
            // Load code system mapping
            var context = base.CreateContext(templateProvider, data);
            var codeMapping = templateProvider.GetTemplate("CodeSystem/CodeSystem");
            if (codeMapping?.Root?.NodeList?.First() != null)
            {
                context["CodeMapping"] = codeMapping.Root.NodeList.First();
            }

            return context;
        }

        protected override void CreateTraceInfo(object data, TraceInfo traceInfo)
        {
            if (traceInfo is Hl7v2TraceInfo hl7v2TraceInfo)
            {
                hl7v2TraceInfo.UnusedSegments = Hl7v2TraceInfo.CreateTraceInfo(data as Hl7v2Data).UnusedSegments;
            }
        }
    }
}
