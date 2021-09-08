using Shared.Helpers.Services;
using Shared.Helpers.Sms;
using Shared.Integration;
using System;
using System.Collections.Generic;
using System.Text;

namespace InforU.Extensions
{
    public static class MetricsServiceExtensions
    {
        public static void TrackSmsEvent(this IMetricsService ms, bool success, SmsMessage message)
        {
            try
            {
                ms.TrackEvent(
                    eventName: success ? Metrics.SmsSent : Metrics.SmsError,
                    properties: new Dictionary<string, string>() {
                        { nameof(message.MerchantID), message.MerchantID?.ToString() },
                        { nameof(message.TerminalID), message.TerminalID?.ToString() },
                        { nameof(message.MessageId), message.MessageId },
                    },
                    metrics: new Dictionary<string, double>() { }
                    );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to write telemetry: {ex.Message}");
            }
        }
    }
}
