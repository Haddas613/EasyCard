using RapidOneInvoices.Models;
using Shared.Integration.Models.Invoicing;
using System;
using System.Collections.Generic;
using System.Text;

namespace RapidOneInvoices.Converters
{
   public class RapidInvoiceConverter
    {
        public static RapidInvoiceCreateDocumentRequest GetInvoiceCreateDocumentRequest(InvoicingCreateDocumentRequest message)
        {
           //var json = new RapidInvoiceCreateDocumentRequest
          /*  {
                CustomerCell = message.DealDetails?.ConsumerPhone,
                     CustomerCode  = message.DealDetails.
                CustomerAddress = GetCustomerAddress(message.DealDetails?.ConsumerAddress),
                CustomerEmail = message.DealDetails.ConsumerEmail,
                CustomerName = message.ConsumerName,
                CustomerPhoneNumber = message.DealDetails?.ConsumerPhone,
                CustomerTaxId = message.ConsumerNationalID,
                Description = message.DealDetails?.DealDescription,
                DocumentType = GetECInvoiceDocumentType((message.InvoiceDetails?.InvoiceType).GetValueOrDefault()).ToString(),
                SendEmail = true,
                TotalAmount = message.InvoiceAmount,

                DiscountAmount = message.TotalDiscount.GetValueOrDefault(0),
                TaxAmount = message.VATTotal,
                TaxPercentage = message.VATRate,
                TotalAmountBeforeDiscount = message.InvoiceAmount,
                TotalNetAmount = message.NetTotal,
                TotalPaidAmount = message.InvoiceAmount,

                TransactionDateTime = message.InvoiceDate?.ToString("o"),
                Rows = GetRows(message),
            };
            */
           //
           //var payments = GetPaymentFromCard(message);
           //if (payments != null)
           //{
           //    json.Payments = new List<ECInvoicePayment>
           //    {
           //        payments
           //    };
           //}

            return null;
        }

    }
}
