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
    }
}
