using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace Microsoft.Health.HL7.Receiver
{
    /// ADD EXCEPTION HANDLING
    public class EncountersRepository
    {
        Bundle _bundle;

        public static async Task<EncountersRepository> CreateRepository(string bundleJson)
        {
            EncountersRepository repo = new EncountersRepository();
            repo._bundle = await DeserializeBundleAsync(bundleJson);
            return repo;
        }

        public static async Task<EncountersRepository> CreateRepository(string fhirServer, string patientReference)
        {
            EncountersRepository repo = new EncountersRepository();

            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync($"{fhirServer}/Encounter?subject={patientReference}");
            response.EnsureSuccessStatusCode();
            string bundleJson = await response.Content.ReadAsStringAsync();

            repo._bundle = await DeserializeBundleAsync(bundleJson);
            return repo;
        }

        /// <summary>
        /// Deserialize the json bunlde coming into the repo via string or server
        /// MOVE TO UTILS
        /// </summary>
        /// <param name="bundleJson"></param>
        /// <returns></returns>
        public static async Task<Bundle> DeserializeBundleAsync(string bundleJson)
        {
            var options = new JsonSerializerOptions().ForFhir(typeof(Bundle).Assembly);
            var bundle = JsonSerializer.Deserialize<Bundle>(bundleJson, options);
            return bundle;
        }

        /// <summary>
        /// serialize a bundle
        /// MOVE TO UTILS
        /// </summary>
        /// <param name="bundle"></param>
        /// <returns></returns>
        public static async Task<string> SerializeBundleAsync(Bundle bundle)
        {
            string bundleJson;

            var serializer = new FhirJsonSerializer(new SerializerSettings()
            {
                Pretty = true
            });

            bundleJson = serializer.SerializeToString(bundle);
            //var options = new JsonSerializerOptions().ForFhir(typeof(Bundle).Assembly);
            //bundleJson = JsonSerializer.Serialize<Bundle>(bundle);
            return bundleJson;
        }

        public Bundle CopyBundle()
        {
            return _bundle.DeepCopy() as Bundle;
        }

        public IEnumerable<string> PatientReferences
        {
            get
            {
                return Encounters.Select(x => x.Subject.Reference);
            }           
        }

        public IEnumerable<Encounter> Encounters
        {
            get
            {
                return _bundle.GetResources().Where(x => x.TypeName == "Encounter").Select(x => x as Encounter).OrderBy(x => x.Period.Start);
            }
        }
        public IEnumerable<Location> Locations
        {
            get
            {
                return _bundle.GetResources().Where(x => x.TypeName == "Location").Select(x => x as Location); 
            }
        }
    }
}
