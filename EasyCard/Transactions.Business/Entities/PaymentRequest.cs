using Newtonsoft.Json.Linq;
using Shared.Business;
using Shared.Business.Financial;
using Shared.Business.Security;
using Shared.Helpers;
using Shared.Integration.Models.Invoicing;
using Shared.Integration.Models.Processor;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Transactions.Shared.Enums;

namespace Transactions.Business.Entities
{
    public class PaymentRequest : IEntityBase<Guid>, IAuditEntity, IFinancialItem
    {
        public PaymentRequest()
        {
            PaymentRequestTimestamp = DateTime.UtcNow;
            PaymentRequestID = Guid.NewGuid().GetSequentialGuid(PaymentRequestTimestamp.Value);
            DealDetails = new DealDetails();
            InvoiceDetails = new InvoiceDetails();
        }

        /// <summary>
        /// Primary reference
        /// </summary>
        public Guid PaymentRequestID { get; set; }

        /// <summary>
        /// Date-time when deal created initially in UTC
        /// </summary>
        public DateTime? PaymentRequestTimestamp { get; set; }

        /// <summary>
        /// Due date
        /// </summary>
        public DateTime? DueDate { get; set; }

        // TODO: make non-nullable
        /// <summary>
        /// Terminal
        /// </summary>
        public Guid? TerminalID { get; set; }

        // TODO: make non-nullable
        /// <summary>
        /// Merchant
        /// </summary>
        public Guid? MerchantID { get; set; }

        public InvoiceDetails InvoiceDetails { get; set; }

        public PinPadDetails PinPadDetails { get; set; }

        /// <summary>
        /// Create document for transaction
        /// </summary>
        public bool IssueInvoice { get; set; }

        public bool AllowPinPad { get; set; }

        /// <summary>
        /// Processing status
        /// </summary>
        public PaymentRequestStatusEnum Status { get; set; }

        /// <summary>
        /// Currency
        /// </summary>
        public CurrencyEnum Currency { get; set; }

        /// <summary>
        /// Number Of payments (cannot be more than 999)
        /// </summary>
        public int NumberOfPayments { get; set; }

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
        /// This amount
        /// </summary>
        public decimal PaymentRequestAmount { get; set; }

        [NotMapped]
        public decimal Amount { get => PaymentRequestAmount; set => PaymentRequestAmount = value; }

        /// <summary>
        /// Will be used for invoice
        /// </summary>
        public decimal VATRate { get; set; }

        public decimal VATTotal { get; set; }

        public decimal NetTotal { get; set; }

        public string CardOwnerName { get; set; }

        public string CardOwnerNationalID { get; set; }

        /// <summary>
        /// Deal information
        /// </summary>
        public DealDetails DealDetails { get; set; }

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
            return PaymentRequestID;
        }

        public string OperationDoneBy { get; set; }

        public Guid? OperationDoneByID { get; set; }

        public string CorrelationId { get; set; }

        public string SourceIP { get; set; }

        public Guid? PaymentTransactionID { get; set; }

        public string RequestSubject { get; set; }

        public string FromAddress { get; set; }

        public DocumentOriginEnum DocumentOrigin { get; set; }

        public bool IsRefund { get; set; }

        // TODO: calculate items, VAT
        [Obsolete]
        public void Calculate()
        {
            if (NumberOfPayments == 0)
            {
                NumberOfPayments = 1;
            }

            if (InitialPaymentAmount == 0)
            {
                InitialPaymentAmount = PaymentRequestAmount;
            }

            if (PaymentRequestAmount > 0)
            {
                if (NetTotal == default)
                {
                    NetTotal = Math.Round(PaymentRequestAmount / (1m + VATRate), 2, MidpointRounding.AwayFromZero);
                }

                if (VATTotal == default)
                {
                    VATTotal = PaymentRequestAmount - NetTotal;
                }
            }

            TotalAmount = InitialPaymentAmount + (InstallmentPaymentAmount * (NumberOfPayments - 1));
        }

        public string RedirectUrl { get; set; }

        public bool UserAmount { get; set; }

        public JObject Extension { get; set; }

        [NotMapped]
        public Guid? BillingDealID { get; set; }

        [NotMapped]
        public string Language { get; set; }

        public string PaymentRequestUrl { get; set; }

        public string Origin { get; set; }

        public bool? AllowInstallments { get; set; }

        public bool? AllowCredit { get; set; }

        public bool? AllowImmediate { get; set; }

        public bool? HidePhone { get; set; }

        public bool? HideEmail { get; set; }

        public bool? HideNationalID { get; set; }

        public bool? ShowAuthCode { get; set; }
    }
}
