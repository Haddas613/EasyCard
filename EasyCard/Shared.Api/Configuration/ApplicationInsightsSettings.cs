using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Api.Configuration
{
    /// <summary>
    /// Custom application insights class. To be injected into UI config
    /// </summary>
    public class ApplicationInsightsSettings
    {
        public string InstrumentationKey { get; set; }
    }
}
