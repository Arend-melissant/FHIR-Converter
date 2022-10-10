using FhirModel = Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using System.Text.Json;

namespace Microsoft.Health.HL7.Receiver
{
    public static class BundleLogger
    {
        public static async Task LogCurrentAsync(EncountersRepository repo)
        {
            var encs = repo.Encounters.ToList();
            var firstEncounter = encs.FirstOrDefault();
            if (firstEncounter != null)
            {
                Console.WriteLine("++++++++++++++++++++++++++++FROM SERVER+++++++++++++++++++++++++++++++++++++++++++++++++++");
                Console.WriteLine($"{firstEncounter.Identifier.FirstOrDefault()?.Value} - {firstEncounter.Subject?.Reference}");
                foreach (var enc in encs)
                {
                    Console.WriteLine($"------------------------------------------------------------------------------------------");
                    if (enc.Location != null && enc.Location.Any())
                    {
                        foreach (var loc in enc.Location)
                        {
                            Console.WriteLine($"{enc.Status} - {enc.Period.Start} - {enc.Period.End} - {loc.Location.Reference} ({loc.Status})");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"{enc.Status} - {enc.Period.Start} - {enc.Period.End} (no location)");
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
        public static async Task LogEncountersAsync(EncountersRepository repo)
        {
            Dictionary<string, string> map = new Dictionary<string, string>();
            foreach(var loc in repo.Locations)
            {
                map.Add("Location/" + loc.Id, loc.Description);
            }

            var encs = repo.Encounters.ToList();
            var firstEncounter = encs.FirstOrDefault();
            if (firstEncounter != null)
            {
                Console.WriteLine("+++++++++++++++++++++++++++++CURRENT++++++++++++++++++++++++++++++++++++++++++++++++++++++");
                Console.WriteLine($"{firstEncounter.Identifier.FirstOrDefault()?.Value} - {firstEncounter.Subject?.Reference}");
                foreach (var enc in encs)
                {
                    Console.WriteLine("------------------------------------------------------------------------------------------");
                    if (enc.Location != null && enc.Location.Any())
                    {
                        foreach (var loc in enc.Location)
                        {
                            Console.WriteLine($"{enc.Status} - {enc.Period.Start} - {enc.Period.End} - {/*map[*/loc.Location.Reference/*]*/} ({loc.Status})");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"{enc.Status} - {enc.Period.Start} - {enc.Period.End} (no location)");
                    }
                }
                Console.WriteLine("------------------------------------------------------------------------------------------");
                //return firstEncounter.Subject?.Reference;
            }
            else
            {
                Console.WriteLine("----------------------------------NO DATA ------------------------------------------");
            }
        }
    }
}
