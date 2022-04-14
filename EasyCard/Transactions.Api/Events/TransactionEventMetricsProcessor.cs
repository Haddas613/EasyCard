using Shared.Helpers.Events;
using Shared.Helpers.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Business.Entities;

namespace Transactions.Api.Events
{
    public class TransactionEventMetricsProcessor : IEventProcessor
    {
        private readonly IMetricsService ms;

        public TransactionEventMetricsProcessor(IMetricsService ms)
        {
            this.ms = ms;
        }

        public bool CanProcess(CustomEvent evnt)
        {
            return evnt.Entity is PaymentTransaction;
        }

        public Task ProcessEvent(CustomEvent evnt)
        {
            var paymentTransaction = evnt.Entity as PaymentTransaction;

            try
            {
                ms.TrackEvent(
                    eventName: evnt.EventName,
                    properties: new Dictionary<string, string>()
                    {
                        { nameof(evnt.EventID), evnt.EventID.ToString() },
                        { nameof(paymentTransaction.PaymentTransactionID), paymentTransaction.PaymentTransactionID.ToString() },
                        { nameof(paymentTransaction.CorrelationId), paymentTransaction.CorrelationId },
                        { nameof(paymentTransaction.MerchantID), paymentTransaction.MerchantID.ToString() },
                        { nameof(paymentTransaction.TerminalID), paymentTransaction.TerminalID.ToString() },
                    },
                    metrics: new Dictionary<string, double>() { { nameof(paymentTransaction.TransactionAmount), (double)paymentTransaction.TransactionAmount } }
                    );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to write telemetry: {ex.Message}");
            }

            return Task.FromResult(true);
        }
    }
}
