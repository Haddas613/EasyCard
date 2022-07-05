using Microsoft.EntityFrameworkCore;
using Shared.Api.Extensions.Filtering;
using Shared.Helpers;
using Shared.Helpers.Models;
using System;
using System.Linq;
using Transactions.Api.Models.Transactions;
using Transactions.Business.Entities;
using Transactions.Shared.Enums;
using SharedIntegration = Shared.Integration;

namespace Transactions.Api.Extensions.Filtering
{
    public static class TransactionFilteringExtensions
    {
        public static IQueryable<PaymentTransaction> Filter(this IQueryable<PaymentTransaction> src, TransactionsFilter filter)
        {
            if (filter.PaymentTransactionID != null)
            {
                src = src.Where(t => t.PaymentTransactionID == filter.PaymentTransactionID);
                return src;
            }

            if (filter.InitialTransactionID != null)
            {
                src = src.Where(t => t.InitialTransactionID == filter.InitialTransactionID);
            }

            if (!string.IsNullOrWhiteSpace(filter.ShvaDealIDLastDigits))
            {
                src = src.Where(t => EF.Functions.Like(t.ShvaTransactionDetails.ShvaDealID, $"%{filter.ShvaDealIDLastDigits}"));
            }

            if (!string.IsNullOrWhiteSpace(filter.PaymentTransactionIDShort))
            {
                src = src.Where(t => EF.Functions.Like(t.PaymentTransactionID.ToString(), $"{filter.PaymentTransactionIDShort}%"));
            }

            if (filter.TerminalID != null)
            {
                src = src.Where(t => t.TerminalID == filter.TerminalID);
            }

            if (filter.MerchantID != null)
            {
                src = src.Where(t => t.MerchantID == filter.MerchantID);
            }

            if (filter.AmountFrom != null && filter.AmountFrom > 0)
            {
                src = src.Where(t => t.TransactionAmount >= filter.AmountFrom);
            }

            if (filter.AmountTo != null && filter.AmountTo > 0)
            {
                src = src.Where(t => t.TransactionAmount <= filter.AmountTo);
            }

            if (filter.Currency != null)
            {
                src = src.Where(t => t.Currency == filter.Currency);
            }

            src = HandleDateFiltering(src, filter);

            // TODO: we can try to transmit transactions which are failed to transmit
            if (filter.NotTransmitted)
            {
                src = src.Where(t => t.Status == Shared.Enums.TransactionStatusEnum.AwaitingForTransmission);
            }
            else if (filter.QuickStatusFilter != null)
            {
                src = FilterByQuickStatus(src, filter.QuickStatusFilter.Value);
            }
            else if (filter.Statuses != null && filter.Statuses.Count > 0)
            {
                //TODO: use OR builder (invalid cast)
                src = src.Where(t => filter.Statuses.Contains(t.Status));
            }

            if (filter.JDealType != null)
            {
                src = src.Where(t => t.JDealType == filter.JDealType);
            }

            if (filter.TransactionType != null)
            {
                src = src.Where(t => t.TransactionType == filter.TransactionType);
            }

            if (filter.SpecialTransactionType != null)
            {
                src = src.Where(t => t.SpecialTransactionType == filter.SpecialTransactionType);
            }

            if (filter.CardPresence != null)
            {
                src = src.Where(t => t.CardPresence == filter.CardPresence);
            }

            if (!string.IsNullOrEmpty(filter.ShvaShovarNumber))
            {
                src = src.Where(t => t.ShvaTransactionDetails.ShvaShovarNumber == filter.ShvaShovarNumber);
            }

            if (!string.IsNullOrEmpty(filter.ShvaTransactionID))
            {
                src = src.Where(t => t.ShvaTransactionDetails.ShvaDealID == filter.ShvaTransactionID);
            }

            if (filter.ClearingHouseTransactionID != null)
            {
                src = src.Where(t => t.ClearingHouseTransactionDetails.ClearingHouseTransactionID == filter.ClearingHouseTransactionID);
            }

            if (filter.ConsumerID != null)
            {
                src = src.Where(t => t.DealDetails.ConsumerID == filter.ConsumerID);
            }

            if (filter.ConsumerExternalReference != null)
            {
                src = src.Where(t => t.DealDetails.ConsumerExternalReference == filter.ConsumerExternalReference);
            }

            if (!string.IsNullOrWhiteSpace(filter.CardNumber))
            {
                var cardNumber = filter.CardNumber.Replace("*", string.Empty).Trim();

                // don't do anything if card number is nothing but a * symbol
                if (!string.IsNullOrEmpty(cardNumber))
                {
                    src = src.Where(t => EF.Functions.Like(t.CreditCardDetails.CardNumber, filter.CardNumber.UseWildCard(true)));
                }
            }

            if (!string.IsNullOrWhiteSpace(filter.ConsumerEmail))
            {
                src = src.Where(t => EF.Functions.Like(t.DealDetails.ConsumerEmail, filter.ConsumerEmail.UseWildCard(true)));
            }

            if (!string.IsNullOrWhiteSpace(filter.CardOwnerName))
            {
                src = src.Where(t => EF.Functions.Like(t.CreditCardDetails.CardOwnerName, filter.CardOwnerName.UseWildCard(true)));
            }

            if (!string.IsNullOrWhiteSpace(filter.CardOwnerNationalID))
            {
                src = src.Where(t => EF.Functions.Like(t.CreditCardDetails.CardOwnerNationalID, filter.CardOwnerNationalID.UseWildCard(true)));
            }

            if (filter.CreditCardTokenID != null)
            {
                src = src.Where(t => t.CreditCardToken == filter.CreditCardTokenID);
            }

            if (filter.BillingDealID != null)
            {
                src = src.Where(t => t.BillingDealID == filter.BillingDealID);
            }

            if (!string.IsNullOrWhiteSpace(filter.DealDescription))
            {
                src = src.Where(t => EF.Functions.Like(t.DealDetails.DealDescription, filter.DealDescription.UseWildCard(true)));
            }

            if (!string.IsNullOrWhiteSpace(filter.DealReference))
            {
                src = src.Where(t => t.DealDetails.DealReference == filter.DealReference);
            }

            if (filter.Solek != null)
            {
                src = src.Where(t => t.ShvaTransactionDetails.Solek == filter.Solek);
            }

            if (!string.IsNullOrWhiteSpace(filter.CreditCardVendor))
            {
                src = src.Where(t => t.CreditCardDetails.CardVendor == filter.CreditCardVendor);
            }

            if (filter.TerminalTemplateID.HasValue)
            {
                src = src.Where(t => t.TerminalTemplateID == filter.TerminalTemplateID.Value);
            }

            if (filter.FinalizationStatus.HasValue)
            {
                src = src.Where(t => t.FinalizationStatus == filter.FinalizationStatus);
            }

            if (filter.DocumentOrigin.HasValue)
            {
                src = src.Where(t => t.DocumentOrigin == filter.DocumentOrigin);
            }

            if (filter.HasInvoice.GetValueOrDefault(PropertyPresenceEnum.All) != PropertyPresenceEnum.All)
            {
                if (filter.HasInvoice == PropertyPresenceEnum.Yes)
                {
                    src = src.Where(t => t.InvoiceID != null);
                }
                else
                {
                    src = src.Where(t => t.InvoiceID == null);
                }
            }

            if (filter.HasMasavFile.GetValueOrDefault())
            {
                src = src.Where(t => t.MasavFileID != null);
            }

            if (filter.IsPaymentRequest.GetValueOrDefault())
            {
                src = src.Where(t => t.PaymentRequestID != null);
            }

            if (filter.PaymentType != null)
            {
                src = src.Where(t => t.PaymentTypeEnum == filter.PaymentType.Value);
            }

            return src;
        }

        public static IQueryable<PaymentTransaction> Filter(this IQueryable<PaymentTransaction> src, TransmissionFilter filter)
        {
            if (filter.TerminalID != null)
            {
                src = src.Where(t => t.TerminalID == filter.TerminalID);
            }

            src = src.Where(d => d.PaymentTypeEnum == SharedIntegration.Models.PaymentTypeEnum.Card);
            src = src.Where(d => d.Status == Shared.Enums.TransactionStatusEnum.AwaitingForTransmission);

            return src;
        }

        public static IQueryable<PaymentTransaction> Filter(this IQueryable<PaymentTransaction> src, TransmitReportFilter filter)
        {
            if (filter.Success)
            {
                src = src.Where(t => t.ShvaTransactionDetails.TransmissionDate != null);
            }
            else
            {
                src = src.Where(t => t.Status == Shared.Enums.TransactionStatusEnum.TransmissionToProcessorFailed);
            }

            if (filter.TerminalID != null)
            {
                src = src.Where(t => t.TerminalID == filter.TerminalID);
            }

            if (filter.MerchantID != null)
            {
                src = src.Where(t => t.MerchantID == filter.MerchantID);
            }

            if (filter.QuickDateFilter != null)
            {
                var dateRange = CommonFiltertingExtensions.QuickDateToDateRange(filter.QuickDateFilter.Value);

                src = src.Where(t => t.UpdatedDate >= dateRange.DateFrom && t.UpdatedDate <= dateRange.DateTo);
            }
            else
            {
                if (filter.DateFrom != null)
                {
                    src = src.Where(t => t.UpdatedDate.Value.Date >= filter.DateFrom.Value);
                }

                if (filter.DateTo != null)
                {
                    src = src.Where(t => t.UpdatedDate.Value.Date <= filter.DateTo.Value);
                }
            }

            return src;
        }

        private static IQueryable<PaymentTransaction> HandleDateFiltering(IQueryable<PaymentTransaction> src, TransactionsFilter filter)
        {
            if (filter.TransmissionQuickDateFilter != null)
            {
                var dateRange = CommonFiltertingExtensions.QuickDateToDateRange(filter.TransmissionQuickDateFilter.Value);

                src = src.Where(t => t.ShvaTransactionDetails.TransmissionDate >= dateRange.DateFrom && t.ShvaTransactionDetails.TransmissionDate <= dateRange.DateTo);
            }
            else if (filter.QuickDateFilter != null) //TODO: Quick time filters using SequentialGuid https://stackoverflow.com/questions/54920200/entity-framework-core-guid-greater-than-for-paging
            {
                var dateRange = CommonFiltertingExtensions.QuickDateToDateRange(filter.QuickDateFilter.Value);

                src = src.Where(t => t.TransactionDate >= dateRange.DateFrom && t.TransactionDate <= dateRange.DateTo);
            }
            else
            {
                if (filter.DateFrom != null)
                {
                    src = src.Where(t => t.TransactionDate >= filter.DateFrom.Value);
                }

                if (filter.DateTo != null)
                {
                    src = src.Where(t => t.TransactionDate <= filter.DateTo.Value);
                }
            }

            return src;
        }

        private static IQueryable<PaymentTransaction> FilterByQuickStatus(IQueryable<PaymentTransaction> src, QuickStatusFilterTypeEnum typeEnum)
            => typeEnum switch
            {
                QuickStatusFilterTypeEnum.Pending => src.Where(t => (int)t.Status >= 0 && (int)t.Status < 40 && t.Status != Shared.Enums.TransactionStatusEnum.AwaitingForTransmission),
                QuickStatusFilterTypeEnum.Completed => src.Where(t => t.Status == Shared.Enums.TransactionStatusEnum.Completed),
                QuickStatusFilterTypeEnum.Chargeback => src.Where(t => t.Status == Shared.Enums.TransactionStatusEnum.Chargeback),
                QuickStatusFilterTypeEnum.Canceled => src.Where(t => t.Status == Shared.Enums.TransactionStatusEnum.CancelledByMerchant),
                QuickStatusFilterTypeEnum.Failed => src.Where(t => (int)t.Status < 0),
                QuickStatusFilterTypeEnum.AwaitingForTransmission => src.Where(t => t.Status == Shared.Enums.TransactionStatusEnum.AwaitingForTransmission),
                _ => src,
            };
    }
}
