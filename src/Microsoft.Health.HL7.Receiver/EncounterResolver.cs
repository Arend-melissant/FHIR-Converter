using Hl7.Fhir.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Health.HL7.Receiver
{
    public static class EncounterResolver
    {
        //Need to resolve Encounter from HL7 message with the encounters from the server
        //FIRST VERSION: No planned admits, HL7 encounter needs to be later than any encounter on the server
        //is this needed by the HL7 protocol (??), can't change something 'in between'.
        //So not a problem if the messages are received in order (which is garaunteed by socket comm??)

        //We need the ADT HL7 event type to know what needs to be doen (esp. for things like cancel (A12, ..))
        //This means that we probably only need to modify the last known encounter on the server to fix the data we know now but didn't when then initial HL7 event arrived

        //WARNING: all of the above means that this must be run in sequence, so no parralelisation!!!!!

        public static async Task<Bundle> Resolve(EncountersRepository hl7repo, EncountersRepository serverrepo, string adtEvent)
        {
            Bundle bundle = hl7repo.CopyBundle();
            var encountersOnserver = serverrepo.Encounters.ToList();
            int encounterCountOnServer = encountersOnserver.Count;

            Encounter lastEncounterOnServer = null;
            Encounter beforeLastEncounterOnServer = null;

            if (encounterCountOnServer > 0)
            {
                lastEncounterOnServer = encountersOnserver[encounterCountOnServer - 1];
            }

            if (encounterCountOnServer > 1)
            {
                beforeLastEncounterOnServer = encountersOnserver[encounterCountOnServer - 2];
            }

            var currentEncounter = hl7repo.Encounters.FirstOrDefault();
            //we need at least one server encounter which is before the hl7 encounter
            //start time on both encounters needs be not null
            if (lastEncounterOnServer != null && currentEncounter != null && 
                lastEncounterOnServer.Period.StartElement.IsValid() &&
                currentEncounter.Period.StartElement.IsValid() &&
                lastEncounterOnServer.Period.StartElement < currentEncounter.Period.StartElement)
            {
                var lastEncounterCopy = lastEncounterOnServer.DeepCopy() as Encounter;
                Bundle.HTTPVerb action = Bundle.HTTPVerb.PUT;

                switch (adtEvent)
                {
                    case "ADT_A12":
                        //action = Bundle.HTTPVerb.DELETE;
                        lastEncounterCopy.Status = Encounter.EncounterStatus.Cancelled;
                        if (beforeLastEncounterOnServer != null)
                        {
                            var beforeLastEncounterCopy = beforeLastEncounterOnServer.DeepCopy() as Encounter;
                            beforeLastEncounterCopy.Period.EndElement = null;
                            var blentry = new Bundle.EntryComponent();
                            blentry.Resource = beforeLastEncounterCopy;
                            blentry.FullUrl = $"urn:uuid:{beforeLastEncounterCopy.Id}";
                            blentry.Request = new Bundle.RequestComponent()
                            {
                                Method = Bundle.HTTPVerb.PUT,
                                Url = "Encounter/" + beforeLastEncounterCopy.Id
                            };
                            bundle.Entry.Add(blentry);
                        }
                        break;
                    default:
                        lastEncounterCopy.Period.EndElement = currentEncounter.Period.StartElement;
                        break;
                }

                var entry = new Bundle.EntryComponent();
                entry.Resource = lastEncounterCopy;
                entry.FullUrl = $"urn:uuid:{lastEncounterCopy.Id}";
                entry.Request = new Bundle.RequestComponent()
                {
                    Method = action,
                    Url = "Encounter/" + lastEncounterCopy.Id
                };
                bundle.Entry.Add(entry);
            }
            return bundle;
        }
    }
}
