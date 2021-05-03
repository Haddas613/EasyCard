using Microsoft.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Business.Entities;
using Transactions.Shared.Enums;

namespace Transactions.Api.Services
{
    public class MetricsService : IMetricsService
    {
        private readonly TelemetryClient telemetry;

        public MetricsService(TelemetryClient telemetry)
        {
            this.telemetry = telemetry;
        }

        public void TrackTransactionEvent(PaymentTransaction paymentTransaction, TransactionOperationCodesEnum eventName)
        {
            try
            {
                telemetry.TrackEvent(
                    eventName: eventName.ToString(),
                    properties: new Dictionary<string, string>() { { nameof(paymentTransaction.PaymentTransactionID), paymentTransaction.PaymentTransactionID.ToString() } },
                    metrics: new Dictionary<string, double>() { { nameof(paymentTransaction.TransactionAmount), (double)paymentTransaction.TransactionAmount } }
                    );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to write telemetry: {ex.Message}");
            }
        }
    }
}
