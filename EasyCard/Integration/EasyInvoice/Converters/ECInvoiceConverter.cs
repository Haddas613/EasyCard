using EasyInvoice.Models;
using Shared.Helpers;
using Shared.Integration.Models;
using Shared.Integration.Models.Invoicing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyInvoice.Converters
{
    public static class ECInvoiceConverter
    {
        public static ECInvoiceCreateDocumentRequest GetInvoiceCreateDocumentRequest(InvoicingCreateDocumentRequest message)
        {
            var json = new ECInvoiceCreateDocumentRequest
            {
                CustomerAddress = GetCustomerAddress(message.DealDetails?.ConsumerAddress),
                CustomerEmail = message.DealDetails.ConsumerEmail,
                CustomerName = message.ConsumerName,
                CustomerPhoneNumber = message.DealDetails?.ConsumerPhone,
                CustomerTaxId = message.ConsumerNationalID,
                Description = message.DealDetails?.DealDescription,
                DocumentType = GetECInvoiceDocumentType((message.InvoiceDetails?.InvoiceType).GetValueOrDefault()).ToString(),
                SendEmail = true,
                TotalAmount = message.InvoiceAmount,

                DiscountAmount = 0, // TODO: discount
                TaxAmount = message.VATTotal,
                TaxPercentage = message.VATRate,
                TotalAmountBeforeDiscount = message.InvoiceAmount,
                TotalNetAmount = message.NetTotal,
                TotalPaidAmount = message.InvoiceAmount,

                TransactionDateTime = message.InvoiceDate?.ToString("o"),
                Rows = GetRows(message),

                Payments = new List<ECInvoicePayment>
                {
                    GetPaymentFromCard(message)
                }
            };

            return json;
        }

        public static IList<ECInvoiceRow> GetRows(InvoicingCreateDocumentRequest message)
        {
            if (!(message.DealDetails?.Items?.Count() > 0))
            {
                return new List<ECInvoiceRow>
                {
                    new ECInvoiceRow
                    {
                        Sku = "-",
                        Name = "Default item", // TODO: settings
                        Quantity = 1,
                        TotalAmount = message.InvoiceAmount,

                        Price = message.InvoiceAmount,
                        PriceNet = message.NetTotal,
                        TaxAmount = message.VATTotal,
                        TotalNetAmount = message.NetTotal,
                        TotalTaxAmount = message.VATTotal,
                    }
                };
            }
            else
            {
                return message.DealDetails.Items.Select(d => GetRow(d)).ToList();
            }
        }

        public static ECInvoiceCustomerAddress GetCustomerAddress(Address message)
        {
            if (message == null)
            {
                return null;
            }

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
                Sku = message.SKU ?? "-",
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

                // TODO: installments
            };

            return res;
        }

        public static ECInvoiceDocumentType GetECInvoiceDocumentType(InvoiceTypeEnum documentType)
        {
            return documentType switch
            {
                InvoiceTypeEnum.CreditNote => ECInvoiceDocumentType.CREDIT_NOTE,
                InvoiceTypeEnum.Invoice => ECInvoiceDocumentType.INVOICE,
                InvoiceTypeEnum.InvoiceWithPaymentInfo => ECInvoiceDocumentType.INVOICE_WITH_PAYMENT_INFO,
                InvoiceTypeEnum.PaymentInfo => ECInvoiceDocumentType.PAYMENT_INFO,
                InvoiceTypeEnum.RefundInvoice => ECInvoiceDocumentType.REFUND_INVOICE,
                _ => ECInvoiceDocumentType.INVOICE_WITH_PAYMENT_INFO,
            };
        }
    }
}
