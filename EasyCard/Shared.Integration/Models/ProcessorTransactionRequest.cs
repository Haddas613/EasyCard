using Shared.Api.Models.Enums;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Integration.Models
{
    public class ProcessorTransactionRequest
    {
        /// <summary>
        /// Shva terminal settings
        /// </summary>
        public object ProcessorSettings { get; set; }

        /// <summary>
        /// Unique transaction ID
        /// </summary>
        public string TransactionID { get; set; }

        public string EasyCardTerminalID { get; set; }

        /// <summary>
        /// Shva terminal ID
        /// </summary>
        public string ProcessorTerminalID { get; set; }

        public TransactionTypeEnum TransactionType { get; set; }

        public JDealTypeEnum JDealType { get; set; }

        /// <summary>
        /// Indicates ths this is refund deal
        /// </summary>
        public bool Refund { get; set; }

        /// <summary>
        /// Currency
        /// </summary>
        public CurrencyEnum Currency { get; set; }

        /// <summary>
        /// 50 telephone deal
        /// 00 regular (megnetic)
        /// </summary>
        public CardPresenceEnum CardPresence { get; set; }

        /// <summary>
        /// Current installment
        /// </summary>
        public int CurrentInstallment { get; set; }

        /// <summary>
        /// This transaction amount
        /// </summary>
        public decimal TransactionAmount { get; set; }

        /// <summary>
        /// Initial installment payment
        /// </summary>
        public decimal InitialPaymentAmount { get; set; }

        /// <summary>
        /// TotalAmount = InitialPaymentAmount + (NumberOfInstallments - 1) * InstallmentPaymentAmount
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Amount of one instalment payment
        /// </summary>
        public decimal InstallmentPaymentAmount { get; set; }

        /// <summary>
        /// Legal transaction day
        /// </summary>
        public DateTime? TransactionDate { get; set; }

        /// <summary>
        /// Real credit card number
        /// </summary>
        public CreditCardToken CreditCardToken { get; set; }

        /// <summary>
        /// Will be ShvaCreatedTransactionDetails; TODO: possible needs to be used additional model or ProcessorTransactionRequest
        /// </summary>
        public object InitialTransaction { get; set; }

        /// <summary>
        /// To be used for credit or installments 
        /// </summary>
        public int NumberOfPayments { get; set; }
    }
}
