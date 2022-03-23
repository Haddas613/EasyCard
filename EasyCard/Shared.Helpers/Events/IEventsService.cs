using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Helpers.Events
{
    public interface IEventsService
    {
        Task Raise(CustomEvent evnt);
    }
}
