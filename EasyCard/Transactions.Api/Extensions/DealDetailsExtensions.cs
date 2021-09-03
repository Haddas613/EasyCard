using Merchants.Business.Entities.Billing;
using Merchants.Business.Entities.Terminal;
using Merchants.Shared.Models;
using Shared.Business.Financial;
using Shared.Helpers;
using Shared.Integration.Models.Processor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Business.Entities;
using SharedIntegration = Shared.Integration;

namespace Transactions.Api.Extensions
{
    public static class DealDetailsExtensions
    {
        public static void UpdateDealDetails(this DealDetails dealDetails, Consumer consumer, TerminalSettings terminalSettings, IFinancialItem financialItem)
        {
            if (string.IsNullOrWhiteSpace(dealDetails.ConsumerEmail))
            {
                dealDetails.ConsumerEmail = consumer?.ConsumerEmail;
            }

            if (string.IsNullOrWhiteSpace(dealDetails.ConsumerPhone))
            {
                dealDetails.ConsumerPhone = consumer?.ConsumerPhone;
            }

            if (string.IsNullOrWhiteSpace(dealDetails.ConsumerExternalReference))
            {
                dealDetails.ConsumerExternalReference = consumer?.ExternalReference;
            }

            if (string.IsNullOrWhiteSpace(dealDetails.ConsumerName))
            {
                dealDetails.ConsumerName = consumer?.ConsumerName;
            }

            if (string.IsNullOrWhiteSpace(dealDetails.ConsumerNationalID))
            {
                dealDetails.ConsumerNationalID = consumer?.ConsumerNationalID;
            }

            if (!(dealDetails.Items?.Count() > 0))
            {
                dealDetails.Items = new List<SharedIntegration.Models.Item>
                {
                    new SharedIntegration.Models.Item
                    {
                         Quantity = 1,
                         Price = financialItem.Amount,
                         Amount = financialItem.Amount,
                         ItemName = terminalSettings.DefaultItemName,
                         NetAmount = financialItem.NetTotal,
                         SKU = terminalSettings.DefaultSKU,
                         VAT = financialItem.VATTotal,
                         VATRate = financialItem.VATRate
                    }
                };
            }

            if (string.IsNullOrWhiteSpace(dealDetails.DealDescription))
            {
                dealDetails.DealDescription = terminalSettings.DefaultChargeDescription;
            }

            foreach (var item in dealDetails.Items)
            {
                if (string.IsNullOrWhiteSpace(item.ItemName))
                {
                    item.ItemName = terminalSettings.DefaultItemName;
                }

                if (string.IsNullOrWhiteSpace(item.SKU))
                {
                    item.SKU = terminalSettings.DefaultSKU;
                }
            }
        }

        public static void UpdateDealDetails(this DealDetails dealDetails, SharedIntegration.Models.PaymentDetails.CreditCardPaymentDetails creditCardDetails)
        {
            if (creditCardDetails != null)
            {
                if (string.IsNullOrWhiteSpace(dealDetails.ConsumerName))
                {
                    dealDetails.ConsumerName = creditCardDetails.CardOwnerName;
                }

                if (string.IsNullOrWhiteSpace(dealDetails.ConsumerNationalID))
                {
                    dealDetails.ConsumerNationalID = creditCardDetails.CardOwnerNationalID;
                }
            }
        }

        public static void UpdateDealDetails(this DealDetails dealDetails, CreditCardDetails creditCardDetails)
        {
            if (creditCardDetails != null)
            {
                if (string.IsNullOrWhiteSpace(dealDetails.ConsumerName))
                {
                    dealDetails.ConsumerName = creditCardDetails.CardOwnerName;
                }

                if (string.IsNullOrWhiteSpace(dealDetails.ConsumerNationalID))
                {
                    dealDetails.ConsumerNationalID = creditCardDetails.CardOwnerNationalID;
                }
            }
        }

        public static void CheckConsumerDetails(this DealDetails dealDetails, Consumer consumer)
        {
            if (consumer != null)
            {
                if (!string.IsNullOrWhiteSpace(consumer.ConsumerNationalID) && !string.IsNullOrWhiteSpace(dealDetails.ConsumerNationalID) && !consumer.ConsumerNationalID.Equals(dealDetails.ConsumerNationalID, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new EntityConflictException(Transactions.Shared.Messages.ConsumerNatIdIsNotEqTranNatId, "Consumer");
                }
            }
        }

        public static void UpdateInvoiceDetails(this SharedIntegration.Models.Invoicing.InvoiceDetails invoiceDetails, TerminalInvoiceSettings terminalSettings, PaymentTransaction transaction = null)
        {
            if (string.IsNullOrWhiteSpace(invoiceDetails.InvoiceSubject))
            {
                invoiceDetails.InvoiceSubject = terminalSettings.DefaultInvoiceSubject;
            }

            if (!(invoiceDetails.SendCCTo?.Count() > 0))
            {
                invoiceDetails.SendCCTo = terminalSettings.SendCCTo;
            }

            if (invoiceDetails.InvoiceType == SharedIntegration.Models.Invoicing.InvoiceTypeEnum.InvoiceWithPaymentInfo && transaction != null && transaction.SpecialTransactionType == SharedIntegration.Models.SpecialTransactionTypeEnum.Refund)
            {
                invoiceDetails.InvoiceType = SharedIntegration.Models.Invoicing.InvoiceTypeEnum.RefundInvoice;
            }
        }

        public static PinPadDetails UpdatePinPadDetails(this SharedIntegration.Models.Processor.PinPadDetails pinpadDetails, TerminalExternalSystem terminalSettings, PaymentTransaction transaction = null)
        {
            if (terminalSettings == null)
            {
                return null;
            }

            if (pinpadDetails == null)
            {
                pinpadDetails = new SharedIntegration.Models.Processor.PinPadDetails();
            }

            if (string.IsNullOrWhiteSpace(pinpadDetails.TerminalID))
            {
                pinpadDetails.TerminalID = terminalSettings.Settings.ToObject<Nayax.NayaxTerminalSettings>().TerminalID;
            }

            return pinpadDetails;
        }
    }
}
