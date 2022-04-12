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

        public static Task RaiseInvoiceEvent(this IEventsService ms, Invoice invoice, string eventName, string errorMessage = null)
        {
            try
            {
                return ms.Raise(
                    new CustomEvent
                    {
                        EventID = Guid.NewGuid().GetSequentialGuid(DateTime.UtcNow),
                        MerchantID = invoice.MerchantID,
                        TerminalID = invoice.TerminalID,
                        CorrelationId = invoice.CorrelationId,
                        Entity = invoice,
                        EventName = eventName,
                        IsFailureEvent = false,
                        EntityExternalReference = invoice.DealDetails?.DealReference,
                        EntityReference = invoice.InvoiceID.ToString(),
                        EntityType = nameof(Invoice),
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

        public static Task RaiseBillingEvent(this IEventsService ms, BillingDeal billingDeal, string eventName, string errorMessage = null)
        {
            try
            {
                return ms.Raise(
                    new CustomEvent
                    {
                        EventID = Guid.NewGuid().GetSequentialGuid(DateTime.UtcNow),
                        MerchantID = billingDeal.MerchantID,
                        TerminalID = billingDeal.TerminalID,
                        CorrelationId = billingDeal.CorrelationId,
                        Entity = billingDeal,
                        EventName = eventName,
                        IsFailureEvent = false,
                        EntityExternalReference = billingDeal.DealDetails?.DealReference,
                        EntityReference = billingDeal.BillingDealID.ToString(),
                        EntityType = nameof(BillingDeal),
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
