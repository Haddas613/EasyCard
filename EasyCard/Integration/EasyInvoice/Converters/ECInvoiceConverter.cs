using EasyInvoice.Models;
using Shared.Helpers;
using Shared.Integration.Models;
using Shared.Integration.Models.Invoicing;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyInvoice.Converters
{
    public static class ECInvoiceConverter
    {
        public static ECInvoiceCreateDocumentRequest GetInvoiceCreateDocumentRequest(InvoicingCreateDocumentRequest message)
        {
            var res = new ECInvoiceCreateDocumentRequest() { };

            //decimal netAmount = message.NetAmount.GetValueOrDefault();
            //decimal vatAmount = message.VAT.GetValueOrDefault();
            //decimal vatRate = message.VATRate.GetValueOrDefault(0.17m) * 100m;


            //var json = new ECInvoiceCreateDocumentRequest
            //{
            //    CustomerAddress = new ECInvoiceCustomerAddress
            //    {
            //        City = message.City,
            //        Street = message.Street,
            //        PostalCode = message.Zip,
            //    },
            //    CustomerEmail = message.CustomerEmail,
            //    CustomerName = message.CustomerName,
            //    CustomerPhoneNumber = message.CustomerPhoneNumber,
            //    CustomerTaxId = message.CustomerTaxId,
            //    Description = string.IsNullOrWhiteSpace(message.Description) ? configuration.InvoiceDescription : message.Description,
            //    DocumentType = message.DocumentType.ToString(),
            //    KeyStorePassword = this.configuration.KeyStorePassword,
            //    SendEmail = true,
            //    TotalAmount = message.TotalAmount,

            //    DiscountAmount = 0,
            //    TaxAmount = vatAmount,
            //    TaxPercentage = vatRate,
            //    TotalAmountBeforeDiscount = message.TotalAmount,
            //    TotalNetAmount = netAmount,
            //    TotalPaidAmount = message.TotalAmount,

            //    TransactionDateTime = message.TransactionDateTime?.ToString("o"),
            //    Rows = new List<ECInvoiceRow> {
            //        new ECInvoiceRow
            //        {
            //            Sku = "-",
            //            Name = message.InvoiceItem,
            //            Quantity = 1,
            //            TotalAmount = message.TotalAmount,

            //            Price = message.TotalAmount,
            //            PriceNet = netAmount,
            //            TaxAmount =  vatAmount,
            //            TotalNetAmount = netAmount,
            //            TotalTaxAmount = vatAmount
            //        }
            //    },

            //    Payments = new List<ECInvoicePayment>
            //    {
            //        new ECInvoicePayment
            //        {
            //             PaymentMethod = message.PaymentInfo?.PaymentMethod.ToString(),
            //             Amount = message.TotalAmount,
            //             CreditCard4LastDigits = message.PaymentInfo?.CreditCard4LastDigits,
            //             CreditCardType = message.PaymentInfo?.CreditCardType,
            //             PaymentDateTime = (message.PaymentInfo?.PaymentDateTime).GetValueOrDefault(DateTime.Today).ToString("o"),
            //             ChequeBank = message.PaymentInfo?.ChequeBank,
            //             ChequeAccount = message.PaymentInfo?.ChequeAccount,
            //             ChequeBranch = message.PaymentInfo?.ChequeBranch,
            //        }
            //    }


            //};

 

            return res;
        }

        public static ECInvoiceCustomerAddress GetCustomerAddress(Address message)
        {
            var res = new ECInvoiceCustomerAddress
            {
                City = message.City,
                Street = message.Street,
                PostalCode = message.Zip,
            };

            return res;
        }

        public static ECInvoiceRow GetRow(Item message)
        {
            var res = new ECInvoiceRow
            {
                Sku = message.SKU,
                Name = message.ItemName,
                Quantity = (int)message.Quantity,
                TotalAmount = message.Amount,

                Price = message.Price,
                PriceNet = message.NetAmount,
                TaxAmount = message.VAT,
                TotalNetAmount = message.NetAmount,
                TotalTaxAmount = message.VAT,
            };

            return res;
        }

        public static ECInvoicePayment GetPaymentFromCard(InvoicingCreateDocumentRequest message)
        {
            var res = new ECInvoicePayment
            {
                PaymentMethod = ECInvoicePaymentMethodEnum.CREDIT_CARD_REGULAR_CREDIT.ToString(),
                Amount = message.InvoiceAmount,
                CreditCard4LastDigits = CreditCardHelpers.GetCardLastFourDigits(message.CreditCardDetails.CardNumber),
                CreditCardType = message.CreditCardDetails.CardVendor,
                PaymentDateTime = message.InvoiceDate.GetValueOrDefault(DateTime.Today).ToString("o"),
            };

            return res;
        }
    }
}
