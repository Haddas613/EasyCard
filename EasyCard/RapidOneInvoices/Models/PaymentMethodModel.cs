using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace RapidOneInvoices.Models
{
    [DataContract]
    public class PaymentMethodModel
    {
        [DataMember(Name = "currency")]
        public string Currency { get; set; }

        [DataMember(Name = "cash")]
        public CashModel[] Cash { get; set; }

        [DataMember(Name = "check")]
        public CheckModel[] Check { get; set; }

        [DataMember(Name = "creditCard")]
        public CreditCardModel[] CreditCard { get; set; }

        [DataMember(Name = "moneyTransfer")]
        public MoneyTransferModel[] MoneyTransfer { get; set; }

        public PaymentMethodModel()
        {
            this.cash = new CashModel[0];
            this.check = new CheckModel[0];
            this.creditCard = new CreditCardModel[0];
            this.moneyTransfer = new MoneyTransferModel[0];
            this.currency = "₪";
        }
    }
}
