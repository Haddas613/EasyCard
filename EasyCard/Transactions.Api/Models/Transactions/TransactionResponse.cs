﻿using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Shared.Helpers;
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Transactions.Shared.Enums;
using Enums = Transactions.Shared.Enums;
using IntegrationModels = Shared.Integration.Models;

namespace Transactions.Api.Models.Transactions
{
    /// <summary>
    /// Payment transaction details
    /// </summary>
    public class TransactionResponse
    {
        /// <summary>
        /// Primary transaction reference
        /// </summary>
        public Guid PaymentTransactionID { get; set; }

        /// <summary>
        /// Legal transaction day
        /// </summary>
        public DateTime? TransactionDate { get; set; }

        /// <summary>
        /// Date-time when transaction created initially in UTC
        /// </summary>
        public DateTime? TransactionTimestamp { get; set; }

        /// <summary>
        /// Reference to initial billing deal
        /// </summary>
        public Guid? InitialTransactionID { get; set; }

        /// <summary>
        /// Current deal (billing)
        /// </summary>
        public int? CurrentDeal { get; set; }

        /// <summary>
        /// Terminal
        /// </summary>
        public Guid? TerminalID { get; set; }

        /// <summary>
        /// Merchant
        /// </summary>
        public Guid? MerchantID { get; set; }

        /// <summary>
        /// Shva or other processor
        /// </summary>
        public long? ProcessorID { get; set; }

        /// <summary>
        /// Clearing House or Upay
        /// </summary>
        public long? AggregatorID { get; set; }

        /// <summary>
        /// EasyInvoice or RapidOne
        /// </summary>
        public long? InvoicingID { get; set; }

        /// <summary>
        /// Marketer ID
        /// </summary>
        public long? MarketerID { get; set; }

        /// <summary>
        /// Processing status
        /// </summary>
        public TransactionStatusEnum Status { get; set; }

        /// <summary>
        /// Generic transaction type
        /// </summary>
        public TransactionTypeEnum TransactionType { get; set; }

        /// <summary>
        /// Special transaction type
        /// </summary>
        public SpecialTransactionTypeEnum SpecialTransactionType { get; set; }

        /// <summary>
        /// J3, J4, J5
        /// </summary>
        public JDealTypeEnum JDealType { get; set; }

        /// <summary>
        /// Rejection Reason
        /// </summary>
        public RejectionReasonEnum? RejectionReason { get; set; }

        /// <summary>
        /// Currency
        /// </summary>
        public CurrencyEnum Currency { get; set; }

        /// <summary>
        /// Telephone deal or Regular (megnetic)
        /// </summary>
        public CardPresenceEnum CardPresence { get; set; }

        /// <summary>
        /// Number Of Installments
        /// </summary>
        public int NumberOfPayments { get; set; }

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
        /// Credit card information
        /// </summary>
        public CreditCardDetails CreditCardDetails { get; set; }

        /// <summary>
        /// Stored credit card details token
        /// </summary>
        public string CreditCardToken { get; set; }

        /// <summary>
        /// Deal information
        /// </summary>
        public DealDetails DealDetails { get; set; }

        /// <summary>
        /// Shva details
        /// </summary>
        public JObject ShvaTransactionDetails { get; set; }

        /// <summary>
        /// PayDay details
        /// </summary>
        public JObject ClearingHouseTransactionDetails { get; set; }

        /// <summary>
        /// Date-time when transaction status updated
        /// </summary>
        public DateTime? UpdatedDate { get; set; }

        /// <summary>
        /// Concurrency key
        /// </summary>
        public byte[] UpdateTimestamp { get; set; }
    }
}
