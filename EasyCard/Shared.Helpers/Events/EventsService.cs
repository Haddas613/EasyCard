using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Helpers.Events
{
    public class EventsService : IEventsService
    {
        private readonly IEnumerable<IEventProcessor> eventProcessors;

        public EventsService(params IEventProcessor[] eventProcessors)
        {
            this.eventProcessors = eventProcessors;
        }

        public Task Raise(CustomEvent evnt)
        {
            return eventProcessors.Where(d => d.CanProcess(evnt)).ForEachAsync(d => d.ProcessEvent(evnt));
        }
    }
}
