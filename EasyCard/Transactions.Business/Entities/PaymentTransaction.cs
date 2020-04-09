using Shared.Business;
using Shared.Helpers;
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Transactions.Shared.Enums;

namespace Transactions.Business.Entities
{
    public class PaymentTransaction : IEntityBase<Guid>
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
        /// Primary key
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
        /// Transaction type
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
        /// 50 telephone deal
        /// 00 regular (megnetic)
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
        /// Credit card
        /// </summary>
        public CreditCardDetails CreditCardDetails { get; set; }

        /// <summary>
        /// Stored credit card details token (should be omitted in case if full credit card details used)
        /// </summary>
        public string CreditCardToken { get; set; }

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

            TotalAmount = InitialPaymentAmount + InstallmentPaymentAmount * (NumberOfPayments - 1);
        }

        public Guid GetID()
        {
            return PaymentTransactionID;
        }
    }
}
