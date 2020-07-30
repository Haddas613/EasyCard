using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Shared.Integration.Models.Invoicing
{
    [DataContract]
    public class Payment
    {

        [DataMember(Name = "amount")]
        public decimal? Amount { get; set; }

        [DataMember(Name = "paymentMethod")]
        public string PaymentMethod { get; set; }

        [DataMember(Name = "paymentDateTime")]
        public string PaymentDateTime { get; set; }

        [DataMember(Name = "creditCardType")]
        public string CreditCardType { get; set; }

        [DataMember(Name = "creditCard4LastDigits")]
        public string CreditCard4LastDigits { get; set; }

        [DataMember(Name = "numberOfPayments")]
        public int NumberOfPayments { get; set; }

        [DataMember(Name = "amountOfFirstPayment")]
        public decimal? AmountOfFirstPayment { get; set; }

        [DataMember(Name = "amountOfEachAdditionalPayment")]
        public decimal? AmountOfEachAdditionalPayment { get; set; }



        [DataMember(Name = "chequeBank")]
        public string ChequeBank { get; set; }

        [DataMember(Name = "chequeBranch")]
        public string ChequeBranch { get; set; }

        [DataMember(Name = "chequeAccount")]
        public string ChequeAccount { get; set; }
    }
}
