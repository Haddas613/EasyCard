using Shared.Business;
using Shared.Business.Financial;
using Shared.Helpers;
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Transactions.Shared.Enums;

namespace Transactions.Business.Entities
{
    public class PaymentTransaction : IEntityBase<Guid>, IFinancialItem
    {
        public PaymentTransaction()
        {
            TransactionTimestamp = DateTime.UtcNow;
            TransactionDate = TimeZoneInfo.ConvertTimeFromUtc(TransactionTimestamp.Value, UserCultureInfo.TimeZone).Date;
            PaymentTransactionID = Guid.NewGuid().GetSequentialGuid(TransactionTimestamp.Value);
            CreditCardDetails = new CreditCardDetails();
            ClearingHouseTransactionDetails = new ClearingHouseTransactionDetails();
            ShvaTransactionDetails = new ShvaTransactionDetails();
            DealDetails = new DealDetails();
        }

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
        /// Reference to initial token transaction
        /// </summary>
        public Guid? InitialTransactionID { get; set; }

        /// <summary>
        /// Reference to initial billing deal
        /// </summary>
        public Guid? BillingDealID { get; set; }

        /// <summary>
        /// Current deal (billing)
        /// </summary>
        public int? CurrentDeal { get; set; }

        /// <summary>
        /// Terminal
        /// </summary>
        public Guid TerminalID { get; set; }

        /// <summary>
        /// Merchant
        /// </summary>
        public Guid MerchantID { get; set; }

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
        /// Payment Type
        /// </summary>
        public PaymentTypeEnum PaymentTypeEnum { get; set; }

        /// <summary>
        /// Status of finalization operations in case of failed transaction, rejection or cancelation
        /// </summary>
        public TransactionFinalizationStatusEnum? FinalizationStatus { get; set; }

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
        /// Rejection Reason Message
        /// </summary>
        public string RejectionMessage { get; set; }

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

        [NotMapped]
        public decimal Amount { get => TransactionAmount; set => TransactionAmount = value; }

        public decimal VATRate { get; set; }

        public decimal VATTotal { get; set; }

        public decimal NetTotal { get; set; }

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
        public Guid? CreditCardToken { get; set; }

        /// <summary>
        /// Deal information
        /// </summary>
        public DealDetails DealDetails { get; set; }

        /// <summary>
        /// Shva details
        /// </summary>
        public ShvaTransactionDetails ShvaTransactionDetails { get; set; }

        /// <summary>
        /// PayDay details
        /// </summary>
        public ClearingHouseTransactionDetails ClearingHouseTransactionDetails { get; set; }

        /// <summary>
        /// Date-time when transaction status updated
        /// </summary>
        public DateTime? UpdatedDate { get; set; }

        /// <summary>
        /// Concurrency key
        /// </summary>
        public byte[] UpdateTimestamp { get; set; }

        /// <summary>
        /// We can know it from checkout page activity
        /// </summary>
        public string ConsumerIP { get; set; }

        /// <summary>
        /// Merchant's IP
        /// </summary>
        public string MerchantIP { get; set; }

        /// <summary>
        /// Request ID
        /// </summary>
        public string CorrelationId { get; set; }

        /// <summary>
        /// Generated invoice ID
        /// </summary>
        public Guid? InvoiceID { get; set; }

        // NOTE: this field required in case if InvoiceID is empty due to exception

        /// <summary>
        /// Create document for transaction
        /// </summary>
        public bool IssueInvoice { get; set; }

        /// <summary>
        /// Payment request reference
        /// </summary>
        public Guid? PaymentRequestID { get; set; }

        // TODO: calculate items
        [Obsolete]
        public void Calculate()
        {
            if (NumberOfPayments == 0)
            {
                NumberOfPayments = 1;
            }

            if (InitialPaymentAmount == 0)
            {
                InitialPaymentAmount = TransactionAmount;
            }

            TotalAmount = InitialPaymentAmount + (InstallmentPaymentAmount * (NumberOfPayments - 1));
        }

        public Guid GetID()
        {
            return PaymentTransactionID;
        }
    }
}
