using Shared.Business;
using Shared.Business.Financial;
using Shared.Business.Security;
using Shared.Helpers;
using Shared.Integration.Models;
using Shared.Integration.Models.Invoicing;
using Shared.Integration.Models.PaymentDetails;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Transactions.Shared.Enums;
using Transactions.Shared.Models;

namespace Transactions.Business.Entities
{
    public class BillingDeal : IEntityBase<Guid>, IAuditEntity, IFinancialItem, ITerminalEntity, IMerchantEntity
    {
        public BillingDeal()
        {
            BillingDealTimestamp = DateTime.UtcNow;
            BillingDealID = Guid.NewGuid().GetSequentialGuid(BillingDealTimestamp.Value);

            //CreditCardDetails = new CreditCardDetails();

            DealDetails = new DealDetails();
        }

        /// <summary>
        /// Primary reference
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
        public Guid TerminalID { get; set; }

        /// <summary>
        /// Merchant
        /// </summary>
        public Guid MerchantID { get; set; }

        /// <summary>
        /// Currency
        /// </summary>
        public CurrencyEnum Currency { get; set; }

        /// <summary>
        /// Single transaction amount
        /// </summary>
        public decimal TransactionAmount { get; set; }

        [NotMapped]
        public decimal Amount { get => TransactionAmount; set => TransactionAmount = value; }

        public decimal VATRate { get; set; }

        public decimal VATTotal { get; set; }

        public decimal NetTotal { get; set; }

        /// <summary>
        /// Amount of transactions processed so far
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Current deal number
        /// </summary>
        public int? CurrentDeal { get; set; }

        /// <summary>
        /// Date-time when last created initially in UTC
        /// </summary>
        public DateTime? CurrentTransactionTimestamp { get; set; }

        /// <summary>
        /// Date-time when next transaction should be generated
        /// </summary>
        public DateTime? NextScheduledTransaction { get; set; }

        /// <summary>
        /// Reference to last deal
        /// </summary>
        public Guid? CurrentTransactionID { get; set; }

        /// <summary>
        /// Credit card information (just to display)
        /// </summary>
        public CreditCardDetails CreditCardDetails { get; set; }

        /// <summary>
        /// Bank account information
        /// </summary>
        public BankDetails BankDetails { get; set; }

        /// <summary>
        /// Stored credit card details token
        /// </summary>
        public Guid? CreditCardToken { get; set; }

        /// <summary>
        /// Deal information
        /// </summary>
        public DealDetails DealDetails { get; set; }

        /// <summary>
        /// Billing Schedule
        /// </summary>
        public BillingSchedule BillingSchedule { get; set; }

        /// <summary>
        /// Invoice details
        /// </summary>
        public InvoiceDetails InvoiceDetails { get; set; }

        /// <summary>
        /// Create document for transaction
        /// </summary>
        public bool IssueInvoice { get; set; }

        public bool InvoiceOnly { get; set; }

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

        public DocumentOriginEnum DocumentOrigin { get; set; }

        public DateTime? PausedFrom { get; set; }

        public DateTime? PausedTo { get; set; }

        public bool UnpauseRequired()
        {
            if (!Paused())
            {
                return false;
            }

            return PausedTo.Value.Date >= DateTime.UtcNow.Date;
        }

        public bool Paused()
        {
            if (!PausedFrom.HasValue || !PausedTo.HasValue)
            {
                return false;
            }

            var utcNow = DateTime.UtcNow.Date;
            return (PausedFrom.Value.Date <= utcNow) && (PausedTo.Value.Date >= utcNow);
        }

        public PaymentTypeEnum PaymentType { get; set; }

        public bool HasError { get; set; }

        public string LastError { get; set; }

        public string LastErrorCorrelationID { get; set; }

        public IEnumerable<PaymentDetails> PaymentDetails { get; set; }

        public string Origin { get; set; }

        public BillingProcessingStatusEnum InProgress { get; set; }

        public void UpdateNextScheduledDate(Guid? paymentTransactionID, DateTime? timestamp, DateTime? legalDate)
        {
            InProgress = BillingProcessingStatusEnum.Pending;
            CurrentTransactionID = paymentTransactionID;
            CurrentTransactionTimestamp = timestamp;
            CurrentDeal = CurrentDeal.HasValue ? CurrentDeal.Value + 1 : 1;
            HasError = false;
            LastError = null;
            if ((BillingSchedule.EndAtType == EndAtTypeEnum.AfterNumberOfPayments && BillingSchedule.EndAtNumberOfPayments.HasValue && CurrentDeal >= BillingSchedule.EndAtNumberOfPayments) ||
                (BillingSchedule.EndAtType == EndAtTypeEnum.SpecifiedDate && BillingSchedule.EndAt.HasValue && BillingSchedule.EndAt <= legalDate))
            {
                NextScheduledTransaction = null;
            }
            else
            {
                NextScheduledTransaction = BillingSchedule?.GetNextScheduledDate(legalDate.Value, CurrentDeal.Value);
            }

            Active = NextScheduledTransaction != null;
        }
    }
}
