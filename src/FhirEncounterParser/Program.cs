

using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using System.Text.Json;

string jsonInput = File.ReadAllText("testData01.json");
HttpClient client = new HttpClient();


while (true)
{

    try
    {
        HttpResponseMessage response = await client.GetAsync("https://localhost:44348/Encounter?_count=400");
        response.EnsureSuccessStatusCode();
        string responseBody = await response.Content.ReadAsStringAsync();
        // Above three lines can be replaced with new helper method below
        // string responseBody = await client.GetStringAsync(uri);

        jsonInput = responseBody;

        var options = new JsonSerializerOptions().ForFhir(typeof(Bundle).Assembly);
        var bundle = JsonSerializer.Deserialize<Bundle>(jsonInput, options);

        var encounters = bundle.GetResources().Where(x => x.TypeName == "Encounter").Select(x => x as Encounter).ToList();
        var hospUids = encounters.Where(x => x.Identifier != null && x.Identifier.Any()).Select(x => x.Identifier.FirstOrDefault().Value).Distinct().ToList();

        foreach (var hospUid in hospUids)
        {
            var encForHospUid = encounters.Where(x => x.Identifier != null && x.Identifier.Any(h => h.Value == hospUid));
            Console.WriteLine($"Hospstay {hospUid}");
            Console.WriteLine($"------------------------------------------------------------------------------------------");
            foreach (var enc in encForHospUid)
            {
                Console.WriteLine($"id: {enc.Identifier.FirstOrDefault()?.Value} - {enc.Status} - {enc.Period.Start ?? "NA"} - {enc.Period.End ?? "NA"} - {enc.Location.FirstOrDefault().Location.Reference}");
            }
            Console.WriteLine($"------------------------------------------------------------------------------------------");
        }
    }
    catch (HttpRequestException e)
    {
        Console.WriteLine("\nException Caught!");
        Console.WriteLine("Message :{0} ", e.Message);
    }

    Console.Read();
}