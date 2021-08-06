using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace RapidOne.Models
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
            this.Cash = new CashModel[0];
            this.Check = new CheckModel[0];
            this.CreditCard = new CreditCardModel[0];
            this.MoneyTransfer = new MoneyTransferModel[0];
            this.Currency = "₪";
        }
    }
}
