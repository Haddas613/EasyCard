using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Helpers.Events
{
    public interface IEventProcessor
    {
        Task ProcessEvent(CustomEvent evnt);

        bool CanProcess(CustomEvent evnt);
    }
}
