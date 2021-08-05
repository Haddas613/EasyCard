using EasyInvoice.Models;
using Shared.Helpers;
using Shared.Integration.Models;
using Shared.Integration.Models.Invoicing;
using Shared.Integration.Models.PaymentDetails;
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

                DiscountAmount = message.TotalDiscount.GetValueOrDefault(0),
                TaxAmount = message.VATTotal,
                TaxPercentage = message.VATRate,
                TotalAmountBeforeDiscount = message.InvoiceAmount,
                TotalNetAmount = message.NetTotal,
                TotalPaidAmount = message.InvoiceAmount,

                TransactionDateTime = message.InvoiceDate?.ToString("o"),
                Rows = GetRows(message),
            };

            //TODO: legacy, switch to PaymentDetails
            //if (message.CreditCardDetails != null)
            //{
            //    var payments = GetPaymentFromCard(message);
            //    if (payments != null)
            //    {
            //        json.Payments = new List<ECInvoicePayment>
            //        {
            //            payments
            //        };
            //    }
            //}
            if (message.PaymentDetails?.Any() == true)
            {
                json.Payments = MapPaymentDetails(message);
            }

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

        public static ECInvoiceCustomerAddress GetCustomerAddress(string message)
        {
            if (message == null)
            {
                return null;
            }

            var res = new ECInvoiceCustomerAddress
            {
                City = string.Empty,
                Street = message,
                PostalCode = string.Empty,
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

        public static List<ECInvoicePayment> MapPaymentDetails(InvoicingCreateDocumentRequest message)
        {
            var result = new List<ECInvoicePayment>();

            foreach (var d in message.PaymentDetails)
            {
                var res = new ECInvoicePayment
                {
                    PaymentMethod = message.TransactionType == TransactionTypeEnum.Credit ? ECInvoicePaymentMethodEnum.CREDIT_CARD_CREDITS.ToString() : (message.NumberOfPayments > 1 ? ECInvoicePaymentMethodEnum.CREDIT_CARD_PAYMENTS.ToString() : ECInvoicePaymentMethodEnum.CREDIT_CARD_REGULAR_CREDIT.ToString()),
                    Amount = message.InvoiceAmount,
                    PaymentDateTime = message.InvoiceDate.GetValueOrDefault(DateTime.Today).ToString("o"),
                    NumberOfPayments = message.NumberOfPayments
                };

                if (message.NumberOfPayments > 1)
                {
                    res.AmountOfFirstPayment = message.InitialPaymentAmount;
                    res.AmountOfEachAdditionalPayment = message.InstallmentPaymentAmount;
                }

                if (d.PaymentType == PaymentTypeEnum.Card && d is CreditCardPaymentDetails cardDetails)
                {
                    res.CreditCard4LastDigits = CreditCardHelpers.GetCardLastFourDigits(cardDetails.CardNumber);
                    res.CreditCardType = ECInvoiceCreditCardTypeEnum.OTHER.ToString();
                }
                else if (d.PaymentType == PaymentTypeEnum.Cheque && d is ChequeDetails chequeDetails)
                {
                    res.PaymentMethod = ECInvoicePaymentMethodEnum.CHEQUE.ToString();
                    res.ChequeAccount = chequeDetails.BankAccount;
                    res.ChequeBank = chequeDetails.Bank?.ToString();
                    res.ChequeBranch = chequeDetails.BankBranch?.ToString();

                    //TODO: cheque number
                }
                else if (d.PaymentType == PaymentTypeEnum.Cash)
                {
                    res.PaymentMethod = ECInvoicePaymentMethodEnum.CASH.ToString();
                }
                else if (d.PaymentType == PaymentTypeEnum.Bank && d is BankTransferDetails bankTransfer)
                {
                    res.PaymentMethod = ECInvoicePaymentMethodEnum.BANK_TRANSFER.ToString();
                    res.ChequeAccount = bankTransfer.BankAccount;
                    res.ChequeBank = bankTransfer.Bank?.ToString();
                    res.ChequeBranch = bankTransfer.BankBranch?.ToString();
                }

                result.Add(res);
            }

            return result;
        }

        public static ECInvoiceDocumentType GetECInvoiceDocumentType(InvoiceTypeEnum documentType)
        {
            return documentType switch
            {
                InvoiceTypeEnum.CreditNote => ECInvoiceDocumentType.CREDIT_NOTE,
                InvoiceTypeEnum.Invoice => ECInvoiceDocumentType.INVOICE,
                InvoiceTypeEnum.InvoiceWithPaymentInfo => ECInvoiceDocumentType.INVOICE_WITH_PAYMENT_INFO,
                InvoiceTypeEnum.PaymentInfo => ECInvoiceDocumentType.PAYMENT_INFO,
                InvoiceTypeEnum.RefundInvoice => ECInvoiceDocumentType.REFUND_INVOICE_WITH_PAYMENT_INFO,
            };
        }
    }
}
