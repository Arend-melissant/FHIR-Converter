using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Microsoft.Health.HL7.Receiver
{
    public static class BundleLogger
    {
        public static async System.Threading.Tasks.Task LogCurrentAsync(string fhirServer, string patientReference)
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync($"{fhirServer}/Encounter?subject={patientReference}");
            response.EnsureSuccessStatusCode();
            string bundleJson = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions().ForFhir(typeof(Bundle).Assembly);
            var bundle = JsonSerializer.Deserialize<Bundle>(bundleJson, options);

            var encounters = bundle.GetResources().Where(x => x.TypeName == "Encounter").Select(x => x as Encounter);

            var firstEncounter = encounters.FirstOrDefault();
            if (firstEncounter != null)
            {
                Console.WriteLine("++++++++++++++++++++++++++++FROM SERVER+++++++++++++++++++++++++++++++++++++++++++++++++++");
                Console.WriteLine($"{firstEncounter.Identifier.FirstOrDefault()?.Value} - {firstEncounter.Subject?.Reference}");
                foreach (var enc in encounters)
                {
                    Console.WriteLine($"------------------------------------------------------------------------------------------");
                    if (enc.Location != null && enc.Location.Any())
                    {
                        foreach (var loc in enc.Location)
                        {
                            Console.WriteLine($"{enc.Status} - {enc.Period.Start ?? "NA"} - {enc.Period.End ?? "NA"} - {loc.Location.Reference} ({loc.Status})");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"{enc.Status} - {enc.Period.Start ?? "NA"} - {enc.Period.End ?? "NA"} (no location)");
                    }
                }
                Console.WriteLine($"------------------------------------------------------------------------------------------");
            }
            else
            {
                Console.WriteLine("----------------------------------NO DATA ------------------------------------------");
            }
        }

        /// <summary>
        /// Logs current encounters
        /// </summary>
        /// <param name="bundleJson"></param>
        /// <returns>currect patient ref</returns>
        public static string LogEncounters(string bundleJson)
        {
            var options = new JsonSerializerOptions().ForFhir(typeof(Bundle).Assembly);
            var bundle = JsonSerializer.Deserialize<Bundle>(bundleJson, options);

            var encounters = bundle.GetResources().Where(x => x.TypeName == "Encounter").Select(x => x as Encounter);
            var locations = bundle.GetResources().Where(x => x.TypeName == "Location").Select(x => x as Location);
            //var hospUids = encounters.Where(x => x.Identifier != null && x.Identifier.Any()).Select(x => x.Identifier.FirstOrDefault().Value).Distinct();

            Dictionary<string, string> map = new Dictionary<string, string>();
            foreach(var loc in locations)
            {
                map.Add("Location/" + loc.Id, loc.Description);
            }

            var firstEncounter = encounters.FirstOrDefault();
            if (firstEncounter != null)
            {
                Console.WriteLine("+++++++++++++++++++++++++++++CURRENT++++++++++++++++++++++++++++++++++++++++++++++++++++++");
                Console.WriteLine($"{firstEncounter.Identifier.FirstOrDefault()?.Value} - {firstEncounter.Subject?.Reference}");
                foreach (var enc in encounters)
                {
                    Console.WriteLine("------------------------------------------------------------------------------------------");
                    if (enc.Location != null && enc.Location.Any())
                    {
                        foreach (var loc in enc.Location)
                        {
                            Console.WriteLine($"{enc.Status} - {enc.Period.Start ?? "NA"} - {enc.Period.End ?? "NA"} - {/*map[*/loc.Location.Reference/*]*/} ({loc.Status})");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"{enc.Status} - {enc.Period.Start ?? "NA"} - {enc.Period.End ?? "NA"} (no location)");
                    }
                }
                Console.WriteLine("------------------------------------------------------------------------------------------");
                return firstEncounter.Subject?.Reference;
            }
            else
            {
                Console.WriteLine("----------------------------------NO DATA ------------------------------------------");
                return string.Empty;
            }
        }
    }
}
