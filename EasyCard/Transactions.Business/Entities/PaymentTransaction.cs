using Shared.Business;
using Shared.Helpers;
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
        }

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
        /// Reference to first installment or to original transaction in case of refund
        /// </summary>
        public Guid? InitialTransactionID { get; set; }

        public Guid? TerminalID { get; set; }

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
        /// TODO: change ExternalSystemSummary to (?)
        /// </summary>
        public long? MarketerID { get; set; }

        public TransactionStatusEnum Status { get; set; }

        public TransactionTypeEnum TransactionType { get; set; }

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

        public CreditCardDetails CreditCardDetails { get; set; }

        /// <summary>
        /// Stored credit card details token (should be omitted in case if full credit card details used)
        /// </summary>
        public string CreditCardToken { get; set; }

        /// <summary>
        /// Deal information
        /// </summary>
        public DealDetails DealDetails { get; set; }

        public ShvaTransactionDetails ShvaTransactionDetails { get; set; }

        public ClearingHouseTransactionDetails ClearingHouseTransactionDetails { get; set; }

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

        public Guid? AggregatorCreateTransactionMessageID { get; set; }

        public Guid? AggregatorCommitTransactionMessageID { get; set; }

        public Guid? ProcessorCreateTransactionMessageID { get; set; }

        public Guid? ProcessorTransmitMessageID { get; set; }
    }
}
