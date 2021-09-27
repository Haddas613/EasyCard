using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Helpers.Configuration
{
    /// <summary>
    /// Custom application insights class. To be injected into UI config
    /// </summary>
    public class ApplicationInsightsSettings
    {
        public string InstrumentationKey { get; set; }

        public string AppInsightsApi { get; set; }

        public string AppInsightsApiAction { get; set; }

        public string AiAppId { get; set; }

        public string AiAppKey { get; set; }
    }
}
