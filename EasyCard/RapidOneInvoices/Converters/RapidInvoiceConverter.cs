using RapidOneInvoices.Models;
using Shared.Integration.Models;
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
            var json = new RapidInvoiceCreateDocumentRequest
            {
                Discount = 0,
                DueDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                InvoiceDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                InvoiceTypeId = getInvoiceType(message.InvoiceDetails.InvoiceType).ToString(),
                Items = getItems(message),
                PaymentMethods = getPaymentMethods(message),
                ToPay = Convert.ToDecimal(message.TotalAmount.ToString("#.#######")),
                Total = Convert.ToDecimal(message.TotalAmount.ToString("#.#######")),
                Vat = Convert.ToDecimal(message.VATTotal.ToString("#.#######")),
                Subtotal = Convert.ToDecimal((message.TotalAmount - message.VATTotal).ToString("#.#######")),
                CustomerCell = message.DealDetails?.ConsumerPhone,
                CustomerCode = message.DealDetails.ExternalConsumerCode,
                CustomerEmail = message.DealDetails.ConsumerEmail,
                CustomerName = message.ConsumerName,
            };

            return json;
        }

        private static List<ProductInInvoiceModel> getItems(InvoicingCreateDocumentRequest message)
        {
            throw new NotImplementedException();//TODO IMPLEMENTATION
        }

        private static PaymentMethodModel getPaymentMethods(InvoicingCreateDocumentRequest message)
        {
            return new PaymentMethodModel
            {
                Currency = "₪",
                Cash = new CashModel[0],
                Check = new CheckModel[0],
                CreditCard = getCreditCardDetails(message),
                MoneyTransfer = getMoneyTransferDetails(message)
            };
        }

        private static MoneyTransferModel[] getMoneyTransferDetails(InvoicingCreateDocumentRequest message)
        {
            var result = new MoneyTransferModel[1];
            MoneyTransferModel mt = new MoneyTransferModel();
            mt.Value = message.TotalAmount;
            // TODO bank details
            //  int bankID = -1;
            //Int32.TryParse(message., out bankID);
            // mt.BankId = bankID;
            //mt.Branch = bankBrach;
            //mt.Account = accountNumber;
            //mt.AccountNumber = accountNumberSettings;
            //mt.Bank = new BankDetailsModel();
            //mt.Bank.id = bankID;
            //mt.Bank.name = "";
            //mt.Bank.code = bankNumber;
            result[0] = mt;
            return result;
        }

        private static CreditCardModel[] getCreditCardDetails(InvoicingCreateDocumentRequest message)
        {
            var result = new CreditCardModel[1];
            CreditCardModel cc = new CreditCardModel();
            switch (message.CreditCardDetails.Solek)
            {
                case "2":
                    cc.Type = 1;
                    break;
                case "1":
                    cc.Type = 2;
                    break;
                case "3":
                    cc.Type = 3;
                    break;
                case "4":
                    cc.Type = 4;
                    break;
                case "6":
                    cc.Type = 5;
                    break;
                default:
                    cc.Type = 5;
                    break;
            }
            cc.Value = message.TotalAmount;
            cc.Number = message.CreditCardDetails.CardNumber;
            cc.Expiration = new ExpirationModel();
            cc.Expiration.Month = message.CreditCardDetails.CardExpiration.Month ?? 1;
            cc.Expiration.Year = message.CreditCardDetails.CardExpiration.Year ?? 1991;
            //mt.VoucherNum = message.DealDetails. voucherNum.Length > 20 ? voucherNum.Substring(voucherNum.Length - 20, 20) : voucherNum; TODO
            cc.Payments = message.NumberOfPayments.ToString();
            cc.FirstPayment = message.InitialPaymentAmount.ToString();
            cc.DealType = getDealType(message.TransactionType);
            if (cc.DealType == 3)//credit
            {
                cc.FirstPayment = null;// string.Empty;// ((double)(total/paymentsInt)).ToString("#.##");
                cc.Payments = string.Empty;
            }
            result[0] = cc;
            return result;
        }

        private static int getDealType(TransactionTypeEnum? transactionType)
        {
            return transactionType switch
            {
                TransactionTypeEnum.Credit => 3,
                TransactionTypeEnum.Installments => 2,
                TransactionTypeEnum.RegularDeal => 1
            };
        }

        private static RapidOneInvoices.Models.Enums.InvoiceTypeEnum getInvoiceType(InvoiceTypeEnum invoiceTypeID)
        {
            return invoiceTypeID switch
            {
                InvoiceTypeEnum.CreditNote => RapidOneInvoices.Models.Enums.InvoiceTypeEnum.CreditCustomer,
                InvoiceTypeEnum.Invoice => RapidOneInvoices.Models.Enums.InvoiceTypeEnum.Invoice,
                InvoiceTypeEnum.InvoiceWithPaymentInfo => RapidOneInvoices.Models.Enums.InvoiceTypeEnum.InvoiceReceipt,
                InvoiceTypeEnum.PaymentInfo => RapidOneInvoices.Models.Enums.InvoiceTypeEnum.Receipt,
                InvoiceTypeEnum.RefundInvoice => RapidOneInvoices.Models.Enums.InvoiceTypeEnum.InvoiceRefund
            };
        }


    }


}

