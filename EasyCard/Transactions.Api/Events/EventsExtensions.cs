using Microsoft.ApplicationInsights;
using Shared.Helpers;
using Shared.Helpers.Events;
using Shared.Helpers.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Business.Entities;
using Transactions.Shared.Enums;

namespace Transactions.Api.Services
{
    public static class EventsExtensions
    {
        public static Task RaiseTransactionEvent(this IEventsService ms, PaymentTransaction paymentTransaction, string eventName, string errorMessage = null)
        {
            try
            {
                return ms.Raise(
                    new CustomEvent
                    {
                        EventID = Guid.NewGuid().GetSequentialGuid(DateTime.UtcNow),
                        MerchantID = paymentTransaction.MerchantID,
                        TerminalID = paymentTransaction.TerminalID,
                        CorrelationId = paymentTransaction.CorrelationId,
                        Entity = paymentTransaction,
                        EventName = eventName,
                        IsFailureEvent = false,
                        EntityExternalReference = paymentTransaction.DealDetails?.DealReference,
                        EntityReference = paymentTransaction.PaymentTransactionID.ToString(),
                        EntityType = nameof(PaymentTransaction),
                        EventTimestamp = DateTime.UtcNow,
                        ErrorMesage = errorMessage
                    }
                    );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to process event: {ex.Message}");
                return Task.FromResult(true);
            }
        }
    }
}
