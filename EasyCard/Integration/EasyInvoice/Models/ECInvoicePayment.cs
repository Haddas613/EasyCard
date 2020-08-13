using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace EasyInvoice.Models
{
    /*
    paymentMethod string yes     Available methods: CASH, CHEQUE, CREDIT_CARD_REGULAR_CREDIT, CREDIT_CARD_PLUS_30, CREDIT_CARD_INSTANT_BILLING, CREDIT_CARD_CLUB_CREDIT, CREDIT_CARD_SUPER_CREDIT, CREDIT_CARD_CREDITS, CREDIT_CARD_PAYMENTS, CREDIT_CARD_INSTALLMENT_CLUB_DEAL.
    paymentDateTime string yes     Date format: yyyy-MM-ddThh:mm.Must be in past.
    creditCardType string no      For paymentMethod = CREDIT_CARD_ * only.Available types: VISA, MASTERCARD, AMEX, DISCOVER, JCB, DINERS_CLUB, ISRACARD, OTHER.
    creditCard4LastDigits string no      For paymentMethod = CREDIT_CARD_ * only.Must be 4 characters long.
        numberOfPayments integer     no For paymentMethod = CREDIT_CARD_CREDITS and paymentMethod = CREDIT_CARD_PAYMENTS only.
    amountOfFirstPayment  float no      For paymentMethod = CREDIT_CARD_PAYMENTS only.
    amountOfEachAdditionalPayment float no  For paymentMethod = CREDIT_CARD_PAYMENTS only.
    chequeBank            string no      For paymentMethod = CHEQUE only.
    chequeBranch          string no      For paymentMethod = CHEQUE only.
    chequeAccount         string no      For paymentMethod = CHEQUE only.
    */

    [DataContract]
    public class ECInvoicePayment
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
