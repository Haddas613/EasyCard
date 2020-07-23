﻿using Shared.Business;
using Shared.Business.Security;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using Transactions.Shared.Enums;
using Transactions.Shared.Models;

namespace Transactions.Business.Entities
{
    public class BillingDeal : IEntityBase<Guid>, IAuditEntity
    {
        public BillingDeal()
        {
            BillingDealTimestamp = DateTime.UtcNow;
            BillingDealID = Guid.NewGuid().GetSequentialGuid(BillingDealTimestamp.Value);
            CreditCardDetails = new CreditCardDetails();
            DealDetails = new DealDetails();
        }

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
        public BillingDealStatusEnum Status { get; set; }

        /// <summary>
        /// Currency
        /// </summary>
        public CurrencyEnum Currency { get; set; }

        /// <summary>
        /// Number Of payments (cannot be more than 999)
        /// </summary>
        public int NumberOfPayments { get; set; }

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
        public DealDetails DealDetails { get; set; }

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

        public Guid GetID()
        {
            return BillingDealID;
        }

        public string OperationDoneBy { get; set; }

        public Guid? OperationDoneByID { get; set; }

        public string CorrelationId { get; set; }

        public string SourceIP { get; set; }

        public bool Active { get; set; }
    }
}