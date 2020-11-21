﻿using Shared.Helpers;
using Shared.Integration.Models.Invoicing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Models.Transactions;
using Transactions.Shared.Enums;
using Transactions.Shared.Models;
using SharedIntegration = Shared.Integration;

namespace Transactions.Api.Models.Billing
{
    public class BillingDealResponse
    {
        /// <summary>
        /// Primary transaction reference
        /// </summary>
        public Guid BillingDealID { get; set; }

        /// <summary>
        /// Date-time when deal created initially in UTC
        /// </summary>
        public DateTime? BillingDealTimestamp { get; set; }

        /// <summary>
        /// Reference to initial transaction
        /// </summary>
        public Guid? InitialTransactionID { get; set; }

        /// <summary>
        /// Terminal
        /// </summary>
        public Guid? TerminalID { get; set; }

        /// <summary>
        /// Merchant
        /// </summary>
        public Guid? MerchantID { get; set; }

        /// <summary>
        /// Currency
        /// </summary>
        public CurrencyEnum Currency { get; set; }

        /// <summary>
        /// This transaction amount
        /// </summary>
        public decimal TransactionAmount { get; set; }

        /// <summary>
        /// TotalAmount = TransactionAmount * NumberOfPayments
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Current deal (billing)
        /// </summary>
        public int? CurrentDeal { get; set; }

        /// <summary>
        /// Date-time when last created initially in UTC
        /// </summary>
        public DateTime? CurrentTransactionTimestamp { get; set; }

        /// <summary>
        /// Reference to last deal
        /// </summary>
        public Guid? CurrentTransactionID { get; set; }

        /// <summary>
        /// Date-time when next transaction should be generated
        /// </summary>
        public DateTime? NextScheduledTransaction { get; set; }

        /// <summary>
        /// Credit card information (just to display)
        /// </summary>
        public CreditCardDetails CreditCardDetails { get; set; }

        /// <summary>
        /// Stored credit card details token
        /// </summary>
        public Guid CreditCardToken { get; set; }

        /// <summary>
        /// Deal information
        /// </summary>
        public SharedIntegration.Models.DealDetails DealDetails { get; set; }

        /// <summary>
        /// Billing Schedule
        /// </summary>
        public BillingSchedule BillingSchedule { get; set; }

        /// <summary>
        /// Date-time when transaction status updated
        /// </summary>
        public DateTime? UpdatedDate { get; set; }

        /// <summary>
        /// Concurrency key
        /// </summary>
        public byte[] UpdateTimestamp { get; set; }

        public string OperationDoneBy { get; set; }

        public Guid? OperationDoneByID { get; set; }

        public string CorrelationId { get; set; }

        public string SourceIP { get; set; }

        public bool Active { get; set; }

        public bool? CardExpired { get; set; }

        /// <summary>
        /// Invoice details
        /// </summary>
        public InvoiceDetails InvoiceDetails { get; set; }
    }
}
