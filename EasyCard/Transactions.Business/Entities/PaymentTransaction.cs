using Newtonsoft.Json.Linq;
using Shared.Business;
using Shared.Business.Financial;
using Shared.Helpers;
using Shared.Integration.Models;
using Shared.Integration.Models.PaymentDetails;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
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
            UpayTransactionDetails = new UpayTransactionDetails();
            ShvaTransactionDetails = new ShvaTransactionDetails();
            PinPadTransactionDetails = new PinPadTransactionsDetails();
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

        [NotMapped]
        public QuickStatusFilterTypeEnum QuickStatus
        {
            get { return GetQuickStatus(Status, JDealType); }
        }

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
        public string OKNumber { get; set; }

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
        /// Total discount from all items
        /// </summary>
        public decimal TotalDiscount { get; set; }

        /// <summary>
        /// Amount of one instalment payment
        /// </summary>
        public decimal InstallmentPaymentAmount { get; set; }

        /// <summary>
        /// Credit card information
        /// </summary>
        public CreditCardDetails CreditCardDetails { get; set; }

        /// <summary>
        /// Bank transfer information
        /// </summary>
        public BankTransferDetails BankTransferDetails { get; set; }

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

        public PinPadTransactionsDetails PinPadTransactionDetails { get; set; }

        /// <summary>
        /// PayDay details
        /// </summary>
        public ClearingHouseTransactionDetails ClearingHouseTransactionDetails { get; set; }

        public UpayTransactionDetails UpayTransactionDetails { get; set; }

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

        /// <summary>
        /// Payment intent reference
        /// </summary>
        public Guid? PaymentIntentID { get; set; }

        public DocumentOriginEnum DocumentOrigin { get; set; }

        public long? TerminalTemplateID { get; set; }

        // TODO: move pinpad details to separate class

        public string PinPadDeviceID { get; set; }

        /// <summary>
        /// Used both in shva and pinpad
        /// </summary>
        public int? ProcessorResultCode { get; set; }

        public long? MasavFileID { get; set; }

        [NotMapped]
        public string ConnectionID { get; set; }

        // TODO: calculate items, VAT
        [Obsolete]
        public void Calculate()
        {
            if (NumberOfPayments == 0)
            {
                NumberOfPayments = 1;
            }

            if (NetTotal == default)
            {
                NetTotal = Math.Round(TransactionAmount / (1m + VATRate), 2, MidpointRounding.AwayFromZero);
            }

            if (VATTotal == default)
            {
                VATTotal = TransactionAmount - NetTotal;
            }

            if (InitialPaymentAmount == 0)
            {
                InitialPaymentAmount = TransactionAmount;
            }

            if (DealDetails?.Items?.Count() > 0)
            {
                TotalDiscount = DealDetails.Items.Sum(e => e.Discount.GetValueOrDefault(0));
            }

            TotalAmount = InitialPaymentAmount + (InstallmentPaymentAmount * (NumberOfPayments - 1));
        }

        public Guid GetID()
        {
            return PaymentTransactionID;
        }

        public JObject Extension { get; set; }

        // TODO: move to BitDetails
        /// <summary>
        /// Resource ID created by bit backend. ID represents the payment initiation. Used for Get /Delete, etc.
        /// </summary>
        public string BitPaymentInitiationId { get; set; }

        // TODO: move to BitDetails
        /// <summary>
        /// Additional UUID used for authentication. When using web client application this ID, along with paymentInitiationId, should be sent upon opening bit payment page (openBitPaymentPage).
        /// </summary>
        public string BitTransactionSerialId { get; set; }

        public static QuickStatusFilterTypeEnum GetQuickStatus(TransactionStatusEnum @enum, JDealTypeEnum jDealType)
        {
            if (@enum == Shared.Enums.TransactionStatusEnum.CancelledByMerchant)
            {
                return QuickStatusFilterTypeEnum.Canceled;
            }

            if (@enum == Shared.Enums.TransactionStatusEnum.AwaitingForTransmission)
            {
                return QuickStatusFilterTypeEnum.AwaitingForTransmission;
            }

            if ((int)@enum > 0 && (int)@enum < 40)
            {
                return QuickStatusFilterTypeEnum.Pending;
            }

            if (@enum == Shared.Enums.TransactionStatusEnum.Completed)
            {
                return QuickStatusFilterTypeEnum.Completed;
            }

            if ((int)@enum < 0)
            {
                return QuickStatusFilterTypeEnum.Failed;
            }

            return QuickStatusFilterTypeEnum.Pending;
        }
    }
}
