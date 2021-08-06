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
                Items = getItems(message),////TODO
                PaymentMethods = getPaymentMethods(message),
                ToPay = Convert.ToDecimal(message.TotalAmount.ToString("#.#######")),
                Total = Convert.ToDecimal(message.TotalAmount.ToString("#.#######")),
                Vat = Convert.ToDecimal(message.VATTotal.ToString("#.#######")),
                Subtotal = Convert.ToDecimal((message.TotalAmount - message.VATTotal).ToString("#.#######")),
                CustomerCell = message.DealDetails?.ConsumerPhone,
                CustomerCode = message.DealDetails?.ExternalConsumerCode,
                CustomerEmail = message.DealDetails?.ConsumerEmail,
                CustomerName = message.ConsumerName,
            };

            return json;
        }

        private static List<ProductInInvoiceModel> getItems(InvoicingCreateDocumentRequest message)
        {
            List<ProductInInvoiceModel> listProducts = new List<ProductInInvoiceModel>();
            ProductInInvoiceModel productInvoice = new ProductInInvoiceModel
            {
                Code = "ROVAT",
                Description = "חיוב מתאריך 05/08/2021",
                Quantity = 1,
                UnitPrice = new UnitPriceModel()
                {
                    Value = 1245 / (decimal)10,
                    Currency = "₪"
                },
                Subtotal = 1245 / (decimal)10,
                VatPercent = 17,
                Vat = 255 / (decimal)10,
                Total = 150,
                ToPay = 150,
                Discount = 0,
                Disabled = false,
                IsDefaultItem = true,
                IsDefaultItemWithVat = true,
                Rate = 1,
                PreventDiscount = false,
                Charge = false
            };

            /*
            
            "items":[
             {
             //"users":[],
             "notes":null,
             }
        ]
        */
            listProducts.Add(productInvoice);
            return listProducts;
        }

        private static PaymentMethodModel getPaymentMethods(InvoicingCreateDocumentRequest message)
        {
            return new PaymentMethodModel
            {
                Currency = "₪",
                Cash = new CashModel[0],
                Check = new CheckModel[0],
                CreditCard = getCreditCardDetails(message),
                MoneyTransfer = new MoneyTransferModel[0]//getMoneyTransferDetails(message)
            };
        }

        private static MoneyTransferModel[] getMoneyTransferDetails(InvoicingCreateDocumentRequest message)
        {
            var result = new MoneyTransferModel[1];
            MoneyTransferModel mt = new MoneyTransferModel();
            //mt.Value = message.TotalAmount;
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
                case "VISA":
                    cc.Type = 1;
                    break;
                case "ISRACARD":
                    cc.Type = 2;
                    break;
                case "DINERS_CLUB":
                    cc.Type = 3;
                    break;
                case "AMEX":
                    cc.Type = 4;
                    break;
                case "LEUMI_CARD":
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
            cc.VoucherNum = getVoucherNum(message.CreditCardDetails?.ShvaShovarNumber);
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

        private static string getVoucherNum(string dealReference)
        {
            return dealReference.Length > 20 ? dealReference.Substring(dealReference.Length - 20, 20) : dealReference;
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

