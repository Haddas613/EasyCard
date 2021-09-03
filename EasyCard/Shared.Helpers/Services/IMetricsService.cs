using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shared.Helpers.Services
{
    public interface IMetricsService
    {
        void TrackEvent(string eventName, Dictionary<string, string> properties, Dictionary<string, double> metrics);
    }
}
