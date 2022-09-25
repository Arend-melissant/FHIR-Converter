// See https://aka.ms/new-console-template for more information
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Microsoft.Health.Fhir.Client;

Console.WriteLine("Hello, World!");

////string bundleJson = File.ReadAllText(@"C:\Calidos\HL7\DataFolder\HL7Out\data05x.json");
//FihrBundle.Upload("https://localhost:44348/", bundleJson);

var client = new FhirClient("https://localhost:44348/");//, settings);
var sp = new SearchParams()
           //.Where("name:exact=ewout")
           //.OrderBy("birthdate", SortOrder.Descending)
           .SummaryOnly().Include("Encounter:location")
           .LimitTo(40);
var result = client.Search<Encounter>(sp);
foreach(var enc in result.Entry)
{
    if (enc.Resource is Encounter)
    {
        Console.WriteLine((enc.Resource as Encounter).Identifier.FirstOrDefault(new Identifier() { Value = "0" }).Value);
    }
    
}

Console.ReadLine();