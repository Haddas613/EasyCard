using RapidOne.Models;
using Shared.Integration.Models;
using Shared.Integration.Models.Invoicing;
using Shared.Integration.Models.PaymentDetails;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RapidOne.Converters
{
    public class RapidInvoiceConverter
    {

        public static CreateFinancialDocumentModelDto GetInvoiceCreateDocumentRequest(InvoicingCreateDocumentRequest message)
        {
            var json = new CreateFinancialDocumentModelDto
            {
                Discount = 0,
                DueDate = message.InvoiceDate,
                InvoiceDate = message.InvoiceDate,
                InvoiceTypeId = GetInvoiceType(message.InvoiceDetails.InvoiceType),
                Items = GetItems(message),
                PaymentMethods = GetPaymentMethods(message),
                ToPay = message.InvoiceAmount.Value,
                Total = message.InvoiceAmount.Value,
                Vat = message.VATTotal,
                Subtotal = message.InvoiceAmount.Value - message.VATTotal,
                CustomerCell = message.DealDetails?.ConsumerPhone,
                CustomerCode = message.DealDetails?.ConsumerExternalReference,
                CustomerEmail = message.DealDetails?.ConsumerEmail,
                CustomerName = message.ConsumerName,
            };

            return json;
        }

        private static IEnumerable<FinDocItemDto> GetItems(InvoicingCreateDocumentRequest message)
        {
            var listProducts = new List<FinDocItemDto>();

            foreach (var item in message.DealDetails.Items)
            {
                var productInvoice = new FinDocItemDto
                {
                    Code = item.ExternalReference ?? "ROVAT",
                    Description = item.ItemName,
                    Quantity = item.Quantity.GetValueOrDefault(1),
                    UnitPrice = new FinDocUnitPriceDto()
                    {
                        Value = item.Price.GetValueOrDefault(),
                        Currency = "₪"
                    },
                    Subtotal = item.NetAmount.GetValueOrDefault(),
                    VatPercent = item.VATRate.GetValueOrDefault(),
                    Vat = item.VAT.GetValueOrDefault(),
                    Total = item.Amount.GetValueOrDefault(),
                    ToPay = item.Amount.GetValueOrDefault(),
                    Discount = item.Discount.GetValueOrDefault(),
                    Rate = 1,
                    Users = new string[] { }, // there is no null-check in R1 so this empty array required
                    Charge = false // TODO
                };

                listProducts.Add(productInvoice);
            }

            return listProducts;
        }

        private static FinDocPaymentMethodDto GetPaymentMethods(InvoicingCreateDocumentRequest message)
        {
            return new FinDocPaymentMethodDto
            {
                Currency = "₪",
                Cash = new FinDocCashDto[0],
                Check = new FinDocCheckDto[0],
                CreditCard = GetCreditCardDetails(message),
                MoneyTransfer = GetMoneyTransferDetails(message)
            };
        }

        private static IEnumerable<FinDocMoneyTransferDto> GetMoneyTransferDetails(InvoicingCreateDocumentRequest message)
        {
            var result = new List<FinDocMoneyTransferDto>();

            foreach (var bankPayment in message.PaymentDetails.Where(p => p.PaymentType == PaymentTypeEnum.Bank).Cast<BankTransferDetails>())
            {
                var mt = new FinDocMoneyTransferDto();

                mt.Bank = new BankDto
                {
                    Id = bankPayment.Bank.GetValueOrDefault()
                };
                mt.Branch = bankPayment.BankBranch.ToString();
                mt.Account = bankPayment.BankAccount;
                mt.Value = message.InvoiceAmount.Value; // TODO: split amounts

                result.Add(mt);
            }

            return result;
        }

        private static IEnumerable<FinDocCreditCardDto> GetCreditCardDetails(InvoicingCreateDocumentRequest message)
        {
            var result = new List<FinDocCreditCardDto>();

            foreach (var creditCardPayment in message.PaymentDetails.Where(p => p.PaymentType == PaymentTypeEnum.Card).Cast<CreditCardPaymentDetails>())
            {
                var cc = new FinDocCreditCardDto();

                cc.Type = GetCardVendor(creditCardPayment.CardVendor?.ToUpper());
                cc.Value = message.InvoiceAmount.Value; // TODO: split amounts
                cc.Number = creditCardPayment.СardNumber;
                cc.Expiration = new FinDocCreditCardExpDto
                {
                    Month = creditCardPayment.CardExpiration.Month.Value,
                    Year = 2000 + creditCardPayment.CardExpiration.Year.Value
                };

                cc.VoucherNum = GetVoucherNum(creditCardPayment?.ShovarNumber);
                cc.Payments = message.NumberOfPayments;
                cc.FirstPayment = message.InitialPaymentAmount;
                cc.DealType = GetDealType(message.TransactionType);

                result.Add(cc);
            }

            return result;
        }

        // TODO: we need to use R1 mapping in terminal configuration - different systems can have different card codes
        private static int? GetCardVendor(string vendor)
        {
            return vendor switch
            {
                "VISA" => 1,
                "ISRACARD" => 2,
                "DINERS_CLUB" => 3,
                "AMEX" => 4,
                "LEUMI_CARD" => 5,
                _ => 5,
            };
        }

        private static string GetVoucherNum(string dealReference)
        {
            if (string.IsNullOrWhiteSpace(dealReference))
                return "1";

            return dealReference.Length > 20 ? dealReference.Substring(dealReference.Length - 20, 20) : dealReference;
        }

        // TODO: we need to use R1 mapping in terminal configuration - credit or installment can have different codes (not only 3 or 2)
        private static int GetDealType(TransactionTypeEnum? transactionType)
        {
            return transactionType switch
            {
                TransactionTypeEnum.Credit => 3,
                TransactionTypeEnum.Installments => 2,
                TransactionTypeEnum.RegularDeal => 1,
                TransactionTypeEnum.Immediate => 1,
                _ => 1
            };
        }

        private static RapidOne.Models.Enums.InvoiceTypeEnum GetInvoiceType(InvoiceTypeEnum invoiceTypeID)
        {
            return invoiceTypeID switch
            {
                InvoiceTypeEnum.CreditNote => Models.Enums.InvoiceTypeEnum.CreditCustomer,
                InvoiceTypeEnum.Invoice => Models.Enums.InvoiceTypeEnum.Invoice,
                InvoiceTypeEnum.InvoiceWithPaymentInfo => Models.Enums.InvoiceTypeEnum.InvoiceReceipt,
                InvoiceTypeEnum.PaymentInfo => Models.Enums.InvoiceTypeEnum.Receipt,
                InvoiceTypeEnum.RefundInvoice => Models.Enums.InvoiceTypeEnum.InvoiceRefund,
                _ => throw new NotImplementedException()
            };
        }
    }
}

