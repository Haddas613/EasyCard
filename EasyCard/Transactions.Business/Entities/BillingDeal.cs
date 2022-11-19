using Shared.Business;
using Shared.Business.Financial;
using Shared.Business.Security;
using Shared.Helpers;
using Shared.Helpers.WebHooks;
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

            Active = true;

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
        public DateTime? NextScheduledTransaction { get; private set; }

        /// <summary>
        /// Reference to last deal
        /// </summary>
        public Guid? CurrentTransactionID { get; set; }

        /// <summary>
        /// Credit card information (just to display)
        /// </summary>
        public CreditCardDetails CreditCardDetails { get; private set; }

        /// <summary>
        /// Bank account information
        /// </summary>
        public BankDetails BankDetails { get; set; }

        /// <summary>
        /// Stored credit card details token
        /// </summary>
        public Guid? CreditCardToken { get; private set; }

        /// <summary>
        /// Deal information
        /// </summary>
        public DealDetails DealDetails { get; set; }

        /// <summary>
        /// Billing Schedule
        /// </summary>
        public BillingSchedule BillingSchedule { get; private set; }

        /// <summary>
        /// Invoice details
        /// </summary>
        public InvoiceDetails InvoiceDetails { get; set; }

        /// <summary>
        /// Create document for transaction
        /// </summary>
        public bool IssueInvoice { get; set; }

        public bool InvoiceOnly { get; set; }

        [NotMapped]
        public BillingDealTypeEnum BillingDealType =>
                (InvoiceOnly, PaymentType) switch
                {
                    (true, _) => BillingDealTypeEnum.InvoiceOnly,
                    (false, PaymentTypeEnum.Card) => BillingDealTypeEnum.CreditCard,
                    (false, PaymentTypeEnum.Bank) => BillingDealTypeEnum.Bank,
                    _ => BillingDealTypeEnum.CreditCard
                };

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

        public bool Active { get; private set; }

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

        public bool HasError { get; private set; }

        public string LastError { get; set; }

        public string LastErrorCorrelationID { get; set; }

        public IEnumerable<PaymentDetails> PaymentDetails { get; set; }

        public string Origin { get; set; }

        public BillingProcessingStatusEnum InProgress { get; set; }

        public int? FailedAttemptsCount { get; set; }

        public DateTime? ExpirationEmailSent { get; set; }

        public DateTime? TokenUpdated { get; set; }

        public DateTime? TokenCreated { get; set; }

        [NotMapped]
        public bool? TokenNotAvailable
        {
            get { return PaymentType == PaymentTypeEnum.Card && CreditCardToken == null; }
        }

        [NotMapped]
        public WebHooksConfiguration WebHooksConfiguration { get; set; }

        public void UpdateCreditCardToken(Guid? token, CreditCardDetails creditCardDetails, DateTime? tokenCreated)
        {
            if (PaymentType != PaymentTypeEnum.Card || InvoiceOnly == true)
            {
                throw new ApplicationException($"It is not possible to set token for billing {BillingDealID} because payment type is {PaymentType} and InvoiceOnly flag is {InvoiceOnly}");
            }

            CreditCardToken = token;
            ExpirationEmailSent = null;
            CreditCardDetails = creditCardDetails;
            TokenCreated = tokenCreated;
            TokenUpdated = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, UserCultureInfo.TimeZone).Date;
        }

        public void ResetToken()
        {
            CreditCardToken = null;
        }

        public void ExtendToken(CardExpiration cardExpiration)
        {
            CreditCardDetails.CardExpiration = cardExpiration;
        }

        public void Activate()
        {
            Active = true;
        }

        public void Deactivate()
        {
            Active = false;
        }

        public void UpdateNextScheduledDatInitial(BillingSchedule billingSchedule, DateTime? existingTransactionTimestamp = null)
        {
            if (BillingSchedule?.Equals(billingSchedule) == true)
            {
                return;
            }

            billingSchedule.Validate(existingTransactionTimestamp, BillingSchedule?.StartAt != billingSchedule.StartAt);
            BillingSchedule = billingSchedule;

            if (existingTransactionTimestamp.HasValue)
            {
                var lastTransactionDate = TimeZoneInfo.ConvertTimeFromUtc(existingTransactionTimestamp.Value, UserCultureInfo.TimeZone).Date;
                DateTime fromDate = lastTransactionDate;
                if (BillingSchedule.StartAt.HasValue && BillingSchedule.StartAt.Value > lastTransactionDate)
                {
                    NextScheduledTransaction = BillingSchedule.StartAt.Value;
                }
                else
                {
                    NextScheduledTransaction = BillingSchedule.GetNextScheduledDate(fromDate, CurrentDeal.Value);
                }
            }
            else
            {
                NextScheduledTransaction = BillingSchedule.GetInitialScheduleDate();
            }

            var legalDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, UserCultureInfo.TimeZone).Date;

            if ((BillingSchedule.EndAtType == EndAtTypeEnum.AfterNumberOfPayments &&
                BillingSchedule.EndAtNumberOfPayments.HasValue && CurrentDeal >= BillingSchedule.EndAtNumberOfPayments) ||
                (BillingSchedule.EndAtType == EndAtTypeEnum.SpecifiedDate && BillingSchedule.EndAt.HasValue &&
                (BillingSchedule.EndAt < legalDate || BillingSchedule.EndAt < NextScheduledTransaction)))
            {
                NextScheduledTransaction = null;
            }

            Active = NextScheduledTransaction != null;
        }

        public void UpdateNextScheduledDatAfterSuccess(Guid? paymentTransactionID, DateTime? timestamp, DateTime? legalDate)
        {
            if (BillingSchedule == null)
            {
                throw new ApplicationException("BillingSchedule is null");
            }

            InProgress = BillingProcessingStatusEnum.Pending;
            CurrentTransactionID = paymentTransactionID;
            CurrentTransactionTimestamp = timestamp;
            CurrentDeal = CurrentDeal.HasValue ? CurrentDeal.Value + 1 : 1;
            HasError = false;
            LastError = null;
            FailedAttemptsCount = 0;
            if ((BillingSchedule.EndAtType == EndAtTypeEnum.AfterNumberOfPayments && BillingSchedule.EndAtNumberOfPayments.HasValue && CurrentDeal >= BillingSchedule.EndAtNumberOfPayments) ||
                (BillingSchedule.EndAtType == EndAtTypeEnum.SpecifiedDate && BillingSchedule.EndAt.HasValue && (BillingSchedule.EndAt <= legalDate || BillingSchedule.EndAt <= NextScheduledTransaction)))
            {
                NextScheduledTransaction = null;
            }
            else
            {
                NextScheduledTransaction = BillingSchedule.GetNextScheduledDate(legalDate.Value, CurrentDeal.Value);
            }

            Active = NextScheduledTransaction != null;
        }

        public void UpdateNextScheduledDatAfterError(string errorMessage, string correlationID, int? failedTransactionsCountBeforeInactivate, int? numberOfDaysToRetryTransaction)
        {
            InProgress = BillingProcessingStatusEnum.Pending;
            HasError = true;
            LastError = errorMessage;
            LastErrorCorrelationID = correlationID;
            FailedAttemptsCount = FailedAttemptsCount.GetValueOrDefault() + 1;

            NextScheduledTransaction = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, UserCultureInfo.TimeZone).Date.AddDays(numberOfDaysToRetryTransaction.GetValueOrDefault(1));

            if (NextScheduledTransaction.Value.Day > 20)
            {
                NextScheduledTransaction = new DateTime(NextScheduledTransaction.Value.Year, NextScheduledTransaction.Value.Month, 1).AddMonths(1);
            }
        }

        public void RevertLastTransactionForBankBillings(string errorMessage)
        {
            CurrentDeal = CurrentDeal.HasValue ? CurrentDeal.Value - 1 : 0;
            InProgress = BillingProcessingStatusEnum.Pending;
            HasError = true;
            LastError = errorMessage;
            FailedAttemptsCount = FailedAttemptsCount.GetValueOrDefault() + 1;
            NextScheduledTransaction = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, UserCultureInfo.TimeZone).Date;
        }
    }
}
