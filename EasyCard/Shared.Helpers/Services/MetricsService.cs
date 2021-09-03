using Microsoft.ApplicationInsights;
using Shared.Helpers.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shared.Helpers.Services
{
    public class MetricsService : IMetricsService
    {
        private readonly TelemetryClient telemetry;
        private readonly ApplicationInsightsSettings config;

        public MetricsService(ApplicationInsightsSettings config)
        {
            var appInsightsConfiguration = new Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration
            {
                InstrumentationKey = config.InstrumentationKey
            };

            this.config = config;
            this.telemetry = new TelemetryClient(appInsightsConfiguration);
        }

        public void TrackEvent(string eventName, Dictionary<string, string> properties, Dictionary<string, double> metrics)
        {
            try
            {
                telemetry.TrackEvent(
                    eventName: eventName.ToString(),
                    properties: properties,
                    metrics: metrics
                    );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to write telemetry: {ex.Message}");
            }
        }
    }
}
