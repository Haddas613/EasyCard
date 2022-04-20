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
using SharedBusiness = Shared.Business;
using SharedIntegration = Shared.Integration;

namespace Transactions.Api.Extensions
{
    public static class DealDetailsExtensions
    {
        public static void UpdateDealDetails(this DealDetails dealDetails, Consumer consumer, TerminalSettings terminalSettings, IFinancialItem financialItem, CreditCardDetailsBase creditCardDetails)
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
                dealDetails.ConsumerName = consumer?.ConsumerName ?? creditCardDetails?.CardOwnerName;
            }

            if (string.IsNullOrWhiteSpace(dealDetails.ConsumerNationalID))
            {
                dealDetails.ConsumerNationalID = consumer?.ConsumerNationalID ?? creditCardDetails?.CardOwnerNationalID;
            }

            if (dealDetails.ConsumerAddress == null)
            {
                dealDetails.ConsumerAddress = consumer?.ConsumerAddress;
            }

            if (dealDetails.ConsumerEcwidID == null)
            {
                dealDetails.ConsumerEcwidID = consumer?.EcwidID;
            }

            if (dealDetails.ConsumerWoocommerceID == null)
            {
                dealDetails.ConsumerWoocommerceID = consumer?.WoocommerceID;
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

            //if (dealDetails.Items?.Count() > 0 &&
            //    (dealDetails.Items?.Sum(i => i.Amount).GetValueOrDefault(0) != financialItem.Amount
            //    || dealDetails.Items?.Sum(i => i.VAT).GetValueOrDefault(0) != financialItem.VATTotal))
            //{
            //    var firstItem = dealDetails.Items.First();
            //    firstItem.Quantity = 1;

            //    if (dealDetails.Items?.Count() > 1)
            //    {
            //        firstItem.Amount = financialItem.Amount - dealDetails.Items?.Sum(i => i.Amount).GetValueOrDefault(0);
            //        firstItem.Price = firstItem.Amount;

            //        //firstItem.VAT = Math.Round(firstItem.Amount.Value / (1m + financialItem.VATRate), 2, MidpointRounding.AwayFromZero);
            //        firstItem.VAT = financialItem.VATTotal - dealDetails.Items?.Sum(i => i.VAT).GetValueOrDefault(0);
            //        firstItem.NetAmount = firstItem.Amount - firstItem.VAT;
            //    }
            //    else
            //    {
            //        firstItem.Amount = financialItem.Amount;
            //        firstItem.Price = firstItem.Amount;
            //        firstItem.VAT = financialItem.VATTotal;
            //        firstItem.NetAmount = firstItem.Amount - firstItem.VAT;
            //    }
            //}

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

        public static void CheckConsumerDetails(this DealDetails dealDetails, Consumer consumer, CreditCardTokenDetails dbToken)
        {
            if (consumer != null)
            {
                if (dbToken != null)
                {
                    if (consumer.ConsumerID != dbToken.ConsumerID)
                    {
                        throw new EntityNotFoundException(SharedBusiness.Messages.ApiMessages.EntityNotFound, "CreditCardToken", null);
                    }

                    // NOTE: consumer can pay using another person credit card
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

        public static void UpdateCreditCardDetails(this CreditCardDetails creditCardDetails, Consumer consumer, Models.Transactions.CreateTransactionRequest model)
        {
            if (string.IsNullOrWhiteSpace(creditCardDetails.CardOwnerName) && consumer != null)
            {
                creditCardDetails.CardOwnerName = consumer.ConsumerName;
            }

            if (model.PinPad == true)
            {
                if (!string.IsNullOrEmpty(model.CardOwnerNationalID))
                {
                    creditCardDetails.CardOwnerNationalID = model.CardOwnerNationalID;
                }

                if (!string.IsNullOrEmpty(model.CardOwnerName))
                {
                    creditCardDetails.CardOwnerName = model.CardOwnerName;
                }
            }
        }
    }
}
