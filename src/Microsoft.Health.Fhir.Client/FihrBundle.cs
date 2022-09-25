namespace Microsoft.Health.Fhir.Client
{
    using Hl7.Fhir.Model;
    using Hl7.Fhir.Rest;
    using Hl7.Fhir.Serialization;

    public static class FihrBundle
    {
        public static void Upload(string server, string bundleString)
        {
            FhirJsonParser fjp = new FhirJsonParser(); 
            Hl7.Fhir.Model.Bundle bundle = fjp.Parse<Hl7.Fhir.Model.Bundle>(bundleString);

            var settings = new FhirClientSettings
            {
                Timeout = 0,
                PreferredFormat = ResourceFormat.Json,
                VerifyFhirVersion = true,
                PreferredReturn = Prefer.ReturnMinimal,
            };
            var client = new FhirClient(server);//, settings);
            var result = client.Transaction(bundle);
        }
    }
}