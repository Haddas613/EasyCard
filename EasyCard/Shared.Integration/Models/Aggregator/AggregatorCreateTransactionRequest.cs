﻿using Newtonsoft.Json.Linq;
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

        /// <summary>
        /// Request ID
        /// </summary>
        public string CorrelationId { get; set; }

        /// <summary>
        /// Original terminal
        /// </summary>
        public string EasyCardTerminalID { get; set; }

        /// <summary>
        /// Specific aggregator settings
        /// </summary>
        public object AggregatorSettings { get; set; }

        /// <summary>
        /// Transaction type
        /// </summary>
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

        /// <summary>
        /// J3, J4, J5
        /// </summary>
        public JDealTypeEnum JDealType { get; set; }

        /// <summary>
        /// Special transaction type
        /// </summary>
        public SpecialTransactionTypeEnum SpecialTransactionType { get; set; }

        /// <summary>
        /// IsPinPad transaction
        /// </summary>
        public bool IsPinPad { get; set; }

        /// <summary>
        /// Is Bit Transaction
        /// </summary>
        public bool IsBit { get; set; }
    }
}
