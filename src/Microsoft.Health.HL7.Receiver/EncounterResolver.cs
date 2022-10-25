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
        //Only in-patients are accepted for the moment, add recuring, planned and ememrgency encounters in the future
        //FIRST VERSION: No planned admits, HL7 encounter needs to be later than any encounter on the server
        //is this needed by the HL7 protocol (??), can't change something 'in between'.
        //So not a problem if the messages are received in order (which is garaunteed by socket comm??)

        //We need the ADT HL7 event type to know what needs to be doen (esp. for things like cancel (A12, ..))
        //This means that we probably only need to modify the last known encounter on the server to fix the data we know now but didn't when then initial HL7 event arrived

        //WARNING: all of the above means that this must be run in sequence, so no parralelisation!!!!!



        public static void AddEntryToBundle(ref Bundle bundle, Resource resourceToAdd)
        {
            var blentry = new Bundle.EntryComponent();
            blentry.Resource = resourceToAdd;
            blentry.FullUrl = $"urn:uuid:{resourceToAdd.Id}";
            blentry.Request = new Bundle.RequestComponent()
            {
                Method = Bundle.HTTPVerb.PUT,
                Url = "Encounter/" + resourceToAdd.Id
            };
            bundle.Entry.Add(blentry);
        }

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
            //this is needed to ensure hl7 messgaes were received in the right order
            //if they were not received in the right order we either ignore this encounter or
            //(better) we store the encounter if figure out what to do by reparding the full list of encounters
            if (lastEncounterOnServer != null && currentEncounter != null &&
                lastEncounterOnServer.Period.StartElement.IsValid() &&
                currentEncounter.Period.StartElement.IsValid() &&
                lastEncounterOnServer.Period.StartElement < currentEncounter.Period.StartElement)
            {
                var lastEncounterCopy = lastEncounterOnServer.DeepCopy() as Encounter;
                Bundle.HTTPVerb action = Bundle.HTTPVerb.PUT;
                Console.WriteLine($"*************************** {adtEvent} ************************");
                switch (adtEvent)
                {
                    case "ADT_A13": //cancel discharge
                        //previous encunter should be 'finished' to indicate discharge, keep this one active (could be location chngne, so also treat as transfer)
                        //change end time of encounter before the discharge (if is exists) to starttime of this (cancel discharge) encounter

                        //CHECK STATUS = finished
                        lastEncounterCopy.Status = Encounter.EncounterStatus.Cancelled;
                        if (beforeLastEncounterOnServer != null)
                        {
                            //fixup end time prev encounter to ongoing (we are in sync, there is nothing in the furure so far)
                            var beforeLastEncounterCopy = beforeLastEncounterOnServer.DeepCopy() as Encounter;
                            beforeLastEncounterCopy.Period.EndElement = currentEncounter.Period.StartElement;

                            AddEntryToBundle(ref bundle, beforeLastEncounterCopy);
                        }
                        break;
                    case "ADT_A12": //cancel transfer
                        //cancel previous encounter (the one to cancel with A12)
                        //remove end time of encounter before the one cancelled (if is exists)

                        //CHECK STATUS = in-progress
                        lastEncounterCopy.Status = Encounter.EncounterStatus.Cancelled;
                        if (beforeLastEncounterOnServer != null)
                        {
                            //fixip end time prev encounter to ongoing (we are in sync, there is nothing in the furure so far)
                            var beforeLastEncounterCopy = beforeLastEncounterOnServer.DeepCopy() as Encounter;
                            beforeLastEncounterCopy.Period.EndElement = null;

                            AddEntryToBundle(ref bundle,beforeLastEncounterCopy);
                        }
                        break;
                    case "ADT_A11": //cancel visit
                    case "ADT_A23": //delete visit
                        //set all active encounters to cancelled (does this event also imply cancelling non IMP encounters??)
                        foreach(var encToCancel in encountersOnserver)
                        {
                            if (encToCancel.Id != lastEncounterCopy.Id)
                            {
                                var tempEncounter = encToCancel.DeepCopy() as Encounter;
                                tempEncounter.Status = Encounter.EncounterStatus.Cancelled;
                                AddEntryToBundle(ref bundle, tempEncounter);
                            }
                            else
                            {
                                lastEncounterCopy.Status = Encounter.EncounterStatus.Cancelled;
                            }
                        }
                        break;
                    default:
                        //fixup end time prev encounter to start of current encounter
                        lastEncounterCopy.Period.EndElement = currentEncounter.Period.StartElement;

                        break;
                }

                AddEntryToBundle(ref bundle, lastEncounterCopy);
            }
            return bundle;
        }
    }
}
