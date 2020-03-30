using Newtonsoft.Json.Linq;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Integration.Models
{
    public class AggregatorCreateTransactionRequest
    {
        /// <summary>
        /// Unique transaction ID
        /// </summary>
        public string TransactionID { get; set; }

        public string EasyCardTerminalID { get; set; }

        /// <summary>
        /// (?)
        /// </summary>
        public string AggregatorTerminalID { get; set; }

        /// <summary>
        /// (?)
        /// </summary>
        public string ProcessorTerminalID { get; set; }

        public JObject AggregatorSettings { get; set; }

        public TransactionTypeEnum TransactionType { get; set; }

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
        /// Number Of Installments
        /// </summary>
        public int NumberOfInstallments { get; set; }

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
        /// Does not have real credit card number
        /// </summary>
        public CreditCardDetails CreditCardDetails { get; set; }

        /// <summary>
        /// Deal information
        /// </summary>
        public DealDetails DealDetails { get; set; }
    }
}
